using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace APTAT.Core.Descent
{
    public interface IDescentWatcher : IDisposable
    {
        TimeSpan Timeout { get; }
        void AddIdentity(Guid agentIdentity);
        void ExcludeIdentity(Guid agentIdentity);
        void NotifyProgress(Guid agentIdentity, int value);
        void Start();
    }

    public class DescentWatcher : IDescentWatcher
    {
        private object _access;
        private Dictionary<Guid, int> _computersProgress;
        private Dictionary<Guid, DateTime> _computersActivity;
        private Timer _timer;
        private TimeSpan _timeout;
        private Action<Guid> _timeoutCallback;

        public TimeSpan Timeout
        {
            get { return _timeout; }
        }

        public DescentWatcher(TimeSpan timeout, Action<Guid> timeoutCallback)
        {
            _timeout = timeout;
            _timeoutCallback = timeoutCallback;

            _access = new object();
            _computersProgress = new Dictionary<Guid, int>();
            _computersActivity = new Dictionary<Guid, DateTime>();
            _timer = new Timer(TimerCallback);
        }

        public void AddIdentity(Guid agentIdentity)
        {
            lock (_access)
            {
                _computersProgress[agentIdentity] = 0;
                _computersActivity[agentIdentity] = DateTime.UtcNow;
            }
        }

        public void ExcludeIdentity(Guid agentIdentity)
        {
            lock (_access)
            {
                _computersProgress[agentIdentity] = Int32.MaxValue;
                _computersActivity[agentIdentity] = DateTime.MaxValue;
            }
        }

        public void NotifyProgress(Guid agentIdentity, int value)
        {
            lock (_access)
            {
                _computersActivity[agentIdentity] = DateTime.UtcNow;
                _computersProgress[agentIdentity] = value;
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
            foreach (var computerActivity in activities)
            {
                if (computerActivity.Value < deadline)
                {
                    ExcludeIdentity(computerActivity.Key);
                    _timeoutCallback(computerActivity.Key);
                }
            }
        }

    }
}
