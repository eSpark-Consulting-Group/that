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

namespace APTAT.Valkyrie.Client
{

    public sealed class ValkyrieClient
    {
        private int _fileScanned;

        public event EventHandler<EventArgs> ScanCompleted;
        public event EventHandler<InProgressEventArgs> InProgress;
       
        private void OnInProgress()
        {
            var tmp = InProgress;
            if (tmp != null)
            {
                double progress = 0;
                if (FilesCount > 0)
                {
                    progress = ((double)_fileScanned / (double)FilesCount) * 100;

                }
                tmp(this, new InProgressEventArgs() { Percentage = Convert.ToInt32(progress), TotalFileCount = FilesCount, ProcessedFileCount = _fileScanned });
            }
        }
        public int FilesCount { get; private set; }

        public int FileScanned
        {
            get
            {
                return _fileScanned;
            }

            set
            {
                _fileScanned = value;
            }
        }

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


        private ConcurrentBag<AnalysisResult> _analysisResults;

        public ValkyrieClient(string cientID)
        {
            AnalysisResults = new ConcurrentBag<AnalysisResult>();
            _clientID = cientID;
        }
        const Int32 maxFileLenght = 26214400;
        private string _clientID;
        private string _apiKey = "B5191D23-7BB3-4871-9094-9416D6025C7A";


        public void AskValkyrie(List<ValkyrieFileInfo> badFiles, string login, string password)
        {
            var folderPath = RemoteFileCopyHelper.CopyFiles(badFiles, login, password);
            SubmitFolderToAutomatic(_clientID, folderPath);
        }

        public void SubmitFolderToAutomatic(string clientID, string folderPath)
        {

            var exceptions = new ConcurrentQueue<Exception>();

            var files = Directory.EnumerateFiles(folderPath);
            FilesCount = files.Count();

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


            //foreach (var filePath in files)
            //{
            //    UploadFile(clientID, filePath);
            //}


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

            var infoRequeast = new InfoRequest() { Client_id = clientID, SHA1 = sha1 , ApiKey = _apiKey};
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
                request.ApiKey = _apiKey;
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
            OnInProgress();
        }
    }
}
