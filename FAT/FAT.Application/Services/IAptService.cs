using System;
using System.Net;
using FAT.Application.Models;
using APTAT.Core;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Valkyrie;
using APTAT.Core.Descent.Apt;
using APTAT.Application.Services.EventArgs;
using APTAT.Core.Apt;
using APTAT.Core.FLS;

namespace FAT.Application.Services
{
    public class DomainEventArgs : EventArgs
    {
        public DomainEventArgs(Object userState, Exception error)
        {
            UserState = userState;
            Exception = error;
        }
        public DomainEventArgs()
        {

        }
        public Object UserState { get; protected set; }
        public Exception Exception { get; set; }
    }

    public class GetByIpAddressEventArgs : DomainEventArgs
    {
        public DeploymentItem[] Items { get; private set; }

        public GetByIpAddressEventArgs(DeploymentItem[] items, Object userState, Exception error = null)
        {
            Items = items;
            UserState = userState;
            Exception = error;
        }
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
        public DeploymentTarget[] Items { get; private set; }
        public DirectoryItemsResolvedEventArgs(DeploymentTarget[] items, Object userState, Exception error = null)
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
        public ScanStatusChangedEventArgs(ScanProgressItem[] scanProgress,Object userState, Exception error = null)
        {
            ScanProgress = scanProgress;
            UserState = userState;
            Exception = error;
        }
    }

    public class GetScanResultCompletedEventArgs : DomainEventArgs
    {
        public ScanResultItem ScanResult { get; private set; }

        public GetScanResultCompletedEventArgs(ScanResultItem scanResult, Exception error = null)
        {
            ScanResult = scanResult;
            Exception = error;
        }
    }

    public class ScanCompletedEventArgs : DomainEventArgs
    {
        public Int32 SuccededCount { get; set; }
        public Int32 FailedCount { get; set; }
        public Int32 CanceledCount { get; set; }
        public Int32 NotPerformedCount { get; set; }  
        public IFilesStatistics FilesStatistics { get; set; }
        public int DistinctFileFound { get; set; }
    }

    public class BuildReportCompletedEventArgs : DomainEventArgs
    {
        public String FileName { get; private set; }

        public BuildReportCompletedEventArgs(String fileName, Exception e, Object userState)
        {
            FileName = fileName;
            Exception = e;
            UserState = userState;
        }
    }

    public class ScanProgressChangedEventArgs : DomainEventArgs
    {
        public double Percentage { get; private set; }
        public Guid OperationGuid { get; private set; }

        public ScanProgressChangedEventArgs(double percentage, Guid operationGuid)
        {
            Percentage = percentage;
            OperationGuid = operationGuid;
        }
    }

    public enum ReportKind
    {
        Executive,
        DetailedPerDevice,
        DetailedPerProgram,
        ExecutiveWithValkyrie,
        DetailedPerDeviceWithValkyrie,
        DetailedPerProgramWithValkyrie
    }

    public interface IAptService : IDisposable
    { 
        event EventHandler<BuildReportCompletedEventArgs> BuildReportCompleted;
       
        
        void UpdateFlsPorts(int udpPort, int tcpPort);
        void StopScan(Guid[] items);
        void BuildReport(Guid operationGuid, ReportKind reportKind, Object userState);
        
        string GetValkyrieDetailsLink();
        OperationResult SubmitEmail(string[] addresses);

        void BeginScan(DeploymentTarget[] targets, IDevicesSourceDescription devicesSourceDescription,
            ScanOptions options);

        GetModifiedResultsForOperationEventArgs GetModifiedResultsForOperation(Guid operationUid, DateTime? lastUtcTime,
            DateTime? lastFlcUtcTime, DateTime? lastValkyrieTime);

        event EventHandler<ScanStartedEventArgs> FatScanStarted;
        event EventHandler<ErrorEventArgs> FatScanCompleted;
        event EventHandler CancelScanStarted;
    }
}
