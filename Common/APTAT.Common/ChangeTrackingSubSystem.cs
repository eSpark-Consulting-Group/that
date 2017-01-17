using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;

namespace APTAT.Common
{
    public class ChangeTrackingSubSystem : IChangeTrackingSubSystem
    {
        private static readonly Guid SubSystemId = new Guid(@"{8E8C8B2B-1E58-4fa0-A7D4-90132CCAEB8B}");
        private const string SubSystemName = @"ChangesTracking";

        public ChangeTrackingSubSystem()
        {
        }

        #region ISubSystem Members

        public string Name
        {
            get { return SubSystemName; }
        }

        public Guid Identifier
        {
            get { return SubSystemId; }
        }

        #endregion

        private readonly Dictionary<Guid, IChangeTrackingListener> _listeners =
            new Dictionary<Guid, IChangeTrackingListener>();
        private readonly object _syncObj = new object();

        #region IChangeTrackingSubSystem Members

        public void RegisterListener(IChangeTrackingListener listener)
        {
            if (listener != null)
            {
                lock (_syncObj)
                {
                    RemoteChangeTrackingListenerProxy proxy = new RemoteChangeTrackingListenerProxy(listener);
                    //IChangeTrackingListener proxy = listener;
                    Guid id;
                    try
                    {
                        id = proxy.SessionId;
                    }
                    catch (InvalidCredentialException ex)
                    {
                        LogHelper.Trace(TraceEventType.Error, "RegisterListener() cannot get console listener session ID: {0}", ex);
                        throw;
                    }
                    if (!_listeners.ContainsKey(id))
                    {
                        LogHelper.Trace(TraceEventType.Verbose, @"ChangeTracking.RegisterListener {0} /Count before {1}/", id, _listeners.Count);
                        _listeners.Add(id, proxy);
                    }
                    else
                    {
                        LogHelper.Trace(TraceEventType.Verbose, @"ChangeTracking.RegisterListener {0} alredy exist!", id);
                        return;
                    }
                }
            }
        }

        public void UnregisterListener(IChangeTrackingListener listener)
        {
            if (listener != null)
            {
                lock (_syncObj)
                {
                    Guid id;
                    try
                    {
                        id = listener.SessionId;
                    }
                    catch (InvalidCredentialException ex)
                    {
                        LogHelper.Trace(TraceEventType.Error, "UnregisterListener() cannot get console listener session ID: {0}", ex);
                        throw;
                    }
                    if (_listeners.ContainsKey(id))
                    {
                        LogHelper.Trace(TraceEventType.Verbose, @"ChangeTracking.UnregisterListener {0} /Count before {1}/", id, _listeners.Count);
                        _listeners.Remove(id);
                    }
                    else
                    {
                        LogHelper.Trace(TraceEventType.Verbose, @"ChangeTracking.UnregisterListener {0} don't exist!", id);
                        return;
                    }
                }
            }
        }

        public void NotifyChanged(ChangesTrackingItem item, bool exceptSelfSession)
        {
            NotifyChanged(new ChangesTrackingItem[] { item }, exceptSelfSession);
        }

        private void NotifyChanged(ChangesTrackingItem[] items, bool exceptSelfSession)
        {
            List<Guid> failed = new List<Guid>();
            //GenericContextData<Guid> clientUID = CallContext.GetData(@"ClientUID") as GenericContextData<Guid>;
            lock (_syncObj)
            {
                foreach (KeyValuePair<Guid, IChangeTrackingListener> pair in _listeners)
                {
                    try
                    {
                        //if (exceptSelfSession && null != clientUID && pair.Key.Equals(clientUID.Data))
                        //{
                        //    continue;
                        //}
                        //UpdateSessionInfo(items, clientUID);
                        pair.Value.Changed(items);
                    }
                    catch (Exception)
                    { 
                        failed.Add(pair.Key);
                    }
                }
                foreach (Guid guid in failed)
                {
                    _listeners.Remove(guid);
                }
            }
        }

        #endregion
    }
   
}


