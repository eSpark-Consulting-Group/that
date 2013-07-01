using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using APTAT.Application.Services;
using Telerik.Windows.Controls;

namespace APTAT.Application.Models
{
    public class MainViewViewModel : ViewModel
    {
        public event EventHandler<ScanCompletedEventArgs> ScanProcessCompleted;

        private readonly IAptService _aptService;
        private readonly IStorageService _storage;
        private Guid _operationGuid;
        private DirectoryItem[] _selectedTargets;

        private Boolean _isReportEnabled;
        private Boolean? _isScanInProgress;
        private Int32 _scannedItemsCount;
        
        private Int32 _unknownFilesCount;
        private Int32 _malwareFilesCount;

        private ThreatInfoItem[] _onlineItems;
        private ThreatInfoItem _selectedItem;
        private ObservableCollection<ThreatInfoItem> _threatInfoItems;

        public Boolean? IsScanInProgress
        {
            get { return _isScanInProgress; }
            set
            {
                _isScanInProgress = value;
                RaisePropertyChanged("IsScanInProgress");
            }
        }

        public Int32 UnknownFilesCount
        {
            get { return _unknownFilesCount; }
            set
            {
                _unknownFilesCount = value;
                RaisePropertyChanged("UnknownFilesCount");
            }
        }

        public Int32 MalwareFilesCount
        {
            get { return _malwareFilesCount; }
            set
            {
                _malwareFilesCount = value;
                RaisePropertyChanged("MalwareFilesCount");
            }
        }

        public Int32 ScannedItemsCount
        {
            get { return _scannedItemsCount; }
            set
            {
                _scannedItemsCount = value;
                RaisePropertyChanged("ScannedItemsCount");
            }
        }

        public Int32 Percentage { get; set; }

        public String Header
        {
            get
            {
                if (IsScanInProgress == true && OnlineItemsCount > 1)
                {
                    return String.Format("APT ASSESSMENT TOOL ({0}% scanning...)", Percentage);
                }
                return "APT ASSESSMENT TOOL";
            }
        }

        public Int32 OnlineItemsCount
        {
            get
            {
                if(_onlineItems == null)
                {
                    return 0;
                }

                return _onlineItems.Length;
            }
        }

        public Boolean IsDetailsEnabled
        {
            get { return SelectedItem != null && SelectedItem.IsRootItem == false; }
        }

        public Boolean IsReportEnabled
        {
            get { return _isReportEnabled; }
            set
            {
                _isReportEnabled = value;
                RaisePropertyChanged("IsReportEnabled");
            }
        }

        public Int32 ItemsCount
        {
            get
            {
               if(Items == null)
               {
                   return 0;
               }

                return Items.Count;
            }
        }

        public ThreatInfoItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                RaisePropertyChanged("IsDetailsEnabled");

                if(_selectedItem == null)
                {
                    _storage.SetValue(KnownKeys.UnknownFile, null);    
                }
                else
                {
                    _storage.SetValue(KnownKeys.UnknownFile, _selectedItem.BadFile);    
                }
                
            }
        }

        public Boolean IsItemsEmpty
        {
            get { return Items == null || Items.Count == 0; }
        }


        public ObservableCollection<ThreatInfoItem> Items
        {
            get { return _threatInfoItems; }
            set
            {
                _threatInfoItems = value;
                RaisePropertyChanged("Items");
                RaisePropertyChanged("ItemsCount");
                RaisePropertyChanged("IsItemsEmpty");
            }
        }

        public DelegateCommand StartScanCommand { get; private set; }

        public DelegateCommand StopScanCommand { get; private set; }

        
        
        public MainViewViewModel()
        {
            _aptService = AptService.Instance;
            _storage = new StorageService();

            StartScanCommand = new DelegateCommand(InvokeStartScan);
            StopScanCommand = new DelegateCommand(InvokeStopScan);
        }

        private void InvokeStartScan(Object arg)
        {
            Items = new ObservableCollection<ThreatInfoItem>();
            _onlineItems = new ThreatInfoItem[0];
            SelectedItem = null;
            UnknownFilesCount = 0;
            IsReportEnabled = false;

            ICollection<Object> value = _storage.GetValue<ICollection<Object>>(KnownKeys.CheckedDirectoryItems, true);
            if (value != null)
            {
                _selectedTargets = value.OfType<DirectoryItem>().ToArray();
                if (_selectedTargets.Length > 0)
                {
                    IsScanInProgress = null;
                    IsBusy = true;        
                    _aptService.ResolveDirectoryItems(_selectedTargets, this);
                }
            }
        }

        private void InvokeStopScan(Object arg)
        {
            foreach (ThreatInfoItem onlineItem in _onlineItems)
            {
                if(onlineItem.ScanStage != ScanStage.OperationCompleted && onlineItem.ScanStage != ScanStage.OperationFailed)
                {
                    onlineItem.StatusMessage = "Cancelling...";    
                }
            }

            _aptService.StopScan();
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            IsScanInProgress = false;
           
            _aptService.DirectoryItemsResolved += OnDirectoryItemsResolved;
            _aptService.BeginScanCompleted += OnBeginScanCompleted;
            _aptService.ScanStatusChanged += OnScanStatusChanged;
            _aptService.GetScanResultCompleted += OnGetScanResultCompleted;
            _aptService.ScanCompleted += OnScanCompleted;
            _aptService.ScanProgressChanged += OnScanProgressChanged;
        }

    

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _aptService.DirectoryItemsResolved -= OnDirectoryItemsResolved;
            _aptService.BeginScanCompleted -= OnBeginScanCompleted;
            _aptService.ScanStatusChanged -= OnScanStatusChanged;
            _aptService.GetScanResultCompleted -= OnGetScanResultCompleted;
            _aptService.ScanCompleted -= OnScanCompleted;
            _aptService.ScanProgressChanged -= OnScanProgressChanged;
        }

        private void OnScanCompleted(Object sender, ScanCompletedEventArgs e)
        {
            if(ScanProcessCompleted != null)
            {
                ScanProcessCompleted(this, e);    
            }
        }

        private void OnDirectoryItemsResolved(Object sender, DirectoryItemsResolvedEventArgs e)
        {
            _onlineItems = e.Items.Where(item => item.IsOnline).ToArray();
            RaisePropertyChanged("OnlineItemsCount");

            Items = new ObservableCollection<ThreatInfoItem>(e.Items);
            IsBusy = false;
            IsScanInProgress = true;

            String login = _storage.GetValue<String>(KnownKeys.Login, false);
            String password = _storage.GetValue<String>(KnownKeys.Password, true);
            String domain = _storage.GetValue<String>(KnownKeys.Domain, false);

            foreach (ThreatInfoItem item in Items)
            {
                if(item.IsOnline == false)
                {
                    item.StatusMessage = "This computer is currently offline";
                }
            }

            if(_onlineItems.Length > 0)
            {
                _aptService.BeginScan(Items.ToArray(), domain, login, password, this);    
            }
            else
            {
                IsScanInProgress = false;
            }
        }

        private void OnBeginScanCompleted(Object sender, BeginScanCompletedEventArgs e)
        {
            if(e.Exception != null)
            {
              //  MessageBox.Show(String.Format("Exception: {0}", e.Exception.Message));    
            }
            else
            {
                _operationGuid = e.OperationGuid;
            }
        }

        private void ScanCompleted()
        {
            IsScanInProgress = false;
            IsReportEnabled = true;
            Percentage = 100;
            
            _storage.SetValue(KnownKeys.OperationGuid, _operationGuid);
            // MessageBox.Show("Scan Completed. " + UnknownFilesCount + " unknown files found.", "Scan Completed!");
        }

        private void OnScanProgressChanged(Object sender, ScanProgressChangedEventArgs e)
        {
            if(_operationGuid == e.OperationGuid)
            {
                Percentage = e.Percentage;
                RaisePropertyChanged("Header");
            }
        }

        private void OnScanStatusChanged(Object sender, ScanStatusChangedEventArgs e)
        {
            for(Int32 i = 0; i < e.ScanProgress.Length; ++i)
            {
               ScanProgressItem scanProgress = e.ScanProgress[i];
               if (scanProgress.OperationUid != _operationGuid)
               {
                   continue;
               }
               for (Int32 j = 0; j < _onlineItems.Length; ++j)
               {
                   ThreatInfoItem infoItem = _onlineItems[j];
                   if(infoItem.ComputerGuid == scanProgress.ComputerGuid)
                   {
                       infoItem.TotalFilesCount = scanProgress.TotalFilesCount;
                       infoItem.UnknownFilesCount = scanProgress.UnknownFilesCount;
                       infoItem.MalwareFilesCount = scanProgress.MalwareFilesCount;
                       infoItem.StatusMessage = scanProgress.StatusMessage;
                       if(infoItem.ScanStage != ScanStage.OperationCompleted && scanProgress.ScanStage == ScanStage.OperationCompleted)
                       {
                           _aptService.GetScanResult(infoItem.ComputerGuid, this);
                          
                       }
           
                       infoItem.ScanStage = scanProgress.ScanStage;
                       break;
                   }
               }
            }

            Int32 completed = 0;
            Int32 scanned = 0;
            Int32 unknownFiles = 0;
            Int32 malwareFiles = 0;
            for (Int32 j = 0; j < _onlineItems.Length; ++j)
            {
                ThreatInfoItem infoItem = _onlineItems[j];
                if (infoItem.ScanStage == ScanStage.OperationFailed || infoItem.ScanStage == ScanStage.OperationCompleted || infoItem.ScanStage == ScanStage.OperationCanceled)
                {
                    completed++;
                    scanned++;
                }

                //if(infoItem.ScanStage == ScanStage.OperationCompleted)
                //{
                //    scanned++;
                //}

                unknownFiles += infoItem.UnknownFilesCount;
                malwareFiles += infoItem.MalwareFilesCount;
            }

            if(unknownFiles > UnknownFilesCount)
            {
                UnknownFilesCount = unknownFiles;    
            }

            if(malwareFiles > MalwareFilesCount)
            {
                MalwareFilesCount = malwareFiles;
            }
            
            ScannedItemsCount = scanned;

            if (completed == OnlineItemsCount)
            {
                ScanCompleted();
            }

            RaisePropertyChanged("Header");
        }

        private void OnGetScanResultCompleted(Object sender, GetScanResultCompletedEventArgs e)
        {
            ThreatInfoItem parentItem = _onlineItems.FirstOrDefault(item => item.ComputerGuid == e.ScanResult.TargetUid);
            if (parentItem != null)
            {
        
                ObservableCollection<ThreatInfoItem> items = new ObservableCollection<ThreatInfoItem>();        
                if(e.ScanResult.BadFiles != null)
                {
                    foreach (BadFileItem badFile in e.ScanResult.BadFiles)
                    {
                        items.Add(new ThreatInfoItem
                        {
                            IsUnknown =  badFile.IsUnknown,
                            IsMalware = badFile.IsMalware,
                            Name = badFile.Name,
                            Size = badFile.Size,
                            SizeString = badFile.Size / 1024 + " KB",
                            IsRootItem = false,
                            IsOnline = true,
                            BadFile = badFile,
                        });
                    }    
                }

                parentItem.Items = items;
            }
        }
      
    }
}
