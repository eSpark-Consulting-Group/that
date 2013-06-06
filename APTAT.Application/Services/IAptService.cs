using System;
using APTAT.Application.Models;

namespace APTAT.Application.Services
{
    public class DomainEventArgs : EventArgs
    {
        public Object UserState { get; protected set; }

        public Exception Exception { get; protected set; }
    }

    public class GetDirectoryItemsEventArgs : DomainEventArgs
    {
        public DirectoryItem Parent { get; private set; }

        public DirectoryItem[] Items { get; private set; }

        public GetDirectoryItemsEventArgs(DirectoryItem parent, DirectoryItem[] items, Object userState, Exception error = null)
        {
            Parent = parent;
            Items = items;
            UserState = userState;
            Exception = error;
        }
    }

    public class DirectoryItemsResolvedEventArgs : DomainEventArgs
    {
        public ThreatInfoItem[] Items { get; private set; }

        public DirectoryItemsResolvedEventArgs(ThreatInfoItem[] items, Object userState, Exception error = null)
        {
            Items = items;
            UserState = userState;
            Exception = error;
        }
    }

    public class BeginScanCompletedEventArgs : DomainEventArgs
    {
        public Guid OperationGuid { get; private set; }

        public BeginScanCompletedEventArgs(Guid operationGuid, Object userState, Exception error = null)
        {
            OperationGuid = operationGuid;
            UserState = userState;
            Exception = error;
        }
    }

    public class ScanStatusChangedEventArgs : DomainEventArgs
    {
        public ScanProgressItem[] ScanProgress { get; private set; }

        public Guid OperationGuid { get; private set; }

        public ScanStatusChangedEventArgs(ScanProgressItem[] scanProgress,Object userState, Exception error = null)
        {
            ScanProgress = scanProgress;
            UserState = userState;
            Exception = error;
        }
    }

    public interface IAptService
    {
        event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryItemsCompleted;
        event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryRootCompleted;
        event EventHandler<DirectoryItemsResolvedEventArgs> DirectoryItemsResolved;
        event EventHandler<BeginScanCompletedEventArgs> BeginScanCompleted;
        event EventHandler<ScanStatusChangedEventArgs> ScanStatusChanged;

        void  GetDirectoryItems(String domainController, String admin, String password, DirectoryItem parent, Object userState);

        void GetDirectoryRoot(String domainController, String admin, String password, Object userState);

        void ResolveDirectoryItems(DirectoryItem[] directoryItems, Object userState);

        void BeginScan(ThreatInfoItem[] items, String login, String password, Object userState);
    }
}
