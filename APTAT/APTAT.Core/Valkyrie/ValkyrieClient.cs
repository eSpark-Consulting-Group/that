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

using DTO = Valkyrie.WebApiProvider.Enums;
using APTAT.Core.Descent.Copy;
using APTAT.Core.DescentCopy;
using APTAT.DeploymentCore.Descent.Helpers;

namespace APTAT.Core.Valkyrie
{
    public sealed class ValkyrieClient : IValkyrieClient
    {
        private readonly int _changeTrakingIntervalInMs = (int)TimeSpan.FromMinutes(3).TotalMilliseconds;
        private readonly int _delayAfterStartScanInSeconds = TimeSpan.FromSeconds(2).Seconds;
        private const Int32 MaxFileLenght = 26214400;
        private string _apiKey;
        private int _fileScanned;
        private int _filesCount;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private readonly string _endpointId;
        private string[] _emails = null;

        private readonly ConcurrentDictionary<string, IValkyrieAnalysisResult> _pendingItemsResolved
            = new ConcurrentDictionary<string, IValkyrieAnalysisResult>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, ValkyrieAnalyseOperationContext> _automaticAnalyseInProgressByHash
            = new Dictionary<string, ValkyrieAnalyseOperationContext>(StringComparer.OrdinalIgnoreCase);
        private readonly Object _automaticAnalyseSync = new object();
        private string _aptScanId;
        private bool _aptScanEnded;
        private readonly Object _aptScanIdLock = new object();
        private readonly HashSet<IValkyrieFileInfo> _lookupContexts = new HashSet<IValkyrieFileInfo>();
        private readonly ConcurrentDictionary<string, IValkyrieFileInfo> _remoteCopyQueue 
            = new ConcurrentDictionary<string, IValkyrieFileInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Object _lookupSync = new object();
        private bool _isMainScanFinished = true;

        private readonly IScheduler _progressNotifyTimer;
        private readonly IScheduler _checkStatusesTimer;
        private readonly ICopyHelper _copyHelper;
        private readonly IDescentFactory _impersonationHelperFactory;
        private readonly IScheduler _delayTimer;
        private int _machineCount;
        private string _login;
        private string _password;
        private bool _isCheckingInProgress = false;

        bool _submitAllowed;

        public event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<IValkyrieAnalysisResult>> AnalyzeProgressChanged;
        public event EventHandler<AnalyzeCompletedEventArgs> AnalyzeCompleted;
        public event EventHandler<ValkyrieAnalyzeStartedEventArgs> AnalyzeStarted;

        private readonly IValkyrieWebApiProvider _provider;
        private Dictionary<string, IValkyrieFileInfo> _filesToSubmit;

        public ValkyrieClient(IMachineKeyProvider machineKeyProvider, IValkyrieWebApiProvider provider, IScheduler progressNotifyTimer, 
            IScheduler checkStatusTimer, IScheduler delayTimer, ICopyHelper copyHelper, IDescentFactory impersonationHelperFactory)
        {
            _provider = provider;
            _impersonationHelperFactory = impersonationHelperFactory;
            _endpointId = machineKeyProvider.GlobalMachineKey;
            _copyHelper = copyHelper;
            _progressNotifyTimer = progressNotifyTimer;
            _checkStatusesTimer = checkStatusTimer;
            _delayTimer = delayTimer;
            _progressNotifyTimer.SetInterval(TimeSpan.FromSeconds(1));
            _checkStatusesTimer.SetInterval(TimeSpan.FromMilliseconds(_changeTrakingIntervalInMs));
            _progressNotifyTimer.TimeoutCallback += ProgressNotifyTimerCallback;
            _checkStatusesTimer.TimeoutCallback += CheckStatusesTimerCallback;
            _delayTimer.SetInterval(TimeSpan.FromSeconds(_delayAfterStartScanInSeconds));
            _copyHelper.CopyCompleted += _copyHelper_CopyCompleted;
        }

        private void _copyHelper_CopyCompleted(object sender, CopyDescentCompletedEventArgs e)
        {
            var cancellationToken = _cancellationToken;
            if (cancellationToken.IsCancellationRequested) return;
            var valkyrieFiles = e.Results.OfType<IValkyrieFileInfo>().ToList();
            if (valkyrieFiles.Any())
                AddFilesToAnalyze(valkyrieFiles, true);
        }

        public void AllowSubmit(Dictionary<string, IValkyrieFileInfo> updatedData)
        {
            _filesToSubmit = updatedData;
            _submitAllowed = true;
            Task.Factory.StartNew(() => CheckStatusesTimerCallback(this, null));
        }
        public void AddFilesToAnalyze(List<IValkyrieFileInfo> filesToAnalyze, bool shouldStartImmediately)
        {
            if (!filesToAnalyze.Any()) return;
            lock (_lookupSync)
            {
                foreach (var fileInfo in filesToAnalyze)
                    _lookupContexts.Add(fileInfo);
            }
            if (shouldStartImmediately)
            {
                Task.Factory.StartNew(() => CheckStatusesTimerCallback(this, null));
            }
        }

        private RegisterWithResponse Register(ICamRegistration camRegistration, string productName,
            NetworkCredential networkCredential = null)
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

        private void ProgressNotifyTimerCallback(object state, EventArgs elapsedEventArgs)
        {
            if (_isMainScanFinished)
            {
                OnAnalyzeProgressChanged(_pendingItemsResolved);
            }
            _progressNotifyTimer.Start(false);
        }


        private void CheckStatusesTimerCallback(object sender, EventArgs elapsedEventArgs)
        {
            lock (_checkStatusesTimer)
            {
                if (_checkStatusesTimer.IsEnabled)
                    _checkStatusesTimer.Stop();
                if (_isCheckingInProgress)
                {
                    if (sender == this)
                        _checkStatusesTimer.SetInterval(TimeSpan.FromMilliseconds(50));
                    return;
                }
                _isCheckingInProgress = true;
                _checkStatusesTimer.SetInterval(TimeSpan.FromMilliseconds(_changeTrakingIntervalInMs));
            }
            var waitNextInterval = false;
            if (!_isMainScanFinished)
                waitNextInterval = true;
            else if (_aptScanId == null )
            {
                lock (_aptScanIdLock)
                {
                    if (_aptScanId == null)
                        waitNextInterval = true;
                }
            }
            CancellationToken? cancellationToken = null;
            if (_cancellationTokenSource == null)
                waitNextInterval = true;
            else
                cancellationToken = _cancellationToken;
            if (!waitNextInterval)
            {
                CheckNewStatusesOnValkyrie(cancellationToken.Value);
            }
            var resetTimerImmediately = false;
            if (cancellationToken?.IsCancellationRequested??false)
            {
                lock (_lookupSync)
                {
                    if (_lookupContexts.Any())
                        resetTimerImmediately = true;
                }
                lock (_automaticAnalyseSync)
                {
                    if (_automaticAnalyseInProgressByHash.Any())
                        resetTimerImmediately = true;
                }
                if (!resetTimerImmediately)
                    Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : Valkyrie queue is Empty");
            }
            lock (_checkStatusesTimer)
            {
                if (resetTimerImmediately)
                    _checkStatusesTimer.SetInterval(TimeSpan.FromMilliseconds(50));
                _isCheckingInProgress = false;
                _checkStatusesTimer.Start(false);
            }
        }

        private void CheckNewStatusesOnValkyrie(CancellationToken cancellationToken)
        {
            SubmitToAutomaticAnalysis(cancellationToken);
            CheckInAnalysisFiles(cancellationToken);
            HandleCredentialsErrors(cancellationToken);
        }

        private void HandleCredentialsErrors(CancellationToken cancellationToken)
        {
            List<IValkyrieFileInfo> filesToCopy = _remoteCopyQueue.Values.ToList();

            foreach (var item in filesToCopy)
            {
                IValkyrieFileInfo result;
                _remoteCopyQueue.TryRemove(item.Sha1, out result);
            }
            if (filesToCopy.Any())
            {
                Trace.WriteLine(
                        $"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff}: ValyrieClient requested {filesToCopy.Count} files from copyHelper");
                var res = _copyHelper.RequestFiles(filesToCopy.ToArray(), _login, _password);
            }
        }

        private void CheckInAnalysisFiles(CancellationToken cancellationToken)
        {
            List<ValkyrieAnalyseOperationContext> filesToCheckAnalysisResult;
            lock (_automaticAnalyseSync)
            {
                filesToCheckAnalysisResult = _automaticAnalyseInProgressByHash.Values.ToList();
                _automaticAnalyseInProgressByHash.Clear();
            }
            foreach (var context in filesToCheckAnalysisResult)
            {
                CheckAnalysisResultResponse automaticAnalysisResponse = null;
                if (!cancellationToken.IsCancellationRequested)
                    try
                    {
                        automaticAnalysisResponse =
                            _provider.CheckAnalysisResult(new CheckAnalysisResultRequest(analysisId: context.AnalysisId,
                                            apiKey: _apiKey));
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(
                            $"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : CheckAnalysisResult failed = {context.HashSha1} Reason : {ex.Message}" +
                            $" {ex.InnerException?.Message ?? string.Empty}");
                    }
                MergeAutomaticAnalysisResult(automaticAnalysisResponse, context, cancellationToken);
            }
        }

        private void MergeResult(IValkyrieAnalysisResult result, IValkyrieFileInfo fileInfo)
        {
            if (!result.IsSubmitted && !result.IsRemoteNeeded && (result.Verdict == ValkyrieVerdict.FirstSeen ||
                    result.Verdict == ValkyrieVerdict.Undetected || result.Verdict == ValkyrieVerdict.Pending))
                lock (_lookupSync)
                {
                    _lookupContexts.Add(fileInfo);
                }
            if (result.Verdict != fileInfo.PreviousValkyrieVerdict)
            {
                fileInfo.PreviousValkyrieVerdict = result.Verdict;
                _pendingItemsResolved.TryAdd(result.Sha1, result);
            }
            if (result.IsRemoteNeeded)
            {
                _remoteCopyQueue.TryAdd(fileInfo.Sha1, fileInfo);
            }
        }

        private void AddLookupContext(IValkyrieFileInfo fileInfo, ValkyrieVerdict previousVerdict, CancellationToken cancellationToken)
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
            if (cancellationToken.IsCancellationRequested)
            {
                _pendingItemsResolved.TryAdd(fileInfo.HashSha1,
                    CancelOperation(fileInfo.ValkyrieFileInfo.GetFullPath(), fileInfo.HashSha1));
            }
            else if (result != null && result.ReturnCode == ErrorCodesEnum.AnalysisNotFound)
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


        private void SubmitToAutomaticAnalysis(CancellationToken cancellationToken)
        {
            List<IValkyrieFileInfo> badFiles;
            lock (_lookupSync)
            {
                badFiles = _lookupContexts.ToList();
                _lookupContexts.Clear();
            }
            var endpointId = _endpointId;
            ParallelOptions opt = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            Parallel.ForEach(badFiles, opt, f =>
            {
                GetValkyrieVerdictForOneFile(f, endpointId, cancellationToken);
            });
        }

        private void GetValkyrieVerdictForOneFile(IValkyrieFileInfo f, string endpointId, CancellationToken cancellationToken)
        {
            try
            {
                var result = UploadFile(f, endpointId, cancellationToken);
                if (result == null) throw new ArgumentException("ValkyrieAnalysisResult can not be null");
                MergeResult(result, f);
            }
            catch (Exception e)
            {
                IValkyrieAnalysisResult result = null;
                if (e is WebException && e.Message == "The request was aborted: The request was canceled.")
                {
                    result = ValkyrieAnalysisResult.CreateSkipInfo(f.Sha1, "The request was canceled by user",
                        SkippReasonEnum.Cancelled);
                }
                else
                {
                    SkippReasonEnum s = SkippReasonEnum.Failed;
                    if (e is InvalidUserNameException)
                        s = SkippReasonEnum.UserNameIsInvalid;
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
                MergeResult(result, f);

                Trace.WriteLine(
                    $"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : Submit Failed SHA1 = {f.Sha1} Reason : {e.Message}" +
                    $" {e.InnerException?.Message ?? string.Empty}");
            }
        }

        public void AnalyzeAsyncCancel(bool cancelledByUser = true)
        {
            if (_cancellationTokenSource != null)
            {
                SendEndScanAsync();
                _cancellationTokenSource.Cancel();
                Task.Factory.StartNew(() => CheckStatusesTimerCallback(this, null));
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

            _pendingItemsResolved.Clear();
            _cancellationTokenSource?.Dispose();

            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationTokenSource = cancellationTokenSource;
            _cancellationToken = _cancellationTokenSource.Token;
            _isMainScanFinished = true;
            AnalyzeCompleted?.Invoke(this, new AnalyzeCompletedEventArgs(exception, task.IsCanceled, task.AsyncState));

            Task.Factory.StartNew(() => CheckStatusesTimerCallback(this, null));

        }

        private void OnAnalyzeProgressChanged(ConcurrentDictionary<string, IValkyrieAnalysisResult> items)
        {
            if (items.Count == 0) return;

            var tmp = AnalyzeProgressChanged;
            if (tmp != null)
            {
                double progress = 0;

                if (_filesCount > 0)
                {
                    progress = (_fileScanned / (double)_filesCount) * 100;

                }
                var progressInt = Convert.ToInt32(progress);

                var newResults = items.Values.ToArray();

                foreach (var item in newResults)
                {
                    IValkyrieAnalysisResult result;
                    items.TryRemove(item.Sha1, out result);
                }
                tmp(this,
                    new ValkyrieAnalyzeProgressChangedEventArgs<IValkyrieAnalysisResult>(progressInt,
                         new ProgressInfo<IValkyrieAnalysisResult>(_filesCount, _fileScanned, newResults)));

            }
        }

        private void OnAnalyzeStarted(string apiKey, string scanId)
        {
            AnalyzeStarted?.Invoke(this, new ValkyrieAnalyzeStartedEventArgs(apiKey, scanId));
        }

        private IValkyrieAnalysisResult UploadFile(IValkyrieFileInfo fileInfo, string endpointId, CancellationToken cancellationToken)
        {
            IValkyrieAnalysisResult result = null;

            try
            {
                var sha1 = fileInfo.Sha1;
                var originalFilePath = fileInfo.GetFullPath();
                if (cancellationToken.IsCancellationRequested)
                {
                    return CancelOperation(originalFilePath, sha1);
                }
                var infoRequeast = new InfoRequest(sha1: sha1, apiKey: _apiKey, endpointId: endpointId,
                    aptScanId: _aptScanId, path: fileInfo.Path);
                IBasicInfoResponse basicInfo = _provider.GetBasicInfo(infoRequeast);
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
                if (!(basicInfo.Upload && (basicInfo.Verdict == DTO.ValkyrieVerdict.Undetected || basicInfo.Verdict == DTO.ValkyrieVerdict.FirstSeen)))
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
                    if (!_submitAllowed)
                        return ValkyrieAnalysisResult.CreateSkipInfo(fileInfo.Sha1, "Waiting for remote operation", SkippReasonEnum.WaitNextInterval);

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
                    using (var helper = _impersonationHelperFactory.CreateImpersonationHelper())
                    {
                        string host = null;
                        if (!fileInfo.Computer.IsLocalComputer && _login != null)
                        {
                            host = helper.Impersonate(new RemoteHostInfo(fileInfo.Computer.Uid, fileInfo.Computer.IPAddress,
                                _login, _password,
                                fileInfo.Computer.IsLocalComputer), cancellationToken, false);
                        }
                        //var data = ReadFile(fileInfo, login, password);
                        var copyResult = fileInfo.CopyResult;
                        var remoteFilePath = copyResult == null ? fileInfo.GetNetworkPath(host) : fileInfo.GetNetworkPath(copyResult.RemoteFilePath, host);
                        Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff}: reading remote file {remoteFilePath}");
                        var fileName = Path.GetFileName(orgFilePath);
                        try
                        {
                            using (var fStream = File.OpenRead(remoteFilePath))
                            {
                                if (cancellationToken.IsCancellationRequested)
                                {
                                    return CancelOperation(originalFilePath, sha1);
                                }
                                response = _provider.SubmitToAutomaticAnalysis(request, fStream, cancellationToken, fileName);
                            }
                            IValkyrieFileInfo fileInfoNew = null;
                            if (_filesToSubmit != null && _filesToSubmit.ContainsKey(fileInfo.Sha1))
                            {
                                fileInfoNew = _filesToSubmit[fileInfo.Sha1];
                            }
                            else
                            {
                                fileInfoNew = fileInfo;
                            }
                            List<FilePath> paths = new List<FilePath>();
                            foreach (var item in fileInfoNew.FilePathes)
                            {
                                var uri = new Uri(item);
                                paths.Add(new FilePath()
                                {
                                    MachineName = uri.Host,
                                    Path = uri.LocalPath.Substring(uri.Host.Length + 3)
                                });
                            }

                            fatSubmitFilesResponse = _provider.FatSubmitFiles(new FatSubmitFilePathsRequest()
                            {
                                Sha1 = sha1,
                                ApiKey = _apiKey,
                                FilePaths = paths.ToArray(),
                                ScanId = _aptScanId
                            });

                        }
                        catch (System.IO.IOException ex)
                        {
                            foreach(var data in ex.Data)
                            {
                                var stringData = data as string;
                                if (stringData == null)
                                    stringData = data.ToString();
                                Trace.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: {stringData}");
                            }
                            if (ex.Message.StartsWith("The user name or password is incorrect."))
                                throw new InvalidUserNameException(ex);
                            throw;
                        }
                        finally
                        {
                            if (copyResult != null)
                            {
                                copyResult.Dispose();
                                fileInfo.CopyResult = null;
                            }
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
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network, $"{ex.GetType()} Exception was accured during uploading file {fileInfo.Sha1}");
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Network, ex.ToString());
                throw;
            }
            finally
            {

            }

            return result;
        }

        private static ValkyrieAnalysisResult CancelOperation(string originalFilePath, string sha1)
        {
            var result = ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Cancelled by user.", SkippReasonEnum.Cancelled);
            Trace.WriteLine($"{DateTime.UtcNow:MM/dd/yy H:mm:ss.ffff} : File {originalFilePath} cancelled by user");
            Trace.Flush();
            return result;
        }

        public string GetValkyrieDetailsLink()
        {
            return GetValkyrieDetailsLink(_apiKey, _aptScanId);
        }

        public string GetValkyrieDetailsLink(string apiKey, string scanId)
        {
            if (!string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(scanId))
                return $@"{_provider.GetBaseUri()}signup_apikey/{apiKey}?scan_id={scanId}";
            return null;
        }

        public void SendEndScanAsync()
        {
            if (_aptScanId != null && !_aptScanEnded)
                Task.Factory.StartNew(t =>
                {
                    _provider.EndScanWithErrorHandling(new EndScanRequest(_apiKey, _machineCount, _aptScanId));
                    _aptScanEnded = true;
                }, TaskCreationOptions.None);
        }

        public async Task SendEndScan()
        {
            await Task.Factory.StartNew(t =>
            {
                if (_aptScanId != null && !_aptScanEnded)
                {
                    _provider.EndScan(new EndScanRequest(_apiKey, _machineCount, _aptScanId));
                    _aptScanEnded = true;
                }
            }, TaskCreationOptions.None);
        }

        public void StartAnalyze(ICamRegistration camRegistration, string login, string password, int machineCount,
            NetworkCredential proxyCredential, string productName)
        {
            _machineCount = machineCount;
            _fileScanned = 0;
            _filesCount = 0;
            string apiKeyInternal = null;
            string scanIdInternal = null;
            Task.Factory.StartNew(t =>
            {
                _isMainScanFinished = false;
                _apiKey = null;
                _aptScanId = null;
                _cancellationTokenSource?.Cancel();

                _submitAllowed = false;
                _filesToSubmit = new Dictionary<string, IValkyrieFileInfo>();
                _pendingItemsResolved.Clear();
                lock (_lookupSync)
                {
                    _lookupContexts.Clear();
                }
                lock (_automaticAnalyseSync)
                {
                    _automaticAnalyseInProgressByHash.Clear();
                }


                var regInfo = Register(camRegistration, productName, proxyCredential);
                if (regInfo.ReturnCode != 0)
                {
                    var ex = new ValkyrieRegistrationFailedException(regInfo);
                    throw ex;
                }
                apiKeyInternal = regInfo.Key;
                _login = login;
                _password = password;

                StartAndFinalizeScanResponse startScanResult = null;
                lock (_aptScanIdLock)
                {
                    startScanResult = _provider.StartScan(new StartScanRequest(regInfo.Key));
                    scanIdInternal = startScanResult.AptScanId;
                }

                if (startScanResult.ReturnCode != 0)
                {
                    Trace.WriteLine("apt_start_scan failed. " + startScanResult.ResultDescription);
                    throw new InvalidOperationException(startScanResult.ResultDescription);
                }


                var regInfo2 = _provider.GetBasicInfoEmpty();

                if (regInfo2.ReturnCode != ErrorCodesEnum.BadRequest)
                {
                    var resp = new RegisterWithResponse();
                    resp.ReturnCode = (int)regInfo2.ReturnCode;
                    resp.ResultDescription = regInfo2.ResultDescription;
                    var ex = new ValkyrieRegistrationFailedException(resp);
                    throw ex;
                }
                using (ManualResetEventSlim manualResetEvent = new ManualResetEventSlim(false))
                {
                    EventHandler callback = (o, ea) => manualResetEvent?.Set();
                    _delayTimer.TimeoutCallback += callback;
                    _delayTimer.Start(false);
                    manualResetEvent.Wait(TimeSpan.FromSeconds(_delayAfterStartScanInSeconds));
                    _delayTimer.TimeoutCallback -= callback;
                }
                _apiKey = apiKeyInternal;
                _aptScanId = scanIdInternal;
                _aptScanEnded = false;
                _progressNotifyTimer.Start(false);
                OnAnalyzeStarted(_apiKey, _aptScanId);


                if (_emails != null)
                    SubmitEmailAsync(_emails);

            }, TaskCreationOptions.None)
                .ContinueWith(OnAnalyzeCompleted, TaskContinuationOptions.NotOnCanceled);
        }

   
        public void Dispose()
        {
            _progressNotifyTimer.TimeoutCallback -= ProgressNotifyTimerCallback;
            _checkStatusesTimer.TimeoutCallback -= CheckStatusesTimerCallback;
            _progressNotifyTimer?.Dispose();
            _checkStatusesTimer?.Dispose();
            _delayTimer?.Dispose();
            _copyHelper.CopyCompleted += _copyHelper_CopyCompleted;
            _cancellationTokenSource?.Dispose();
        }

        public void SetBaseUri(Uri uri)
        {
            _provider.SetBaseUri(uri);
        }

        public void InitEmail(string[] addresses)
        {
            _emails = addresses;
        }

        public async Task<ValkyrieResponse> SubmitEmailAsync(string[] addresses)
        {
            return await Task.Run(() =>
            {
                if (_apiKey == null || _aptScanId == null)
                    throw new InvalidOperationException("Analyze isn't started");

                var result = _provider.SubmitEmail(new SubscribeEmailRequest { ApiKey = _apiKey, ScanId = _aptScanId, CustomerEmails = addresses });
                if (result.ReturnCode == ErrorCodesEnum.Success)
                    _emails = addresses;

                return result;
            });
        }
    }
}
