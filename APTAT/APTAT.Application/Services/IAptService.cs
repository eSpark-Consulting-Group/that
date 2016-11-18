using System;
using System.Net;
using APTAT.Application.Models;
using APTAT.Core;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Valkyrie;
using APTAT.Core.Descent.Apt;

namespace APTAT.Application.Services
{
    public class DomainEventArgs : EventArgs
    {
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

        public Guid OperationGuid { get; private set; }

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
        
        public Int32 MalwaresFound { get; set; }

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
        event EventHandler<BeginScanCompletedEventArgs> BeginScanCompleted;
        event EventHandler<ScanStatusChangedEventArgs> ScanStatusChanged;
        event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        event EventHandler<GetScanResultCompletedEventArgs> ComputerScanCompleted;
        event EventHandler<BuildReportCompletedEventArgs> BuildReportCompleted;
        event EventHandler<ScanProgressChangedEventArgs> ScanProgressChanged;   
        event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> ValkyrieAnalyzeProgressChanged;
        event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;
        event EventHandler ValkyrieAnalyzeStarted;
        event EventHandler<FlsRequestEventArgs> FlsAnalyzeProgressChanged;
        void BeginScan(DeploymentTarget[] items, String login, String password, Object userStatem, IDevicesSourceDescription devicesSourceDescription, ScanOptions options);
        
        void StopScan();

        void BuildReport(Guid operationGuid, ReportKind reportKind, Object userState);

        void BeginValkyrieAnalysis(string login, string password, string camLicenseId, string camUserName, string camPassword,
            NetworkCredential networkCredential = null);
        
        
        string GetValkyrieDetailsLink();
    }
}
