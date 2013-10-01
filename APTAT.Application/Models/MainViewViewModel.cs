using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using APTAT.Application.Services;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using System.Threading;

namespace APTAT.Application.Models
{
    

    public class MainViewViewModel : ViewModel
    {
        public event EventHandler<ScanCompletedEventArgs> ScanProcessCompleted;
        public event EventHandler<DomainEventArgs> DomainTargetsResolveFailure;

        private String _login;
        private String _password;

        private readonly IAptService _aptService;
        private readonly IStorageService _storage;
        private Guid _operationGuid;
        private DeploymentItem[] _selectedTargets;

       // private Boolean _resolvingTargets;
        private Boolean _isReportEnabled;
        private Boolean? _isScanInProgress;
        private Int32 _scannedItemsCount;
        
        private Int32 _unknownFilesCount;
        private Int32 _malwareFilesCount;

        private ThreatInfoItem[] _onlineItems;
        private ThreatInfoItem _selectedItem;
        private ObservableCollection<ThreatInfoItem> _items;
        private ObservableCollection<ThreatInfoItem> _cache;

        //public Boolean ResolvingTargets
        //{
        //    get { return _resolvingTargets; }
        //    set
        //    {
        //        _resolvingTargets = value;
        //        RaisePropertyChanged("ResolvingTargets");
        //    }
        //}

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
                if (IsScanInProgress == true )
                {
                    return String.Format("APT Assessment Tool ({0}% scanning...)", Percentage);
                }
                return "APT Assessment Tool";
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
            get { return _cache == null || _cache.Count == 0; }
        }


        public ObservableCollection<ThreatInfoItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
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
            _cache = new ObservableCollection<ThreatInfoItem>();
            _onlineItems = new ThreatInfoItem[0];
            SelectedItem = null;
            UnknownFilesCount = 0;
            IsReportEnabled = false;

            ICollection<Object> value = _storage.GetValue<ICollection<Object>>(KnownKeys.CheckedDirectoryItems, true);
            if (value != null)
            {
                _selectedTargets = value.OfType<DeploymentItem>().ToArray();
                if (_selectedTargets.Length > 0)
                {
                    IsScanInProgress = null;
                    IsBusy = true;
                    _login = _storage.GetValue<String>(KnownKeys.Login, false);
                    _password = _storage.GetValue<String>(KnownKeys.Password, true);
                    if (_login == null || _password == null)
                    {
                        throw new InvalidOperationException("specify login and password");
                    }

                    _aptService.ResolveDirectoryItems(_selectedTargets, _login, _password, this);

                }
            }

            //var items = new ObservableCollection<ThreatInfoItem>();
            //for (int i = 0; i < 100; ++i)
            //{
            //    ThreatInfoItem item = new ThreatInfoItem();
            //    item.Name = "Test " + i;
            //    items.Add(item);
            //    item.IsRootItem = true;
            //    item.IsOnline = true;
            //    item.ScanStage = ScanStage.ScanInProgress;
            //    item.Items = new RadObservableCollection<ThreatInfoItem>();


                
            //}

            //Items = items;
            //_cache = items;



            //Thread t = new Thread(T);
            //t.Start();
        }

        //void T()
        //{
            


           
        //    Action<Int32> action = (index) =>
        //    {



        //        ThreatInfoItem item = _cache[index];
        //        item.TempItems = new ObservableCollection<ThreatInfoItem>();


        //            for (Int32 j = 0; j < 5; ++j)
        //            {
        //                item.TempItems.Add(new ThreatInfoItem
        //                {
        //                    IsOnline = true,

                            
        //                    BadFile = new BadFileItem
        //                    {
        //                        Name = "Bad",
        //                    },
        //                    IsCompletedSubItem = false,
        //                    Name = "Bad File",
        //                    IsUnknown = true,
        //                });
        //            }

        //            item.ScanStage = ScanStage.OperationCompleted;
        //            item.UnknownFilesCount = 5;

        //            Int32 itemIndex = Items.IndexOf(item);
        //            if (itemIndex != -1)
        //            {
        //                Items.Remove(item);
        //                Items.Insert(itemIndex, item);
        //            }
                    
                
               
                
        //    };

        //    for (Int32 i = 0; i < 100; i+= 2)
        //    {
        //        Thread.Sleep(500);
        //        DispatcherHelper.Current.BeginInvoke(action, i);
        //    }

        //    for (Int32 i = 1; i < 100; i += 2)
        //    {
        //        Thread.Sleep(500);
        //        DispatcherHelper.Current.BeginInvoke(action, i);
        //    }
            

        //}

        private void InvokeStopScan(Object arg)
        {
            foreach (ThreatInfoItem onlineItem in _onlineItems)
            {
                if(onlineItem.ScanStage != ScanStage.OperationCompleted && onlineItem.ScanStage != ScanStage.OperationFailed && onlineItem.ScanStage != ScanStage.OperationCanceled)
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
            IsBusy = false;
            IsScanInProgress = true;
            ScannedItemsCount = 0;

            if (e.Exception != null)
            {
                if (DomainTargetsResolveFailure != null)
                {
                    DomainTargetsResolveFailure(this, new DomainEventArgs {Exception = e.Exception});
                }
                IsScanInProgress = false;
                return;
            }

            if (e.Items == null)
            {
                return;
            }


            _onlineItems = e.Items.Where(item => item.IsOnline).ToArray();
            RaisePropertyChanged("OnlineItemsCount");

            _cache = new ObservableCollection<ThreatInfoItem>(e.Items);
            Items =
                new ObservableCollection<ThreatInfoItem>(
                    _cache.Where(item => item.NameAndIpAddress.ContainsString(Keyword)));
           
    
            String domain = _storage.GetValue<String>(KnownKeys.Domain, false);

            foreach (ThreatInfoItem item in _cache)
            {
                if (item.IsOnline == false)
                {
                    item.StatusMessage = "This computer is currently offline";
                }
            }

            if (_onlineItems.Length > 0)
            {
                if (_login == null || _password == null)
                {
                    throw new InvalidOperationException("specify login and password");
                }
                _aptService.BeginScan(_cache.ToArray(), domain, _login, _password, this);
                _login = null;
                _password = null;
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

        private void ScanCompleted(Boolean enableReports)
        {
            IsScanInProgress = false;
            IsReportEnabled = enableReports;
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
            for (Int32 i = 0; i < e.ScanProgress.Length; ++i)
            {
                ScanProgressItem scanProgress = e.ScanProgress[i];
                if (scanProgress.OperationUid != _operationGuid)
                {
                    continue;
                }
                for (Int32 j = 0; j < _onlineItems.Length; ++j)
                {
                    ThreatInfoItem infoItem = _onlineItems[j];
                    if (infoItem.ComputerGuid == scanProgress.ComputerGuid)
                    {
                        infoItem.TotalFilesCount = scanProgress.TotalFilesCount;
                        infoItem.UnknownFilesCount = scanProgress.UnknownFilesCount;
                        infoItem.MalwareFilesCount = scanProgress.MalwareFilesCount;
                        infoItem.StatusMessage = scanProgress.StatusMessage;
                        if (infoItem.ScanStage != ScanStage.OperationCompleted &&
                            scanProgress.ScanStage == ScanStage.OperationCompleted)
                        {
                            _aptService.GetScanResult(infoItem.ComputerGuid, this);
                        }

                        if (infoItem.ScanStage != scanProgress.ScanStage)
                        {
                            infoItem.ScanStage = scanProgress.ScanStage;
                            Int32 itemIndex = Items.IndexOf(infoItem);
                            if (itemIndex != -1)
                            {
                                Items.Remove(infoItem);
                                Items.Insert(itemIndex, infoItem);
                            }
                        }
                        else
                        {
                            infoItem.ScanStage = scanProgress.ScanStage;
                        }
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
                if (infoItem.ScanStage == ScanStage.OperationFailed ||
                    infoItem.ScanStage == ScanStage.OperationCompleted ||
                    infoItem.ScanStage == ScanStage.OperationCanceled)
                {
                    completed++;

                }

                if (infoItem.ScanStage == ScanStage.OperationCompleted)
                {
                    scanned++;
                }

                unknownFiles += infoItem.UnknownFilesCount;
                malwareFiles += infoItem.MalwareFilesCount;
            }

            if (unknownFiles > UnknownFilesCount)
            {
                UnknownFilesCount = unknownFiles;
            }

            if (malwareFiles > MalwareFilesCount)
            {
                MalwareFilesCount = malwareFiles;
            }

            ScannedItemsCount = completed;

            if (completed == OnlineItemsCount)
            {
                ScanCompleted(scanned > 0);
            }

            RaisePropertyChanged("Header");
        }

        private void OnGetScanResultCompleted(Object sender, GetScanResultCompletedEventArgs e)
        {
            ThreatInfoItem parentItem = _onlineItems.FirstOrDefault(item => item.ComputerGuid == e.ScanResult.TargetUid);
            if (parentItem != null)
            {

                RadObservableCollection<ThreatInfoItem> items = new RadObservableCollection<ThreatInfoItem>();        
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

                parentItem.TempItems = items;
            }
        }

        protected override void OnKeywordChanged(string oldValue, string newValue)
        {
            base.OnKeywordChanged(oldValue, newValue);

            if (Items != null)
            {
                foreach (ThreatInfoItem threatInfoItem in Items)
                {
                    if(threatInfoItem.Items != null)
                    {
                        foreach (ThreatInfoItem item in threatInfoItem.Items)
                        {
                            item.IsCompletedSubItem = false;
                        }    
                    }      
                }
                Items =
                   new ObservableCollection<ThreatInfoItem>(
                       _cache.Where(item => item.NameAndIpAddress.ContainsString(Keyword) || item.StatusMessage.Contains(Keyword)));

                
                SelectedItem = null;    
            }
        }
      
    }
}
