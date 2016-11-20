using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Core.Properties;


namespace APTAT.Core
{

    public sealed class FlsManager : IDisposable, IFlsManager
    {
        
        private CancellationToken _cancellationToken;
        private readonly FlsApiProvider _apiProvider = null;
        private Action _cancelWork = null;
      //  private ConcurrentBag<FlsResult> _allResults = new ConcurrentBag<FlsResult>();
        private readonly BlockingCollection<string> _inputQueue = new BlockingCollection<string>();
        public event EventHandler<FlsRequestEventArgs> FlsAnalyzed;
        private  CancellationTokenSource _cancellationTokenSource;
        private bool _isWorking = false;
        private readonly int _uploadInterval;
        private readonly int _uploadAtOnce;
        
        public static void ClearCollection<T>( BlockingCollection<T> blockingCollection)
        {
            if (blockingCollection == null)
            {
                throw new ArgumentNullException(nameof(blockingCollection));
            }

            while (blockingCollection.Count > 0)
            {
                T item;
                blockingCollection.TryTake(out item);
            }
        }

        private void DoWorkItem(string[] hashes )
        {
            try
            {
                FlsResponse response = _apiProvider.RequestType0(hashes);
                var results = response.GetAnalyzed();
                OnFlsAnalyzed(results, null);
                
           //     foreach (var r in results){_allResults.Add(r);}

                var failed = response.GetFailedToAnalyzeHashes();
                if (failed.Any())
                {
                    Enqueue(failed.ToArray());
                }
            }
            catch (Exception)
            {
                Enqueue(hashes);
                //throw;
            }
     //       Task.Delay(TimeSpan.FromMinutes(1));

         //   Thread.Sleep(30000); //TODO: Remove after test
        }

        public FlsManager()
        {
            _apiProvider = new FlsApiProvider();
            _apiProvider.Init();
            _uploadInterval = int.Parse(Settings.Default.FlsUploadInterval);
            _uploadAtOnce = int.Parse(Settings.Default.FlsUploadHashesAtOnce);
        }

        

        public void UpdateFlsPorts(int udpPort, int tcpPort)
        {
           _apiProvider.UpdateFlsPorts(udpPort: udpPort, tcpPort: tcpPort);
        }

        public async Task Enqueue(IEnumerable<string> hashes)
        {
            //var analyzed = _allResults.Select(r => r.Sha1).ToList();
            //var result = hashes.Where(p => !analyzed.Any(p2 => p2 == p));

            if (!_isWorking)
            {
        //        _allResults = new ConcurrentBag<FlsResult>();
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;
                Task.Run(Start, _cancellationToken);
            }
            await Task.Factory.StartNew(() =>
            {
                foreach (var h in hashes)
                {
                    _inputQueue.Add(h, _cancellationToken);
                }
                //          _inputQueue.CompleteAdding();
            }, _cancellationToken);

        }

        public void Cancel()
        {
            if (_cancellationTokenSource != null && _cancellationTokenSource.Token.CanBeCanceled)
            {
                _cancellationTokenSource.Cancel();
            }
            _isWorking = false;

            ClearCollection(_inputQueue);
            //FlsResult someItem;
            //while (!_allResults.IsEmpty)
            //{
            //    _allResults.TryTake(out someItem);
            //}
        }


        private async Task Start()
        {
            try
            {
                if (_isWorking) throw new InvalidOperationException("FLSManager is working");
                _isWorking = true;
                
                var buffer = new List<String>();

                while (true)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    for (int i = 0; i < _inputQueue.Count; i++)
                    {
                        var hash = _inputQueue.Take();
                        buffer.Add(hash);
                    }
                    if (buffer.Any())
                    {
                        await AnalyzeAsync(buffer.ToArray());
                        buffer.Clear();
                    }
                    await Task.Delay(TimeSpan.FromSeconds(_uploadInterval), _cancellationToken);
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                _isWorking = false;
            }

        }
        
        private async Task AnalyzeAsync(string[] hashes)
        {
         
            try
            {
                // prepare to handle cancellation
                var cts = new CancellationTokenSource();
                
                _cancelWork = () =>
                {
                    cts.Cancel();
                };
                
                // offload Parallel.For from the UI thread 
                // as a long-running operation
                Task task = Task.Factory.StartNew(async () =>
                {

                    string[][] hashGroups = Split(hashes, _uploadAtOnce).ToArray();
                    foreach (var h in hashGroups)
                    {
                        DoWorkItem(h);
                    }
                    await Task.Delay(3000);
                    _cancellationToken.ThrowIfCancellationRequested();
                }, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
                
                await task.ContinueWith(_ =>
                {
                    try
                    {
                        task.Wait(_cancellationToken); // rethrow any error
                    }
                    catch (Exception ex)
                    {
                        while (ex is AggregateException && ex.InnerException != null)
                            ex = ex.InnerException;
                        OnFlsAnalyzed(new IFlsResult[] {}, ex);
                    }

                }, TaskScheduler.Default);
                await task;
            }
            catch (Exception ex)
            {
                OnFlsAnalyzed(new IFlsResult[] { }, ex);
            }
        }

        private static IEnumerable<T[]> Split<T>(T[] array, int size)
        {
            for (var i = 0; i < (float) array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size).ToArray();
            }
        }

        public void Dispose()
        {
            _inputQueue.CompleteAdding();
            _inputQueue.Dispose();
        }

        private void OnFlsAnalyzed(IFlsResult[] results, Exception exception)
        {
            var e = new FlsRequestEventArgs(results, exception);
            FlsAnalyzed?.Invoke(this, e);
        }
    }
}