using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Valkyrie.WebApiProvider;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;
using APTAT.Common;

namespace APTAT.Valkyrie
{
    public sealed class ValkyrieClient : IValkyrieClient
    {
        
        private readonly int _changeTrakingIntervalInMs = (int)TimeSpan.FromMinutes(3).TotalMilliseconds;
        private readonly IMachineKeyProvider _machineKeyProvider;
        private const Int32 MaxFileLenght = 26214400;
        private string _apiKey;
        private int _fileScanned;
        private int _filesCount;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly string _endpointId;
        private ConcurrentBag<ValkyrieAnalysisResult> _analysisResults;
        private ConcurrentDictionary<string, ValkyrieAnalysisResult> _analysedItems
            = new ConcurrentDictionary<string, ValkyrieAnalysisResult>(StringComparer.OrdinalIgnoreCase);

        private ConcurrentDictionary<string, ValkyrieAnalysisResult> _pendingItemsResolved
            = new ConcurrentDictionary<string, ValkyrieAnalysisResult>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, ValkyrieAnalyseOperationContext> _automaticAnalyseInProgressByHash
            = new Dictionary<string, ValkyrieAnalyseOperationContext>(StringComparer.OrdinalIgnoreCase); 
        private readonly Object _automaticAnalyseSync = new object();
        private string _aptScanId;
        private readonly Object _aptScanIdLock = new object();
        private readonly HashSet<ValkyrieFileInfo> _lookupContexts = new HashSet<ValkyrieFileInfo>();

        private readonly Object _lookupSync = new object();
        private bool _checkingTimerIsStop = true;
        private bool _isMainScanFinished = true;
        private bool _completed = false;

        private Timer _progressNotifyTimer;
        private Timer _checkStatusesTimer;
        private int _machineCount;
        private string _login;
        private string _password;

        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult>> AnalyzeProgressChanged;
        public event EventHandler<AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>> AnalyzeCompleted;
        public event EventHandler AnalyzeStarted;

        private readonly  IValkyrieWebApiProvider _provider;
        
        public ValkyrieClient(IMachineKeyProvider machineKeyProvider, IValkyrieWebApiProvider provider)
        {
            _machineKeyProvider = machineKeyProvider;
            _provider = provider;
            _endpointId = _machineKeyProvider.GlobalMachineKey;
            _analysisResults = new ConcurrentBag<ValkyrieAnalysisResult>();
            _progressNotifyTimer = new Timer(ProgressNotifyTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _checkStatusesTimer = new Timer(CheckStatusesTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void AddFilesToAnalyze(List<ValkyrieFileInfo> filesToAnalyze)
        {
            if (!filesToAnalyze.Any()) return;
            bool needStartTimer = false;
            lock (_lookupSync)
            {
                needStartTimer = _checkingTimerIsStop;
                _checkingTimerIsStop = false;
                foreach (var fileInfo in filesToAnalyze)
                    _lookupContexts.Add(fileInfo);
                needStartTimer = needStartTimer && _isMainScanFinished;
            }
            if (needStartTimer)
                _checkStatusesTimer?.Change(TimeSpan.FromSeconds(5).Milliseconds, Timeout.Infinite);
        }

        private RegisterWithResponse Register(CamRegistration camRegistration, string productName, 
            NetworkCredential networkCredential = null )
        {

            if (camRegistration == null || camRegistration.IsEmpty)
                throw new InvalidOperationException("CAM license or CAM credentials are required");

            if (networkCredential != null && ProxySettings.Proxy == null)
                ProxySettings.Proxy = GetProxy(networkCredential);
            RegisterWithResponse regInfo;

            var asm = Assembly.GetExecutingAssembly().GetName();

            
            if (string.IsNullOrWhiteSpace(camRegistration.LicenseId) == false)
            {
                regInfo = _provider.RegisterWithCamLicense(new RegisterWithCamLicenseRequest()
                {
                    LicenseId = camRegistration.LicenseId,
                    ProductName = productName,
                    ProductVersion = asm.Version.ToString()
                });

            }
            else
            {
                regInfo = _provider.RegisterWithCamCredentials(new RegisterWithCamCredentials
                {
                    UserName = camRegistration.UserName,
                    Password = camRegistration.Password,
                    ProductName = productName,
                    ProductVersion = asm.Version.ToString()
                });
            }
            _apiKey = regInfo.Key;
            return regInfo;
        }

        private IWebProxy GetProxy(NetworkCredential networkCredential)
        {
            var proxy = WebRequest.GetSystemWebProxy();
            if (networkCredential != null)
            {
             //   proxy = new WebProxy();
                proxy.Credentials = networkCredential;
            }
            return proxy;
        }

        private void ProgressNotifyTimerCallback(object state)
        {
            _progressNotifyTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            OnAnalyzeProgressChanged(_analysedItems);
            if (_isMainScanFinished && _analysedItems.Count == 0)  return;
            _progressNotifyTimer?.Change(1000, Timeout.Infinite);
        }

        private void CheckStatusesTimerCallback(object state)
        {
            _checkStatusesTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            var waitNextInterval = false;
            var needEndScan = false;
            bool stopTimer = false;
            if (_aptScanId == null)
            {
                lock (_aptScanIdLock)
                {
                    while (true)
                    {
                        if (_aptScanId != null)
                            break;
                        if (_apiKey == null)
                            waitNextInterval = true;
                        var startScanResult = _provider.StartScan(new StartScanRequest(_apiKey));
                        if (startScanResult.ReturnCode == ErrorCodesEnum.Success)
                        {
                            _aptScanId = startScanResult.AptScanId;
                            needEndScan = true;
                        }
                        else
                            waitNextInterval = true;
                    }
                }
            }
            if (!waitNextInterval)
            {
                CheckNewStatusesOnValkyrie();
                lock (_lookupSync)
                {
                    var pendingExists = _lookupContexts.Any() || _automaticAnalyseInProgressByHash.Any();
                    _checkingTimerIsStop =
                        stopTimer = !pendingExists || _cancellationTokenSource.IsCancellationRequested;
                }
                OnAnalyzeProgressChanged(_pendingItemsResolved, true);
                if (needEndScan)
                    Task.Factory.StartNew(t =>
                    {
                        _provider.EndScan(new EndScanRequest(_apiKey, _machineCount, _aptScanId));
                    }, TaskCreationOptions.None);
            }
            if (!stopTimer)
                _checkStatusesTimer?.Change(_changeTrakingIntervalInMs, Timeout.Infinite);
        }

        private void CheckNewStatusesOnValkyrie()
        {
            List<ValkyrieFileInfo> filesToAnalyze;
            lock (_lookupSync)
            {
                filesToAnalyze = _lookupContexts.ToList();
            }
            SubmitToAutomaticAnalysis(filesToAnalyze, _cancellationTokenSource.Token, MergeResult, false);
            List<ValkyrieAnalyseOperationContext> filesToCheckAnalysisResult;
            lock (_automaticAnalyseSync)
            {
                filesToCheckAnalysisResult = _automaticAnalyseInProgressByHash.Values.ToList();
            }
            foreach (var context in filesToCheckAnalysisResult)
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                    return;
                CheckAnalysisResultResponse automaticAnalysisResponse = null;
                try
                {
                    automaticAnalysisResponse =
                        _provider.CheckAnalysisResult(new CheckAnalysisResultRequest(analysisId: context.AnalysisId,
                                        apiKey: _apiKey));
                }
                catch (Exception)
                {
                    
                }

                if (automaticAnalysisResponse != null && (automaticAnalysisResponse.ReturnCode == ErrorCodesEnum.Success
                    || automaticAnalysisResponse.ReturnCode == ErrorCodesEnum.AnalysisNotFound))
                    MergeAutomaticAnalysisResult(automaticAnalysisResponse, context);
            }
        }

        private void MergeResult(ValkyrieAnalysisResult result, ValkyrieFileInfo fileInfo)
        {
            if (result.IsSubmitted || (result.Verdict != ValkyrieVerdict.FirstSeen &&
                                       result.Verdict != ValkyrieVerdict.Pending))
            {
                if (result.Verdict != ValkyrieVerdict.Undetected)
                    lock (_lookupSync)
                    {
                        _lookupContexts.Remove(fileInfo);
                    }
                if (_cancellationTokenSource.IsCancellationRequested)
                    return;
                if (result.Verdict != fileInfo.PreviousValkyrieVerdict)
                {
                    fileInfo.PreviousValkyrieVerdict = result.Verdict;
                    _pendingItemsResolved.TryAdd(result.SHA1, result);
                }
            }
        }

        private void AddLookupContext(ValkyrieFileInfo fileInfo, ValkyrieVerdict previousVerdict)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                lock (_lookupSync)
                {
                    fileInfo.PreviousValkyrieVerdict = ValkyrieVerdict.NotAnalyzed;
                    _lookupContexts.Add(fileInfo);
                }
        }

        private void MergeAutomaticAnalysisResult(CheckAnalysisResultResponse result, ValkyrieAnalyseOperationContext fileInfo)
        {
            if (result.ReturnCode == ErrorCodesEnum.AnalysisNotFound)
            {
                AddLookupContext(fileInfo.ValkyrieFileInfo, ValkyrieVerdict.NotAnalyzed);
                lock (_automaticAnalyseSync)
                {
                    _automaticAnalyseInProgressByHash.Remove(fileInfo.HashSha1);
                }
            }
            else if (result.ReturnCode == ErrorCodesEnum.Success)
            {
                if (result.Verdict.ToBusiness() == ValkyrieVerdict.Undetected)
                    AddLookupContext(fileInfo.ValkyrieFileInfo, result.Verdict.ToBusiness());
                lock (_automaticAnalyseSync)
                {
                    _automaticAnalyseInProgressByHash.Remove(fileInfo.HashSha1);
                }
                if (!_cancellationTokenSource.IsCancellationRequested)
                    _pendingItemsResolved.TryAdd(result.SHA1, ValkyrieAnalysisResult.Create(result));
            }
        }

       
        private void SubmitToAutomaticAnalysis(List<ValkyrieFileInfo> badFiles, 
            CancellationToken cancellationToken, Action< ValkyrieAnalysisResult,ValkyrieFileInfo> addAction,  bool parallels = true)
        {
            var endpointId = _endpointId;
            ParallelOptions opt = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            if (parallels)
                Parallel.ForEach(badFiles, opt, f =>
                {
                    GetValkyrieVerdictForOneFile(f, endpointId, addAction);
                });
            else
                foreach (var f in badFiles)
                {
                    GetValkyrieVerdictForOneFile(f, endpointId, addAction);
                }
        }

        private void GetValkyrieVerdictForOneFile(ValkyrieFileInfo f, string endpointId, Action<ValkyrieAnalysisResult, ValkyrieFileInfo> addAction)
        {
            try
            {
                var result = UploadFile(f, endpointId);
                if (result == null) throw new ArgumentException("ValkyrieAnalysisResult can not be null");
                addAction(result, f);
            }
            catch (Exception e)
            {
                ValkyrieAnalysisResult result = null;
                if (e is WebException && e.Message == "The request was aborted: The request was canceled.")
                {
                    result = ValkyrieAnalysisResult.CreateSkipInfo(f.Sha1, "The request was canceled by user",
                        SkippReasonEnum.Cancelled);
                }
                else
                {
                    SkippReasonEnum s = SkippReasonEnum.Failed;
                    string m = e.Message;
                    if (e.Message.StartsWith("Could not find file"))
                    {
                        m = "Could not find file";
                        s = SkippReasonEnum.FileNotFound;
                    }
                    else if (e is WebException)
                        s = SkippReasonEnum.WebException;
                    result = ValkyrieAnalysisResult.CreateSkipInfo(f.Sha1, m, s);
                }
                addAction(result, f);

                Trace.WriteLine(
                    $"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : Submit Failed SHA1 = {f.Sha1} Reason : {e.Message}" +
                    $" {e.InnerException?.Message ?? string.Empty}");
            }
        }

        public void AnalyzeAsyncCancel()
        {
            _cancellationTokenSource?.Cancel();
            _pendingItemsResolved.Clear();
            _automaticAnalyseInProgressByHash.Clear();
            _cancellationTokenSource?.Cancel();
        }


        private void OnAnalyzeCompleted()
        {
            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                Task.Factory.StartNew(() =>
                {
                    tmp(this, new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(_analysisResults.ToArray(), null,
                            true,null));
                }, CancellationToken.None);
            }
        }

        private void OnAnalyzeCompleted(Task task)
        {
            Exception exception = null;
            if (task.Exception != null)
            {
                var aggregateException = task.Exception;
                if (aggregateException.InnerExceptions.Count == 1)
                {
                    exception = aggregateException.InnerExceptions.First();
                    Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : {exception.Message}");
                }
                else
                {
                    var messages = aggregateException.InnerExceptions.Select(e => e.Message).ToArray();
                    var exMessage = String.Join(Environment.NewLine, messages);
                    Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : {exMessage}");
                    exception = aggregateException;
                }
            }
            else if (_aptScanId != null)
                Task.Factory.StartNew(t =>
                    { 
                        _provider.EndScan(new EndScanRequest(_apiKey, _machineCount, _aptScanId)); 
                    }, TaskCreationOptions.None);

            Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : Scan Completed for API_KEY = {_apiKey}");
            AnalyzeCompleted?.Invoke(this,
                new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(_analysisResults.ToArray(), exception,
                    task.IsCanceled, task.AsyncState));
            bool needStartChecking = false;
            lock (_lookupSync)
            {
                bool anyPending = _lookupContexts.Any() || _automaticAnalyseInProgressByHash.Any();
                if (_checkingTimerIsStop && anyPending)
                {
                    _checkingTimerIsStop = false;
                    needStartChecking = true;
                }
                _isMainScanFinished = true;
            }
            if (needStartChecking)
                _checkStatusesTimer.Change(_changeTrakingIntervalInMs, Timeout.Infinite);

        }

        private void OnAnalyzeProgressChanged(ConcurrentDictionary<string, ValkyrieAnalysisResult> items, bool isUpdate = false)
        {
            if (items.Count == 0) return;

            var tmp = AnalyzeProgressChanged;
            if (tmp != null)
            {
                double progress = 0;

                if (_filesCount > 0)
                {
                    progress = (_fileScanned/(double) _filesCount)*100;

                }
                var progressInt = Convert.ToInt32(progress);

                var newResults = items.Values.ToArray();

                foreach (var item in newResults)
                {
                    ValkyrieAnalysisResult result;
                    items.TryRemove(item.SHA1, out result);
                }
                if (_cancellationTokenSource.IsCancellationRequested)
                    return;
                tmp(this,
                    new ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult>(progressInt,
                        new ProgressInfo<ValkyrieAnalysisResult>(_filesCount, _fileScanned, newResults), isUpdate));

            }
        }

        private void OnAnalyzeStarted()
        {
            AnalyzeStarted?.Invoke(this, EventArgs.Empty);
        }
              

        //private ValkyrieAnalysisResult UploadFileOldStyle(ValkyrieFileInfo fileInfo, Guid endpointId)
        //{
        //    ValkyrieAnalysisResult result = null;

        //    try
        //    {
        //        var sha1 = fileInfo.Sha1;
        //        var originalFilePath = fileInfo.GetFullPath();
        //        if (_cancellationTokenSource.IsCancellationRequested)
        //        {
        //            return CancelOperation(originalFilePath, sha1);
        //        }

        //        if (fileInfo.Size > MaxFileLenght)
        //        {
        //            result = ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Maximum file size per upload to 25 MB.",
        //                SkippReasonEnum.Skipped);
        //            Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : File {originalFilePath} " +
        //                            $"greater than 25 Mb");
        //            Trace.Flush();
        //            return result;
        //        }

        //        var infoRequeast = new InfoRequest(sha1, endpointId, null,  _aptScanId, _apiKey, fileInfo.Paths.ToArray());
        //        BasicInfoResponse basicInfo = ValkyrieWebApiProvider.GetBasicInfo(infoRequeast);
        //        if (basicInfo.ReturnCode != ErrorCodesEnum.Success)
        //            throw new InvalidOperationException(basicInfo.ReturnCode.GetEnumDescription());

        //        Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff}: SHA1 {sha1} FilePath " +
        //                        $"{originalFilePath} Presence = {basicInfo.Presence}");
        //        Trace.Flush();

        //        if (basicInfo.Presence)
        //        {
        //            result = ValkyrieAnalysisResult.Create(basicInfo);
        //        }
        //        else
        //        {
        //            Trace.WriteLine(
        //                $"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff}: SHA1 {sha1} FilePath {originalFilePath} Presence " +
        //                $"= {basicInfo.Presence}, [File Uploaded]");
                
        //            var request = new SubmitToAutomaticRequest
        //            {
        //                ApiKey = _apiKey,
        //                SubmitToken = basicInfo.SubmitToken,
        //                FilePath = WebUtility.HtmlDecode(originalFilePath),
        //                EndpointId = endpointId,
        //                AptScanId = _aptScanId
        //            };

        //            //var data = ReadFile(fileInfo, login, password);
        //            var fStream = File.OpenRead(fileInfo.GetNetworkPath());
        //            var response = ValkyrieWebApiProvider.SubmitToAutomaticAnalysis(request, fStream, _cancellationTokenSource.Token);
                    
        //            CheckAnalysisResultResponse automaticAnalysisResponse = new CheckAnalysisResultResponse()
        //            {
        //                ReturnCode = ErrorCodesEnum.None
        //            };

        //            var timeOutInMinutes = 5;
        //            var timeOut = (int)TimeSpan.FromMinutes(timeOutInMinutes).TotalMilliseconds;
        //            bool success = false;
        //            bool analyzeNotFinished = true;
        //            double elapsed = 0;
        //            DateTime utcStarted = DateTime.UtcNow;
        //            while (analyzeNotFinished && (elapsed < timeOut))
        //            {
        //                if (_cancellationTokenSource.IsCancellationRequested)
        //                {
        //                    return CancelOperation(originalFilePath, sha1);
        //                }

        //                Thread.Sleep(1000);
        //                elapsed = (int)(DateTime.UtcNow - utcStarted).TotalMilliseconds;
        //                automaticAnalysisResponse =
        //                    ValkyrieWebApiProvider.CheckAnalysisResult(new CheckAnalysisResultRequest(analysisId: response.AnalysisId, 
        //                    apiKey: _apiKey));
        //                analyzeNotFinished = automaticAnalysisResponse.ReturnCode == ErrorCodesEnum.AnalysisNotFinished;

        //            }
        //            success = automaticAnalysisResponse.ReturnCode == ErrorCodesEnum.Success;
        //            if (analyzeNotFinished)
        //            {
        //                throw new TimeoutException("Automatic analysis has not been completed after " +
        //                                           timeOutInMinutes + " minutes");
        //            }
        //            if (!success)
        //            {
        //                throw new InvalidOperationException(automaticAnalysisResponse.ReturnCode.GetEnumDescription());
        //            }
        //            result = ValkyrieAnalysisResult.Create(automaticAnalysisResponse);

        //        }
        //    }
        //    finally 
        //    { 

        //    }
            
        //    return result;
        //}

        private ValkyrieAnalysisResult UploadFile(ValkyrieFileInfo fileInfo, string endpointId)
        {
            ValkyrieAnalysisResult result = null;

            try
            {
                var sha1 = fileInfo.Sha1;
                var originalFilePath = fileInfo.GetFullPath();
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    return CancelOperation(originalFilePath, sha1);
                }
                                
                var infoRequeast = new InfoRequest(sha1: sha1, apiKey: _apiKey, endpointId: endpointId, aptScanId: _aptScanId, path: fileInfo.Path);
              BasicInfoResponse basicInfo = _provider.GetBasicInfo(infoRequeast);
                if (basicInfo.ReturnCode != ErrorCodesEnum.Success)
                    throw new InvalidOperationException(basicInfo.ReturnCode.GetEnumDescription());
                Trace.WriteLine(
                    $"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff}: SHA1 {sha1} FilePath {originalFilePath} " +
                    $"FlsVerdict {fileInfo.FlsCloud} Upload {basicInfo.Upload} Presence = {basicInfo.Presence}");
                Trace.Flush();
                if (basicInfo.Upload)
                {
                    if (fileInfo.Size > MaxFileLenght)
                    {
                        result = ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Maximum file size per upload to 25 MB.",
                            SkippReasonEnum.Skipped);
                        Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : File {originalFilePath} greater than 25 Mb");
                        Trace.Flush();
                        return result;
                    }
                }
                if (!basicInfo.Upload || fileInfo.FlsCloud != CloudVerdict.FirstSeen)
                {
                    result = ValkyrieAnalysisResult.Create(basicInfo);
                }
                else
                {
                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        return CancelOperation(originalFilePath, sha1);
                    }
                    lock (_automaticAnalyseSync)
                    {
                        ValkyrieAnalyseOperationContext context;
                        _automaticAnalyseInProgressByHash.TryGetValue(fileInfo.Sha1, out context);
                        if (context != null)
                        {
                            return ValkyrieAnalysisResult.Create(basicInfo, context.AnalysisId, true);
                        }
                    }

                    var orgFilePath = WebUtility.HtmlDecode(originalFilePath); //???

                    var request = new SubmitToAutomaticRequest
                    {
                        ApiKey = _apiKey,
                        SubmitToken = basicInfo.SubmitToken,                        
                        EndpointId = endpointId,
                        AptScanId = _aptScanId,
                        FilePath = WebUtility.HtmlDecode(originalFilePath)
                    };

                    SubmitToAutomaticResponse response;
                    ValkyrieResponse fatSubmitFilesResponse;
                    using (fileInfo.IsLocalComputer || _login == null ? null: 
                        new ImpersonationHelper(_login, "", _password, ImpersonationHelper.Logon32LogonType.NewCredentials))
                    {
                        //var data = ReadFile(fileInfo, login, password);
                        var remoteFilePath = fileInfo.GetNetworkPath();
                        Trace.WriteLine( $"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff}: reading remote file {remoteFilePath}");
                        var fileName = Path.GetFileName(orgFilePath);
                        using (var fStream = File.OpenRead(remoteFilePath))
                        {
                            if (_cancellationTokenSource.IsCancellationRequested)
                            {
                                return CancelOperation(originalFilePath, sha1);
                            }
                            response = _provider.SubmitToAutomaticAnalysis(request, fStream, _cancellationTokenSource.Token, fileName);
                            fatSubmitFilesResponse = _provider.FatSubmitFiles(new FatSubmitFilePathsRequest()
                            {
                                Sha1 = sha1,
                                ApiKey = _apiKey,
                                FilePaths = fileInfo.Paths.ToArray(),
                                ScanId = _aptScanId

                            });
                        }
                    }
                    if (response.ReturnCode != ErrorCodesEnum.Success)
                        throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());
                    if (fatSubmitFilesResponse.ReturnCode != ErrorCodesEnum.Success)
                        throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());

                    Trace.WriteLine(
                        $"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff}: SHA1: {sha1} FilePath: {originalFilePath} " +
                        $"Presence = {basicInfo.Presence}, [File Uploaded]");
                    lock (_automaticAnalyseSync)
                    {
                        _automaticAnalyseInProgressByHash[fileInfo.Sha1] = new ValkyrieAnalyseOperationContext
                        {
                            AnalysisId = response.AnalysisId,
                            HashSha1 = fileInfo.Sha1,
                            ValkyrieFileInfo = fileInfo
                        };
                    }

                    result = ValkyrieAnalysisResult.Create(basicInfo, response.AnalysisId, true);

                }
            }
            catch (Exception ex)
            {
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network,$"{ex.GetType()} Exception was accured during uploading file {fileInfo.Sha1}");
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network,ex.ToString());
                throw;
            }
            finally
            {

            }

            return result;
        }

        private static ValkyrieAnalysisResult CancelOperation(string originalFilePath,  string sha1)
        {
            var result = ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Cancelled by user.", SkippReasonEnum.Cancelled);
            Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : File {originalFilePath} cancelled by user");            
            Trace.Flush();
            return result;
        }

        private void AddResult(ValkyrieAnalysisResult result, ValkyrieFileInfo info)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;
            _analysisResults.Add(result);

            Interlocked.Increment(ref _fileScanned);
            _analysedItems.TryAdd(result.SHA1, result);

            if ((result.Verdict == ValkyrieVerdict.Undetected 
                || result.Verdict == ValkyrieVerdict.Pending
                || result.Verdict == ValkyrieVerdict.FirstSeen)
                && !result.IsSubmitted)
            {
                lock (_lookupSync)
                {
                    info.PreviousValkyrieVerdict = result.Verdict;
                    _lookupContexts.Add(info);
                }
            }

            //Task.Factory.StartNew(OnAnalyzeProgressChanged, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public string GetValkyrieDetailsLink()
        {
            return $@"{_provider.GetBaseUri()}signup_apikey/{_apiKey}";
        }

        public void StartAnalyze(CamRegistration camRegistration, List<ValkyrieFileInfo> filesToAnalyze,
            string login, string password, int machineCount, NetworkCredential proxyCredential, string productName)
        {
            _analysedItems = new ConcurrentDictionary<string, ValkyrieAnalysisResult>();
            _machineCount = machineCount;
            _lookupContexts.Clear();
            _fileScanned = 0;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            cancellationToken.Register(OnAnalyzeCompleted);

            Task.Factory.StartNew(t =>
            {
                _isMainScanFinished = false;
                var regInfo = Register(camRegistration, productName, proxyCredential);
                if (regInfo.ReturnCode != 0)
                {
                    var ex = new ValkyrieRegistrationFailedException(regInfo);
                    throw ex;
                }
                _analysisResults = new ConcurrentBag<ValkyrieAnalysisResult>();
                _pendingItemsResolved.Clear();
                _automaticAnalyseInProgressByHash.Clear();
                _login = login;
                _password = password;
                OnAnalyzeStarted();
                if (!filesToAnalyze.Any()) return;
                
                StartAndFinalizeScanResponse startScanResult;
                lock (_aptScanIdLock)
                {
                    startScanResult = _provider.StartScan(new StartScanRequest(regInfo.Key));
                    _aptScanId = startScanResult.AptScanId;
                }

                if (startScanResult.ReturnCode != 0)
                {
                    Trace.WriteLine("apt_start_scan failed. " + startScanResult.ResultDescription);
                    throw new InvalidOperationException(startScanResult.ResultDescription);
                }
                Thread.Sleep(2000);
                _progressNotifyTimer.Change(1000, Timeout.Infinite);
                _filesCount = filesToAnalyze.Count;
                SubmitToAutomaticAnalysis(filesToAnalyze, _cancellationTokenSource.Token, AddResult);
            }, TaskCreationOptions.None, cancellationToken)
                .ContinueWith(OnAnalyzeCompleted, TaskContinuationOptions.NotOnCanceled);
        }

        public void Dispose()
        {
            _progressNotifyTimer?.Dispose();
            _progressNotifyTimer = null;
            _checkStatusesTimer?.Dispose();
            _checkStatusesTimer = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        public void SetBaseUri(Uri uri)
        {
            _provider.SetBaseUri(uri);
        }
    }
}
