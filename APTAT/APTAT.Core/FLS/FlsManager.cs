using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Core.FLS;
using APTAT.Core.Properties;
using APTAT.Common;

namespace APTAT.Core
{

    public sealed class FlsManager : IDisposable, IFlsManager
    {
        private CancellationToken _cancellationToken;
        private readonly IFlsApiProvider _apiProvider = null;
        private readonly IScheduler _timer = null;
        private readonly BlockingCollection<string> _inputQueue = new BlockingCollection<string>();
        public event EventHandler<FlsRequestEventArgs<IFlsResult>> FlsAnalyzed;
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
                IFlsResponse response = _apiProvider.RequestType0(hashes);

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
                    EnqueueInternal(failed.ToArray(), false);
                }
            }
            catch (Exception)
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;
                EnqueueInternal(hashes, false);
            }
        }

        public FlsManager(IFlsApiProvider apiProvider, IScheduler timer)
        {
            _timer = timer;
            _apiProvider = apiProvider;
            _uploadInterval = int.Parse(Settings.Default.FlsUploadInterval);
            _uploadAtOnce = int.Parse(Settings.Default.FlsUploadHashesAtOnce);
        }

        public void UpdateFlsPorts(int udpPort, int tcpPort)
        {
            _apiProvider.UpdateFlsPorts(udpPort: udpPort, tcpPort: tcpPort);
        }

        private void EnqueueInternal(IEnumerable<string> hashes, bool startIfCanceled)
        {
            bool shouldStart = false;
            if (!_isWorking)
            {
                if (!startIfCanceled && _cancellationToken.IsCancellationRequested)
                    return;

                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
                _cancellationToken = _cancellationTokenSource.Token;
                shouldStart = true;
            }
            foreach (var h in hashes)
            {
                _inputQueue.Add(h, _cancellationToken);
            }
            if (shouldStart)
                Start();


        }

        private void _timer_TimeoutCallback(object sender, EventArgs e)
        {
            var buffer = new List<String>();
            if (_cancellationToken.IsCancellationRequested)
            {
                return;
            }
            var count = _inputQueue.Count;
            for (int i = 0; i < count; i++)
            {
                var hash = _inputQueue.Take();
                buffer.Add(hash);
            }
            if (buffer.Any())
            {
                try
                {
                    string[][] hashGroups = Split(buffer.ToArray(), _uploadAtOnce).ToArray();
                    foreach (var h in hashGroups)
                    {
                        if (_cancellationToken.IsCancellationRequested)
                            return;
                        DoWorkItem(h);
                    }
                }
                catch (Exception ex)
                {
                    OnFlsAnalyzed(new IFlsResult[0], ex);
                }
                buffer.Clear();
            }
            _timer.Start(false);
        }

        public void Enqueue(IEnumerable<string> hashes)
        {
            EnqueueInternal(hashes, true);
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

            _timer.Stop();
        }


        private void Start()
        {            
            try
            {
                if (_isWorking) throw new InvalidOperationException("FLSManager is working");
                _isWorking = true;

                _timer.TimeoutCallback += _timer_TimeoutCallback;
                _timer.SetInterval(TimeSpan.FromSeconds(_uploadInterval));
                _timer.Start(false);
            }
            catch (Exception ex)
            {
                _isWorking = false;
                throw ex;
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
            _timer.Dispose();
        }

        private void OnFlsAnalyzed(IFlsResult[] results, Exception exception)
        {
            if (_cancellationToken.IsCancellationRequested)
                return;
            var e = new FlsRequestEventArgs<IFlsResult>(results, exception);
            FlsAnalyzed?.Invoke(this, e);
        }
    }
}