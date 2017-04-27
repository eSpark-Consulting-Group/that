using System;
using System.Collections.ObjectModel;
using APTAT.Application.Models.Apt;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore;
using APTAT.Core.Valkyrie;

namespace FAT.Application.Models
{
    public interface IThreatInfoItem
    {
        IBadFileItem BadFile { get; }
        bool Clean { get; }
        int CleanFilesCount { get; set; }
        Guid ComputerGuid { get; set; }
        Collection<IThreatInfoItem> Computers { get; set; }
        IThreatInfoItem Parent { get; set; }

        IComputer DeploymentTarget { get; }
        int DistinctUnknownFilesCount { get; }
        bool Failed { get; }
        IThreatInfoItem[] FilesToCheckOnValkyrieCount { get; }
        UiFinalVerdict FinalVerdict { get; }
        bool HasBadFiles { get; }
        bool HasClean { get; }
        bool HasFailed { get; }
        bool HasMalwares { get; }
        bool HasPending { get; }
        bool HasUnknowns { get; }
        string InstancesCount { get; }
        string IpAddress { get; set; }
        bool IsComputer { get; set; }
        bool IsLocal { get; set; }
        bool IsMalware { get; }
        bool IsOnline { get; set; }
        bool IsUnknown { get; }
        bool IsWhiteListed { get; set; }
        ObservableCollection<IThreatInfoItem> Items { get; }
        string LdapPath { get; set; }
        int MalwareFilesCount { get; set; }
        string MalwareProbability { get; }
        float? MalwareProbabilityValue { get; set; }
        string Name { get; set; }
        string NameAndIpAddress { get; }
        string NameAndIpAddressSorting { get; }
        int NotAnalyzedFilesCount { get; }
        string OperationStatus { get; set; }
        bool Pending { get; }
        int PendingFilesCount { get; set; }
        bool ScanCompleted { get; }
        ScanStage ScanStage { get; set; }
        ScanStatus ScanStatus { get; }
        string ShortName { get; set; }
        bool ShouldCheckOnValkyrie { get; }
        long Size { get; set; }
        string SizeSorting { get; set; }
        string SizeString { get; set; }
        string SkipReason { get; set; }
        string StatusMessage { get; }
        string StatusMessageSystem { get; set; }
        int TotalFilesCount { get; }
        double TotalProgressPercentage { get; }
        int UnknownFilesCount { get; set; }
        double ValkyrieProgressPercentage { get; set; }
        ValkyrieVerdict ValkyrieVerdict { get; set; }
        string VerdictString { get; }

        void AddFile(IThreatInfoItem IThreatInfoItem);
        void RemoteScanCompleted();
        void RestoreConnectingState();
        string ToString();
        void TrySetFailedStatus();
        void UpdateCounters(IScanResultItem scanProgress, IScanCounterDiff counterDiff);
        bool UpdateFileVerdict(IFileVerdictContext fileVerdictContext);
        void UpdateFileVerdict(APTAT.Core.Descent.Apt.CloudVerdict verdict);
        void UpdateVerdictForFiles(IFileVerdictContext[] fileVerdictContext, IScanCounterDiff scanCounterDiff);
    }
}