using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace APTAT.Core
{
    
    public sealed class FlsManager : IDisposable, IFlsManager
    {
        
        private readonly CancellationToken _cancellationToken;
        private readonly FlsApiProvider _apiProvider = null;
        private Action _cancelWork = null;
        private ConcurrentBag<FlsResult> _allResults = new ConcurrentBag<FlsResult>();
        private readonly BlockingCollection<string> _inputQueue = new BlockingCollection<string>();
        public event EventHandler<FlsRequestEventArgs> FlsAnalyzed;
        private void DoWorkItem(string[] hashes )
        {

            try
            {
                FlsResponse response = _apiProvider.RequestType0(hashes);
                var results = response.GetAnalyzed();
                OnFlsAnalyzed(results, null);
                
                foreach (var r in results){_allResults.Add(r);}
                Enqueue(response.GetFailedToAnalyzeHashes());
            }
            catch (Exception)
            {
                
                throw;
            }

            //Thread.Sleep(1000); //TODO: Remove after test
        }

        public FlsManager()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = cancellationTokenSource.Token;
            _apiProvider = new FlsApiProvider();
            _apiProvider.Init();
        }

        public async Task Enqueue(IEnumerable<string> hashes)
        {
            if (!_isWorking) Task.Run(Start, _cancellationToken);
            await Task.Factory.StartNew(() =>
            {
                foreach (var h in hashes)
                {
                    _inputQueue.Add(h, _cancellationToken);
                }
                //          _inputQueue.CompleteAdding();
            }, _cancellationToken);

        }

        private bool _isWorking = false;
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
                    await Task.Delay(TimeSpan.FromSeconds(1), _cancellationToken);
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
            _allResults = new ConcurrentBag<FlsResult>();
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

                    string[][] hashGroups = Split(hashes, 20).ToArray();
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
                        OnFlsAnalyzed(new FlsResult[] {}, ex);
                    }

                }, TaskScheduler.Default);
                await task;
            }
            catch (Exception ex)
            {
                OnFlsAnalyzed(new FlsResult[] { }, ex);
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

        private void OnFlsAnalyzed(FlsResult[] results, Exception exception)
        {
            var e = new FlsRequestEventArgs(results, exception);
            FlsAnalyzed?.Invoke(this, e);
        }
    }
}