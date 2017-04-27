using System;
using System.Net;
using System.Threading.Tasks;
using FAT.Application.Models;
using APTAT.Core;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Valkyrie;
using APTAT.Core.Descent.Apt;
using APTAT.Application.Services.EventArgs;
using APTAT.Core.Apt;
using APTAT.Core.FLS;
using APTAT.DeploymentCore;

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
    
    public class BuildReportResult
    {
        public String FileName { get; }
        public byte[] DocumentBytes { get; }
        public Exception Exception { get; }
        public Boolean IsGenerated { get; }
        public BuildReportResult(string fileName, byte[] documentBytes, Exception e, Boolean reportGenerated)
        {
            DocumentBytes = documentBytes;
            Exception = e;
            FileName = fileName;
            IsGenerated = reportGenerated;
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

        void UpdateFlsPorts(int udpPort, int tcpPort);
        void StopScan(Guid[] items);
        Task<BuildReportResult> BuildReportAsync(ReportKind reportKind);

        string GetValkyrieDetailsLink();
        string GetValkyrieDetailsLink(string apiKey, string scanId);
        IOperationResult SubmitEmail(string[] addresses);

        void BeginScan(IComputer[] targets, IDevicesSourceDescription devicesSourceDescription,
            ScanOptions options);

        GetModifiedResultsForOperationEventArgs GetModifiedResultsForOperation(Guid operationUid, TimeSpan? lastUtcTime,
            DateTime? lastFlcUtcTime, DateTime? lastValkyrieTime);

        Task OnAppClosing();

        event EventHandler<ScanStartedEventArgs> FatScanStarted;
        event EventHandler<ErrorEventArgs> FatScanCompleted;
        event EventHandler CancelScanStarted;
    }
}
