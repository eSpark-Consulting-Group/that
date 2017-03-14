using System;
using System.Timers;

namespace APTAT.Common
{
    public class Scheduler : IScheduler
    {
        public event EventHandler TimeoutCallback;

        public bool IsEnabled { get { return _timer.Enabled; } }

        private readonly Timer _timer;
        public Scheduler()
        {
            _timer = new Timer();
            _timer.Elapsed += TimerCallback;
        }
        public bool Start(bool autoReset = true)
        {
            if (_timer.Enabled) return false;
            
            _timer.AutoReset = autoReset;
            _timer.Start();
            return true;
        }

        public void SetInterval(TimeSpan period)
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
