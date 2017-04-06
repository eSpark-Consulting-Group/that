using APTAT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Timer = System.Threading.Timer;

namespace APTAT.DeploymentCore.Descent
{
    public class DescentWatcher : IDescentWatcher
    {
        private readonly object _access;
        private readonly Dictionary<Guid, DateTime> _computersActivity;
        private readonly IScheduler _timer = null;
        private readonly TimeSpan _timeout;
        private readonly Action<Guid> _timeoutCallback;

        public DescentWatcher(TimeSpan timeout, Action<Guid> timeoutCallback, IScheduler timer)
        {
            _timeout = timeout;
            _timeoutCallback = timeoutCallback;
            _access = new object();
            _computersActivity = new Dictionary<Guid, DateTime>();

            _timer = timer;
            _timer.TimeoutCallback += TimerCallback;
            TimeSpan interval = new TimeSpan(0, 0, 1);
            _timer.SetInterval(interval);
        }
        public TimeSpan DescentTimeout
        {
            get { return _timeout; }
        }

        public void AddIdentity(Guid agentIdentity)
        {
            lock (_access)
            {
                _computersActivity[agentIdentity] = DateTime.UtcNow;
            }
        }

        public void ExcludeIdentity(Guid agentIdentity)
        {
            lock (_access)
            {
                _computersActivity.Remove(agentIdentity);
            }
        }

        public void NotifyProgress(Guid agentIdentity)
        {
            lock (_access)
            {
                _computersActivity[agentIdentity] = DateTime.UtcNow;
            }
        }

        private void TimerCallback(object state, EventArgs e)
        {
            var deadline = DateTime.UtcNow - _timeout;
            KeyValuePair<Guid, DateTime>[] activities;
            lock (_access)
                activities = _computersActivity.ToArray();
            if (activities.Length == 0)
                Stop();
            foreach (var computerActivity in activities)
            {
                if (computerActivity.Value < deadline)
                {
                    ExcludeIdentity(computerActivity.Key);
                    _timeoutCallback(computerActivity.Key);
                }
            }
        }

        public void Start()
        {
            _timer.Start(false);
        }
        void Stop()
        {
            _timer.Stop();
            Dispose();
        }
        public void Dispose()
        {
            _timer.Dispose();
        }

    }
}
