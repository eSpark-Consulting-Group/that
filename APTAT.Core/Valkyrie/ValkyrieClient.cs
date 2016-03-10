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
    public sealed class ValkyrieClient
    {
        private const Int32 MaxFileLenght = 26214400;
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

        public ValkyrieClient()
        {
            AnalysisResults = new ConcurrentBag<ValkyrieAnalysisResult>();
        }

        private CancellationTokenSource _cancellationTokenSource;
        private TaskScheduler _taskScheduler;

        public RegisterWithResponse Register(CamRegistration camRegistration, string proxyLogin = null, string proxyPassword = null)
        {
            if (camRegistration == null || camRegistration.IsEmpty) throw new InvalidOperationException("CAM license or CAM credentials are required");

            ProxySettings.Proxy = TryGetProxy(proxyLogin, proxyPassword);
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

        public void AnalyzeAsync(List<ValkyrieFileInfo> badFiles, string login, string password, int machineCount, string apiKey)
        {
            try
            {
                _machineCount = machineCount;
                _apiKey = apiKey;

                Trace.WriteLine("API_KEY = " + apiKey);
                Trace.Flush();
                OnAnalyzeProgressChanged();
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

                _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();


                Task.Factory.StartNew(t =>
                {
                    try
                    {
                        SubmitToAutomaticAnalysis(badFiles, login, password, _cancellationTokenSource.Token);
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

        //public SubmitToManualResponse SubmitToManual(string filePath, string apiKey, int priority = 3)
        //{
        //    FileInfo f = new FileInfo(filePath);

        //    var sha1 = HashCalculator.ComputeSHA1Hash(filePath);


        //    if (f.Length > MaxFileLenght)
        //    {
        //        throw new InvalidOperationException(String.Format("{0}{1}", sha1, "Maximum file size per upload to 25 MB."));

        //    }

        //    var info = ValkyrieWebApiProvider.GetBasicInfo(new InfoRequest() { ApiKeys = apiKey, SHA1 = sha1 });

        //    if (info.Presence)
        //    {
        //        return ValkyrieWebApiProvider.SubmitToManualAnalysisWithoutUpload(new SubmitToManualRequestSHA1()
        //        {
        //            ApiKeys = apiKey,
        //            FilePath = filePath,
        //            Priority = priority,
        //            SHA1 = sha1,
        //            SubmitToken = info.SubmitToken
        //        });
        //    }
        //    else
        //    {
        //        return ValkyrieWebApiProvider.SubmitToManualAnalysis(new SubmitToManualRequest()
        //        {
        //            ApiKeys = apiKey,
        //            FilePath = filePath,
        //            Priority = priority,
        //            SubmitToken = info.SubmitToken
        //        }, filePath, CancellationToken.None);
        //    }

        //}


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

        private void SubmitToAutomaticAnalysis(List<ValkyrieFileInfo> badFiles, string login, string password, CancellationToken cancellationToken, bool parallel = true)
        {
            FilesCount = badFiles.Count;
            Guid endpointId = GetEndpointId();
            var destFolder = CreateTmpFolder();
            var exceptions = new ConcurrentQueue<Exception>();
            if (parallel)
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
                        var filePath = GetRemoteFile(f.Path, f.Sha1, login, password, f.Ip, destFolder);
                        var orgPath = f.GetFullPath();
                        UploadFile(filePath, orgPath, endpointId);
                        File.Delete(filePath);
                    }
                    catch (Exception e)
                    {

                        if (e is WebException && e.Message == "The request was aborted: The request was canceled.")
                        {
                            AddResult(ValkyrieAnalysisResult.CreateSkipInfo(f.Sha1, "The request was canceled by user", SkippReasonEnum.Cancelled));
                        }
                        else
                        {
                            AddResult(ValkyrieAnalysisResult.CreateSkipInfo(f.Sha1, e.Message, SkippReasonEnum.Failed));
                        }

                        Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Submit Failed SHA1 = {1} Reason : {2} {3}", DateTime.UtcNow, f.Sha1, e.Message, e.InnerException != null ? e.InnerException.Message : string.Empty));
                        Trace.Flush();

                        var exp = new InvalidOperationException("Could not upload file " + f.Path, e);
                        exceptions.Enqueue(exp);
                    }
                });


                var ex = new AggregateException(exceptions.ToArray());
                if (exceptions.Count > 0) throw ex;
            }
            else
            {
                foreach (var f in badFiles)
                {
                    var tmpfilePath = GetRemoteFile(f.Path, f.Sha1, login, password, f.Ip, destFolder);
                    UploadFile(tmpfilePath, f.GetFullPath(), endpointId);
                }
            }
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

        private int _machineCount;
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
            ValkyrieWebApiProvider.EndScan(new EndScanRequest(_apiKey, _machineCount));
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
                var malwareCount = AnalysisResults.Count(f => f.Verdict == InfoVerdictEnum.Malware);
                double progress = 0;

                if (FilesCount > 0)
                {
                    progress = (_fileScanned / (double)FilesCount) * 100;

                }
                var progressInt = Convert.ToInt32(progress);

                tmp(this, new ProgressChangedEventArgs(progressInt, new ProgressInfo<ValkyrieAnalysisResult>(FilesCount, _fileScanned, malwareCount, AnalysisResults.ToArray())));
            }
        }

        private void UploadFile(string filePath, string originalFilePath, Guid endpointId)
        {
            try
            {
                var sha1 = HashCalculator.ComputeSHA1Hash(filePath);

                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    AddResult(ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Cancelled by user.", SkippReasonEnum.Cancelled));
                    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : File {1} cancelled by user", DateTime.UtcNow,
                        originalFilePath));
                    Trace.Flush();
                    return;

                }

                FileInfo f = new FileInfo(filePath);


                if (f.Length > MaxFileLenght)
                {
                    AddResult(ValkyrieAnalysisResult.CreateSkipInfo(sha1, "Maximum file size per upload to 25 MB.", SkippReasonEnum.Skipped));
                    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : File {1} greater than 25 Mb", DateTime.UtcNow,
                        originalFilePath));
                    Trace.Flush();
                    return;
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
                    ValkyrieAnalysisResult result = ValkyrieAnalysisResult.Create(basicInfo);
                    AddResult(result);
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
                    var result = ValkyrieAnalysisResult.Create(response);
                    AddResult(result);
                }
            }
            //catch (Exception e)
            //{ 
            //    Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss} : {1}", DateTime.UtcNow, e.Message));
            //    Trace.Flush();
            //    throw e;
            //}
            finally
            {

                File.Delete(filePath);
                Trace.WriteLine(string.Format("{0:MM/dd/yy H:mm:ss.ffff} : Delete {1}", DateTime.UtcNow, filePath));
                Trace.Flush();

            }
        }

        private void AddResult(ValkyrieAnalysisResult result)
        {
            AnalysisResults.Add(result);
            Interlocked.Increment(ref _fileScanned);
            Task.Factory.StartNew(OnAnalyzeProgressChanged, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public string GetValkyrieDetailsLink()
        {
            return string.Format(@"{0}signup_apikey/{1}", ValkyrieApiUrls.GetBaseUri(), _apiKey);
        }
    }
}
