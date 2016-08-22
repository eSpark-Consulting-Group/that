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
using APTAT.Valkyrie.Client;
using Valkyrie.WebApiProvider;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;
using Valkyrie.WebApiProvider.Helpers;
using Microsoft.Win32;

namespace ValkyrieHashAnalyzer
{
    public sealed class ValkyrieClient
    {
        private string _apiKey;
        private int _fileScanned;
        public int FileScanned
        {
            get
            {
                return _fileScanned;
            }
        }

        public int FilesCount { get; private set; }

        public ConcurrentBag<ValkyrieAnalysisResult> AnalysisResults { get; set; }

        public ValkyrieClient(TaskScheduler ts)
        {
            AnalysisResults = new ConcurrentBag<ValkyrieAnalysisResult>();
            _taskScheduler = ts;
        }

        private CancellationTokenSource _cancellationTokenSource;
        private TaskScheduler _taskScheduler;

        public RegisterWithResponse Register(CamRegistration camRegistration, string proxyLogin = null, string proxyPassword = null)
        {
            if (camRegistration == null || camRegistration.IsEmpty) throw new InvalidOperationException("CAM license or CAM credentials are required");

            ProxySettings.Proxy = TryGetProxy(proxyLogin, proxyPassword);

            RegisterWithResponse regInfo;

            var asm = Assembly.GetExecutingAssembly().GetName();

            if (string.IsNullOrWhiteSpace(camRegistration.LicenseId) == false)
            {
                regInfo = ValkyrieWebApiProvider.RegisterWithCamLicense(new RegisterWithCamLicenseRequest()
                {
                    LicenseId = camRegistration.LicenseId,
                    ProductName = asm.Name,
                    ProductVersion = asm.Version.ToString()
                });

            }
            else
            {
                regInfo = ValkyrieWebApiProvider.RegisterWithCamCredentials(new RegisterWithCamCredentials
                {
                    UserName = camRegistration.UserName,
                    Password = camRegistration.Password,
                    ProductName = asm.Name,
                    ProductVersion = asm.Version.ToString()
                });
            }

            return regInfo;
        }

        private IWebProxy TryGetProxy(string login, string password)
        {
            var proxy = WebRequest.GetSystemWebProxy();
            if (!String.IsNullOrWhiteSpace(login) && !String.IsNullOrWhiteSpace(password))
            {
                proxy.Credentials = new NetworkCredential(login, password);
            }
            return proxy;
        }

        public void AnalyzeAsync(List<string> badFiles, string apiKey)
        {
            try
            {
                _apiKey = apiKey;
                FilesCount = badFiles.Count;
                Trace.WriteLine("API_KEY = " + apiKey);
                Trace.Flush();
                Thread.Sleep(2000);//time out asked by Valkyrie team 
                OnAnalyzeProgressChanged();
                Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Scan Started for API_KEY = {1}", DateTime.UtcNow, apiKey));
                Trace.Flush();

                var startScanResult = ValkyrieWebApiProvider.StartScan(new StartScanRequest(apiKey));
                if (startScanResult.ReturnCode != 0)
                {
                    Trace.WriteLine("apt_start_scan failed. " + startScanResult.ResultDescription);
                    Trace.Flush();
                }

                AnalysisResults = new ConcurrentBag<ValkyrieAnalysisResult>();
                _fileScanned = 0;

                _cancellationTokenSource = new CancellationTokenSource();

                var cancellationToken = _cancellationTokenSource.Token;

                cancellationToken.Register(OnAnalyzeCompleted);

                //  _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();


                Task.Factory.StartNew(t =>
                {
                    try
                    {
                        SubmitToAutomaticAnalysis(badFiles, _cancellationTokenSource.Token);
                    }
                    catch (AggregateException exception)
                    {
                        var messages = exception.InnerExceptions.Select(e => e.Message).ToArray();
                        var exMessage = String.Join(Environment.NewLine, messages);
                        Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : {1}", DateTime.UtcNow, exMessage));
                        Trace.Flush();
                        OnAnalyzeCompleted(exception);
                    }

                }, TaskCreationOptions.None, cancellationToken)
                .ContinueWith(OnAnalyzeCompleted, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, _taskScheduler);

            }
            catch (Exception exception)
            {
                throw exception;
                Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : {1}", DateTime.UtcNow, exception.Message));
                Trace.Flush();
                var tmp = AnalyzeCompleted;
                if (tmp != null)
                {
                    var agrEx = new AggregateException(exception);
                    tmp(this, new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(AnalysisResults.ToArray(), agrEx, false, null));
                }
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
                key.SetValue(keyValueName, guid, RegistryValueKind.String);
                key.Dispose();
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

        private void SubmitToAutomaticAnalysis(List<string> badFiles, CancellationToken cancellationToken)
        {
            Guid endpointId = GetEndpointId();
            Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff}: EndpointId {1}",
                    DateTime.UtcNow, endpointId));

            var exceptions = new ConcurrentQueue<Exception>();
            if (true)
            {
                ParallelOptions opt = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                Parallel.ForEach(badFiles, opt, f =>
                {
                    try
                    {
                        UploadFile(f, endpointId);
                    }
                    catch (Exception e)
                    {

                        if (e is WebException && e.Message == "The request was aborted: The request was canceled.")
                        {
                            AddResult(ValkyrieAnalysisResult.CreateSkipInfo(f, "The request was canceled by user", SkippReasonEnum.Cancelled));
                        }
                        else
                        {
                            AddResult(ValkyrieAnalysisResult.CreateSkipInfo(f, e.Message, SkippReasonEnum.Failed));
                        }

                        Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Submit Failed SHA1 = {1} Reason : {2} {3}", DateTime.UtcNow, f, e.Message, e.InnerException != null ? e.InnerException.Message : string.Empty));
                        Trace.Flush();

                        var exp = new InvalidOperationException("Could not upload file " + f, e);
                        exceptions.Enqueue(exp);
                    }
                });


                var ex = new AggregateException(exceptions.ToArray());
                if (exceptions.Count > 0) throw ex;
            }
            //else
            //{
            //    foreach (var f in badFiles)
            //    {
            //        UploadFile(f, endpointId);
            //    }
            //}

        }

        public void AnalyzeAsyncCancel()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
        }

        public event AnalyzeCompletedEventHandler<ValkyrieAnalysisResult> AnalyzeCompleted;
        public event ProgressChangedEventHandler AnalyzeProgressChanged;
        public event ProgressChangedEventHandler OnResultAdded;

        private void OnAnalyzeCompleted()
        {
            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                Task.Factory.StartNew(() =>
                    {
                        tmp(this, new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(AnalysisResults.ToArray(), null, true, null));
                    }, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
            }
        }


        private void OnAnalyzeCompleted(Exception exception)
        {
            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                Task.Factory.StartNew(

                    () =>
                    {
                        tmp(this, new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(AnalysisResults.ToArray(), exception, false, null));
                    }, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
            }

        }
        private void OnAnalyzeCompleted(Task task)
        {
            ValkyrieWebApiProvider.EndScan(new EndScanRequest(_apiKey, 1));
            Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Scan Completed for API_KEY = {1}", DateTime.UtcNow, _apiKey));
            Trace.Flush();

            if (task.Exception != null) throw task.Exception;
            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                tmp(this, new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(AnalysisResults.ToArray(), task.Exception, task.IsCanceled, task.AsyncState));
            }
        }

        private void OnAnalyzeProgressChanged()
        {
            var tmp = AnalyzeProgressChanged;
            if (tmp != null)
            {
                var malwareCount = AnalysisResults.Count(f => f.Verdict == ValkyrieVerdict.Malware || f.Verdict == ValkyrieVerdict.PotentiallyUnwantedApplication);
                double progress = 0;

                if (FilesCount > 0)
                {
                    progress = (_fileScanned / (double)FilesCount) * 100;

                }
                var progressInt = Convert.ToInt32(progress);

                tmp(this, new ProgressChangedEventArgs(progressInt, new ProgressInfo<ValkyrieAnalysisResult>(FilesCount, _fileScanned, malwareCount, AnalysisResults.ToArray())));
            }
        }

        private void UploadFile(string sha1, Guid endpointId)
        {
            try
            {
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    AddResult(ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Cancelled by user.", SkippReasonEnum.Cancelled));
                    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : File {1} cancelled by user", DateTime.UtcNow,
                        sha1));
                    Trace.Flush();
                    return;
                }

                var infoRequeast = new InfoRequest { SHA1 = sha1, ApiKey = _apiKey, EndpointId = endpointId };
                BasicInfoResponse basicInfo = ValkyrieWebApiProvider.GetBasicInfo(infoRequeast);
                if (basicInfo.ReturnCode != ErrorCodesEnum.Success)
                    throw new InvalidOperationException(basicInfo.ReturnCode.GetEnumDescription());

                Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff}: SHA1 {1} FilePath {2} Presence = {3}",
                    DateTime.UtcNow, sha1, String.Empty, basicInfo.Presence));
                Trace.Flush();

                if (basicInfo.Presence)
                {
                    ValkyrieAnalysisResult result = ValkyrieAnalysisResult.Create(basicInfo);
                    AddResult(result);
                }
                else
                {
                    Trace.WriteLine(
                        string.Format("{0:MM/dd/yy H:mm:ss.ffff}: SHA1 {1} FilePath {2} Presence = {3}",
                            DateTime.UtcNow, sha1, String.Empty, basicInfo.Presence));
                    var result = ValkyrieAnalysisResult.CreateSkipInfo(sha1, "No presense.", SkippReasonEnum.Skipped);
                    result.OperationStatus = String.Empty;
                    AddResult(result);

                    Trace.Flush();
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss} : {1}", DateTime.UtcNow, e.Message));
                Trace.Flush();
                throw e;
            }

        }

        private void AddResult(ValkyrieAnalysisResult result)
        {
            AnalysisResults.Add(result);
            Interlocked.Increment(ref _fileScanned);
            Task.Factory.StartNew(OnAnalyzeProgressChanged, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);

            OnResultAddedInvoker(result);

        }

        private void OnResultAddedInvoker(ValkyrieAnalysisResult result)
        {
            Task.Factory.StartNew(() =>
            {
                var tmp = OnResultAdded;
                if (tmp != null)
                {
                    tmp(this, new ProgressChangedEventArgs(0, result));
                }
            }, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }


        public string GetValkyrieDetailsLink()
        {
            return string.Format(@"{0}signup_apikey/{1}", ValkyrieApiUrls.GetBaseUri(), _apiKey);
        }
    }
}
