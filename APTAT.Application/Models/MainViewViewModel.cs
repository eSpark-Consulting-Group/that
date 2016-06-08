using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Windows;
using APTAT.Application.Services;
using Telerik.Windows.Controls;
using Telerik.Windows.Data;
using APTAT.Core;
using APTAT.Core.Valkyrie;
using APTAT.Application.Messages;
using APTAT.Application.Extensions;
using APTAT.Core.Descent;
using System.Collections;
using System.Windows.Threading;

namespace APTAT.Application.Models
{

    public sealed class MainViewViewModel : ViewModel
    {
        public event EventHandler<Services.ScanCompletedEventArgs> ScanProcessCompleted;
        public event EventHandler<DomainEventArgs> DomainTargetsResolveFailure;
        public event EventHandler ValkyrieAnalyzeStarted;
        public event EventHandler<AnalyzeCompletedEventArgs<FileResult>> ValkyrieAnalyzeCompleted;

        private string _login;
        private string _password;
        private string _camLicenseId;
        private string _camUserLogin;
        private string _camUserPassword;
        private string _tcpPort;
        private string _udpPort;

        private readonly IAptService _aptService;
        private readonly IWmiService _wmiService;
        private readonly ITargetsDiscoveryService _targetsDiscoveryService;
        private readonly IStorageService _storage;

        private Guid _operationGuid;
        private DeploymentItem[] _selectedTargets;

        private Boolean _isReportEnabled;
        private Boolean _isValkyrieReportEnabled;
        private Boolean? _isValkyrieScanInProgress;
        private Boolean _isValkyrieAnalysCompleted;
        private Boolean? _isScanInProgress = false;
        private Boolean _cancelInProgress = false;

        private Int32 _scannedItemsCount;
        private Int32 _unknownFilesCount;
        private Int32 _malwareFilesCount;

        private ThreatInfoItem[] _onlineItems;
        private ThreatInfoItem _selectedItem;
        private object _selectedValkyrieItem;

        private Collection<ThreatInfoItem> _items;
        private Collection<ThreatInfoItem> _computers;
        private ObservableCollection<FileResult> _itemsValkyrie;
        private ObservableCollection<ComputerInformation> _computersValkyrie;

        private Collection<ThreatInfoItem> _cache;
        private Collection<ThreatInfoItem> _cacheComputers;
        private Collection<FileResult> _cacheValkyrie;
        private Collection<ComputerInformation> _cacheValkyrieComputers;

        public bool IsAnyScanInProgress
        {
            get
            {
                return IsScanInProgress.GetValueOrDefault() || IsValkyrieScanInProgress.GetValueOrDefault();
            }
        }

        public Boolean CancelInProgress
        {
            get { return _cancelInProgress; }
            set
            {
                if (_cancelInProgress == value) return;
                _cancelInProgress = value;
                App.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    StopScanCommand.InvalidateCanExecute();
                });
                RaisePropertyChanged("Header");
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

                App.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    StartScanCommand.InvalidateCanExecute();
                    StopScanCommand.InvalidateCanExecute();
                });
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

                App.Current.Dispatcher.BeginInvoke((Action) delegate 
                { 
                    StartScanCommand.InvalidateCanExecute();
                    StopScanCommand.InvalidateCanExecute();
                });

            }
        }

        public int DistinctFilesCount
        {
            get { return ValkyrieProgressInfo.TotalFileCount; }
        }

        public ProgressInfo<FileResult> ValkyrieProgressInfo { get; set; }


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

        private Int32 _failedToAnalyzeFilesCount;
        public Int32 FailedToAnalyzeFilesCount
        {
            get { return _failedToAnalyzeFilesCount; }
            set
            {
                _failedToAnalyzeFilesCount = value;
            }
        }

        private Int32 _noVerdictFilesCount;
        public Int32 NoVerdictFilesCount
        {
            get { return _noVerdictFilesCount; }
            set
            {
                _noVerdictFilesCount = value;
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

        public Double Percentage { get; set; }

        public String Header
        {
            get
            {
                var commonHeaderName = (string)System.Windows.Application.Current.FindResource("APTApplicationTitle");
                if (commonHeaderName == null)
                    throw new ArgumentNullException("Failed to obtain window header value from application resources");

                string header = "Canceling...";
                if (CancelInProgress)
                    return String.Format("{0} ({1})", commonHeaderName, header);
                
                if (IsScanInProgress == true)
                {
                    if (Percentage == 0)
                        header = "Starting scan...";
                    else
                        header = String.Format("{0:0.00}% Scanning...", Percentage).Replace(".00", string.Empty);
                    return String.Format("{0} ({1})", commonHeaderName, header);
                }
                if (IsValkyrieScanInProgress == true && ValkyrieProgressInfo != null)
                {
                    header = String.Format("{0:0.00}% Valkyrie scanning...", Percentage).Replace(".00", string.Empty);
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

        public int TotalItemsCount { get; set; }

        public Boolean IsDetailsEnabled
        {
            get { return SelectedItem != null && !SelectedItem.IsRootItem; }
        }

        public Boolean IsValkyrieDetailsEnabled
        {
            get { return SelectedValkyrieItem != null && SelectedValkyrieItem is FileResult; }
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

        private bool _IsAnalyseWithValkyrieEnabled;
        public Boolean IsAnalyseWithValkyrieEnabled
        {
            get { return _IsAnalyseWithValkyrieEnabled; }
            set
            {
                _IsAnalyseWithValkyrieEnabled = value;
                RaisePropertyChanged("IsAnalyseWithValkyrieEnabled");
            }
        }
        public bool ShowFlsReportsMenu
        {
            get { return !IsValkyrieReportEnabled; }
        }

        public bool IsValkyrieReportEnabled
        {
            get { return _isValkyrieReportEnabled; }
            set
            {
                _isValkyrieReportEnabled = value;
                RaisePropertyChanged("IsValkyrieReportEnabled");
                RaisePropertyChanged("ShowFlsReportsMenu");
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

                SelectCommand.InvalidateCanExecute();

                if (_selectedItem == null)
                {
                    _storage.SetValue(KnownKeys.UnknownFile, null);
                }
                else
                {
                    _storage.SetValue(KnownKeys.UnknownFile, Tuple.Create<BadFileItem, FileResult>(_selectedItem.BadFile, null));
                }

            }
        }

        public object SelectedValkyrieItem
        {
            get { return _selectedValkyrieItem; }
            set
            {
                _selectedValkyrieItem = value == null ? null : (value is FileResult ? value : null);
                RaisePropertyChanged("SelectedValkyrieItem");
                SelectValkyrieCommand.InvalidateCanExecute();

                if (_selectedValkyrieItem == null)
                {
                    _storage.SetValue(KnownKeys.UnknownFile, null);
                }
                else
                {
                    _storage.SetValue(KnownKeys.UnknownFile, Tuple.Create<BadFileItem, FileResult>(null, (FileResult)_selectedValkyrieItem));
                }

            }
        }

        public Boolean IsItemsEmpty
        {
            get { return _cacheComputers == null || _cacheComputers.Count == 0; }
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

        public bool IsErrorsFound
        {
            get { return FailedToAnalyzeFilesCount > 0 || NoVerdictFilesCount > 0; }
        }

        public bool IsProtected
        {
            get
            {
                return !IsMalwaresFound && !IsUnknownFound && !IsErrorsFound;
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

        public ObservableCollection<ComputerInformation> ComputersValkyrie
        {
            get { return _computersValkyrie; }
            set
            {
                _computersValkyrie = value;
                RaisePropertyChanged("ComputersValkyrie");
            }
        }

        public Collection<ThreatInfoItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public Collection<ThreatInfoItem> Computers
        {
            get { return _computers; }
            set
            {
                _computers = value;
                RaisePropertyChanged("Computers");
                RaisePropertyChanged("IsItemsEmpty");
            }
        }

        public DelegateCommand StartScanCommand { get; private set; }
        public DelegateCommand StopScanCommand { get; private set; }

        public DelegateCommand SelectCommand { get; private set; }
        public DelegateCommand SelectValkyrieCommand { get; private set; }

        
        public MainViewViewModel(ITargetsDiscoveryService targetsDiscoveryService, 
            IAptService aptService, IStorageService storage, IWmiService wmiService)
        {
            
            _aptService = aptService;
            _targetsDiscoveryService = targetsDiscoveryService;
            _wmiService = wmiService;
            _storage = storage;


            _isValkyrieAnalysCompleted = false;

            StartScanCommand = new DelegateCommand(o =>
                AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.StartScan, InvokeStartScan),
                o => !IsAnyScanInProgress && IsScanInProgress.HasValue);
            StopScanCommand = new DelegateCommand(o =>
                AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.StopScan, InvokeStopScan), 
                o => IsAnyScanInProgress && IsScanInProgress.HasValue && !CancelInProgress);
            SelectCommand = new DelegateCommand(o => { }, o => IsDetailsEnabled);
            SelectValkyrieCommand = new DelegateCommand(o => { }, o => IsValkyrieDetailsEnabled);

            ValkyrieProgressInfo = ProgressInfo<FileResult>.Empty;
        }

        private void InvokeStartScan(Object arg)
        {
            IsQuickScan = (bool)arg;

            Items = new Collection<ThreatInfoItem>();
            Computers = new Collection<ThreatInfoItem>();
            ItemsValkyrie = new ObservableCollection<FileResult>();
            ComputersValkyrie = new ObservableCollection<ComputerInformation>();

            _cache = new Collection<ThreatInfoItem>();
            _cacheComputers = new Collection<ThreatInfoItem>();
            _cacheValkyrie = new Collection<FileResult>();
            _cacheValkyrieComputers = new Collection<ComputerInformation>();

            _onlineItems = new ThreatInfoItem[0];

            SelectedItem = null;
            UnknownFilesCount = 0;
            MalwareFilesCount = 0;
            ScannedItemsCount = 0;
            FailedToAnalyzeFilesCount = 0;
            NoVerdictFilesCount = 0;
            Percentage = 0;
            IsReportEnabled = false;
            IsAnalyseWithValkyrieEnabled = false;
            IsValkyrieReportEnabled = false;

            AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.Scan);
            Set(() => SelectScan, true);
            Set(() => GroupingEnabled, false);
            Set(() => ValkyrieEnabled, false);
            GroupByComputers = true;

            ICollection<Object> value = _storage.GetValue<ICollection<Object>>(KnownKeys.CheckedDirectoryItems, true);
            if (value != null)
            {
                _selectedTargets = value.OfType<DeploymentItem>().ToArray();
                if (_selectedTargets.Length > 0)
                {
                    IsBusy = true;
                    IsScanInProgress = null;
                    
                    _login = _storage.GetValue<String>(KnownKeys.Login, false);
                    _password = _storage.GetValue<String>(KnownKeys.Password, true);
                    _tcpPort= _storage.GetValue<String>(KnownKeys.TcpPort, false);
                    _udpPort = _storage.GetValue<String>(KnownKeys.UdpPort, false);

                    if (_login == null || _password == null)
                    {
                        throw new InvalidOperationException("specify login and password");
                    }

                    _targetsDiscoveryService.ResolveDirectoryItems(_selectedTargets, _login, _password, this);

                }
            }
        }        

        private void InvokeStopScan(Object arg)
        {
            CancelInProgress = true;

            _aptService.StopScan();
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            _targetsDiscoveryService.DirectoryItemsResolved += OnDirectoryItemsResolved;

            _aptService.BeginScanCompleted += OnBeginScanCompleted;
            _aptService.ScanStatusChanged += OnScanStatusChanged;
            _aptService.GetScanResultCompleted += OnGetScanResultCompleted;
            _aptService.ScanCompleted += OnScanCompleted;
            _aptService.ScanProgressChanged += OnScanProgressChanged;
            _aptService.ValkyrieAnalyzeProgressChanged += OnValkyrieAnalyzeProgressChanged;
            _aptService.ValkyrieAnalyzeCompleted += OnValkyrieAnalyzeCompleted;
            _aptService.ValkyrieAnalyzeStarted += OnValkyrieAnalyzeStarted;
        }

        void OnValkyrieAnalyzeStarted(object sender, EventArgs e)
        {
            OnBeforeBeginValkyrieAnalysis();
            var tmp = ValkyrieAnalyzeStarted;
            if (tmp != null)
            {
                tmp(this, e);
            }
        }

        void HandleWebException(WebException ex)
        {
            var httpWebResponse = ex.Response as HttpWebResponse;
            if (httpWebResponse != null)
            {
                HttpStatusCode code = httpWebResponse.StatusCode;
                if (code == HttpStatusCode.ProxyAuthenticationRequired)
                {
                    Trace.WriteLine(code.ToString());

                    SettingsWindow window = new SettingsWindow();
                    window.Owner = System.Windows.Application.Current.MainWindow;
                    window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                    string proxy = String.Empty;
                    var defaultProxy = WebProxy.GetDefaultProxy();
                    if (defaultProxy != null)
                    {
                        proxy = defaultProxy.Address.AbsoluteUri;
                    }

                    string message = String.Format("The server {0} requires a username and password.", proxy);
                    window.MessageText.Text = message;

                    if (window.ShowDialog() == true)
                    {
                        NetworkCredential cred = new NetworkCredential(window.LoginTextBox.Text,
                            window.PasswordBox.Password);
                        Trace.WriteLine("Proxy used: " + WebRequest.GetSystemWebProxy());
 
                        BeginValkyrieAnalysis(cred);
                        return;
                    }
                }
            }
        }

        void OnValkyrieAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<FileResult> e)
        {
            if (e.Error != null)
            {
                var webException = e.Error as WebException;
                if (webException != null)
                    HandleWebException(webException);
            }

            MergeResults(e.Result);
            _cacheValkyrie = new Collection<FileResult>(ItemsValkyrie);
            _cacheValkyrieComputers = new Collection<ComputerInformation>(ComputersValkyrie);

            if (e.Cancelled)
                foreach (var comp in ComputersValkyrie)
                    comp.Canceling = true;

            CancelInProgress = false;

            Set(() => ValkyrieSendingFiles, false);
            IsValkyrieScanInProgress = false;
            IsAnalyseWithValkyrieEnabled = true;
            IsValkyrieReportEnabled = true;
            IsValkyrieAnalysCompleted = true;

            RaisePropertyChanged("Header");
            RaisePropertyChanged("ValkyrieDetailsLink");

            var tmp = ValkyrieAnalyzeCompleted;
            if (tmp != null)
            {
                tmp(this, e);
            }
        }

        public bool ValkyrieSendingFiles { get; set; }
        public bool SelectScan { get; set; }
        public bool SelectValkyrieScan { get; set; }

        private void OnValkyrieAnalyzeProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            Set(() => ValkyrieSendingFiles, false);

            var info = (ProgressInfo<FileResult>)e.UserState;
            if (info != null && !CancelInProgress)
            {
                ValkyrieProgressInfo = info;
                Percentage = info.Percentage;
                RaisePropertyChanged("Header");
                MergeResults(info.Results);
            }
        }

        private void MergeResults(FileResult[] results)
        {
            if (results == null)
                return;

            if (!ItemsValkyrie.Any())
            {
                UnknownFilesCount = 0;
                MalwareFilesCount = 0;
            }

            foreach (var result in results)
            {
                var file = ItemsValkyrie.FirstOrDefault(_ => _.Equals(result));
                if (file != null)
                {
                    file.MergeComputers(result.Computers);
                    if (file.Path != result.Path)
                    {
                        file = ItemsValkyrie.FirstOrDefault(_ => _.Equals(result) && _.Path == result.Path);
                        if (file == null)
                        {
                            UnknownFilesCount += result.IsUnknown ? 1 : 0;
                            MalwareFilesCount += result.IsMalware ? 1 : 0;
                        }
                    }
                }
                else
                {
                    ItemsValkyrie.Add(result);
                    UnknownFilesCount += result.IsUnknown ? 1 : 0;
                    MalwareFilesCount += result.IsMalware ? 1 : 0;
                }

                foreach (var computer in result.Computers)
                {
                    var comp = ComputersValkyrie.FirstOrDefault(_ => _.FullName == computer.FullName);
                    if (comp != null)
                        comp.MergeFiles(result);
                    else if (Computers.Any(_ => _.ComputerGuid == computer.TargetUid))
                    {
                        comp = new ComputerInformation(computer);
                        comp.Count = Computers.FirstOrDefault(_ => _.ComputerGuid == computer.TargetUid).Items.Count;
                        comp.MergeFiles(result);
                        ComputersValkyrie.Add(comp);
                    }
                }
            }
        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _targetsDiscoveryService.DirectoryItemsResolved -= OnDirectoryItemsResolved;
            _aptService.BeginScanCompleted -= OnBeginScanCompleted;
            _aptService.ScanStatusChanged -= OnScanStatusChanged;
            _aptService.GetScanResultCompleted -= OnGetScanResultCompleted;
            _aptService.ScanCompleted -= OnScanCompleted;
            _aptService.ScanProgressChanged -= OnScanProgressChanged;
            _aptService.ValkyrieAnalyzeProgressChanged -= OnValkyrieAnalyzeProgressChanged;
            _aptService.ValkyrieAnalyzeCompleted -= OnValkyrieAnalyzeCompleted;
            _aptService.ValkyrieAnalyzeStarted -= OnValkyrieAnalyzeStarted;
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

            var items = e.Items.Select(t => new ThreatInfoItem(t)).ToArray();

            Set(() => TotalItemsCount, items.Count());

            _onlineItems = items.Where(item => item.IsOnline).ToArray();
            RaisePropertyChanged("OnlineItemsCount");


            _cacheComputers = new Collection<ThreatInfoItem>(items);
            Computers = new Collection<ThreatInfoItem>(_cacheComputers);
            

            foreach (var item in _cacheComputers)
            {
                if (!item.IsOnline)
                {
                    item.StatusMessage = "This computer is not accessible.";
                }
            }

            if (_onlineItems.Length > 0)
            {
                if (_login == null || _password == null)
                {
                    throw new InvalidOperationException("specify login and password");
                }

                if (String.IsNullOrWhiteSpace(_udpPort) || String.IsNullOrWhiteSpace(_tcpPort))
                {
                    throw new InvalidOperationException("specify FLS tcp and udp ports");
                }

                string scanScope = @"?:";
                if (IsQuickScan)
                {
                    scanScope = @"processes|drivers|autoruns";
                }

                _aptService.BeginScan(e.Items, _login, _password, this, _storage.GetValue<DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, false)
                    , scanScope, _tcpPort, _udpPort);
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
            _operationGuid = e.OperationGuid;

            _scanIterator = new Dictionary<Guid, ScanProgressItem>();
            _scanIteratorTimer = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
            _scanIteratorTimer.Interval = TimeSpan.FromMilliseconds(500d);
            _scanIteratorTimer.Tick += IterateThroughScan;
            _scanIteratorTimer.Start();
        }

        private void ScanCompleted(Boolean enableReports)
        {
            if (_scanIteratorTimer.IsEnabled)
                _scanIteratorTimer.Stop();

            CancelInProgress = false;
            
            IsScanInProgress = false;
            IsReportEnabled = enableReports;
            IsAnalyseWithValkyrieEnabled = enableReports;
            Set(() => GroupingEnabled, true);

            _storage.SetValue(KnownKeys.OperationGuid, _operationGuid);

        }

        public void OnBeforeBeginValkyrieAnalysis()
        {
            ValkyrieProgressInfo = ProgressInfo<FileResult>.CreateFromTotal(DistinctFilesCount);

            IsAnalyseWithValkyrieEnabled = false;
            IsValkyrieScanInProgress = true;
            IsValkyrieAnalysCompleted = false;

            ItemsValkyrie = new ObservableCollection<FileResult>();
            ComputersValkyrie = new ObservableCollection<ComputerInformation>();
            _cacheValkyrie = new Collection<FileResult>();
            _cacheValkyrieComputers = new Collection<ComputerInformation>();

            Keyword = string.Empty;

            AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.ValkyrieScan);
            Set(() => ValkyrieEnabled, true);
            Set(() => SelectValkyrieScan, false);
            Set(() => SelectValkyrieScan, true);
            Set(() => ValkyrieSendingFiles, true);
        }

        public void BeginValkyrieAnalysis(string camLicenseId, string camUserLogin, string camUserPassword)
        {
            _camLicenseId = camLicenseId;
            _camUserLogin = camUserLogin;
            _camUserPassword = camUserPassword;
            BeginValkyrieAnalysis();
        }

        private void BeginValkyrieAnalysis(NetworkCredential networkCredential = null)
        {
            
            _aptService.BeginValkyrieAnalysis(_login, _password, _camLicenseId, _camUserLogin, _camUserPassword,
                networkCredential);
        }

        private void OnScanProgressChanged(Object sender, ScanProgressChangedEventArgs e)
        {
            if (_operationGuid == e.OperationGuid)
            {
                if (!CancelInProgress)
                {
                    Percentage = e.Percentage;

                    RaisePropertyChanged("Header");
                }
            }
        }

        private DispatcherTimer _scanIteratorTimer;
        private Dictionary<Guid, ScanProgressItem> _scanIterator;
        private void OnScanStatusChanged(Object sender, ScanStatusChangedEventArgs e)
        {
            foreach (var scan in e.ScanProgress)
            {
                if (scan.OperationUid != _operationGuid)
                    continue;

                _scanIterator[scan.ComputerGuid] = scan;
            }
        }

        private void IterateThroughScan(object sender, EventArgs e)
        {
            if (_scanIteratorTimer.IsEnabled)
                _scanIteratorTimer.Stop();
            else
                return;

            Int32 completed = 0;
            Int32 scanned = 0;
            Int32 unknownFiles = 0;
            Int32 malwareFiles = 0;
            Int32 failedToAnalyzeFiles = 0;
            Int32 noVerdictFiles = 0;

            foreach (var infoItem in _onlineItems)
            {
                ScanProgressItem scanProgress = null;
                if (infoItem.ScanStage != ScanStage.OperationFailed &&
                    infoItem.ScanStage != ScanStage.OperationCompleted &&
                    infoItem.ScanStage != ScanStage.OperationCanceled)
                    _scanIterator.TryGetValue(infoItem.ComputerGuid, out scanProgress);

                if (scanProgress != null)
                {
                    infoItem.TotalFilesCount = scanProgress.TotalFilesCount;
                    infoItem.UnknownFilesCount = scanProgress.UnknownFilesCount;
                    infoItem.MalwareFilesCount = scanProgress.MalwareFilesCount;
                    infoItem.FailedToAnalyzeCount = scanProgress.FailedToAnalyzeFilesCount;
                    infoItem.NoVerdictFilesCount = scanProgress.NoVerdictFilesCount;
                    infoItem.StatusMessage = scanProgress.StatusMessage;

                    if (infoItem.ScanStage != ScanStage.OperationCompleted &&
                        scanProgress.ScanStage == ScanStage.OperationCompleted)
                    {
                        _aptService.GetScanResult(infoItem.ComputerGuid, this);
                    }

                    infoItem.ScanStage = scanProgress.ScanStage;
                }

                if (infoItem.ScanStage == ScanStage.OperationFailed ||
                    infoItem.ScanStage == ScanStage.OperationCompleted ||
                    infoItem.ScanStage == ScanStage.OperationCanceled)
                    completed++;

                if (infoItem.ScanStage == ScanStage.OperationCompleted)
                    scanned++;
                
                unknownFiles += infoItem.UnknownFilesCount;
                malwareFiles += infoItem.MalwareFilesCount;
                failedToAnalyzeFiles += infoItem.FailedToAnalyzeCount;
                noVerdictFiles += infoItem.NoVerdictFilesCount;
            }

            if (unknownFiles > UnknownFilesCount)
            {
                UnknownFilesCount = unknownFiles;
            }

            if (malwareFiles > MalwareFilesCount)
            {
                MalwareFilesCount = malwareFiles;
            }

            if (failedToAnalyzeFiles > FailedToAnalyzeFilesCount)
            {
                FailedToAnalyzeFilesCount = failedToAnalyzeFiles;
            }

            if (noVerdictFiles > NoVerdictFilesCount)
            {
                NoVerdictFilesCount = noVerdictFiles;
            }

            ScannedItemsCount = scanned;

            if (completed == OnlineItemsCount)
            {
                ScanCompleted(scanned > 0);
            }
            else
                _scanIteratorTimer.Start();

            RaisePropertyChanged("Header");
        }

        private void OnGetScanResultCompleted(Object sender, GetScanResultCompletedEventArgs e)
        {
            var parentItem = _onlineItems.FirstOrDefault(item => item.ComputerGuid == e.ScanResult.TargetUid);
            if (parentItem != null)
            {
                var items = new Collection<ThreatInfoItem>();

                if (e.ScanResult.BadFiles != null)
                {
                    foreach (BadFileItem badFile in e.ScanResult.BadFiles)
                    {
                        var item = new ThreatInfoItem(badFile);
                        items.Add(item);
                        item = new ThreatInfoItem(badFile);

                        var exist = _cache.FirstOrDefault(_ => _.BadFile.Sha1 == item.BadFile.Sha1);
                        var comp = new ThreatInfoItem { IpAddress = parentItem.IpAddress, Name = parentItem.Name, ShortName = item.Name, IsRootItem = true };
                        if (exist != null)
                            exist.Computers.Add(comp);
                        else
                        {
                            item.Computers = new Collection<ThreatInfoItem>();
                            item.Computers.Add(comp);
                            _cache.Add(item);
                            Items.Add(item);
                        }
                    }
                }

                parentItem.Items = items;
                parentItem.ScanStage = parentItem.ScanStage;
            }
        }

        protected override void OnKeywordChanged(string oldValue, string newValue)
        {
            base.OnKeywordChanged(oldValue, newValue);

            if (newValue == oldValue)
                return;

            if (_cacheComputers.Any())
            {
                Computers = new Collection<ThreatInfoItem>(
                       _cacheComputers.Where(item => item.NameAndIpAddress.ContainsString(Keyword)).ToList());

                SelectedItem = null;
            }

            if (_cache.Any())
            {
                Items = new Collection<ThreatInfoItem>(
                       _cache.Where(item => item.NameAndIpAddress.ContainsString(Keyword)).ToList());
            }

            if (_cacheValkyrie.Any())
            {
                ItemsValkyrie = new ObservableCollection<FileResult>(
                       _cacheValkyrie.Where(item => item.ValkyrieAnalysisResult != null && item.Name.ContainsString(Keyword)));
                SelectedValkyrieItem = null;
            }

            if (_cacheValkyrieComputers.Any())
            {
                ComputersValkyrie = new ObservableCollection<ComputerInformation>(
                       _cacheValkyrieComputers.Where(item => item.FullName.ContainsString(Keyword)));
            }
        }


        public bool IsQuickScan { get; set; }

        private bool _groupByFiles;
        public bool GroupByFiles
        {
            get { return _groupByFiles; }
            set 
            {
                _groupByFiles = value;
                _groupByComputers = !value;
                RaisePropertyChanged("GroupByFiles");
                RaisePropertyChanged("GroupByComputers");
            }
        }

        private bool _groupByComputers = true;
        public bool GroupByComputers
        {
            get { return _groupByComputers; }
            set
            {
                _groupByFiles = !value;
                _groupByComputers = value;
                RaisePropertyChanged("GroupByFiles");
                RaisePropertyChanged("GroupByComputers");
            }
        }

        public bool GroupingEnabled { get; set; }

        public bool ValkyrieEnabled { get; set; }

        public void SetReportKind(ReportKind kind)
        {
            _storage.SetValue(KnownKeys.ReportKind, kind);
        }

        public void OnAppClose()
        {
            IsBusy = true;
            InvokeStopScan(null);
        }
    }

}
