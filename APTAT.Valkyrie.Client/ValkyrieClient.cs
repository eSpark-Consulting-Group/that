using APTAT.Valkyrie.Client.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Valkyrie.Client.Helpers;
using System.ComponentModel;
using APTAT.Valkyrie.Client.Models;

namespace APTAT.Valkyrie.Client
{

    public sealed class ValkyrieClient
    {
        private const Int32 maxFileLenght = 26214400;        
        private const string ApiKey = "d552623a-47d5-4dab-81d8-039f596c241f";
        
        private string _clientID;
        private int _fileScanned;
        public int FileScanned
        {
            get
            {
                return _fileScanned;
            }
            private set
            {
                _fileScanned = value;
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
        
        public ValkyrieClient(string cientID)
        {
            AnalysisResults = new ConcurrentBag<AnalysisResult>();
            _clientID = cientID;
        }     

        //public void AskValkyrie(List<ValkyrieFileInfo> badFiles, string login, string password)
        //{
        //    var folderPath = RemoteFileCopyHelper.CopyFiles(badFiles, login, password);
        //    SubmitFolderToAutomaticAnalysis(_clientID, folderPath);
        //}

        
        private CancellationTokenSource _cancellationTokenSource;
        private TaskScheduler _taskScheduler;
        public void AnalyzeAsync(string folderPath)
        {
            AnalyzeAsync(null, null, null, folderPath);
        }

        public void AnalyzeAsync(List<ValkyrieFileInfo> badFiles, string login, string password, string folderPath = null)
        {
            Guid taskId = Guid.NewGuid();
            AnalysisResults = new ConcurrentBag<AnalysisResult>();
            _fileScanned = 0;

            this._cancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = this._cancellationTokenSource.Token;
            cancellationToken.Register(() =>
            {
                var tmp = AnalyzeCanceled;
                if (tmp != null) tmp(this, EventArgs.Empty);
            });

            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            bool useBadFilesCollection = String.IsNullOrWhiteSpace(folderPath) && badFiles != null;
            var task = Task.Factory.StartNew((t) =>
                {
                    if (useBadFilesCollection)
                    {
                        folderPath = RemoteFileCopyHelper.CopyFiles(badFiles, login, password);
                    }
                    SubmitFolderToAutomaticAnalysis(_clientID, folderPath);
                }, TaskCreationOptions.LongRunning, cancellationToken
            )
                .ContinueWith(t =>
                {
                    OnAnalyzeCompleted(t);
                }, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, _taskScheduler);

            task.ContinueWith((t) => {
                var error = t.Exception;
            }, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted,_taskScheduler);

        }

        public void AnalyzeAsyncCancel()
        {
            this._cancellationTokenSource.Cancel();
        }
              
        public event AnalyzeCompletedEventHandler<AnalysisResult> AnalyzeCompleted;
        public event ProgressChangedEventHandler AnalyzeProgressChanged;
        public event EventHandler AnalyzeCanceled;

        private void OnAnalyzeCompleted(Task task)
        {
            if (task.Exception != null) throw task.Exception;
            var tmp = AnalyzeCompleted;
            if (tmp != null)
            {
                tmp(this, new AnalyzeCompletedEventArgs<AnalysisResult>(this.AnalysisResults.ToArray(), task.Exception, task.IsCanceled, task.AsyncState));
            }
            
        } 
        private void OnAnalyzeProgressChanged()
        {
            var tmp = AnalyzeProgressChanged;
            if (tmp != null)
            {
                var malwareCount = this.AnalysisResults.Where(f => f.Verdict == Enums.InfoVerdictEnum.Malware).Count();
                double progress = 0;

                if (FilesCount > 0)
                {
                    progress = ((double)_fileScanned / (double)FilesCount) * 100;

                }
                var progressInt = Convert.ToInt32(progress);

                tmp(this, new ProgressChangedEventArgs(progressInt, new ProgressInfo(FilesCount, _fileScanned, malwareCount)));
            }
        }


        private void SubmitFolderToAutomaticAnalysis(string clientID, string folderPath, bool useParallel = false)
        {
            var exceptions = new ConcurrentQueue<Exception>();

            var files = Directory.EnumerateFiles(folderPath).ToArray();
            FilesCount = files.Count();

            if (useParallel)
            {
                Parallel.ForEach(files, (filePath) =>
                {
                    try
                    {
                        UploadFile(clientID, filePath);
                    }
                    catch (Exception e)
                    {

                        AddResult(AnalysisResult.CreateSkipInfo(filePath, e.Message));
                        exceptions.Enqueue(e);
                    }
                });

                if (exceptions.Count > 0) throw new AggregateException(exceptions);

            }
            else
            {
                foreach (var filePath in files)
                {
                    Thread.Sleep(1000);
                    UploadFile(clientID, filePath);
                    //if (_cancellationTokenSource != null)
                    //{
                    //    if (_cancellationTokenSource.Token.IsCancellationRequested)
                    //    {
                    //        break;
                    //    }
                    //}
                }
            }
        }

        private void UploadFile(string clientID, string filePath)
        {
            var sha1 = HashCalculator.ComputeSHA1Hash(filePath);
            FileInfo f = new FileInfo(filePath);
            
            if (f.Length > maxFileLenght)
            {
                AddResult(AnalysisResult.CreateSkipInfo(sha1, "Maximum file size per upload to 25 MB."));
                return;
            }

            var infoRequeast = new InfoRequest() { Client_id = clientID, SHA1 = sha1, ApiKey = ApiKey };
            BasicInfoResponse basicInfo = ValkyrieService.GetBasicInfo(infoRequeast);

            if (basicInfo.Presence)
            {
                AnalysisResult result = AnalysisResult.Create(basicInfo);
                AddResult(result);
            }
            else
            {
                var request = new SubmitToAutomaticRequest();
                request.Client_id = clientID;
                request.ApiKey = ApiKey;
                request.SubmitToken = basicInfo.SubmitToken;
                request.FilePath = filePath;
                var payload = request.ToNameValueCollection();

                var uploadResult = MultipartPostHelper.Post(request.FilePath, ValkyrieApiUrls.SubmitToAutomaticAnalysis.AbsoluteUri, payload);
                var response = JsonConvert.DeserializeObject<SubmitToAutomaticResponse>(uploadResult);
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
    }
}
