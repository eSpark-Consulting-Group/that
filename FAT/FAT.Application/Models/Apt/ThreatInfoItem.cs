using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
        Clean = 1,
        Malware,
        Unknown,
        NotAnalyzed,
        InProgress
    }


    public class ThreatInfoItem : DomainObject
    {
        private Int32 _totalFilesCount;
        private Int32 _unknownFilesCount;
        private Int32 _malwareFilesCount;
        private Int32 _cleanFilesCount;
        private Int32 _notAnalyzedFilesCount;
        private Int32 _pendingFilesCount;
        private double? _valkyrieProgressPercentageCached;

        private String _statusMessage;
        private ValkyrieVerdict _valkyrieVerdict;
        private String _statusMessageMemento;

        private ScanStage _scanStage;
        private readonly Dictionary<string, List<ThreatInfoItem>> _itemsDict 
            = new Dictionary<string, List<ThreatInfoItem>>(StringComparer.OrdinalIgnoreCase);

        public Collection<ThreatInfoItem> Computers { get; set; }
        public float? MalwareProbabilityValue { get; set; }
        public BadFileItem BadFile { get; }

        public DeploymentTarget DeploymentTarget { get; }
        private double RemoteScanningPercentage { get; set; }

        public string SkipReason { get; set; }

        public string OperationStatus { get; set; }
        public bool IsWhiteListed { get; set; }

        public Boolean HasBadFiles
        {
            get
            {
                return UnknownFilesCount > 0 || MalwareFilesCount > 0 || PendingFilesCount > 0 ||
                       NotAnalyzedFilesCount > 0;
            }
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
        public string MalwareProbability
        {
            get
            {
                if (MalwareProbabilityValue.HasValue)
                {
                    return MalwareProbabilityValue.Value.ToString("P", CultureInfo.InvariantCulture);
                }
                return String.Empty;
            }
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
                if (BadFile.Cloud == CloudVerdict.Malware)
                    return UiFinalVerdict.Malware;
                if(BadFile.Cloud == CloudVerdict.Safe)
                    return UiFinalVerdict.Clean;
                switch (ValkyrieVerdict)
                {
                    case ValkyrieVerdict.NotAnalyzed:
                        return UiFinalVerdict.NotAnalyzed;
                    case ValkyrieVerdict.PotentiallyUnwantedApplication:
                    case ValkyrieVerdict.Malware:
                        return UiFinalVerdict.Malware;
                    case ValkyrieVerdict.Pending:
                    case ValkyrieVerdict.FirstSeen:
                        return UiFinalVerdict.InProgress;
                    case ValkyrieVerdict.Undetected:
                        return UiFinalVerdict.Unknown;
                    case ValkyrieVerdict.Clean:
                        return UiFinalVerdict.Clean;
                    case ValkyrieVerdict.PendingFls:
                    case ValkyrieVerdict.InQueue:
                        {
                            switch (BadFile.Cloud)
                            {
                                case CloudVerdict.Safe:
                                    return UiFinalVerdict.Clean;
                                case CloudVerdict.FirstSeen:
                                case CloudVerdict.FailedToAnalyze:
                                case CloudVerdict.NoVerdict:
                                case CloudVerdict.Unknown:
                                    return UiFinalVerdict.InProgress;
                                case CloudVerdict.Malware:
                                    return UiFinalVerdict.Malware;

                                default:
                                    throw new ArgumentOutOfRangeException(nameof(CloudVerdict));
                            }
                        }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ValkyrieVerdict));
                }
            }
        }

        public string VerdictString
        {
            get
            {
                if (IsComputer)
                    return string.Empty;

                switch (FinalVerdict)
                {
                    case UiFinalVerdict.Clean:
                        return "Clean";
                    case UiFinalVerdict.InProgress:
                        return "In Analysis";
                    case UiFinalVerdict.Malware:
                        return "Malware";
                    case UiFinalVerdict.NotAnalyzed:
                        return $"Failed ({OperationStatus.Replace(Environment.NewLine, "")})";
                    case UiFinalVerdict.Unknown:
                        return "No Threat Found";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(FinalVerdict));
                }
            }
        }

      
        public ThreatInfoItem[] FilesToCheckOnValkyrieCount { get; private set; }
        public void RemoteScanCompleted()
        {
            ThreatInfoItem[] files = Items.Where(i => i.ShouldCheckOnValkyrie).ToArray();
            FilesToCheckOnValkyrieCount = files;
            ReCalculateValkyriePercentage();
        }

        public double ValkyrieProgressPercentage { get; set; }


        private void ReCalculateValkyriePercentage()
        {
            if (_valkyrieProgressPercentageCached.HasValue)
                ValkyrieProgressPercentage = _valkyrieProgressPercentageCached.Value;
            else if (FilesToCheckOnValkyrieCount == null) ValkyrieProgressPercentage = 0;
            else if (FilesToCheckOnValkyrieCount.Length == 0)
            {
                ValkyrieProgressPercentage = 100;
                RaisePropertyChanged(nameof(ScanStatus));
            }
            else
            {
                var count = FilesToCheckOnValkyrieCount.Count(f => f.ValkyrieVerdict != ValkyrieVerdict.PendingFls
                                                                   && f.ValkyrieVerdict != ValkyrieVerdict.InQueue);
                var percentage = count / (double) FilesToCheckOnValkyrieCount.Length * 100;
                if (Math.Abs(percentage - 100) < 0.01)
                {
                    if( ScanStage == ScanStage.OperationCancelling)
                        ScanStage = ScanStage.OperationCanceled;
                    _valkyrieProgressPercentageCached = percentage;
                    ValkyrieProgressPercentage = percentage;
                    RaisePropertyChanged(nameof(ScanStatus));
                }
                else
                   ValkyrieProgressPercentage = percentage;
            }
        }

        public bool ShouldCheckOnValkyrie => BadFile.IsUnknown || BadFile.IsMalware;

        public bool IsMalware
        {
            get
            {
                return BadFile.IsMalware ||
                    ValkyrieVerdict == ValkyrieVerdict.Malware ||
                    ValkyrieVerdict == ValkyrieVerdict.PotentiallyUnwantedApplication;
            }
        }

        public bool Pending
        {
            get
            {
                return !IsMalware && !IsUnknown && !Clean &&
                    (ValkyrieVerdict == ValkyrieVerdict.PendingFls || 
                    ValkyrieVerdict == ValkyrieVerdict.InQueue ||
                    ValkyrieVerdict == ValkyrieVerdict.FirstSeen || 
                    ValkyrieVerdict == ValkyrieVerdict.Pending);
            }
        }

        public bool IsUnknown
        {
            get
            {
                return !IsMalware && 
                    ValkyrieVerdict == ValkyrieVerdict.Undetected;
            }

        }

        public bool Clean
        {
            get
            {
                return BadFile.IsClean ||
                    ValkyrieVerdict == ValkyrieVerdict.Clean;
            }
        }

        public Boolean Failed
        {
            get
            {
                return !IsUnknown && !IsMalware && !Clean && !Pending;
            }
        }

        public bool HasClean
        {
            get
            {
                return ScanStage == ScanStage.OperationCompleted && !HasBadFiles;
            }
        }

        public Boolean HasMalwares
        {
            get
            {
                return ScanStage == ScanStage.OperationCompleted && MalwareFilesCount > 0;
            }
        }

        public Boolean HasUnknowns
        {
            get
            {
                return ScanStage == ScanStage.OperationCompleted && !HasMalwares && UnknownFilesCount > 0;
            }
        }

        public bool HasFailed
        {
            get
            {
                return ScanStage == ScanStage.OperationFailed || (ScanStage == ScanStage.OperationCompleted && !HasMalwares && !HasUnknowns && !HasPending && NotAnalyzedFilesCount > 0);
            }
        }

        public Boolean HasPending
        {
            get
            {
                return ScanStage == ScanStage.OperationCompleted && !HasMalwares && !HasUnknowns && PendingFilesCount > 0;
            }
        }

        public Boolean IsComputer { get; set; }

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

                if (ScanStage == ScanStage.OperationFailed)
                {
                    return new ScanStatus(ScanStatusEnum.Failed);
                }

                if (ScanStage == ScanStage.OperationCanceled)
                {
                    return new ScanStatus(ScanStatusEnum.Canceled);
                }

                if (TotalProgressPercentage == 100)
                {
                    return new ScanStatus(ScanStatusEnum.Completed);
                }

                return new ScanStatus(IsComputer ? ScanStatusEnum.InProgress : ScanStatusEnum.Child);
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
                RaisePropertyChanged(nameof(ScanStatus));
                RaisePropertyChanged(nameof(StatusMessage));
                RaisePropertyChanged("ScanCompleted");

                RaisePropertyChanged(nameof(HasClean));
                RaisePropertyChanged(nameof(HasMalwares));
                RaisePropertyChanged(nameof(HasUnknowns));
                RaisePropertyChanged(nameof(HasFailed));
                RaisePropertyChanged(nameof(HasPending));
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
            PendingFilesCount = 0;
            MalwareFilesCount = 0;
            UnknownFilesCount = 0;
            _valkyrieProgressPercentageCached = null;
            NotAnalyzedFilesCount = 0;
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

                        case ScanStage.OperationCancelling:
                        return "Canceling...";


                    case ScanStage.ScanCompleted:
                    case ScanStage.ScanInProgress:
                    case ScanStage.ScanResultsObtaning:
                    case ScanStage.ScanResultsObtaned:
                    case ScanStage.ScanResultsProcessing:
                    case ScanStage.DescentOperationCompleted:
                    case ScanStage.OperationCompleted:
                    case ScanStage.RemoteOperationCompleted:
                        return GetStatusMessage();


                    default:
                        throw new ArgumentOutOfRangeException();
                }

            }

        }
        
        private string GetStatusMessage()
        {

            string errorCounterText = PendingFilesCount > 0 ?
                    $"In Analysis {PendingFilesCount}."
                    : String.Empty;

            
            
            if (Math.Abs(TotalProgressPercentage - 100) < 0.1)
            {
                string message = $"Completed: Total scanned: {TotalFilesCount}. Unknown: {UnknownFilesCount}. Malicious: {MalwareFilesCount}.";
                if (PendingFilesCount > 0)
                {
                    message += $" In Analysis: {PendingFilesCount}.";
                }
                if (NotAnalyzedFilesCount > 0)
                {
                    message += $" Not Analyzed: {NotAnalyzedFilesCount}";
                }
                return message;
            }
            else
            {
                
                var m =     $"{TotalFilesCount} scanned ({$"{TotalProgressPercentage:0.00}".Replace(".00", string.Empty)}%). Unknown: {UnknownFilesCount}. Malicious: {MalwareFilesCount}. {errorCounterText}";
                return m;
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
                RaisePropertyChanged(nameof(HasClean));
                RaisePropertyChanged(nameof(HasUnknowns));
                RaisePropertyChanged(nameof(HasPending));
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
                RaisePropertyChanged(nameof(HasClean));
                RaisePropertyChanged(nameof(HasMalwares));
                RaisePropertyChanged(nameof(HasUnknowns));
                RaisePropertyChanged(nameof(HasPending));
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

        public Int32 PendingFilesCount
        {
            get { return _pendingFilesCount; }
            set
            {
                if (_pendingFilesCount == value)
                {
                    return;
                }

                _pendingFilesCount = value;
                RaisePropertyChanged(nameof(PendingFilesCount));
                RaisePropertyChanged(nameof(HasBadFiles));
                RaisePropertyChanged(nameof(HasClean));
                RaisePropertyChanged(nameof(HasPending));
                RaisePropertyChanged(nameof(HasFailed));
            }
        }

        public Int32 NotAnalyzedFilesCount
        {
            get { return _notAnalyzedFilesCount; }
            private set
            {
                if (_notAnalyzedFilesCount == value)
                {
                    return;
                }

                _notAnalyzedFilesCount = value;
                RaisePropertyChanged(nameof(NotAnalyzedFilesCount));
                RaisePropertyChanged(nameof(HasBadFiles));
                RaisePropertyChanged(nameof(HasClean));
                RaisePropertyChanged(nameof(HasFailed));
            }
        }



        public Int32 TotalFilesCount
        {
            get { return _totalFilesCount; }
            private set
            {
                if (_totalFilesCount == value)
                {
                    return;
                }
                _totalFilesCount = value;
                RaisePropertyChanged(nameof(TotalFilesCount));
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

        public double TotalProgressPercentage
        {
            get
            {

                if (ScanStage == ScanStage.OperationCanceled || ScanStage == ScanStage.OperationFailed ||
                    ScanStage == ScanStage.ScanFailed)
                    return 100;
                var valkyriePercent = ValkyrieProgressPercentage / 2;
                if (valkyriePercent > 50) valkyriePercent = 50;
                return RemoteScanningPercentage/2 + valkyriePercent;
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
            IsComputer = true;
            IsOnline = deploymentTarget.IsOnline;
            ComputerGuid = deploymentTarget.Uid;
            LdapPath = deploymentTarget.LdapPath;

            if (Name == IpAddress)
                Name = deploymentTarget.DnsName;
        }

        public ThreatInfoItem(BadFileItem badFile)
        {
            Name = badFile.Name;
            Size = badFile.Size;
            SizeSorting = badFile.SizeSorting;
            SizeString = badFile.FileSizeFormatted;
            IsComputer = false;
            IsOnline = true;
            ScanStage = ScanStage.OperationCompleted;
            BadFile = badFile;
            _valkyrieVerdict = ValkyrieVerdict.PendingFls;
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
                IsWhiteListed = fileVerdictContext.ValkyrieAnalysisResult.IsWhiteListed;
                MalwareProbabilityValue = fileVerdictContext.ValkyrieAnalysisResult.MalwareProbability;
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
            if (!IsComputer) throw new NotSupportedException();
            bool anyUpdated = false;
            bool shouldUpdateStatus = false;
            var cleanDiff = 0;
            var unknownDiff = 0;
            var malwareDiff = 0;
            var notAnalyzedDiff = 0;
            var inAnalysizDiff = 0;
            foreach (var verdictContext in fileVerdictContext)
            {
                List<ThreatInfoItem> items;
                if (_itemsDict.TryGetValue(verdictContext.Sha1, out items))
                {
                    shouldUpdateStatus = true;
                    foreach (var item in items)
                    {
                        switch (item.FinalVerdict)
                        {
                            case UiFinalVerdict.InProgress:
                                inAnalysizDiff--;
                                break;
                            case UiFinalVerdict.Unknown:
                                unknownDiff--;
                                break;
                            case UiFinalVerdict.Malware:
                                malwareDiff--;
                                break;
                            case UiFinalVerdict.NotAnalyzed:
                                notAnalyzedDiff--;
                                break;
                            case UiFinalVerdict.Clean:
                                cleanDiff--;
                                break;
                        }
                        anyUpdated = anyUpdated | item.UpdateFileVerdict(verdictContext);
                        switch (item.FinalVerdict)
                        {
                            case UiFinalVerdict.Clean:
                                cleanDiff++;
                                break;
                            case UiFinalVerdict.InProgress:
                                inAnalysizDiff++;
                                break;
                            case UiFinalVerdict.Unknown:
                                unknownDiff++;
                                break;
                            case UiFinalVerdict.Malware:
                                malwareDiff++;
                                break;
                            case UiFinalVerdict.NotAnalyzed:
                                notAnalyzedDiff++;
                                break;
                        }
                    }
                }
            }
            if (anyUpdated)
            {
                CleanFilesCount += cleanDiff;
                scanCounterDiff.CleanDiff += cleanDiff;
                UnknownFilesCount += unknownDiff;
                scanCounterDiff.UnknownDiff += unknownDiff;
                MalwareFilesCount += malwareDiff;
                scanCounterDiff.MalwareDiff += malwareDiff;
                NotAnalyzedFilesCount += notAnalyzedDiff;
                scanCounterDiff.NotAnalyzedDiff += notAnalyzedDiff;
                PendingFilesCount += inAnalysizDiff;
                scanCounterDiff.InProgressDiff += inAnalysizDiff;
            }
            if (shouldUpdateStatus)
            {
                ReCalculateValkyriePercentage();
                RaisePropertyChanged(nameof(StatusMessage));
            }
        }


        public void UpdateCounters(ScanResultItem scanProgress, IScanCounterDiff counterDiff)
        {
            counterDiff.CleanDiff += scanProgress.CleanFilesCount - CleanFilesCount;
            counterDiff.InProgressDiff += scanProgress.UnknownFilesCount + scanProgress.FailedToAnalyzeFilesCount +
                                                scanProgress.NoVerdictFilesCount - PendingFilesCount;
            counterDiff.MalwareDiff += scanProgress.MalwareFilesCount - MalwareFilesCount;
            
            TotalFilesCount = scanProgress.TotalFilesCount;
            CleanFilesCount = scanProgress.CleanFilesCount;
            PendingFilesCount = scanProgress.UnknownFilesCount + scanProgress.NoVerdictFilesCount + scanProgress.FailedToAnalyzeFilesCount;
            MalwareFilesCount = scanProgress.MalwareFilesCount;
            RemoteScanningPercentage = scanProgress.Percentage;
            StatusMessageSystem = scanProgress.StatusMessage;
            ScanStage = scanProgress.ScanStage;
            RaisePropertyChanged(nameof(StatusMessage));
        }

        public void AddFile(ThreatInfoItem threatInfoItem)
        {
            if (!IsComputer) throw new NotSupportedException();
            if (threatInfoItem.IsComputer) throw new ArgumentException();
            var shouldRaise = Items.Count == 0;
            Items.Add(threatInfoItem);
            if (_itemsDict.ContainsKey(threatInfoItem.BadFile.Sha1))
                _itemsDict[threatInfoItem.BadFile.Sha1].Add(threatInfoItem);
            else
                _itemsDict.Add(threatInfoItem.BadFile.Sha1, new List<ThreatInfoItem> {threatInfoItem});
            if (shouldRaise)
                RaisePropertyChanged(nameof(Items));
        }

        public void TrySetCancelledStatus()
        {
            if (ScanStage != ScanStage.OperationCompleted) return;
            if (ValkyrieProgressPercentage != 100)
            {
                ScanStage = ScanStage.OperationCancelling;
            }
        }

        public override string ToString()
        {
            return NameAndIpAddress;
        }
    }
}
