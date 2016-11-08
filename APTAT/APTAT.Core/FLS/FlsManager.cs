using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace APTAT.Core.FLS
{
    public class FlsManager
    {
        private FlsApiProvider _apiProvider = null;
        private Action _cancelWork = null;
        private ConcurrentBag<FlsResponse> _results = new ConcurrentBag<FlsResponse>();

        public void InitFlsClient(string flsHostName = "fls.security.comodo.com", short port = 4448)
        {
            IPHostEntry ip = Dns.GetHostEntry(flsHostName);
            if (!ip.AddressList.Any()) throw new InvalidOperationException("Could not resolve host " + flsHostName);
            _apiProvider = new FlsApiProvider(ip.AddressList[0], port);
        }
        
        public async Task AnalyzeAsync(string[] hashes, Action<double, FlsResponse> progressReport, Action<FlsResponse[]> notifyCompleted, Action<Exception> handleError, CancellationToken cancellationToken)
        {
            string[][] hashParts = Split(hashes, 20).ToArray();
            foreach (var h in hashParts)
            {
                await AnalyzeWithFlsAsync(h, progressReport, notifyCompleted, handleError, cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }

        private async Task AnalyzeWithFlsAsync(string[] hashs, Action<double, FlsResponse> progressReport, Action<FlsResponse[]> notifyCompleted, Action<Exception> handleError, CancellationToken cancellationToken)
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
                        DoWorkItem(hashs, item, cancellationToken, progressReport, loopState));
                    // observe cancellation
                    cancellationToken.ThrowIfCancellationRequested();
                }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);


                await task.ContinueWith(_ =>
                {
                    try
                    {
                        task.Wait(cancellationToken); // rethrow any error
                    }
                    catch (Exception ex)
                    {
                        while (ex is AggregateException && ex.InnerException != null)
                            ex = ex.InnerException;
                        handleError(ex);
                    }
                    notifyCompleted(_results.ToArray());
                }, TaskScheduler.Default);
                await task;
            }
            catch (Exception ex)
            {
                handleError(ex);
                notifyCompleted(_results.ToArray());
            }
        }

        private void DoWorkItem(string[] hashes, int item, CancellationToken token, Action<Double, FlsResponse> progressReport, ParallelLoopState loopState)
        {
            if (token.IsCancellationRequested)
            {
                loopState.Stop();
                return;
            }

            var result = _apiProvider.RequestType0(hashes[item]);
            _results.Add(result);
            var progress = _results.Count / (double) hashes.Length;
            progressReport(progress, result);
        }
        private static IEnumerable<T[]> Split<T>(T[] array, int size)
        {
            for (var i = 0; i < (float) array.Length / size; i++)
            {
                yield return array.Skip(i * size).Take(size).ToArray();
            }
        }
    }
}


