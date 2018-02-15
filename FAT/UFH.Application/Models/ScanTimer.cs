using System;
using System.Windows.Threading;

namespace UFH.Application.Models
{
    public class ScanTimer : IScanTimer
    {
        private readonly DispatcherTimer _timer;

        public ScanTimer()
        {
            _timer = new DispatcherTimer(DispatcherPriority.Normal, App.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher);
            _timer.Interval = TimeSpan.FromMilliseconds(500d);
            _timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ScanIterationHandler?.Invoke();
        }

        public Action ScanIterationHandler { private get; set; }

        public bool ScanActive
        {
            get { return _timer.IsEnabled; }
        }

        public bool Start()
        {
            if (!_timer.IsEnabled)
            {
                _timer.Start();
                return true;
            }

            return false;
        }

        public bool Stop()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                return true;
            }

            return false;
        }
    }
}
