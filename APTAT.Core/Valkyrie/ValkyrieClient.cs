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
        private Timer _progressNotifyTimer;
        private int _machineCount;

        public event EventHandler<ProgressChangedEventArgs> AnalyzeProgressChanged;
        public event EventHandler<AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>> AnalyzeCompleted;
        public event EventHandler AnalyzeStarted;

        public ValkyrieClient()
        {
            _analysisResults = new ConcurrentBag<ValkyrieAnalysisResult>();
        }

       

        private RegisterWithResponse Register(CamRegistration camRegistration, NetworkCredential networkCredential = null)
        {
            if (camRegistration == null || camRegistration.IsEmpty) throw new InvalidOperationException("CAM license or CAM credentials are required");

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
            _apiKey = regInfo.ApiKey;
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
            OnAnalyzeProgressChanged();
        }

        private string GetRemoteFile(string fileName, string sha1, string login, string password, string ipAddress, string destPath)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true


            };
            //  Items.First(i => i.TempItems.Any(ti => ti.BadFile.Sha1.Equals(badFile.Sha1))).IpAddress;
            var target = string.Format("\\\\{0}\\{1}", ipAddress, Path.GetPathRoot(fileName).Replace(":\\", "$"));

            string destination = Path.Combine(destPath, sha1);
            string destinationFile = string.Format("{0}.tmp", destination);
            string source = target + fileName.Remove(0, 2);
            startInfo.Arguments = string.Format("/C net use \"{0}\" {1} /user:{2} & copy \"{3}\" {4}", target, password, login, source, destinationFile);

            process.StartInfo = startInfo;

            process.Start();

            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();

            Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : {1}", DateTime.UtcNow, string.Format("Started process. Copy file from {0} to {1}", source, destinationFile)));

            process.WaitForExit();
            Trace.WriteLine(String.Format("Process exit code : {0}", process.ExitCode));
            Trace.WriteLine(String.Format("Process standard output : {0}", stdout));
            if (String.IsNullOrWhiteSpace(stderr) == false)
            {
                Trace.WriteLine(String.Format("Process standard error : {0}", stderr));
            }

            Trace.Flush();
            return destinationFile;
        }

        private static string CreateTmpFolder()
        {
            var usrTempFolder = Environment.GetEnvironmentVariable("TEMP");
            if (string.IsNullOrWhiteSpace(usrTempFolder))
            {
                usrTempFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            var valkyrieFolder = Path.Combine(usrTempFolder, "ValkyrieTmp");
            bool exists = System.IO.Directory.Exists(valkyrieFolder);
            if (!exists)
            {
                System.IO.Directory.CreateDirectory(valkyrieFolder);
            }
            else
            {
                DirectoryInfo downloadedMessageInfo = new DirectoryInfo(valkyrieFolder);

                foreach (FileInfo file in downloadedMessageInfo.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in downloadedMessageInfo.GetDirectories())
                {
                    dir.Delete(true);
                }
            }

            return valkyrieFolder;
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

        private void SubmitToAutomaticAnalysis(List<ValkyrieFileInfo> badFiles, string login, string password, CancellationToken cancellationToken)
        {
            _filesCount = badFiles.Count;
            Guid endpointId = GetEndpointId();
            var destFolder = CreateTmpFolder();
            var exceptions = new ConcurrentQueue<Exception>();
            
                ParallelOptions opt = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                Parallel.ForEach(badFiles, opt, f =>
                {

                    try
                    {
                        var filePath = GetRemoteFile(f.Path, f.Sha1, login, password, f.Ip, destFolder);
                        var orgPath = f.GetFullPath();
                        var result = UploadFile(filePath, orgPath, endpointId);
                        if (result == null) throw new ArgumentException("ValkyrieAnalysisResult can not be null");
                        AddResult(result);                        
                    }
                    catch (Exception e)
                    {
                        ValkyrieAnalysisResult result = null;
                        if (e is WebException && e.Message == "The request was aborted: The request was canceled.")
                        {
                            result = ValkyrieAnalysisResult.CreateSkipInfo(f.Sha1, "The request was canceled by user", SkippReasonEnum.Cancelled);                            
                        }
                        else
                        {
                            string m = e.Message;
                            if (e.Message.StartsWith("Could not find file"))
                            {
                                m = "Could not find file";
                            }
                            result = ValkyrieAnalysisResult.CreateSkipInfo(f.Sha1, m, SkippReasonEnum.Failed);
                        }
                        AddResult(result);
                        
                        Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Submit Failed SHA1 = {1} Reason : {2} {3}", DateTime.UtcNow, f.Sha1, e.Message, e.InnerException != null ? e.InnerException.Message : string.Empty));

                        
                    }
                });
        
            
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
                        tmp(this, new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(_analysisResults.ToArray(), null, true, null));
                    }, CancellationToken.None);
            }

        }

        private void OnAnalyzeCompleted(Task task)
        {
            if (_progressNotifyTimer != null)
            {
                _progressNotifyTimer.Dispose();
                _progressNotifyTimer = null;
            }
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
            Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Scan Completed for API_KEY = {1}", DateTime.UtcNow, _apiKey));
            
            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                tmp(this, new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(_analysisResults.ToArray(), exception, task.IsCanceled, task.AsyncState));
            }
        }

        private void OnAnalyzeProgressChanged()
        {
            if (_analysedItems.Count == 0) return;

            var tmp = AnalyzeProgressChanged;
            if (tmp != null)
            {
                var malwareCount = _analysisResults.Count(f => f.Verdict == ValkyrieVerdict.Malware || f.Verdict == ValkyrieVerdict.PotentiallyUnwantedApplication);
                double progress = 0;

                if (_filesCount > 0)
                {
                    progress = (_fileScanned / (double)_filesCount) * 100;

                }
                var progressInt = Convert.ToInt32(progress);

                var newResults = _analysedItems.Values.ToArray();

                foreach (var item in newResults)
                {
                    ValkyrieAnalysisResult result;
                    _analysedItems.TryRemove(item.SHA1, out result);
                }

                tmp(this, new ProgressChangedEventArgs(progressInt, new ProgressInfo<ValkyrieAnalysisResult>(_filesCount, _fileScanned, malwareCount, 0, newResults)));

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

        private ValkyrieAnalysisResult UploadFile(string filePath, string originalFilePath, Guid endpointId)
        {
            ValkyrieAnalysisResult result = null;
            try
            {
                var sha1 = HashCalculator.ComputeSHA1Hash(filePath);
                                
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    result = ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Cancelled by user.", SkippReasonEnum.Cancelled);
                    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : File {1} cancelled by user", DateTime.UtcNow,
                        originalFilePath));
                    Trace.Flush();
                    return result;
                }

                FileInfo f = new FileInfo(filePath);


                if (f.Length > MaxFileLenght)
                {
                    result = ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Maximum file size per upload to 25 MB.", SkippReasonEnum.Skipped);
                    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : File {1} greater than 25 Mb", DateTime.UtcNow,
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
                        string.Format("{0:MM/dd/yy H:mm:ss.ffff}: SHA1 {1} FilePath {2} Presence = {3}, [File Uploaded]",
                            DateTime.UtcNow, sha1, originalFilePath, basicInfo.Presence));
                    Trace.Flush();
                    var request = new SubmitToAutomaticRequest
                    {
                        ApiKey = _apiKey,
                        SubmitToken = basicInfo.SubmitToken,
                        FilePath = WebUtility.HtmlDecode(originalFilePath),
                        EndpointId = endpointId
                    };

                    var response = ValkyrieWebApiProvider.SubmitToAutomaticAnalysis(request, filePath, _cancellationTokenSource.Token);

                    CheckAnalysisResultResponse automaticAnalysisResponse = new CheckAnalysisResultResponse() { ReturnCode = ErrorCodesEnum.None };

                    var timeOut = TimeSpan.FromMinutes(5);
                    bool success = false;
                    int elapsed = 0;
                    while ((!success) && (elapsed < timeOut.TotalMilliseconds))
                    {
                        Thread.Sleep(1000);
                        elapsed += 1000;
                        automaticAnalysisResponse = ValkyrieWebApiProvider.CheckAnalysisResult(new CheckAnalysisResultRequest() { ApiKey = _apiKey, AnalysisId = response.AnalysisId });
                        success = (automaticAnalysisResponse.ReturnCode != ErrorCodesEnum.AnalysisNotFinished);                      
                    }
                    if (!success)
                    {
                        throw new TimeoutException("Automatic analysis has not been completed after " + timeOut.TotalMinutes + " minutes");
                    }
                    result = ValkyrieAnalysisResult.Create(automaticAnalysisResponse);
             
                }
            }            
            finally
            {
                try
                {
                    File.Delete(filePath);
                    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Delete {1}", DateTime.UtcNow, filePath));
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Delete Failed {1} with error: {2}", DateTime.UtcNow, filePath, ex.Message));
                }                
                Trace.Flush();
            }
            return result;
        }

        private void AddResult(ValkyrieAnalysisResult result)
        {
            _analysisResults.Add(result);

            Interlocked.Increment(ref _fileScanned);
            _analysedItems.TryAdd(result.SHA1, result);

            //Task.Factory.StartNew(OnAnalyzeProgressChanged, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public string GetValkyrieDetailsLink()
        {
            return String.Format(@"{0}signup_apikey/{1}", ValkyrieApiUrls.GetBaseUri(), _apiKey);
        }


        public void StartAnalyze(CamRegistration camRegistration, List<ValkyrieFileInfo> filesToAnaluyze,
            string login, string password, int machineCount, NetworkCredential proxyCredential = null)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _analysedItems = new ConcurrentDictionary<string, ValkyrieAnalysisResult>();
            _machineCount = machineCount;
            _fileScanned = 0;
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
                OnAnalyzeStarted();
                Thread.Sleep(2000); //time out asked by Valkyrie team 
                var startScanResult = ValkyrieWebApiProvider.StartScan(new StartScanRequest(regInfo.Key));
                if (startScanResult.ReturnCode != 0)
                {
                    Trace.WriteLine("apt_start_scan failed. " + startScanResult.ResultDescription);
                    throw new InvalidOperationException();
                }
                _progressNotifyTimer = new Timer(ProgressNotifyTimerCallback, null, 1000, 200);
                SubmitToAutomaticAnalysis(filesToAnaluyze, login, password, _cancellationTokenSource.Token);
                ValkyrieWebApiProvider.EndScan(new EndScanRequest(_apiKey, _machineCount));
            }, TaskCreationOptions.None, cancellationToken)
                .ContinueWith(OnAnalyzeCompleted,TaskContinuationOptions.NotOnCanceled);
        }
    }
}
