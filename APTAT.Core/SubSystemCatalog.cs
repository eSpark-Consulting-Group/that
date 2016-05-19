using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;
using APTAT.Core.Descent;

namespace APTAT.Core
{
    public interface IBusinessObject 
    {
    }

    public interface ISubSystem : IService
    {
        string Name { get; }
        Guid Identifier { get; }
        void Init(ISubSystemCatalog catalog);
    }

    public interface ISubSystemCatalog
    {
        T Find<T>();

        void RemoveSessionId(Guid guid);
    }

    public class SubSystemCatalog : ISubSystemCatalog
    {
        private readonly List<ISubSystem> _subSystems = new List<ISubSystem>();
        private readonly IChangeTrackingSubSystem _changeTrackingSubSystem = new ChangeTrackingSubSystem();

        public SubSystemCatalog()
        {
            _subSystems.Add(_changeTrackingSubSystem);
            _subSystems.Add(new DescentSubSystem());

            foreach (ISubSystem system in _subSystems)
            {
                system.Init(this);
            }
        }


        public T Find<T>()
        {
            foreach (ISubSystem system in _subSystems)
            {
                if (system is T)
                {
                    return (T)system;
                }
            }
            throw new InvalidOperationException(string.Format(@"Subsystem with type '{0}' not found", typeof(T).Name));
        }

        public void RemoveSessionId(Guid guid)
        {
            
        }
    }

    public interface IChangeTrackingSubSystem : ISubSystem
    {
        void RegisterListener(IChangeTrackingListener listener);
        void UnregisterListener(IChangeTrackingListener listener);
        void NotifyChanged(ChangesTrackingItem item, bool exceptSelfSession);

    }

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

    public class ChangeTrackingSubSystem : IChangeTrackingSubSystem
    {
        private static readonly Guid SubSystemId = new Guid(@"{8E8C8B2B-1E58-4fa0-A7D4-90132CCAEB8B}");
        private const string SubSystemName = @"ChangesTracking";

        public ChangeTrackingSubSystem()
        {
            int temp = 0;
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

        public void Init(ISubSystemCatalog catalog)
        {
            _catalog = catalog;
        }

        #endregion

        private readonly Dictionary<Guid, IChangeTrackingListener> _listeners =
            new Dictionary<Guid, IChangeTrackingListener>();
        private readonly object _syncObj = new object();

        private ISubSystemCatalog _catalog;

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
                        _catalog.RemoveSessionId(pair.Key);
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
    

    public interface IChangeTrackingListener
    {
        void Changed(params ChangesTrackingItem[] items);
        Guid SessionId { get; }
    }

    public enum ChangeType
    {
        Added,
        Changed,
        Deleted
    }

    [Serializable]
    public class ChangesTrackingItem
    {
        public readonly ChangeType ChangeType;
        public readonly IBusinessObject ChangedItem;

        public ChangesTrackingItem(ChangeType type, IBusinessObject changed)
        {
            ChangeType = type;
            ChangedItem = changed;
        }
    }
}
