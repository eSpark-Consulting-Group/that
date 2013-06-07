using System;
using System.Collections.ObjectModel;

namespace APTAT.Application.Models
{
    public class ThreatInfoItem : DomainObject
    {
        private Int32 _progress;
        private Int32 _totalFilesCount;
        private Int32 _unknownFilesCount;
        private String _statusMessage;
        private ScanStage _scanStage;
        private ObservableCollection<ThreatInfoItem> _items;

        public Boolean IsRootItem { get; set; }

        public Boolean IsOnline { get; set; }

        public String Name { get; set; }

        public String Size { get; set; }

        public String Type { get; set; }

        public Guid ComputerGuid { get; set; }

        public Object Target { get; set; }

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
            }
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

        public ThreatInfoItem()
        {
            StatusMessage = "Initializing...";
        }
    }
}
