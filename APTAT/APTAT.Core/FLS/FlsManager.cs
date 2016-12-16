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
        private readonly BlockingCollection<string> _inputQueue = new BlockingCollection<string>();
        public event EventHandler<FlsRequestEventArgs> FlsAnalyzed;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isWorking = false;
        private readonly int _uploadInterval;
        private readonly int _uploadAtOnce;

        private static void ClearCollection<T>(BlockingCollection<T> blockingCollection)
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

        private void DoWorkItem(string[] hashes)
        {
            try
            {
                FlsResponse response = _apiProvider.RequestType0(hashes);

                if (_cancellationToken.IsCancellationRequested)
                    return;
                var results = response.GetAnalyzed();
                if (results.Any())
                    OnFlsAnalyzed(results, null);



                var failed = response.GetFailedToAnalyzeHashes();
                if (failed.Any())
                {
                    if (_cancellationToken.IsCancellationRequested)
                        return;
                    Enqueue(failed.ToArray(), false);
                }
            }
            catch (Exception)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;
                Enqueue(hashes, false);
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

        public void Enqueue(IEnumerable<string> hashes, bool startIfCanceled = true)
        {
            if (!_isWorking)
            {
                if (!startIfCanceled && _cancellationToken.IsCancellationRequested)
                    return;

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;
               
                Task.Run(() => { Start(); }, _cancellationToken);
            }

            foreach (var h in hashes)
            {
                _inputQueue.Add(h, _cancellationToken);
            }
        }

        public void Cancel()
        {
            
            if (_cancellationTokenSource != null && _cancellationTokenSource.Token.CanBeCanceled)
            {
                _cancellationTokenSource.Cancel();
            }
            else
            {
                return;
            }

            _isWorking = false;
            if (_inputQueue != null)
            {
                ClearCollection(_inputQueue);
            }
            
        }


        private void Start()
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
                        AnalyzeAsync(buffer.ToArray());
                        buffer.Clear();
                    }

                    _cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(_uploadInterval));
                    //Thread.Sleep(TimeSpan.FromSeconds(_uploadInterval));
                    //Task.Delay(TimeSpan.FromSeconds(_uploadInterval), _cancellationToken).Wait();
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

        private Task AnalyzeAsync(string[] hashes)
        {

            try
            {
                // offload Parallel.For from the UI thread 
                // as a long-running operation
                Task task = Task.Factory.StartNew(() =>
               {

                   string[][] hashGroups = Split(hashes, _uploadAtOnce).ToArray();
                   foreach (var h in hashGroups)
                   {
                       if (_cancellationToken.IsCancellationRequested)
                           return;
                       DoWorkItem(h);
                   }                   
                   _cancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                   _cancellationToken.ThrowIfCancellationRequested();
               }, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                task.ContinueWith(_ =>
                {
                    try
                    {
                        task.Wait(_cancellationToken); // rethrow any error
                    }
                    catch (Exception ex)
                    {
                        while (ex is AggregateException && ex.InnerException != null)
                            ex = ex.InnerException;
                        OnFlsAnalyzed(new IFlsResult[0], ex);
                    }

                }, TaskScheduler.Default);
                return task;
            }
            catch (Exception ex)
            {
                OnFlsAnalyzed(new IFlsResult[0], ex);
                return Task.FromException(ex);
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
            if (_cancellationToken.IsCancellationRequested)
                return;
            var e = new FlsRequestEventArgs(results, exception);
            FlsAnalyzed?.Invoke(this, e);
        }
    }
}