using System;
using System.Net;
using APTAT.Application.Models;
using APTAT.Application.Models.Valkyrie;
using APTAT.Core;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Valkyrie;
using APTAT.Core.Descent.Apt;
using System.Collections.Generic;
using APTAT.Core.FLS;

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

        public bool RescanAttempt { get; private set; }

        public BeginScanCompletedEventArgs(Guid operationGuid, bool rescanAttempt, Object userState, Exception error = null)
        {
            OperationGuid = operationGuid;
            RescanAttempt = rescanAttempt;
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
        public Guid OperationUid { get; set; }
        public Int32 SuccededCount { get; set; }
        public Int32 FailedCount { get; set; }
        public Int32 CanceledCount { get; set; }
        public Int32 NotPerformedCount { get; set; }  
    }

    public class GetFileStatisticsEventArgs : ScanCompletedEventArgs
    { 
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
        event EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieFile>> ValkyrieAnalyzeProgressChanged;
        event AnalyzeCompletedEventHandler<ValkyrieFile> ValkyrieAnalyzeCompleted;
        event EventHandler ValkyrieAnalyzeStarted;
        event EventHandler<FlsRequestEventArgs<IFlsResult>> FlsAnalyzeProgressChanged;
        void BeginScan(DeploymentTarget[] items, Object userState,
            IDevicesSourceDescription devicesSourceDescription, ScanOptions options);
        void UpdateFlsPorts(int udpPort, int tcpPort);
        void StopScan(Guid[] items);
        void BuildReport(ReportKind reportKind, Object userState);
        void BeginValkyrieAnalysis(string login, string password, string camLicenseId, string camUserName, 
            string camPassword, NetworkCredential networkCredential);
        double GetProgressOfOperations(HashSet<Guid> operationUids);
        GetFileStatisticsEventArgs GetCurrentStatistics();

        string GetValkyrieDetailsLink();
    }
}
