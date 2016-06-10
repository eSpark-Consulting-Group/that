using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
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
using Microsoft.Win32;
using APTAT.Core.Descent;
using APTAT.Core.Descent.Apt;

namespace APTAT.Core.Valkyrie
{
    internal sealed class ValkyrieClient : IValkyrieClient
    {
        private const Int32 MaxFileLenght = 26214400;
        private string _apiKey;
        private int _fileScanned;
        private int _filesCount;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentBag<ValkyrieAnalysisResult> _analysisResults;
        private ConcurrentDictionary<string, ValkyrieAnalysisResult> _analysedItems
            = new ConcurrentDictionary<string, ValkyrieAnalysisResult>();
        
        private readonly Dictionary<string, ValkyrieAnalyseOperationContext> _automaticAnalyseInProgressByHash
            = new Dictionary<string, ValkyrieAnalyseOperationContext>(); 
        private readonly Object _automaticAnalyseSync = new object();

        private readonly HashSet<ValkyrieFileInfo> _lookupContexts = new HashSet<ValkyrieFileInfo>();
        private readonly Object _lookupSync = new object();

        private Timer _progressNotifyTimer;
        private Timer _checkStatusesTimer;
        private int _machineCount;
        private string _login;
        private string _password;

        public event EventHandler<ProgressChangedEventArgs> AnalyzeProgressChanged;
        public event EventHandler<AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>> AnalyzeCompleted;
        public event EventHandler AnalyzeStarted;

        public ValkyrieClient()
        {
            _analysisResults = new ConcurrentBag<ValkyrieAnalysisResult>();
            _progressNotifyTimer = new Timer(ProgressNotifyTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _checkStatusesTimer = new Timer(CheckStatusesTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }



        private RegisterWithResponse Register(CamRegistration camRegistration,
            NetworkCredential networkCredential = null)
        {
            if (camRegistration == null || camRegistration.IsEmpty)
                throw new InvalidOperationException("CAM license or CAM credentials are required");

            if (networkCredential != null)
                ProxySettings.Proxy = TryGetProxy(networkCredential);
            //var machineId = NetAddressHelper.GetMacWithIPAddress();
            RegisterWithResponse regInfo;

            var asm = Assembly.GetExecutingAssembly().GetName();

            if (string.IsNullOrWhiteSpace(camRegistration.LicenseId) == false)
            {
                regInfo = ValkyrieWebApiProvider.RegisterWithCamLicense(new RegisterWithCamLicenseRequest()
                {
                    LicenseId = camRegistration.LicenseId,
                    ProductName = "CDM",
                    ProductVersion = asm.Version.ToString()
                });

            }
            else
            {
                regInfo = ValkyrieWebApiProvider.RegisterWithCamCredentials(new RegisterWithCamCredentials
                {
                    UserName = camRegistration.UserName,
                    Password = camRegistration.Password,
                    ProductName = "CDM",
                    ProductVersion = asm.Version.ToString()
                });
            }
            _apiKey = regInfo.Key;
            return regInfo;
        }

        private IWebProxy TryGetProxy(NetworkCredential networkCredential)
        {
            var proxy = WebRequest.GetSystemWebProxy();
            if (networkCredential != null)
            {
                proxy.Credentials = networkCredential;
            }
            return proxy;
        }

        private void ProgressNotifyTimerCallback(object state)
        {
            if (_progressNotifyTimer != null)
                _progressNotifyTimer.Change(Timeout.Infinite, Timeout.Infinite);
            OnAnalyzeProgressChanged();
            if (_progressNotifyTimer != null)
                _progressNotifyTimer.Change(1000, Timeout.Infinite);
        }

        private void CheckStatusesTimerCallback(object state)
        {
            if (_progressNotifyTimer != null)
                _progressNotifyTimer.Change(Timeout.Infinite, Timeout.Infinite);
            CheckNewStatusesOnValkyrie();
            if (_progressNotifyTimer != null)
                _progressNotifyTimer.Change(1000, Timeout.Infinite);
        }

        private void CheckNewStatusesOnValkyrie()
        {
            List<ValkyrieFileInfo> filesToAnalyze;
            lock (_lookupSync)
            {
                filesToAnalyze = _lookupContexts.ToList();
            }
            SubmitToAutomaticAnalysis(filesToAnalyze, _cancellationTokenSource.Token, false);
            List<ValkyrieAnalyseOperationContext> filesToCheckAnalysisResult;
            lock (_automaticAnalyseSync)
            {
                filesToCheckAnalysisResult = _automaticAnalyseInProgressByHash.Values.ToList();
            }
            foreach (var context in filesToCheckAnalysisResult)
            {
                var automaticAnalysisResponse =
                    ValkyrieWebApiProvider.CheckAnalysisResult(new CheckAnalysisResultRequest()
                    {
                        ApiKey = _apiKey,
                        AnalysisId = context.AnalysisId
                    });
                if ( automaticAnalysisResponse.ReturnCode == ErrorCodesEnum.Success
                    || automaticAnalysisResponse.ReturnCode == ErrorCodesEnum.AnalysisNotFound )
                    AddResult(ValkyrieAnalysisResult.Create(automaticAnalysisResponse),context.ValkyrieFileInfo);
            }
        }

        private Guid GetEndpointId()
        {
            string subKeyName = @"SOFTWARE\COMODO\APTAT";
            const string keyValueName = "EndpointID";

            RegistryKey key = Registry.CurrentUser.OpenSubKey(subKeyName, true);

            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(subKeyName);
                var guid = Guid.NewGuid();
                if (key != null)
                {
                    key.SetValue(keyValueName, guid, RegistryValueKind.String);
                    key.Dispose();
                }
                return guid;
            }
            else
            {
                var id = key.GetValue(keyValueName);
                if (id == null)
                {
                    var guid = Guid.NewGuid();
                    key.SetValue(keyValueName, guid, RegistryValueKind.String);
                    key.Dispose();
                    return guid;
                }
                else
                {
                    key.Dispose();
                    Guid result;
                    Guid.TryParse(id.ToString(), out result);
                    return result;
                }
            }
        }

       
        private void SubmitToAutomaticAnalysis(List<ValkyrieFileInfo> badFiles, 
            CancellationToken cancellationToken, bool parallels = true)
        {
            using (ImpersonationHelper helper = new ImpersonationHelper(_login, "", _password, ImpersonationHelper.Logon32LogonType.NewCredentials))
            {
                _filesCount = badFiles.Count;
                Guid endpointId = GetEndpointId();
                var exceptions = new ConcurrentQueue<Exception>();

                ParallelOptions opt = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
                if (parallels)
                    Parallel.ForEach(badFiles, opt, f =>
                    {
                        GetValkyrieVerdictForOneFile(f, endpointId);
                    });
                else
                    foreach (var f in badFiles)
                    {
                        GetValkyrieVerdictForOneFile(f, endpointId);
                    }

            }

        }

        private void GetValkyrieVerdictForOneFile(ValkyrieFileInfo f, Guid endpointId)
        {
            try
            {
                var result = UploadFile(f, endpointId);
                if (result == null) throw new ArgumentException("ValkyrieAnalysisResult can not be null");
                AddResult(result, f);
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
                AddResult(result, f);

                Trace.WriteLine(
                    string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Submit Failed SHA1 = {1} Reason : {2} {3}",
                        DateTime.UtcNow, f.Sha1, e.Message,
                        e.InnerException != null ? e.InnerException.Message : string.Empty));
            }
        }

        public void AnalyzeAsyncCancel()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }


        private void OnAnalyzeCompleted()
        {
            if (_progressNotifyTimer != null)
            {
                _progressNotifyTimer.Dispose();
                _progressNotifyTimer = null;
            }
            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                Task.Factory.StartNew(() =>
                {
                    tmp(this,
                        new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(_analysisResults.ToArray(), null, true,
                            null));
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
                    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : {1}", DateTime.UtcNow, exception.Message));
                }
                else
                {
                    var messages = aggregateException.InnerExceptions.Select(e => e.Message).ToArray();
                    var exMessage = String.Join(Environment.NewLine, messages);
                    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : {1}", DateTime.UtcNow, exMessage));
                    exception = aggregateException;
                }
            }
            else
                Task.Factory.StartNew(t =>
                    { 
                        ValkyrieWebApiProvider.EndScan(new EndScanRequest(_apiKey, _machineCount)); 
                    }, TaskCreationOptions.None);

            Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Scan Completed for API_KEY = {1}",
                DateTime.UtcNow, _apiKey));

            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                tmp(this,
                    new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(_analysisResults.ToArray(), exception,
                        task.IsCanceled, task.AsyncState));
            }
            if (_lookupContexts.Any() || _automaticAnalyseInProgressByHash.Any())
            {
                _checkStatusesTimer.Change(1000, Timeout.Infinite);
            }
        }

        private void OnAnalyzeProgressChanged()
        {
            if (_analysedItems.Count == 0) return;

            var tmp = AnalyzeProgressChanged;
            if (tmp != null)
            {
                var malwareCount =
                    _analysisResults.Count(
                        f =>
                            f.Verdict == ValkyrieVerdict.Malware ||
                            f.Verdict == ValkyrieVerdict.PotentiallyUnwantedApplication);
                double progress = 0;

                if (_filesCount > 0)
                {
                    progress = (_fileScanned/(double) _filesCount)*100;

                }
                var progressInt = Convert.ToInt32(progress);

                var newResults = _analysedItems.Values.ToArray();

                foreach (var item in newResults)
                {
                    ValkyrieAnalysisResult result;
                    _analysedItems.TryRemove(item.SHA1, out result);
                }

                tmp(this,
                    new ProgressChangedEventArgs(progressInt,
                        new ProgressInfo<ValkyrieAnalysisResult>(_filesCount, _fileScanned, malwareCount, 0, newResults)));

            }
        }

        private void OnAnalyzeStarted()
        {
            var tmp = AnalyzeStarted;
            if (tmp != null)
            {
                tmp(this, EventArgs.Empty);
            }
        }
              

        private ValkyrieAnalysisResult UploadFileOldStyle(ValkyrieFileInfo fileInfo, Guid endpointId)
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

                if (fileInfo.Size > MaxFileLenght)
                {
                    result = ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Maximum file size per upload to 25 MB.",
                        SkippReasonEnum.Skipped);
                    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : File {1} greater than 25 Mb",
                        DateTime.UtcNow,
                        originalFilePath));
                    Trace.Flush();
                    return result;
                }

                var infoRequeast = new InfoRequest { SHA1 = sha1, ApiKey = _apiKey, EndpointId = endpointId };
                BasicInfoResponse basicInfo = ValkyrieWebApiProvider.GetBasicInfo(infoRequeast);
                if (basicInfo.ReturnCode != ErrorCodesEnum.Success)
                    throw new InvalidOperationException(basicInfo.ReturnCode.GetEnumDescription());

                Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff}: SHA1 {1} FilePath {2} Presence = {3}",
                    DateTime.UtcNow, sha1, originalFilePath, basicInfo.Presence));
                Trace.Flush();

                if (basicInfo.Presence)
                {
                    result = ValkyrieAnalysisResult.Create(basicInfo);
                }
                else
                {
                    Trace.WriteLine(
                        string.Format(
                            "{0:MM/dd/yy H:mm:ss.ffff}: SHA1 {1} FilePath {2} Presence = {3}, [File Uploaded]",
                            DateTime.UtcNow, sha1, originalFilePath, basicInfo.Presence));
                
                    var request = new SubmitToAutomaticRequest
                    {
                        ApiKey = _apiKey,
                        SubmitToken = basicInfo.SubmitToken,
                        FilePath = WebUtility.HtmlDecode(originalFilePath),
                        EndpointId = endpointId
                    };

                    //var data = ReadFile(fileInfo, login, password);
                    var fStream = File.OpenRead(fileInfo.GetNetworkPath());
                    var response = ValkyrieWebApiProvider.SubmitToAutomaticAnalysis(request, fStream, _cancellationTokenSource.Token);
                    
                    CheckAnalysisResultResponse automaticAnalysisResponse = new CheckAnalysisResultResponse()
                    {
                        ReturnCode = ErrorCodesEnum.None
                    };

                    var timeOutInMinutes = 5;
                    var timeOut = (int)TimeSpan.FromMinutes(timeOutInMinutes).TotalMilliseconds;
                    bool success = false;
                    bool analyzeNotFinished = true;
                    double elapsed = 0;
                    DateTime utcStarted = DateTime.UtcNow;
                    while (analyzeNotFinished && (elapsed < timeOut))
                    {
                        if (_cancellationTokenSource.IsCancellationRequested)
                        {
                            return CancelOperation(originalFilePath, sha1);
                        }

                        Thread.Sleep(1000);
                        elapsed = (int)(DateTime.UtcNow - utcStarted).TotalMilliseconds;
                        automaticAnalysisResponse =
                            ValkyrieWebApiProvider.CheckAnalysisResult(new CheckAnalysisResultRequest()
                            {
                                ApiKey = _apiKey,
                                AnalysisId = response.AnalysisId,
                            });
                        analyzeNotFinished = automaticAnalysisResponse.ReturnCode == ErrorCodesEnum.AnalysisNotFinished;

                    }
                    success = automaticAnalysisResponse.ReturnCode == ErrorCodesEnum.Success;
                    if (analyzeNotFinished)
                    {
                        throw new TimeoutException("Automatic analysis has not been completed after " +
                                                   timeOutInMinutes + " minutes");
                    }
                    if (!success)
                    {
                        throw new InvalidOperationException(automaticAnalysisResponse.ReturnCode.GetEnumDescription());
                    }
                    result = ValkyrieAnalysisResult.Create(automaticAnalysisResponse);

                }
            }
            finally 
            { 

            }
            
            return result;
        }

        private ValkyrieAnalysisResult UploadFile(ValkyrieFileInfo fileInfo, Guid endpointId)
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

                var infoRequeast = new InfoRequest { SHA1 = sha1, ApiKey = _apiKey, EndpointId = endpointId };
                BasicInfoResponse basicInfo = ValkyrieWebApiProvider.GetBasicInfo(infoRequeast);
                if (basicInfo.ReturnCode != ErrorCodesEnum.Success)
                    throw new InvalidOperationException(basicInfo.ReturnCode.GetEnumDescription());
                Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff}: SHA1 {1} FilePath {2} FlsVerdict {3} Upload {4} Presence = {5}",
                    DateTime.UtcNow, sha1, originalFilePath, fileInfo.FlsCloud, basicInfo.Upload, basicInfo.Presence));
                Trace.Flush();

                if (!basicInfo.Upload || fileInfo.FlsCloud != CloudVerdict.FirstSeen)
                {
                    result = ValkyrieAnalysisResult.Create(basicInfo);
                }
                else
                {
                    lock (_automaticAnalyseSync)
                    {
                        ValkyrieAnalyseOperationContext context;
                        _automaticAnalyseInProgressByHash.TryGetValue(fileInfo.Sha1, out context);
                        if (context != null)
                        {
                            return ValkyrieAnalysisResult.Create(basicInfo, context.AnalysisId, true);
                        }
                    }
                    if (fileInfo.Size > MaxFileLenght)
                    {
                        result = ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Maximum file size per upload to 25 MB.",
                            SkippReasonEnum.Skipped);
                        Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : File {1} greater than 25 Mb",
                            DateTime.UtcNow,
                            originalFilePath));
                        Trace.Flush();
                        return result;
                    }
                    Trace.WriteLine(
                        string.Format(
                            "{0:MM/dd/yy H:mm:ss.ffff}: SHA1 {1} FilePath {2} Presence = {3}, [File Uploaded]",
                            DateTime.UtcNow, sha1, originalFilePath, basicInfo.Presence));

                    var request = new SubmitToAutomaticRequest
                    {
                        ApiKey = _apiKey,
                        SubmitToken = basicInfo.SubmitToken,
                        FilePath = WebUtility.HtmlDecode(originalFilePath),
                        EndpointId = endpointId
                    };

                    //var data = ReadFile(fileInfo, login, password);
                    var fStream = File.OpenRead(fileInfo.GetNetworkPath());
                    var response = ValkyrieWebApiProvider.SubmitToAutomaticAnalysis(request, fStream, _cancellationTokenSource.Token);
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
            finally
            {

            }

            return result;
        }

        private static ValkyrieAnalysisResult CancelOperation(string originalFilePath,  string sha1)
        {
            var result = ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Cancelled by user.", SkippReasonEnum.Cancelled);
            Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : File {1} cancelled by user", DateTime.UtcNow, originalFilePath));            
            Trace.Flush();
            return result;
        }

        private void AddResult(ValkyrieAnalysisResult result, ValkyrieFileInfo info)
        {
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
                    _lookupContexts.Add(info);
                }
            }

            //Task.Factory.StartNew(OnAnalyzeProgressChanged, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public string GetValkyrieDetailsLink()
        {
            return String.Format(@"{0}signup_apikey/{1}", ValkyrieApiUrls.GetBaseUri(), _apiKey);
        }


        public void StartAnalyze(CamRegistration camRegistration, List<ValkyrieFileInfo> filesToAnaluyze,
            string login, string password, int machineCount, NetworkCredential proxyCredential = null)
        {
            _analysedItems = new ConcurrentDictionary<string, ValkyrieAnalysisResult>();
            _machineCount = machineCount;
            _fileScanned = 0;
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            cancellationToken.Register(OnAnalyzeCompleted);

            Task.Factory.StartNew(t =>
            {
                var regInfo = Register(camRegistration, proxyCredential);
                if (regInfo.ReturnCode != 0)
                {
                    var ex = new ValkyrieRegistrationFailedException(regInfo);
                    throw ex;
                }

                var startScanResult = ValkyrieWebApiProvider.StartScan(new StartScanRequest(regInfo.Key));
                if (startScanResult.ReturnCode != 0)
                {
                    Trace.WriteLine("apt_start_scan failed. " + startScanResult.ResultDescription);
                    throw new InvalidOperationException(startScanResult.ResultDescription);
                }
                _login = login;
                _password = password;
                OnAnalyzeStarted();
                Thread.Sleep(2000);
                _progressNotifyTimer.Change(1000, Timeout.Infinite);
                SubmitToAutomaticAnalysis(filesToAnaluyze, _cancellationTokenSource.Token);
                
                
                
            }, TaskCreationOptions.None, cancellationToken)
                .ContinueWith(OnAnalyzeCompleted, TaskContinuationOptions.NotOnCanceled);
        }

        public void Dispose()
        {
            if (_progressNotifyTimer != null)
            {
                _progressNotifyTimer.Dispose();
                _progressNotifyTimer = null;
            }
            if (_checkStatusesTimer != null)
            {
                _checkStatusesTimer.Dispose();
                _checkStatusesTimer = null;
            }
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
    }
}
