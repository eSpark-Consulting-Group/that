using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using APTAT.Application.Services;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using System.Windows.Controls;
using APTAT.Valkyrie.Client;
using APTAT.Core;
using APTAT.Valkyrie.Client.Models;


namespace APTAT.Application.Models
{

    public sealed class MainViewViewModel : ViewModel
    {
        public event EventHandler<Services.ScanCompletedEventArgs> ScanProcessCompleted;
        public event EventHandler<DomainEventArgs> DomainTargetsResolveFailure;

        private String _login;
        private String _password;

        private readonly IAptService _aptService;
        private readonly IStorageService _storage;
        private Guid _operationGuid;
        private DeploymentItem[] _selectedTargets;

        // private Boolean _resolvingTargets;
        private Boolean _isReportEnabled;
        private Boolean _isValkyrieReportEnabled;
        private Boolean? _isScanInProgress;
        private Int32 _scannedItemsCount;

        private Int32 _unknownFilesCount;
        private Int32 _malwareFilesCount;

        private ThreatInfoItem[] _onlineItems;
        private ThreatInfoItem _selectedItem;
        private ObservableCollection<ThreatInfoItem> _items;
        private ObservableCollection<FileResult> _itemsValkyrie;
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

        private UserControl _currentViewControl;
        public UserControl CurrentViewControl
        {
            get
            {
                return _currentViewControl;
            }
            set
            {
                _currentViewControl = value;
                RaisePropertyChanged("CurrentViewControl");
            }
        }

        public string StatusBarText
        {
            get
            {
                if (IsValkyrieScanInProgress == true)
                {
                    return String.Format(" of {0} files analysed", DistinctFilesCount);
                }
                return " unknown programs detected";
            }
         
        }

        public bool IsAnyScanInProgress
        {
            get
            {
                return IsScanInProgress.GetValueOrDefault() || IsValkyrieScanInProgress.GetValueOrDefault();
            }
        }

        public Boolean? IsScanInProgress
        {
            get { return _isScanInProgress; }
            set
            {
                _isScanInProgress = value;
                RaisePropertyChanged("IsScanInProgress");
                RaisePropertyChanged("IsAnyScanInProgress");
            }
        }

        public bool IsValkyrieAnalysCompleted
        {
            get { return _isValkyrieAnalysCompleted; }
            set
            {
                _isValkyrieAnalysCompleted = value;
                RaisePropertyChanged("IsValkyrieAnalysCompleted");
            }
        }

        public bool? IsValkyrieScanInProgress
        {
            get { return _isValkyrieScanInProgress; }
            set
            {
                _isValkyrieScanInProgress = value;
                RaisePropertyChanged("IsValkyrieScanInProgress");
                RaisePropertyChanged("IsAnyScanInProgress");
                RaisePropertyChanged("StatusBarText");
                
            }
        }


        public bool IsValkyrieAnalysStarted
        {
            get { return ValkyrieProgressInfo.ProcessedFileCount > 0; }
        }

        public int DistinctFilesCount
        {
            get { return ValkyrieProgressInfo.TotalFileCount; }
        }
        private ProgressInfo<FileResult> _valkyrieProgressInfo;

        public ProgressInfo<FileResult> ValkyrieProgressInfo
        {
            get { return _valkyrieProgressInfo; }
            set
            {
                _valkyrieProgressInfo = value;
                RaisePropertyChanged("ValkyrieProgressInfo");
                RaisePropertyChanged("DistinctFilesCount");
                RaisePropertyChanged("StatusBarText");
                RaisePropertyChanged("IsValkyrieAnalysStarted");
                RaisePropertyChanged("MalwareFilesCount");
            }
        }


        public Int32 UnknownFilesCount
        {
            get { return _unknownFilesCount; }
            set
            {
                _unknownFilesCount = value;
                RaisePropertyChanged("UnknownFilesCount");
                RaisePropertyChanged("IsUnknownFound");
                RaisePropertyChanged("IsProtected");
            }
        }

        public Int32 MalwareFilesCount
        {
            get { return _malwareFilesCount; }
            set
            {
                _malwareFilesCount = value;
                RaisePropertyChanged("MalwareFilesCount");
                RaisePropertyChanged("IsMalwaresFound");
                RaisePropertyChanged("IsProtected");

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
                var commonHeaderName = (string)System.Windows.Application.Current.FindResource("APTApplicationTitle");
                if (commonHeaderName == null)
                    throw new ArgumentNullException("Failed to obtain window header value from application resources");

                if (IsScanInProgress == true)
                {
                    return String.Format("{0} ({1}% scanning...)", commonHeaderName, Percentage);
                }
                if (IsValkyrieScanInProgress == true && ValkyrieProgressInfo != null)
                {
                    var info = ValkyrieProgressInfo;
                    var header = String.Format("Valkyrie progress {0}%", Percentage);
                    return String.Format("{0} ({1})", commonHeaderName, header);
                }

                return commonHeaderName;
            }
        }

        public Int32 OnlineItemsCount
        {
            get
            {
                if (_onlineItems == null)
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

        public bool IsValkyrieReportEnabled
        {
            get { return _isValkyrieReportEnabled; }
            set
            {
                _isValkyrieReportEnabled = value;
                RaisePropertyChanged("IsValkyrieReportEnabled");
            }
        }

        public Int32 ItemsCount
        {
            get
            {
                if (Items == null)
                {
                    return 0;
                }

                return Items.Count;
            }
        }

        public FileResult SelectedItemValkyrie
        {
            get { return _selectedItemValkyrie; }
            set
            {
                _selectedItemValkyrie = value;
                RaisePropertyChanged("SelectedItemValkyrie");
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

                if (_selectedItem == null)
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

        public string ValkyrieDetailsLink
        {
            get { return _aptService.GetValkyrieDetailsLink(); }
        }

        public Boolean IsMalwaresFound
        {
            get { return MalwareFilesCount > 0; }
        }

        public bool IsUnknownFound
        {
            get { return UnknownFilesCount > 0; }
        }

        public bool IsProtected
        {
            get
            {
                if (!IsMalwaresFound && !IsUnknownFound)
                    return true;
                else
                    return false;
            }
        }


        public ObservableCollection<FileResult> ItemsValkyrie
        {
            get { return _itemsValkyrie; }
            set
            {
                _itemsValkyrie = value;
                RaisePropertyChanged("ItemsValkyrie");

            }
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

        public DevicesSourceDescription DevicesSourceDescription;
        private FileResult _selectedItemValkyrie;
        private readonly UserControl _scanInfoTreeControl;
        private readonly UserControl _valkyrieResultControl;

        public MainViewViewModel()
        {
            _isValkyrieAnalysCompleted = false;
            _aptService = AptService.Instance;

            _storage = new StorageService();

            StartScanCommand = new DelegateCommand(InvokeStartScan);
            StopScanCommand = new DelegateCommand(InvokeStopScan);
            _scanInfoTreeControl = new ScanInfoTreeView() { DataContext = this };
            _valkyrieResultControl = new ResultTabsControl { DataContext = this };
            ValkyrieProgressInfo = ProgressInfo<FileResult>.Empty;
            CurrentViewControl = _scanInfoTreeControl;

        }

        private void InvokeStartScan(Object arg)
        {
            Items = new ObservableCollection<ThreatInfoItem>();
            ItemsValkyrie = new ObservableCollection<FileResult>();
            _cache = new ObservableCollection<ThreatInfoItem>();
            _onlineItems = new ThreatInfoItem[0];
            SelectedItem = null;
            UnknownFilesCount = 0;
            MalwareFilesCount = 0;
            ScannedItemsCount = 0;
            Percentage = 0;
            IsReportEnabled = false;
            IsValkyrieReportEnabled = false;
            CurrentViewControl = _scanInfoTreeControl;

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
                if (onlineItem.ScanStage != ScanStage.OperationCompleted && onlineItem.ScanStage != ScanStage.OperationFailed && onlineItem.ScanStage != ScanStage.OperationCanceled)
                {
                    onlineItem.StatusMessage = "Canceling...";
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
            _aptService.ValkyrieAnalyzeProgressChanged += OnValkyrieAnalyzeProgressChanged;
            _aptService.ValkyrieAnalyzeCompleted += OnValkyrieAnalyzeCompleted;
        }

        void OnValkyrieAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<FileResult> e)
        {
            if (e.Error != null)
            {
                ShowError(e.Error);
            }

            
            ItemsValkyrie = new ObservableCollection<FileResult>(e.Result);
            SelectedItemValkyrie = ItemsValkyrie.FirstOrDefault();
            RaisePropertyChanged("ItemsValkyrie");
            IsValkyrieScanInProgress = false;

            IsValkyrieReportEnabled = true;
            _isValkyrieAnalysCompleted = true;
            RaisePropertyChanged("IsValkyrieAnalysCompleted");
            RaisePropertyChanged("Header");
            RaisePropertyChanged("ValkyrieDetailsLink");
        }

        private static void ShowError(Exception exception)
        {
            if (exception == null) return;
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(exception.Message);
            var ae = exception as AggregateException;
            List<string> errorList = new List<string>();
            if (ae != null)
            {
                foreach (var innerException in ae.InnerExceptions)
                {
                    if (errorList.Contains(innerException.Message)) continue;
                    errorList.Add(innerException.Message);
                    
                }
            }
            ErrorLogWindow error = new ErrorLogWindow(errorList)
            {
                Owner = System.Windows.Application.Current.MainWindow
            };
            error.ShowDialog();
        }


        private void OnValkyrieAnalyzeProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            Percentage = e.ProgressPercentage;
            RaisePropertyChanged("Header");
            var info = (ProgressInfo<FileResult>)e.UserState;
            
            ValkyrieProgressInfo = info;

            if (info != null)
            {
                UnknownFilesCount = info.ProcessedFileCount;
                if (info.MalwareFilesCount > 0)
                {
                    MalwareFilesCount = info.MalwareFilesCount;
                }

                var results = info.Results.Where(l=>l.ValkyrieAnalysisResult != null);
                ItemsValkyrie = new ObservableCollection<FileResult>(results);
            }
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
            _aptService.ValkyrieAnalyzeProgressChanged -= OnValkyrieAnalyzeProgressChanged;
            _aptService.ValkyrieAnalyzeCompleted -= OnValkyrieAnalyzeCompleted;
        }

        private void OnScanCompleted(Object sender, APTAT.Application.Services.ScanCompletedEventArgs e)
        {
            if (ScanProcessCompleted != null)
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
                    DomainTargetsResolveFailure(this, new DomainEventArgs { Exception = e.Exception });
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

                DevicesSourceDescription = _storage.GetValue<APTAT.Core.DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, false);

                if (DevicesSourceDescription == null)
                    throw new InvalidOperationException("Devices Source Description was not correctly initialised");

                _aptService.BeginScan(_cache.ToArray(), domain, _login, _password, this);
                //_login = null;
                //_password = null;
                DevicesSourceDescription = null;
            }
            else
            {
                IsScanInProgress = false;
            }

        }

        private void OnBeginScanCompleted(Object sender, BeginScanCompletedEventArgs e)
        {

            IsValkyrieScanInProgress = false;
            IsValkyrieAnalysCompleted = false;
            if (e.Exception != null)
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

        }

        public void BeginValkyrieAnalysis()
        {
            ValkyrieProgressInfo = ProgressInfo<FileResult>.CreateFromTotal(DistinctFilesCount);

            IsValkyrieScanInProgress = true;
            IsValkyrieAnalysCompleted = false;
            UnknownFilesCount = 0;
            CurrentViewControl = _valkyrieResultControl;
            _aptService.BeginValkyrieAnalysis(_login, _password);
        }

        private void OnScanProgressChanged(Object sender, ScanProgressChangedEventArgs e)
        {
            if (_operationGuid == e.OperationGuid)
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
                        infoItem.StatusMessage = scanProgress.StatusMessage.Contains("Malwares found: 0.") ? scanProgress.StatusMessage.Replace("Malwares found: 0.", "") : scanProgress.StatusMessage;
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

                if (e.ScanResult.BadFiles != null)
                {
                    foreach (BadFileItem badFile in e.ScanResult.BadFiles)
                    {
                        items.Add(new ThreatInfoItem
                        {
                            IsUnknown = badFile.IsUnknown,
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
                    if (threatInfoItem.Items != null)
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



        private bool? _isValkyrieScanInProgress;
        private string _statusBarText;
        private bool _isValkyrieAnalysCompleted;
    }
}
