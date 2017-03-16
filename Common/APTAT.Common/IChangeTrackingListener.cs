using System;

namespace APTAT.Common
{
    public interface IChangeTrackingSubSystem : ISubSystem
    {
        void RegisterListener(IChangeTrackingListener listener);
        void UnregisterListener(IChangeTrackingListener listener);
        void NotifyChanged(IChangesTrackingItem item, bool exceptSelfSession);

    }
    public interface IBusinessObject
    {
    }


    public interface IService //marker
    {
    }
    public interface ISubSystem : IService
    {
        string Name { get; }
        Guid Identifier { get; }
    }
    public interface IDependencyLocatorModule
    {
        void RegisterDependencies(ICompositionHelper resolver);
    }
    public interface IChangeTrackingListener
    {
        void Changed(params IChangesTrackingItem[] items);
        Guid SessionId { get; }
    }
    public enum ChangeType
    {
        Added,
        Changed,
        Deleted
    }

    [Serializable]
    public class ChangesTrackingItem : IChangesTrackingItem
    {
        public ChangeType ChangeType { get; }
        public IBusinessObject ChangedItem { get; }

        public ChangesTrackingItem(ChangeType type, IBusinessObject changed)
        {
            ChangeType = type;
            ChangedItem = changed;
        }
    }

    public interface IChangesTrackingItem
    {
        ChangeType ChangeType { get; }
        IBusinessObject ChangedItem { get; }
    }
}
