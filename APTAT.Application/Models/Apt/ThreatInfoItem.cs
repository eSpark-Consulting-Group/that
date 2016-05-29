using System;
using System.Collections.ObjectModel;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Application.Models
{
    public enum ScanStatusEnum
    {
        Offline,
        InProgress,
        Cancelled,
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
        private Int32 _progress;
        private Int32 _totalFilesCount;
        private Int32 _unknownFilesCount;
        private Int32 _malwareFilesCount;
        private String _statusMessage;

        private ScanStage _scanStage;
        private ObservableCollection<ThreatInfoItem> _items;
        private int _errorFilesCount;
        private int _serverErrorFilesCount;

        public Boolean HasBadFiles
        {
            get { return UnknownFilesCount > 0 || MalwareFilesCount > 0 || ServerErrorFilesCount > 0 || ErrorFilesCount > 0; }
        }
        
        public Boolean IsUnknown { get; set; }

        public Boolean IsMalware { get; set; }

        public Boolean IsRootItem { get; set; }

        public Boolean IsOnline { get; set; }

        public String Name { get; set; }

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

        public String SizeString { get; set; }

        public Int64 Size { get; set; }

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
                    return new ScanStatus(ScanStatusEnum.Cancelled);
                }

                return new ScanStatus(ScanStatusEnum.InProgress);
            }
        }

        public ScanStage ScanStage
        {
            get { return _scanStage; }
            set
            {
                if(_scanStage == value)
                {
                    return;
                }

                _scanStage = value;
                RaisePropertyChanged("ScanStage");
                RaisePropertyChanged("ScanStatus");
                RaisePropertyChanged("ScanInProgress");
            }
        }

        public bool ScanCompleted
        {
            get { return ScanStage == ScanStage.OperationCompleted && Items != null && Items.Count > 0; }
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
            }
        }

        public Int32 ErrorFilesCount
        {
            get { return _errorFilesCount; }
            set
            {
                if (_errorFilesCount == value)
                {
                    return;
                }

                _errorFilesCount = value;
                RaisePropertyChanged("ErrorFilesCount");
                RaisePropertyChanged("HasBadFiles");
            }
        }

        public Int32 ServerErrorFilesCount
        {
            get { return _serverErrorFilesCount; }
            set
            {
                if (_serverErrorFilesCount == value)
                {
                    return;
                }

                _serverErrorFilesCount = value;
                RaisePropertyChanged("ServerErrorFilesCount");
                RaisePropertyChanged("HasBadFiles");
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

        public Int32 Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged("Progress");
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

        public BadFileItem BadFile { get; set; }

        public ThreatInfoItem()
        {
            StatusMessage = "Initializing...";
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

        public override string ToString()
        {
            return Name;
        }
    }
}
