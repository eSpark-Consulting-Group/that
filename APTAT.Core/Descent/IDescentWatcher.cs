using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace APTAT.Core.Descent
{
    public interface IDescentWatcher : IDisposable
    {
        void AddIdentity(Guid agentIdentity);
        void ExcludeIdentity(Guid agentIdentity);
        TimeSpan DescentTimeout { get; }
        void NotifyProgress(Guid agentIdentity);
        void Start();
    }

    public class DescentWatcher : IDescentWatcher
    {
        private readonly object _access;
        private readonly Dictionary<Guid, DateTime> _computersActivity;
        private readonly Timer _timer;
        private readonly TimeSpan _timeout;
        private readonly Action<Guid> _timeoutCallback;

        public DescentWatcher(TimeSpan timeout, Action<Guid> timeoutCallback)
        {
            _timeout = timeout;
            _timeoutCallback = timeoutCallback;

            _access = new object();
            _computersActivity = new Dictionary<Guid, DateTime>();
            _timer = new Timer(TimerCallback);
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

        public void Start()
        {
            _timer.Change(TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }

        

        public void Dispose()
        {
            _timer.Dispose();
        }

        private void TimerCallback(object state)
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

        void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

    }
}
