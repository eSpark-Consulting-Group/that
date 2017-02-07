using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using APTAT.Application.Models.Apt;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Valkyrie;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace FAT.Application.Models
{
    public enum ScanStatusEnum
    {
        Offline,
        InProgress,
        Child,
        Canceled,
        Completed,
        Failed
    }

    public enum UiFinalVerdict
    {
        Clean,
        Unknown,
        Malware,
        InAnalysis,
        NotAnalyse
    }


    public class ThreatInfoItem : DomainObject
    {
        private Int32 _totalFilesCount;
        private Int32 _unknownFilesCount;
        private Int32 _malwareFilesCount;
        private Int32 _cleanFilesCount;
        private String _statusMessage;
        private ValkyrieVerdict _valkyrieVerdict;
        private String _statusMessageMemento;

        private ScanStage _scanStage;
        private int _errorFilesCount;
        private int _noVerdictFilesCount;
        private Dictionary<string,ThreatInfoItem> _itemsDict = new Dictionary<string, ThreatInfoItem>(StringComparer.OrdinalIgnoreCase);

        public Collection<ThreatInfoItem> Computers { get; set; }
        public BadFileItem BadFile { get; }

        public DeploymentTarget DeploymentTarget { get; }
        public double ProgressPercentage { get; set; }

        public string SkipReason { get; set; }

        public string OperationStatus { get; set; }

        public Boolean HasBadFiles
        {
            get { return UnknownFilesCount > 0 || MalwareFilesCount > 0 || NoVerdictFilesCount > 0 || FailedToAnalyzeCount > 0; }
        }

        public void UpdateFileVerdict(CloudVerdict verdict)
        {
            BadFile.Cloud = verdict;

            RaisePropertyChanged(nameof(VerdictString));
            RaisePropertyChanged(nameof(IsMalware));
            RaisePropertyChanged(nameof(IsUnknown));
            RaisePropertyChanged(nameof(Clean));
            RaisePropertyChanged(nameof(Failed));
            RaisePropertyChanged(nameof(Pending));
        }

        public ValkyrieVerdict ValkyrieVerdict
        {
            get { return _valkyrieVerdict; }
            set
            {
                _valkyrieVerdict = value;
                RaisePropertyChanged(nameof(VerdictString));
                RaisePropertyChanged(nameof(IsMalware));
                RaisePropertyChanged(nameof(IsUnknown));
                RaisePropertyChanged(nameof(Clean));
                RaisePropertyChanged(nameof(Failed));
                RaisePropertyChanged(nameof(Pending));
            }
        }

        public UiFinalVerdict FinalVerdict
        {
            get
            {
                switch (ValkyrieVerdict)
                {
                    case ValkyrieVerdict.NotAnalyzed:
                        return UiFinalVerdict.NotAnalyse;
                    case ValkyrieVerdict.PotentiallyUnwantedApplication:
                    case ValkyrieVerdict.Malware:
                        return UiFinalVerdict.Malware;
                    case ValkyrieVerdict.Pending:
                    case ValkyrieVerdict.FirstSeen:
                        return UiFinalVerdict.InAnalysis;
                    case ValkyrieVerdict.Undetected:
                        return UiFinalVerdict.Unknown;
                    case ValkyrieVerdict.Clean:
                        return UiFinalVerdict.Clean;
                    case ValkyrieVerdict.PendingFls:
                        {
                            switch (BadFile.Cloud)
                            {
                                case CloudVerdict.Safe:
                                    return UiFinalVerdict.Clean;
                                case CloudVerdict.FirstSeen:
                                case CloudVerdict.FailedToAnalyze:
                                case CloudVerdict.NoVerdict:
                                case CloudVerdict.Unknown:
                                    return UiFinalVerdict.InAnalysis;
                                case CloudVerdict.Malware:
                                    return UiFinalVerdict.Malware;

                                default:
                                    throw new ArgumentOutOfRangeException("BadFile");
                            }
                        }
                    default:
                        throw new ArgumentOutOfRangeException("ValkyrieVerdict");
                }
            }
        }

        public string VerdictString
        {
            get
            {
                switch (FinalVerdict)
                {
                    case UiFinalVerdict.Clean:
                        return "Clean";
                    case UiFinalVerdict.InAnalysis:
                        return "In Analysis";
                    case UiFinalVerdict.Malware:
                        return "Malware";
                    case UiFinalVerdict.NotAnalyse:
                        return $"Not Analyzed ({OperationStatus.Replace(Environment.NewLine, "")})";
                    case UiFinalVerdict.Unknown:
                        return "No Threat Found";
                    default:
                        throw new ArgumentOutOfRangeException("FinalVerdict");
                }
            }
        }

        public void UpdateStatus()
        {
            int malwareCount = 0;
            int failedToAnalyzeCount = 0;
            int noVerdictFilesCount = 0;
            int safeCount = 0;
            int unknownFilesCount = 0;


            foreach (var item in Items)
            {
                if (item.BadFile.IsMalware) malwareCount++;
                else if (item.BadFile.Cloud == CloudVerdict.FailedToAnalyze) failedToAnalyzeCount++;
                else if (item.BadFile.Cloud == CloudVerdict.NoVerdict) noVerdictFilesCount++;
                else if (item.BadFile.Cloud == CloudVerdict.Safe) safeCount++;
                else if (item.BadFile.IsUnknown) unknownFilesCount++;
            }

            if (malwareCount > 0 || failedToAnalyzeCount > 0 || noVerdictFilesCount > 0 || safeCount > 0 || unknownFilesCount > 0)
            {

                FailedToAnalyzeCount = failedToAnalyzeCount;
                NoVerdictFilesCount = noVerdictFilesCount;
                MalwareFilesCount = malwareCount;
                UnknownFilesCount = unknownFilesCount;
                CleanFilesCount = safeCount;

                RaisePropertyChanged(nameof(StatusMessage));
            }
        }
        

        public bool IsMalware
        {
            get
            {
                return BadFile.IsMalware || ValkyrieVerdict == ValkyrieVerdict.Malware ||
                        ValkyrieVerdict == ValkyrieVerdict.PotentiallyUnwantedApplication;
            }
        }

        public bool Pending
        {
            get
            {
                return (BadFile.IsUnknown && ValkyrieVerdict == ValkyrieVerdict.PendingFls) 
                    || (ValkyrieVerdict == ValkyrieVerdict.FirstSeen || ValkyrieVerdict == ValkyrieVerdict.Pending);
            }
        }

        public bool IsUnknown
        {
            get
            {
                return ValkyrieVerdict == ValkyrieVerdict.Undetected;
            }

        }

        public bool Clean
        {
            get
            {
                return BadFile.IsClean || ValkyrieVerdict == ValkyrieVerdict.Clean;
            }
        }

        public Boolean Failed
        {
            get
            {
                return !IsUnknown && !IsMalware && !Clean;
            }
        }


        public Boolean HasMalwares
        {
            get
            {
                return MalwareFilesCount > 0;
            }
        }

        public Boolean HasUnknowns
        {
            get
            {
                return MalwareFilesCount == 0 && UnknownFilesCount > 0;
            }
        }

        public Boolean HasFailed
        {
            get
            {
                return MalwareFilesCount == 0 && UnknownFilesCount == 0 && HasBadFiles;
            }
        }

        public Boolean IsComptuer { get; set; }

        public Boolean IsOnline { get; set; }

        public String Name { get; set; }

        public string ShortName { get; set; }

        public String IpAddress { get; set; }

        public bool IsLocal { get; set; }

        public String NameAndIpAddress
        {
            get
            {
                if (!string.IsNullOrEmpty(IpAddress))
                {
                    return String.Format("{0} ({1})", Name, IpAddress);
                }

                return Name;
            }
        }

        public string NameAndIpAddressSorting
        {
            get
            {
                if (string.IsNullOrEmpty(IpAddress))
                    return (!char.IsDigit(Name[0]) ? "A" : "B") + Name;
                else
                {
                    if (Name != IpAddress)
                        return (!char.IsDigit(Name[0]) ? "A" : "B") + Name;
                    else
                    {
                        var ver = Version.Parse(IpAddress);
                        var str = string.Format("C{0:000}.{1:000}.{2:000}.{3:000}",
                            ver.Major, ver.Minor, ver.Build, ver.Revision);
                        return str;
                    }
                }
            }
        }

        public String SizeString { get; set; }

        public Int64 Size { get; set; }

        public string SizeSorting { get; set; }

        public Guid ComputerGuid { get; set; }

        public string LdapPath { get; set; }
        public ScanStatus ScanStatus
        {
            get
            {
                if (!IsOnline)
                {
                    return new ScanStatus(ScanStatusEnum.Offline);
                }

                if (ScanStage == ScanStage.OperationCompleted)
                {
                    return new ScanStatus(ScanStatusEnum.Completed);
                }

                if (ScanStage == ScanStage.OperationFailed)
                {
                    return new ScanStatus(ScanStatusEnum.Failed);
                }

                if (ScanStage == ScanStage.OperationCanceled)
                {
                    return new ScanStatus(ScanStatusEnum.Canceled);
                }

                return new ScanStatus(IsComptuer ? ScanStatusEnum.InProgress : ScanStatusEnum.Child);
            }
        }

        public ScanStage ScanStage
        {
            get { return _scanStage; }
            set
            {
                if (_scanStage == value)
                    return;

                _scanStage = value;

                if (value == ScanStage.Connecting
                    && string.IsNullOrEmpty(_statusMessageMemento))
                    _statusMessageMemento = StatusMessageSystem;

                RaisePropertyChanged("ScanStage");
                RaisePropertyChanged("ScanStatus");
                RaisePropertyChanged("ScanCompleted");
            }
        }

        public bool ScanCompleted
        {
            get { return (ScanStage == ScanStage.OperationCompleted && Items != null && Items.Count > 0) || Computers != null; }
        }

        public String StatusMessageSystem
        {
            get { return _statusMessage; }
            set
            {
                if (_statusMessage == value)
                {
                    return;
                }

                _statusMessage = value;
                RaisePropertyChanged("StatusMessage");
            }
        }

        public void RestoreConnectingState()
        {
            ScanStage = ScanStage.Connecting;
            StatusMessageSystem = _statusMessageMemento;
            FailedToAnalyzeCount = 0;
            MalwareFilesCount = 0;
            UnknownFilesCount = 0;
            NoVerdictFilesCount = 0;
            CleanFilesCount = 0;
        }

        public String StatusMessage
        {
            get
            {
                switch (ScanStage)
                {
                    case ScanStage.Unknown:
                    case ScanStage.Pending:
                    case ScanStage.Connecting:
                    case ScanStage.CopyingScanner:
                    case ScanStage.ConfiguringScanner:
                    case ScanStage.StartingScanner:
                    case ScanStage.ScanFailed:
                    case ScanStage.OperationFailed:
                    case ScanStage.OperationCanceled:
                        return StatusMessageSystem;


                    case ScanStage.ScanCompleted:
                    case ScanStage.ScanInProgress:
                    case ScanStage.ScanResultsObtaning:
                    case ScanStage.ScanResultsObtaned:
                    case ScanStage.ScanResultsProcessing:
                    case ScanStage.DescentOperationCompleted:
                    case ScanStage.OperationCompleted:

                        return GetStatusMessage();


                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }

        }

        public Int32 LookupInProgress => FailedToAnalyzeCount + NoVerdictFilesCount;
        private string GetStatusMessage()
        {

            string errorCounterText = LookupInProgress > 0 ?
                    $"Lookup in Progress {LookupInProgress}."
                    : String.Empty;


            if (ScanStage == ScanStage.OperationCompleted)
            {
                string message;
                if (LookupInProgress > 0)
                {
                    message = $"Completed: Total files scanned: {TotalFilesCount}. Unknown files: {UnknownFilesCount}. Malicious files: {MalwareFilesCount}. Lookup in Progress: {LookupInProgress}";
                }
                else
                {
                    message = $"Completed: Total files scanned: {TotalFilesCount}. Unknown files: {UnknownFilesCount}. Malicious files: {MalwareFilesCount}.";
                }
                return message;
            }
            else
            {
                return
                    $"{TotalFilesCount} files scanned ({$"{ProgressPercentage:0.00}".Replace(".00", string.Empty)}%). Unknown files: {UnknownFilesCount}. Malicious files: {MalwareFilesCount}. {errorCounterText}";
            }



        }



        public Int32 UnknownFilesCount
        {
            get { return _unknownFilesCount; }
            set
            {
                if (_unknownFilesCount == value)
                {
                    return;
                }
                _unknownFilesCount = value;
                RaisePropertyChanged(nameof(UnknownFilesCount));
                RaisePropertyChanged(nameof(HasBadFiles));
                RaisePropertyChanged(nameof(HasUnknowns));
                RaisePropertyChanged(nameof(HasFailed));
            }
        }

        public Int32 MalwareFilesCount
        {
            get { return _malwareFilesCount; }
            set
            {
                if (_malwareFilesCount == value)
                {
                    return;
                }

                _malwareFilesCount = value;
                RaisePropertyChanged(nameof(MalwareFilesCount));
                RaisePropertyChanged(nameof(HasBadFiles));
                RaisePropertyChanged(nameof(HasMalwares));
                RaisePropertyChanged(nameof(HasUnknowns));
                RaisePropertyChanged(nameof(HasFailed));
            }
        }

        public Int32 CleanFilesCount
        {
            get { return _cleanFilesCount; }
            set
            {
                if (_cleanFilesCount == value)
                {
                    return;
                }

                _cleanFilesCount = value;
                RaisePropertyChanged(nameof(CleanFilesCount));
            }
        }

        public Int32 FailedToAnalyzeCount
        {
            get { return _errorFilesCount; }
            set
            {
                if (_errorFilesCount == value)
                {
                    return;
                }

                _errorFilesCount = value;
                RaisePropertyChanged(nameof(FailedToAnalyzeCount));
                RaisePropertyChanged(nameof(LookupInProgress));
                RaisePropertyChanged(nameof(HasBadFiles));
                RaisePropertyChanged(nameof(HasFailed));
            }
        }

        public Int32 NoVerdictFilesCount
        {
            get { return _noVerdictFilesCount; }
            set
            {
                if (_noVerdictFilesCount == value)
                {
                    return;
                }

                _noVerdictFilesCount = value;
                RaisePropertyChanged(nameof(NoVerdictFilesCount));
                RaisePropertyChanged(nameof(LookupInProgress));
                RaisePropertyChanged(nameof(HasBadFiles));
                RaisePropertyChanged(nameof(HasFailed));
            }
        }



        public Int32 TotalFilesCount
        {
            get { return _totalFilesCount; }
            set
            {
                if (_totalFilesCount == value)
                {
                    return;
                }
                _totalFilesCount = value;
                RaisePropertyChanged("TotalFilesCount");
            }
        }

        public ObservableCollection<ThreatInfoItem> Items { get; }

        public int DistinctUnknownFilesCount
        {
            get
            {
                if (Items == null)
                    return 0;

                var items = Items.Where(_ => _.IsUnknown).GroupBy(_ => _.BadFile.Sha1).Select(_ => _.Key);
                return items.Count();
            }
        }

        public string InstancesCount
        {
            get
            {
                if (Computers == null || !Computers.Any())
                    return string.Empty;

                if (Computers.Count == 1)
                    return "1 instance.";

                var uniqueComputers = Computers.GroupBy(_ => _.ComputerGuid).Select(_ => _.Key).Count();

                return $"{Computers.Count} instances on {uniqueComputers}" +
                    $" computer{(uniqueComputers > 1 ? "s" : string.Empty)}.";
            }
        }

        public ThreatInfoItem()
        {
            StatusMessageSystem = "Initializing...";

            Items = new ObservableCollection<ThreatInfoItem>();
        }



        public ThreatInfoItem(DeploymentTarget deploymentTarget) : this()
        {
            DeploymentTarget = deploymentTarget;
            Name = deploymentTarget.Name;
            IpAddress = deploymentTarget.IPAddress;
            IsLocal = deploymentTarget.IsLocalComputer;
            IsComptuer = true;
            IsOnline = deploymentTarget.IsOnline;
            ComputerGuid = deploymentTarget.Uid;
            LdapPath = deploymentTarget.LdapPath;
        }

        public ThreatInfoItem(BadFileItem badFile)
        {
            Name = badFile.Name;
            Size = badFile.Size;
            SizeSorting = badFile.SizeSorting;
            SizeString = badFile.FileSizeFormatted;
            IsComptuer = false;
            IsOnline = true;
            ScanStage = ScanStage.OperationCompleted;
            BadFile = badFile;
            _valkyrieVerdict = ValkyrieVerdict.PendingFls;;
            if (!string.IsNullOrEmpty(badFile.Product))
                ShortName = $"{Path.GetFileName(badFile.Name)} ({badFile.Product})";
            else
                ShortName = Path.GetFileName(badFile.Name);
        }

        public bool UpdateFileVerdict(FileVerdictContext fileVerdictContext)
        {
            var prevFinalVerdict = FinalVerdict;
            if (fileVerdictContext.ValkyrieAnalysisResult != null)
            {
                SkipReason = fileVerdictContext.ValkyrieAnalysisResult.SkipReason;
                OperationStatus = fileVerdictContext.ValkyrieAnalysisResult.OperationStatus;
                ValkyrieVerdict = fileVerdictContext.ValkyrieAnalysisResult.Verdict;
            }
            if (fileVerdictContext.FlsVerdict != null)
            {
                UpdateFileVerdict(fileVerdictContext.FlsVerdict.Value);
            }
            return prevFinalVerdict != FinalVerdict;
            //MalwareProbabilityValue = valkyrieAnalysisResult.MalwareProbability;
        }

        public void UpdateVerdictForFiles(FileVerdictContext[] fileVerdictContext, IScanCounterDiff scanCounterDiff)
        {
            if (!IsComptuer) throw new NotSupportedException();
            foreach (var verdictContext in fileVerdictContext)
            {
                ThreatInfoItem item;
                if (_itemsDict.TryGetValue(verdictContext.Sha1, out item))
                {
                    switch (item.FinalVerdict)
                    {
                        case UiFinalVerdict.Clean:
                            scanCounterDiff.CleanFilesDiff--;
                            break;
                        case UiFinalVerdict.InAnalysis:
                            scanCounterDiff.InProgressFilesDiff--;
                            break;
                        case UiFinalVerdict.Unknown:
                            scanCounterDiff.UnknownFilesDiff--;
                            break;
                        case UiFinalVerdict.Malware:
                            scanCounterDiff.MaliciousFilesDiff--;
                            break;
                        case UiFinalVerdict.NotAnalyse:
                            scanCounterDiff.NotAnalyzedFilesDiff--;
                            break;
                    }
                    item.UpdateFileVerdict(verdictContext);
                    switch (item.FinalVerdict)
                    {
                        case UiFinalVerdict.Clean:
                            scanCounterDiff.CleanFilesDiff++;
                            break;
                        case UiFinalVerdict.InAnalysis:
                            scanCounterDiff.InProgressFilesDiff++;
                            break;
                        case UiFinalVerdict.Unknown:
                            scanCounterDiff.UnknownFilesDiff++;
                            break;
                        case UiFinalVerdict.Malware:
                            scanCounterDiff.MaliciousFilesDiff++;
                            break;
                        case UiFinalVerdict.NotAnalyse:
                            scanCounterDiff.NotAnalyzedFilesDiff++;
                            break;
                    }
                }
            }
        }

        public void AddFile(ThreatInfoItem threatInfoItem)
        {
            if (!IsComptuer) throw new NotSupportedException();
            if (threatInfoItem.IsComptuer) throw new ArgumentException();
            var shouldRaise = Items.Count == 0;
            Items.Add(threatInfoItem);
            _itemsDict.Add(threatInfoItem.BadFile.Sha1,threatInfoItem);
            if (shouldRaise)
                RaisePropertyChanged(nameof(Items));
        }

        public override string ToString()
        {
            return NameAndIpAddress;
        }
    }
}
