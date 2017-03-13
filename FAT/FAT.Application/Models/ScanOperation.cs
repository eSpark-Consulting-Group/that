using System;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Directory;
using FAT.Application.Models.Apt;

namespace FAT.Application.Models
{
    public class ScanOperation : IScanOperation
    {
        public int ComputersScanned { get; set; }
        public int TotalComputersScanned { get; set; }
        public int FilesScanned { get; set; }
        public int UnknownsCount { get; set; }
        public int MaliciousCount { get; set; }
        public int NotAnalyzedCount { get; set; }
        public int CleanCount { get; set; }
        public int InAnalysisCount { get; set; }
        public Guid OperationUid { get; set; }
        public ScanKind ScanMethod { get; set; }
        public string DiscovererDescription { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Finished { get; internal set; }
        public bool IsFailed { get; set; }
        public bool IsCancelled { get; set; }
        public IDevicesSourceDescription DevicesSourceDescription { get; set; }
        public IValkyrieOperationInfo ValkyrieOperationInfo { get; set; }

        #region UI related

        public Func<IScanOperation, IScanOperationResult> LoadDetailsFunc { get; set; }

        public bool IsCompleted
        {
            get { return !IsFailed && !IsCancelled; }
        }

        public bool HasMalwares
        {
            get { return MaliciousCount > 0; }
        }

        public bool HasUnknowns
        {
            get { return !HasMalwares && UnknownsCount > 0; }
        }

        public bool HasFailed
        {
            get { return !HasMalwares && !HasUnknowns && !HasPending && NotAnalyzedCount > 0; }
        }

        public bool HasPending
        {
            get { return !HasMalwares && !HasUnknowns && InAnalysisCount > 0; }
        }

        public bool HasBadFiles
        {
            get { return HasMalwares || HasUnknowns || HasFailed || HasPending; }
        }

        public string ScanHeader
        {
            get
            {
                if (IsCancelled)
                    return "Scan Canceled.";
                if (IsFailed)
                    return "Scan Failed.";

                return $"Computers scanned: {ComputersScanned}.";
            }
        }

        public string ScanHeader2
        {
            get
            {
                if (ComputersScanned == 0 || FilesScanned == 0)
                    return string.Empty;

                var value = $"Total files scanned: {FilesScanned}. Unknown: {UnknownsCount}. Malicious: {MaliciousCount}. ";

                if (InAnalysisCount > 0)
                    value += $"In Analysis: {InAnalysisCount}. ";
                if (NotAnalyzedCount > 0)
                    value += $"Not Analyzed: {NotAnalyzedCount}. ";

                return value;
            }
        }

        public string ScanTypeStr
        {
            get
            {
                switch (DevicesSourceDescription.Type)
                {
                    case ETargetsDiscovererType.ActiveDirectory:
                        return "Active Directory";
                    case ETargetsDiscovererType.Workgroup:
                        return "Workgroup";
                    case ETargetsDiscovererType.NetworkAddresses:
                        return DiscovererDescription == "localhost" ? "Local" : "Network Addresses";
                }

                return string.Empty;
            }
        }     

        public string ScanMethodStr
        {
            get
            {
                switch (ScanMethod)
                {
                    case ScanKind.Custom:
                        return "Custom Scan";
                    case ScanKind.Full:
                        return "Full Scan";
                    case ScanKind.Quick:
                        return "Quick Scan";
                }

                return string.Empty;
            }
        }

        public bool HasItems
        {
            get { return false; }
        }

        #endregion
    }

    public class ValkyrieOperationInfo : IValkyrieOperationInfo
    {
        public ValkyrieOperationInfo(string apiKey, string scanId)
        {
            ApiKey = apiKey;
            ScanId = scanId;
        }
        public string ApiKey { get;  }
        public string ScanId { get; }

        public string DetailedLink { get; set; }
    }
}
