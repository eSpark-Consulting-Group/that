using System;
using System.Threading;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public interface ITimer : IDisposable
    {
        event EventHandler TimeoutCallback;
        void Start(TimeSpan dueTime, TimeSpan period);
        void Stop();
    }

    public sealed class Timer : ITimer
    {
        private readonly System.Threading.Timer _timer;
        private TimeSpan _dueTime;
        private TimeSpan _period;
        public event EventHandler TimeoutCallback;

        public Timer()
        {
            _timer = new System.Threading.Timer(TimerCallback);
        }

        public void Start(TimeSpan dueTime, TimeSpan period)
        {
            _timer?.Change(dueTime,period);
        }

        public void Stop()
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void TimerCallback(object state)
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            OnTimeoutCallback();
            _timer?.Change(TimeSpan.Zero, _period);
        }

        private void OnTimeoutCallback()
        {
            TimeoutCallback?.Invoke(this, EventArgs.Empty);
        }
    }
}
