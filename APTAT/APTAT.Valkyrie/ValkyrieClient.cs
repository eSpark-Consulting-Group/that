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
        private const Int32 MaxFileLenght = 26214400;
        private string _apiKey;
        private int _fileScanned;
        private int _filesCount;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly string _endpointId;
        private ConcurrentBag<ValkyrieAnalysisResult> _analysisResults;
        private ConcurrentDictionary<string, ValkyrieAnalysisResult> _analysedItems
            = new ConcurrentDictionary<string, ValkyrieAnalysisResult>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, ValkyrieAnalysisResult> _pendingItemsResolved
            = new ConcurrentDictionary<string, ValkyrieAnalysisResult>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, ValkyrieAnalyseOperationContext> _automaticAnalyseInProgressByHash
            = new Dictionary<string, ValkyrieAnalyseOperationContext>(StringComparer.OrdinalIgnoreCase); 
        private readonly Object _automaticAnalyseSync = new object();
        private string _aptScanId;
        private readonly Object _aptScanIdLock = new object();
        private readonly HashSet<ValkyrieFileInfo> _lookupContexts = new HashSet<ValkyrieFileInfo>();

        private readonly Object _lookupSync = new object();
        private bool _isMainScanFinished = true;

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
            _provider = provider;
            _endpointId = machineKeyProvider.GlobalMachineKey;
            _analysisResults = new ConcurrentBag<ValkyrieAnalysisResult>();
            _progressNotifyTimer = new Timer(ProgressNotifyTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _checkStatusesTimer = new Timer(CheckStatusesTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void AddFilesToAnalyze(List<ValkyrieFileInfo> filesToAnalyze, bool shouldStartImmediately)
        {
            if (!filesToAnalyze.Any()) return;
            lock (_lookupSync)
            {
                foreach (var fileInfo in filesToAnalyze)
                    _lookupContexts.Add(fileInfo);
            }
            if (shouldStartImmediately)
                _checkStatusesTimer?.Change(0, Timeout.Infinite);
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
            if (_aptScanId == null)
            {
                lock (_aptScanIdLock)
                {
                    var attemptCount = 0;
                    while (true)
                    {
                        if (_aptScanId != null)
                            break;
                        if (_apiKey == null)
                            waitNextInterval = true;
                        if (attemptCount == 5) break;
                        var startScanResult = _provider.StartScan(new StartScanRequest(_apiKey));
                        if (startScanResult.ReturnCode == ErrorCodesEnum.Success)
                        {
                            _aptScanId = startScanResult.AptScanId;
                            waitNextInterval = false;
                        }
                        attemptCount++;
                    }
                }
            }
            if (!waitNextInterval)
            {
                CheckNewStatusesOnValkyrie();
                OnAnalyzeProgressChanged(_pendingItemsResolved, true);
            }
            var resetTimerImmediately = false;
            if (_cancellationTokenSource.IsCancellationRequested)
            {
                lock (_lookupContexts)
                {
                    if (_lookupContexts.Any())
                        resetTimerImmediately = true;
                }
                lock (_automaticAnalyseSync)
                {
                    if (_automaticAnalyseInProgressByHash.Any())
                        resetTimerImmediately = true;
                }
            }
            _checkStatusesTimer?.Change(resetTimerImmediately? 100 : _changeTrakingIntervalInMs, Timeout.Infinite);
        }

        private void CheckNewStatusesOnValkyrie()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            List<ValkyrieFileInfo> filesToAnalyze;
            lock (_lookupSync)
            {
                filesToAnalyze = _lookupContexts.ToList();
                _lookupContexts.Clear();
            }
            SubmitToAutomaticAnalysis(filesToAnalyze, cancellationToken, MergeResult, false);
            List<ValkyrieAnalyseOperationContext> filesToCheckAnalysisResult;
            lock (_automaticAnalyseSync)
            {
                filesToCheckAnalysisResult = _automaticAnalyseInProgressByHash.Values.ToList();
                _automaticAnalyseInProgressByHash.Clear();
            }
            foreach (var context in filesToCheckAnalysisResult)
            {
                CheckAnalysisResultResponse automaticAnalysisResponse = null;
                Exception ex = null;
                try
                {
                    automaticAnalysisResponse =
                        _provider.CheckAnalysisResult(new CheckAnalysisResultRequest(analysisId: context.AnalysisId,
                                        apiKey: _apiKey));
                }
                catch (Exception)
                {
                    Trace.WriteLine(
                        $"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : CheckAnalysisResult failed = {context.HashSha1} Reason : {ex.Message}" +
                        $" {ex.InnerException?.Message ?? string.Empty}");
                }
                MergeAutomaticAnalysisResult(automaticAnalysisResponse, context, cancellationToken);
            }
        }

        private void MergeResult(ValkyrieAnalysisResult result, ValkyrieFileInfo fileInfo, CancellationToken cancellationToken)
        {
            if (!result.IsSubmitted && (result.Verdict == ValkyrieVerdict.FirstSeen || 
                    result.Verdict == ValkyrieVerdict.Undetected || result.Verdict == ValkyrieVerdict.Pending))
                lock (_lookupSync)
                {
                    _lookupContexts.Add(fileInfo);
                }
            if (result.Verdict != fileInfo.PreviousValkyrieVerdict)
            {
                fileInfo.PreviousValkyrieVerdict = result.Verdict;
                _pendingItemsResolved.TryAdd(result.SHA1, result);
            }
        }

        private void AddLookupContext(ValkyrieFileInfo fileInfo, ValkyrieVerdict previousVerdict, CancellationToken cancellationToken)
        {
            if (!cancellationToken.IsCancellationRequested)
                lock (_lookupSync)
                {
                    fileInfo.PreviousValkyrieVerdict = previousVerdict;
                    _lookupContexts.Add(fileInfo);
                }
        }

        private void MergeAutomaticAnalysisResult(CheckAnalysisResultResponse result, ValkyrieAnalyseOperationContext fileInfo,
            CancellationToken cancellationToken)
        {
            if (result != null && result.ReturnCode == ErrorCodesEnum.AnalysisNotFound)
            {
                AddLookupContext(fileInfo.ValkyrieFileInfo, ValkyrieVerdict.NotAnalyzed, cancellationToken);
            }
            else if (result != null && result.ReturnCode == ErrorCodesEnum.Success)
            {
                if (result.Verdict.ToBusiness() == ValkyrieVerdict.Undetected)
                    AddLookupContext(fileInfo.ValkyrieFileInfo, result.Verdict.ToBusiness(), cancellationToken);
                _pendingItemsResolved.TryAdd(result.SHA1, ValkyrieAnalysisResult.Create(result));
            }
            else
            {
                lock (_automaticAnalyseSync)
                {
                    _automaticAnalyseInProgressByHash.Add(fileInfo.HashSha1, fileInfo);
                }
            }
        }

       
        private void SubmitToAutomaticAnalysis(List<ValkyrieFileInfo> badFiles, 
            CancellationToken cancellationToken, Action< ValkyrieAnalysisResult,ValkyrieFileInfo, CancellationToken> addAction,  bool parallels = true)
        {
            var endpointId = _endpointId;
            ParallelOptions opt = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            if (parallels)
                Parallel.ForEach(badFiles, opt, f =>
                {
                    GetValkyrieVerdictForOneFile(f, endpointId, addAction, cancellationToken);
                });
            else
                foreach (var f in badFiles)
                {
                    GetValkyrieVerdictForOneFile(f, endpointId, addAction, cancellationToken);
                }
        }

        private void GetValkyrieVerdictForOneFile(ValkyrieFileInfo f, string endpointId, Action<ValkyrieAnalysisResult, ValkyrieFileInfo, CancellationToken> addAction
            , CancellationToken cancellationToken)
        {
            try
            {
                var result = UploadFile(f, endpointId, cancellationToken);
                if (result == null) throw new ArgumentException("ValkyrieAnalysisResult can not be null");
                addAction(result, f, cancellationToken);
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
                addAction(result, f, cancellationToken);

                Trace.WriteLine(
                    $"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : Submit Failed SHA1 = {f.Sha1} Reason : {e.Message}" +
                    $" {e.InnerException?.Message ?? string.Empty}");
            }
        }

        public void AnalyzeAsyncCancel()
        {
            if (_cancellationTokenSource != null)
            {
                SendEndScanAsync();
                _cancellationTokenSource.Cancel();
                _checkStatusesTimer?.Change(0, Timeout.Infinite);
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

            Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : Scan Completed for API_KEY = {_apiKey}");
            _isMainScanFinished = true;
            AnalyzeCompleted?.Invoke(this,
                new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(_analysisResults.ToArray(), exception,
                    task.IsCanceled, task.AsyncState));
            _checkStatusesTimer?.Change(_changeTrakingIntervalInMs, Timeout.Infinite);

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
                tmp(this,
                    new ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult>(progressInt,
                         new ProgressInfo<ValkyrieAnalysisResult>(_filesCount, _fileScanned, newResults), isUpdate));

            }
        }

        private void OnAnalyzeStarted()
        {
            AnalyzeStarted?.Invoke(this, EventArgs.Empty);
        }

        private ValkyrieAnalysisResult UploadFile(ValkyrieFileInfo fileInfo, string endpointId, CancellationToken cancellationToken)
        {
            ValkyrieAnalysisResult result = null;

            try
            {
                var sha1 = fileInfo.Sha1;
                var originalFilePath = fileInfo.GetFullPath();
                if (cancellationToken.IsCancellationRequested)
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
                    if (cancellationToken.IsCancellationRequested)
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
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return CancelOperation(originalFilePath, sha1);
                            }
                            response = _provider.SubmitToAutomaticAnalysis(request, fStream, cancellationToken, fileName);

                            FilePath[] paths = new FilePath[fileInfo.Paths.Count];
                            for (int i = 0; i < fileInfo.Paths.Count; i++)
                            {
                                var uri = new Uri(fileInfo.Paths[i]);
                                paths[i] = new FilePath()
                                {
                                    MachineName = uri.Host,
                                    Path = uri.LocalPath.Substring(uri.Host.Length + 3)
                                };
                            }

                            fatSubmitFilesResponse = _provider.FatSubmitFiles(new FatSubmitFilePathsRequest()
                            {
                                Sha1 = sha1,
                                ApiKey = _apiKey,
                                FilePaths = paths, 
                                ScanId = _aptScanId

                            });
                        }
                    }
                    if (response.ReturnCode != ErrorCodesEnum.Success)
                        throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());
                    if (fatSubmitFilesResponse.ReturnCode != ErrorCodesEnum.Success)
                        throw new InvalidOperationException(fatSubmitFilesResponse.ReturnCode.GetEnumDescription());

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

        private void AddResult(ValkyrieAnalysisResult result, ValkyrieFileInfo info, CancellationToken cancellationToken)
        {
            _analysisResults.Add(result);
            Interlocked.Increment(ref _fileScanned);
            _analysedItems.TryAdd(result.SHA1, result);

            if ((result.Verdict == ValkyrieVerdict.Undetected 
                || result.Verdict == ValkyrieVerdict.Pending
                || result.Verdict == ValkyrieVerdict.FirstSeen)
                && !result.IsSubmitted)
            {
                AddLookupContext(info, result.Verdict, cancellationToken);
            }

            //Task.Factory.StartNew(OnAnalyzeProgressChanged, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public string GetValkyrieDetailsLink()
        {
            return $@"{_provider.GetBaseUri()}signup_apikey/{_apiKey}";
        }

        public void SendEndScanAsync()
        {
            if (_aptScanId != null)
                Task.Factory.StartNew(t =>
                {
                    _provider.EndScanWithErrorHandling(new EndScanRequest(_apiKey, _machineCount, _aptScanId));
                }, TaskCreationOptions.None);
        }

        public async Task SendEndScan()
        {
            await Task.Factory.StartNew(t =>
            {
                if (_aptScanId != null)
                {
                    _provider.EndScan(new EndScanRequest(_apiKey, _machineCount, _aptScanId));
                }
            }, TaskCreationOptions.None);
        }

        public void StartAnalyze(CamRegistration camRegistration, List<ValkyrieFileInfo> filesToAnalyze,
            string login, string password, int machineCount, NetworkCredential proxyCredential, string productName)
        {
            _analysedItems = new ConcurrentDictionary<string, ValkyrieAnalysisResult>();
            _machineCount = machineCount;
            _lookupContexts.Clear();
            _fileScanned = 0;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            _filesCount = filesToAnalyze.Count;

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
                
                StartAndFinalizeScanResponse startScanResult = null;
                lock (_aptScanIdLock)
                {
                    if (_aptScanId == null)
                    {
                        startScanResult = _provider.StartScan(new StartScanRequest(regInfo.Key));
                        _aptScanId = startScanResult.AptScanId;
                    }
                }

                if (startScanResult != null && startScanResult.ReturnCode != 0)
                {
                    Trace.WriteLine("apt_start_scan failed. " + startScanResult.ResultDescription);
                    throw new InvalidOperationException(startScanResult.ResultDescription);
                }
                if (!filesToAnalyze.Any()) return;
                
                if (startScanResult != null)
                    Thread.Sleep(2000);
                _progressNotifyTimer.Change(1000, Timeout.Infinite);
                SubmitToAutomaticAnalysis(filesToAnalyze, cancellationToken, AddResult);
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

        public ValkyrieResponse SubmitEmail(string[] addresses)
        {
            return _provider.SubmitEmail(new SubscribeEmailRequest { ApiKey = _apiKey, ScanId = _aptScanId, CustomerEmails = addresses });
        }
    }
}
