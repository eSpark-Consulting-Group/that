using System;
using SystemTimer = System.Timers.Timer;

namespace APTAT.Common
{
    public class Timer : ITimer
    {
        public event EventHandler TimeoutCallback;

        public bool IsEnabled { get { return _timer.Enabled; } }

        private readonly SystemTimer _timer;
        public Timer()
        {
            _timer = new SystemTimer();
            _timer.Elapsed += TimerCallback;
        }
        public bool Start(bool autoReset = true)
        {
            if (_timer.Enabled) return false;
            
            _timer.AutoReset = autoReset;
            _timer.Start();
            return true;
        }

        public void SetInverval(TimeSpan period)
        {
            _timer.Interval = period.TotalMilliseconds;
        }
        public void Stop()
        {
            _timer.Stop();
        }


        private void TimerCallback(object state, EventArgs args)
        {
            TimeoutCallback?.Invoke(this, args);
        }

        public void Dispose()
        {
            _timer.Elapsed -= TimerCallback;
            _timer.Dispose();
        }



    }
}
