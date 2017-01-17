using System;

namespace APTAT.Common
{
    internal class RemoteChangeTrackingListenerProxy : IChangeTrackingListener
    {
        private readonly IChangeTrackingListener _realListener;
        private readonly Guid _sessionId;

        public RemoteChangeTrackingListenerProxy(IChangeTrackingListener realListener)
        {
            _realListener = realListener;
            _sessionId = realListener.SessionId;
        }

        #region IChangeTrackingListener Members

        public void Changed(params ChangesTrackingItem[] items)
        {
            _realListener.Changed(items);
        }

        public Guid SessionId
        {
            get { return _sessionId; }
        }

        //public void StatisticsUpdate(ComputersStat stat)
        //{
        //    m_realListener.StatisticsUpdate(stat);
        //}

        #endregion
    }
}
