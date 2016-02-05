using System;
using APTAT.Application.Models;
using System.Collections.Generic;
using APTAT.Valkyrie.Client;
using APTAT.Core;
using System.ComponentModel;
using APTAT.Core.Valkyrie;
using Valkyrie.WebApiProvider.Contracts;

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

    public class GetWorkgroupEventArgs : DomainEventArgs
    {
        public DeploymentItem[] Items { get; private set; }

        public GetWorkgroupEventArgs(DeploymentItem[] items, Object userState, Exception error = null)
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

    public class GetScanResultCompletedEventArgs : DomainEventArgs
    {
        public ScanResultItem ScanResult { get; private set; }

        public GetScanResultCompletedEventArgs(ScanResultItem scanResult, Object userState, Exception error = null)
        {
            ScanResult = scanResult;
            UserState = userState;
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

        public FilesStatistics FilesStatistics { get; set; }

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
        public Int32 Percentage { get; private set; }

        public Guid OperationGuid { get; private set; }

        public ScanProgressChangedEventArgs(Int32 percent, Guid operationGuid)
        {
            Percentage = percent;
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

    public interface IAptService
    {
        event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryItemsCompleted;
        event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryRootCompleted;
        event EventHandler<DirectoryItemsResolvedEventArgs> DirectoryItemsResolved;
        event EventHandler<BeginScanCompletedEventArgs> BeginScanCompleted;
        event EventHandler<ScanStatusChangedEventArgs> ScanStatusChanged;
        event EventHandler<ScanCompletedEventArgs> ScanCompleted;
        event EventHandler<GetScanResultCompletedEventArgs> GetScanResultCompleted;
        event EventHandler<BuildReportCompletedEventArgs> BuildReportCompleted;
        event EventHandler<ScanProgressChangedEventArgs> ScanProgressChanged;        
        event EventHandler<GetWorkgroupEventArgs> GetWorkgroupCompleted;
        event EventHandler<GetByIpAddressEventArgs> GetByIpAddressesCompleted;
        event ProgressChangedEventHandler ValkyrieAnalyzeProgressChanged;
        event AnalyzeCompletedEventHandler<FileResult> ValkyrieAnalyzeCompleted;

        void GetByIpAddresses(IEnumerable<IpAddressWrapper> ipAddresses, Object userState);

        void GetWorkgroup(String workgroupName, Object userState);

        void BuildReport(Guid operationGuid, ReportKind reportKind, Object userState);

        void GetDirectoryItems(String domainController, String admin, String password, DirectoryItem parent, Object userState);

        void GetDirectoryRoot(String domainController, String admin, String password, Object userState);

        void ResolveDirectoryItems(DeploymentItem[] directoryItems, String admin, String password, Object userState);

        void BeginScan(ThreatInfoItem[] items, String domain, String login, String password, Object userState, String scanScope);

        void GetScanResult(Guid targetUid, Object userState);

        void StopScan();

        void BeginValkyrieAnalysis(string login, string password, string apiKey);
        
        RegisterWithResponse RegisterWithValkyrie(string camLicenseId, string camUserName, string camPassword);
        
        string GetValkyrieDetailsLink();
    }
}
