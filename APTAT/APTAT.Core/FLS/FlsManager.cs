using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace APTAT.Core.FLS
{

    public class FlsManager : IDisposable
    {
        private readonly Action<double, FlsResponse> _progressReport;
        private readonly Action _notifyCompleted;
        private readonly Action<Exception> _handleError;
        private readonly CancellationToken _cancellationToken;
        private readonly FlsApiProvider _client = null;
        private Action _cancelWork = null;
        private ConcurrentBag<FlsResponse> _results = new ConcurrentBag<FlsResponse>();
        private readonly BlockingCollection<string> _inputQueue = new BlockingCollection<string>();

        private void DoWorkItem(string[] hashes, int item, CancellationToken token,
            Action<Double, FlsResponse> progressReport, ParallelLoopState loopState)
        {
            if (token.IsCancellationRequested)
            {
                loopState.Stop();
                return;
            }

            var result = _client.RequestType0(hashes[item]);
            _results.Add(result);
            var progress = _results.Count / (double) hashes.Length;
            progressReport(progress, result);
        }

        public FlsManager(IPAddress flsIp, short flsPort,
            Action<double, FlsResponse> progressReport,
            Action notifyCompleted,
            Action<Exception> handleError,
            CancellationToken cancellationToken)
        {
            _progressReport = progressReport;
            _notifyCompleted = notifyCompleted;
            _handleError = handleError;
            _cancellationToken = cancellationToken;
            _client = new FlsApiProvider(flsIp, flsPort);
        }
        

        public void Add(IEnumerable<string> hashes)
        {
            Task.Factory.StartNew(() =>
            {
                foreach (var h in hashes)
                {
                    _inputQueue.Add(h, _cancellationToken);
                }
                // _inputQueue.CompleteAdding();
            }, _cancellationToken);

        }

        private bool _isWorking = false;


        public async Task Start()
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
                    await Task.Delay(TimeSpan.FromSeconds(5), _cancellationToken);
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

        public async Task AnalyzeAsync(string[] hashes)
        {
            string[][] hashParts = Split(hashes, 20).ToArray();
            foreach (var h in hashParts)
            {
                await AnalyzeWithFlsAsync(h);
                await Task.Delay(TimeSpan.FromSeconds(5), _cancellationToken);
            }
        }

        private async Task AnalyzeWithFlsAsync(string[] hashs)
        {
            _results = new ConcurrentBag<FlsResponse>();
            try
            {
                // prepare to handle cancellation
                var cts = new CancellationTokenSource();


                _cancelWork = () =>
                {
                    cts.Cancel();
                };

                int total = hashs.Length;

                // offload Parallel.For from the UI thread 
                // as a long-running operation
                Task task = Task.Factory.StartNew(() =>
                {
                    Parallel.For(0, total, (item, loopState) =>
                        DoWorkItem(hashs, item, _cancellationToken, _progressReport, loopState));
                    // observe cancellation
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
                        _handleError(ex);
                    }
                    _notifyCompleted();
                }, TaskScheduler.Default);
                await task;
            }
            catch (Exception ex)
            {
                _handleError(ex);
                _notifyCompleted();
            }
        }
        public static IEnumerable<T[]> Split<T>(T[] array, int size)
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
    }
}


