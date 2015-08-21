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

namespace APTAT.Valkyrie.Client
{

    public sealed class ValkyrieClientWithProxy
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

        public ValkyrieClientWithProxy(string clientID)
        {
            AnalysisResults = new ConcurrentBag<AnalysisResult>();
            _proxy = new ValkyrieProxy(clientID);
        }
        const Int32 maxFileLenght = 26214400;
        private ValkyrieProxy _proxy;

        public void UploadFilesToValkyrie(string clientID, IEnumerable<string> files)
        {
            var exceptions = new ConcurrentQueue<Exception>();

            
            FilesCount = files.Count();
            List<String> used = new List<string>();
            Parallel.ForEach(files, (filePath) =>
            {
                try
                {
                    var sha1 = HashCalculator.ComputeSHA1Hash(filePath);
                    if (used.Contains(sha1))
                    {
                        Thread.Sleep(100);
                    }
                    else
                    {
                        used.Add(sha1);
                    }

                    var result = _proxy.AskValkyrie(filePath, sha1);
                    AddResult(result);
                }
                catch (Exception e)
                {

                    AddResult(AnalysisResult.CreateSkipInfo(filePath, e.Message));
                    exceptions.Enqueue(e);
                }
            });

            if (exceptions.Count > 0) throw new AggregateException(exceptions);                    

        }


        private void AddResult(AnalysisResult result)
        {
            AnalysisResults.Add(result);
            Interlocked.Increment(ref _fileScanned);
            OnInProgress();
        }
    }
}
