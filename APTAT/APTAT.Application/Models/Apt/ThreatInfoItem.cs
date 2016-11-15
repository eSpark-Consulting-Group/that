using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Application.Models
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

    public struct ScanStatus : IComparable
    {
        private readonly ScanStatusEnum _value;


        public ScanStatus(ScanStatusEnum value)
        {
            _value = value;
        }

        public override string ToString()
        {
            if(_value == ScanStatusEnum.InProgress)
            {
                return "In Progress";
            }


            return _value.ToString();
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override Boolean Equals(Object obj)
        {
            if(obj is ScanStatus)
            {
                ScanStatus status = (ScanStatus) obj;

                if(status._value == _value)
                {
                    return true;
                }
            }

            return false;
        }


        public int CompareTo(object obj)
        {
            return 0;
        }
    }

    public class ThreatInfoItem : DomainObject
    {
        private Int32 _totalFilesCount;
        private Int32 _unknownFilesCount;
        private Int32 _malwareFilesCount;
        private String _statusMessage;

        private ScanStage _scanStage;
        private ObservableCollection<ThreatInfoItem> _items;
        private int _errorFilesCount;
        private int _noVerdictFilesCount;
        private bool _isUnknown;
        private bool _isMalware;
        private bool _safe;

        public Boolean HasBadFiles
        {
            get { return UnknownFilesCount > 0 || MalwareFilesCount > 0 || NoVerdictFilesCount > 0 || FailedToAnalyzeCount > 0; }
        }

        public void UpdateFileVerdict(CloudVerdict verdict)
        {
            BadFile.Cloud = verdict;
            IsUnknown = BadFile.IsUnknown;
            IsMalware = BadFile.IsMalware;
            Clean = BadFile.Cloud == CloudVerdict.Safe;
        }

        public Boolean IsUnknown
        {
            get { return _isUnknown; }
            set { _isUnknown = value;
                RaisePropertyChanged(nameof(IsUnknown));
            }
        }

        public Boolean IsMalware
        {
            get { return _isMalware; }
            set { _isMalware = value;
                RaisePropertyChanged(nameof(IsMalware));
            }
        }

        public Boolean Clean
        {
            get { return _safe; }
            set
            {
                _safe = value;
                RaisePropertyChanged(nameof(Clean));
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
                return MalwareFilesCount == 0 && UnknownFilesCount == 0;
            }
        }

        public Boolean IsRootItem { get; set; }

        public Boolean IsOnline { get; set; }

        public String Name { get; set; }

        public string ShortName { get; set; }

        public String IpAddress { get; set; }

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
                if(!IsOnline)
                {
                    return new ScanStatus(ScanStatusEnum.Offline);
                }

                if(ScanStage == ScanStage.OperationCompleted)
                {
                    return new ScanStatus(ScanStatusEnum.Completed);
                }

                if(ScanStage == ScanStage.OperationFailed)
                {
                    return new ScanStatus(ScanStatusEnum.Failed);
                }

                if(ScanStage == ScanStage.OperationCanceled)
                {
                    return new ScanStatus(ScanStatusEnum.Canceled);
                }

                return new ScanStatus(IsRootItem ? ScanStatusEnum.InProgress : ScanStatusEnum.Child);
            }
        }

        public ScanStage ScanStage
        {
            get { return _scanStage; }
            set
            {
                _scanStage = value;
                RaisePropertyChanged("ScanStage");
                RaisePropertyChanged("ScanStatus");
                RaisePropertyChanged("ScanCompleted");
            }
        }

        public bool ScanCompleted
        {
            get { return (ScanStage == ScanStage.OperationCompleted && Items != null && Items.Count > 0) || Computers != null; }
        }

        public String StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if(_statusMessage == value)
                {
                    return;
                }

                _statusMessage = value;
                RaisePropertyChanged("StatusMessage");
            }
        }

        public Int32 UnknownFilesCount
        {
            get { return _unknownFilesCount; }
            set
            {
                if(_unknownFilesCount == value)
                {
                    return;
                }
                _unknownFilesCount = value;
                RaisePropertyChanged("UnknownFilesCount");
                RaisePropertyChanged("HasBadFiles");
                RaisePropertyChanged("HasUnknowns");
                RaisePropertyChanged("HasFailed");
            }
        }

        public Int32 MalwareFilesCount
        {
            get { return _malwareFilesCount; }
            set
            {
                if(_malwareFilesCount == value)
                {
                    return;
                }

                _malwareFilesCount = value;
                RaisePropertyChanged("MalwareFilesCount");
                RaisePropertyChanged("HasBadFiles");
                RaisePropertyChanged("HasMalwares");
                RaisePropertyChanged("HasUnknowns");
                RaisePropertyChanged("HasFailed");
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
                RaisePropertyChanged("FailedToAnalyzeCount");
                RaisePropertyChanged("HasBadFiles");
                RaisePropertyChanged("HasFailed");
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
                RaisePropertyChanged("NoVerdictFilesCount");
                RaisePropertyChanged("HasBadFiles");
                RaisePropertyChanged("HasFailed");
            }
        }



        public Int32 TotalFilesCount
        {
            get { return _totalFilesCount; }
            set
            {
                if(_totalFilesCount == value)
                {
                    return;
                }
                _totalFilesCount = value;
                RaisePropertyChanged("TotalFilesCount");
            }
        }

        public ObservableCollection<ThreatInfoItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

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

                return string.Format("{0} instances on {1} computer{2}.", Computers.Count,
                    uniqueComputers, uniqueComputers > 1 ? "s" : string.Empty);
            }
        }

        public Collection<ThreatInfoItem> Computers { get; set; }

        public BadFileItem BadFile { get; private set; }

        public ThreatInfoItem()
        {
            StatusMessage = "Initializing...";

            _items = new ObservableCollection<ThreatInfoItem>();
        }

        public ThreatInfoItem(DeploymentTarget deploymentTarget) : this()
        {
            Name = deploymentTarget.Name;
            IpAddress = deploymentTarget.IPAddress;
            IsRootItem = true;
            IsOnline = deploymentTarget.IsOnline;
            ComputerGuid = deploymentTarget.Uid;
            LdapPath = deploymentTarget.LdapPath;
        }

        public ThreatInfoItem(BadFileItem badFile)
        {
            IsUnknown = badFile.IsUnknown;
            IsMalware = badFile.IsMalware;
            Clean = badFile.Cloud == CloudVerdict.Safe;
            Name = badFile.Name;
            Size = badFile.Size;
            SizeSorting = badFile.SizeSorting;
            SizeString = badFile.FileSizeFormatted;
            IsRootItem = false;
            IsOnline = true;
            BadFile = badFile;
            if (!string.IsNullOrEmpty(badFile.Product))
                ShortName = string.Format("{0} ({1})", Path.GetFileName(badFile.Name), badFile.Product);
            else
                ShortName = Path.GetFileName(badFile.Name);
        }

        public override string ToString()
        {
            return NameAndIpAddress;
        }
    }
}
