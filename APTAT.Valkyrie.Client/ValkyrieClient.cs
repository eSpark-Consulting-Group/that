using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Valkyrie.Client.Contracts;
using APTAT.Valkyrie.Client.Enums;
using APTAT.Valkyrie.Client.Helpers;
using APTAT.Valkyrie.Client.Models;
using Newtonsoft.Json;

namespace APTAT.Valkyrie.Client
{

    public sealed class ValkyrieClient
    {
        private const Int32 MaxFileLenght = 26214400;
        private string _apiKey;
        private DateTime _dateStart;
        private DateTime _dateEnd;
        private readonly string _clientId;
        private int _fileScanned;
        public int FileScanned
        {
            get
            {
                return _fileScanned;
            }
        }

        public int FilesCount { get; private set; }

        private ConcurrentBag<AnalysisResult> _analysisResults;
        public ConcurrentBag<AnalysisResult> AnalysisResults
        {
            get
            {
                return _analysisResults;
            }

            set
            {
                _analysisResults = value;
            }
        }

        public ValkyrieClient(string cientId)
        {
            AnalysisResults = new ConcurrentBag<AnalysisResult>();

            _clientId = cientId;
            
        }

        private CancellationTokenSource _cancellationTokenSource;
        private TaskScheduler _taskScheduler;

        public void AnalyzeAsync(List<ValkyrieFileInfo> badFiles, string login, string password, int machineCount)
        {
            try
            {
                _machineCount = machineCount;
                _dateStart = DateTime.UtcNow;

                var regInfo = ValkyrieService.RegisterWithMachineId(new RegisterWithMachineIdRequest
                {
                    MachineId = NetAddressHelper.GetMacWithIPAddress()
                });

                if (regInfo.ReturnCode != 0) throw new InvalidOperationException("Could not Register with Machine ID " + new RegisterWithMachineIdRequest
                {
                    MachineId = Guid.NewGuid().ToString()
                }.MachineId);

                _apiKey = regInfo.ApiKey;

                Thread.Sleep(2000);
                ValkyrieService.StartScan(new StartScanRequest(_apiKey));

                AnalysisResults = new ConcurrentBag<AnalysisResult>();
                _fileScanned = 0;

                _cancellationTokenSource = new CancellationTokenSource();

                var cancellationToken = _cancellationTokenSource.Token;

                cancellationToken.Register(() =>
                {
                    var tmp = AnalyzeCanceled;
                    if (tmp != null) tmp(this, EventArgs.Empty);
                });

                _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();


                Task.Factory.StartNew(t =>
                {
                    try
                    {
                        SubmitToAutomaticAnalysis(_clientId, badFiles, login, password);
                    }
                    catch (AggregateException exception)
                    {
                        OnAnalyzeCompleted(exception);
                    }

                }, TaskCreationOptions.None, cancellationToken
                    ).ContinueWith(OnAnalyzeCompleted, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, _taskScheduler);



            }
            catch (Exception exception)
            {
                var tmp = AnalyzeCompleted;
                if (tmp != null)
                {
                    var agrEx = new AggregateException(new[] {exception});
                    tmp(this, new AnalyzeCompletedEventArgs<AnalysisResult>(AnalysisResults.ToArray(), agrEx, false, null));
                }
              // OnAnalyzeCompleted(exception);
                //throw;
            }
        }

        private string GetRemoteFile(string fileName, string sha1, string login, string password, string ipAddress, string destPath)
        {
            if (fileName == null) throw new ArgumentNullException("fileName");
            
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe"
            };
            var host = "\\\\" + ipAddress; //  Items.First(i => i.TempItems.Any(ti => ti.BadFile.Sha1.Equals(badFile.Sha1))).IpAddress;
            var target = host + "\\" + Path.GetPathRoot(fileName).Replace(":\\", "$");
            string destination = Path.Combine(destPath, Path.GetFileName(fileName));
            
            string source = target + fileName.Remove(0, 2);
            startInfo.Arguments = string.Format("/C net use \"{0}\" {1} /user:{2} & copy \"{3}\" {4}", target, password, login, source, destination);

            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            return destination;
        }

        private static string CreateTmpFolder()
        {
            var usrTempFolder = Environment.GetEnvironmentVariable("TEMP");
            if (string.IsNullOrWhiteSpace(usrTempFolder))
            {
                usrTempFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            var valkyrieFolder = Path.Combine(usrTempFolder, "ValkyrieTmp");
            bool exists = Directory.Exists(valkyrieFolder);
            if (!exists)
            {
                Directory.CreateDirectory(valkyrieFolder);
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

        private void SubmitToAutomaticAnalysis(string clientId, List<ValkyrieFileInfo> badFiles, string login, string password, bool parallel = true)
        {
            FilesCount = badFiles.Count;

            var destFolder = CreateTmpFolder();
            var exceptions = new ConcurrentQueue<Exception>();
            if (parallel)
            {
                Parallel.ForEach(badFiles, f =>
                {

                    try
                    {
                        var filePath = GetRemoteFile(f.Path, f.Sha1, login, password, f.Ip, destFolder);
                        UploadFile(clientId, filePath);
                    }
                    catch (Exception e)
                    {
                        AddResult(AnalysisResult.CreateSkipInfo(f.Path, e.Message));
                        exceptions.Enqueue(e);
                    }
                });

                var ex = new AggregateException(exceptions.ToArray());
                if (exceptions.Count > 0) throw ex;
            }
            else
            {
                foreach (var f in badFiles)
                {
                    var filePath = GetRemoteFile(f.Path, f.Sha1, login, password, f.Ip, destFolder);
                    UploadFile(clientId, filePath);

                }
            }


        }

        public void AnalyzeAsyncCancel()
        {
            _cancellationTokenSource.Cancel();
        }


        public event AnalyzeCompletedEventHandler<AnalysisResult> AnalyzeCompleted;
        public event ProgressChangedEventHandler AnalyzeProgressChanged;
        public event EventHandler AnalyzeCanceled;
        private int _machineCount;

        private void OnAnalyzeCompleted(Exception exception)
        {
            
            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                Task.Factory.StartNew(
                
                    () =>
                    {
                        tmp(this,
                            new AnalyzeCompletedEventArgs<AnalysisResult>(AnalysisResults.ToArray(), exception, true,
                                null));
                    },CancellationToken.None,TaskCreationOptions.None, _taskScheduler);
            }

        }
        private void OnAnalyzeCompleted(Task task)
        {
            _dateEnd = DateTime.UtcNow;
            ValkyrieService.EndScan(new EndScanRequest(_apiKey, _machineCount));

            if (task.Exception != null) throw task.Exception;
            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                tmp(this, new AnalyzeCompletedEventArgs<AnalysisResult>(AnalysisResults.ToArray(), task.Exception, task.IsCanceled, task.AsyncState));
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

                tmp(this, new ProgressChangedEventArgs(progressInt, new ProgressInfo<AnalysisResult>(FilesCount, _fileScanned, malwareCount, AnalysisResults.ToArray())));
            }
        }

        private void UploadFile(string clientId, string filePath)
        {            
            var sha1 = HashCalculator.ComputeSHA1Hash(filePath);
            FileInfo f = new FileInfo(filePath);

            if (f.Length > MaxFileLenght)
            {
                AddResult(AnalysisResult.CreateSkipInfo(sha1, "Maximum file size per upload to 25 MB."));
                return;
            }

            var infoRequeast = new InfoRequest { Client_id = clientId, SHA1 = sha1, ApiKey = _apiKey };
            BasicInfoResponse basicInfo = ValkyrieService.GetBasicInfo(infoRequeast);
            if(basicInfo.ReturnCode != ErrorCodesEnum.Success) throw  new InvalidOperationException(basicInfo.ReturnCode.GetEnumDescription());
            if (basicInfo.Presence)
            {
                AnalysisResult result = AnalysisResult.Create(basicInfo);
                AddResult(result);
            }
            else
            {
                var request = new SubmitToAutomaticRequest
                {
                    Client_id = clientId,
                    ApiKey = _apiKey,
                    SubmitToken = basicInfo.SubmitToken,
                    FilePath = filePath
                };
                var payload = request.ToNameValueCollection();

                var uploadResult = MultipartPostHelper.Post(request.FilePath, ValkyrieApiUrls.SubmitToAutomaticAnalysis.AbsoluteUri, payload);
                var response = JsonConvert.DeserializeObject<SubmitToAutomaticResponse>(uploadResult);
                if (response.ReturnCode != ErrorCodesEnum.Success) throw new InvalidOperationException(response.ReturnCode.GetEnumDescription());
                var result = AnalysisResult.Create(response);
                AddResult(result);
            }
        }

        private void AddResult(AnalysisResult result)
        {
            AnalysisResults.Add(result);
            Interlocked.Increment(ref _fileScanned);
            Task.Factory.StartNew(OnAnalyzeProgressChanged, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        public string GetValkyrieDetailsLink()
        {
            return ValkyrieApiUrls.GetBaseUri() + @"signup_apikey/" + _apiKey;
        }
    }
}
