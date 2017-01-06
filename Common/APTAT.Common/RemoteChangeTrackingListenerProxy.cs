using APTAT.Common;
using System;

namespace DDT.Core.ChangeTracking
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

        public bool Changed(params ChangesTrackingItem[] items)
        {
            return _realListener.Changed(items);
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
