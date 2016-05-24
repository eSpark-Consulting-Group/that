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
using APTAT.Application.Helpers;
using APTAT.Application.Messages;

namespace APTAT.Application.Models
{

    public sealed class MainViewViewModel : ViewModel
    {
        public event EventHandler<Services.ScanCompletedEventArgs> ScanProcessCompleted;
        public event EventHandler<DomainEventArgs> DomainTargetsResolveFailure;
        public event EventHandler ValkyrieAnalyzeStarted;
        public event EventHandler<AnalyzeCompletedEventArgs<FileResult>> ValkyrieAnalyzeCompleted;

        private String _login;
        private String _password;

        private readonly IAptService _aptService;
        private readonly IWmiService _wmiService;
        private readonly ITargetsDiscoveryService _targetsDiscoveryService;
        private readonly IStorageService _storage;

        private Guid _operationGuid;
        private DeploymentItem[] _selectedTargets;

        private Boolean _isReportEnabled;
        private Boolean _isValkyrieReportEnabled;
        private Boolean? _isScanInProgress = false;
        private Int32 _scannedItemsCount;

        private Int32 _unknownFilesCount;
        private Int32 _malwareFilesCount;

        private ThreatInfoItem[] _onlineItems;
        private ThreatInfoItem _selectedItem;
        private FileResult _selectedValkyrieItem;
        private ObservableCollection<ThreatInfoItem> _items;
        private ObservableCollection<FileResult> _itemsValkyrie;
        private Collection<ThreatInfoItem> _cache;
        private Collection<FileResult> _cacheValkyrie;

        public string StatusBarText
        {
            get
            {
                return " unknown programs detected";
            }
        }

        public bool IsAnyScanInProgress
        {
            get
            {
                return IsScanInProgress.GetValueOrDefault() || !IsScanInProgress.HasValue || IsValkyrieScanInProgress.GetValueOrDefault();
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
                RaisePropertyChanged("StatusBarText");

                App.Current.Dispatcher.BeginInvoke((Action) delegate 
                { 
                    StartScanCommand.InvalidateCanExecute();
                    StopScanCommand.InvalidateCanExecute();
                });

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

        private Int32 _errorFilesCount;
        public Int32 ErrorFilesCount
        {
            get { return _errorFilesCount; }
            set
            {
                _errorFilesCount = value;
            }
        }

        private Int32 _serverErrorFilesCount;
        public Int32 ServerErrorFilesCount
        {
            get { return _serverErrorFilesCount; }
            set
            {
                _serverErrorFilesCount = value;
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

                if (IsScanInProgress == true)
                {
                    return String.Format("{0} ({1}% scanning...)", commonHeaderName, Percentage);
                }
                if (IsValkyrieScanInProgress == true && ValkyrieProgressInfo != null)
                {
                    var header = String.Format("Valkyrie progress {0:##.00}%", Percentage);
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
            get { return SelectedItem != null && !SelectedItem.IsRootItem; }
        }

        public Boolean IsValkyrieDetailsEnabled
        {
            get { return SelectedValkyrieItem != null; }
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

        public FileResult SelectedValkyrieItem
        {
            get { return _selectedValkyrieItem; }
            set
            {
                _selectedValkyrieItem = value;
                RaisePropertyChanged("SelectedValkyrieItem");
                SelectValkyrieCommand.InvalidateCanExecute();

                if (_selectedValkyrieItem == null)
                {
                    _storage.SetValue(KnownKeys.UnknownFile, null);
                }
                else
                {
                    _storage.SetValue(KnownKeys.UnknownFile, Tuple.Create<BadFileItem, FileResult>(null, _selectedValkyrieItem));
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

        public bool IsErrorsFound
        {
            get { return ErrorFilesCount > 0 || ServerErrorFilesCount > 0; }
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
                o => !IsAnyScanInProgress);
            StopScanCommand = new DelegateCommand(o =>
                AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.StopScan, InvokeStopScan), 
                o => IsAnyScanInProgress && IsScanInProgress.HasValue);
            SelectCommand = new DelegateCommand(o => { }, o => IsDetailsEnabled);
            SelectValkyrieCommand = new DelegateCommand(o => { }, o => IsValkyrieDetailsEnabled);

            ValkyrieProgressInfo = ProgressInfo<FileResult>.Empty;
        }

        private void InvokeStartScan(Object arg)
        {
            IsQuickScan = (bool)arg;
            Items = new ObservableCollection<ThreatInfoItem>();
            ItemsValkyrie = new ObservableCollection<FileResult>();
            _cache = new Collection<ThreatInfoItem>();
            _cacheValkyrie = new Collection<FileResult>();
            _onlineItems = new ThreatInfoItem[0];
            SelectedItem = null;
            UnknownFilesCount = 0;
            MalwareFilesCount = 0;
            ScannedItemsCount = 0;
            ErrorFilesCount = 0;
            Percentage = 0;
            IsReportEnabled = false;
            IsAnalyseWithValkyrieEnabled = false;
            IsValkyrieReportEnabled = false;

            AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.Scan);

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

        void OnValkyrieAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<FileResult> e)
        {
            
                if (e.Error != null)
                {
                    ShowError(e.Error);
                    var webException = e.Error as WebException;
                    if (webException != null)
                    {
                        var httpWebResponse = webException.Response as HttpWebResponse;
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
                }
                var tmp = ValkyrieAnalyzeCompleted;
                if (tmp != null)
                {
                    tmp(this, e);
                }

                _cacheValkyrie = new Collection<FileResult>(e.Result);
                IsValkyrieScanInProgress = false;
                IsAnalyseWithValkyrieEnabled = true;
                IsValkyrieReportEnabled = true;
                IsValkyrieAnalysCompleted = true;
                RaisePropertyChanged("IsValkyrieAnalysCompleted");
                RaisePropertyChanged("Header");
                RaisePropertyChanged("ValkyrieDetailsLink");
        }

        private static void ShowError(Exception exception)
        {
            return;
            /*if (exception == null) return;
            if (exception is AggregateException) return;
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
            error.ShowDialog();*/
        }

        private bool _valkyrieSendingFiles;
        public bool ValkyrieSendingFiles
        {
            get { return _valkyrieSendingFiles; }
            set
            {
                if (_valkyrieSendingFiles == value)
                    return;

                _valkyrieSendingFiles = value;
                RaisePropertyChanged("ValkyrieSendingFiles");
            }
        }

        private void OnValkyrieAnalyzeProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            ValkyrieSendingFiles = false;

            var info = (ProgressInfo<FileResult>)e.UserState;
            if (info != null)
            {
                ValkyrieProgressInfo = info;

                Percentage = info.Percentage;
                UnknownFilesCount = UnknownFilesCount + info.UnknownFileCount;
                MalwareFilesCount = MalwareFilesCount + info.MalwareFilesCount;
                
                RaisePropertyChanged("Header");

                foreach (var result in info.Results)
                {
                    ItemsValkyrie.Add(result);
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
            List<ThreatInfoItem> result = new List<ThreatInfoItem>(items.Length);



            _onlineItems = items.Where(item => item.IsOnline).ToArray();
            RaisePropertyChanged("OnlineItemsCount");

            _cache = new Collection<ThreatInfoItem>(items);
            Items = new ObservableCollection<ThreatInfoItem>(
                    _cache.Where(item => item.NameAndIpAddress.ContainsString(Keyword)));

            

            foreach (ThreatInfoItem item in _cache)
            {
                if (item.IsOnline == false)
                {
                    item.StatusMessage = "This computer is not accessible";
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

                _aptService.BeginScan(e.Items, _login, _password, this, scanScope, _tcpPort, _udpPort);
                //_login = null;
                //_password = null;
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
            //            IsAnalyseWithValkyrieEnabled = !IsAnyScanInProgress;
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
            Percentage = 100;
            IsScanInProgress = false;
            IsReportEnabled = enableReports;
            IsAnalyseWithValkyrieEnabled = enableReports;

            _storage.SetValue(KnownKeys.OperationGuid, _operationGuid);

        }

        public void OnBeforeBeginValkyrieAnalysis()
        {
            ValkyrieProgressInfo = ProgressInfo<FileResult>.CreateFromTotal(DistinctFilesCount);

            IsAnalyseWithValkyrieEnabled = false;
            ValkyrieSendingFiles = true;
            IsValkyrieScanInProgress = true;
            IsValkyrieAnalysCompleted = false;
            UnknownFilesCount = 0;

            ItemsValkyrie = new ObservableCollection<FileResult>();
            _cacheValkyrie = new Collection<FileResult>();
            Keyword = string.Empty;

            AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.ValkyrieScan);
        }

        public void BeginValkyrieAnalysis(string camLicenseId, string camUserLogin, string camUserPassword)
        {
            _camLicenseId = camLicenseId;
            _camUserLogin = camUserLogin;
            _camUserPassword = camUserPassword;
            BeginValkyrieAnalysis();
        }
        public void BeginValkyrieAnalysis(NetworkCredential networkCredential = null)
        {
            UnknownFilesCount = 0;
            MalwareFilesCount = 0;
            _aptService.BeginValkyrieAnalysis(_login, _password, _camLicenseId, _camUserLogin, _camUserPassword,
                networkCredential);
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
                        infoItem.ErrorFilesCount = scanProgress.ErrorFilesCount;
                        infoItem.StatusMessage = scanProgress.StatusMessage.Contains("Malwares found: 0.") ?
                                                    scanProgress.StatusMessage.Replace("Malwares found: 0.", "") :
                                                    scanProgress.StatusMessage; if (infoItem.ScanStage != ScanStage.OperationCompleted &&
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
            Int32 errorFiles = 0;
            Int32 serverErrorFiles = 0;

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
                errorFiles += infoItem.ErrorFilesCount;
                serverErrorFiles += infoItem.ServerErrorFilesCount;
            }

            if (unknownFiles > UnknownFilesCount)
            {
                UnknownFilesCount = unknownFiles;
            }

            if (malwareFiles > MalwareFilesCount)
            {
                MalwareFilesCount = malwareFiles;
            }

            if (errorFiles > ErrorFilesCount)
            {
                ErrorFilesCount = errorFiles;
            }

            if (serverErrorFiles > ServerErrorFilesCount)
            {
                ServerErrorFilesCount = serverErrorFiles;
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
                            SizeString = badFile.FileSizeFormatted,
                            IsRootItem = false,
                            IsOnline = true,
                            BadFile = badFile,
                        });
                    }
                }

                parentItem.TempItems = items;
                parentItem.ScanStage = parentItem.ScanStage;
            }
        }

        protected override void OnKeywordChanged(string oldValue, string newValue)
        {
            base.OnKeywordChanged(oldValue, newValue);

            if (newValue == oldValue)
                return;

            if (_cache.Any())
            {
                /*foreach (ThreatInfoItem threatInfoItem in Items)
                {
                    if (threatInfoItem.Items != null)
                    {
                        foreach (ThreatInfoItem item in threatInfoItem.Items)
                        {
                            item.IsCompletedSubItem = false;
                        }
                    }
                }*/
                Items = new ObservableCollection<ThreatInfoItem>(
                       _cache.Where(item => item.NameAndIpAddress.ContainsString(Keyword)));

                SelectedItem = null;
            }

            if (_cacheValkyrie.Any())
            {
                ItemsValkyrie = new ObservableCollection<FileResult>(
                       _cacheValkyrie.Where(item => item.ValkyrieAnalysisResult != null && item.Name.ContainsString(Keyword)));

                SelectedValkyrieItem = null;
            }
        }



        private bool? _isValkyrieScanInProgress;
        //private string _statusBarText;
        private bool _isValkyrieAnalysCompleted;
        private string _camLicenseId;
        private string _camUserLogin;
        private string _camUserPassword;
        private string _tcpPort;
        private string _udpPort;

        public bool IsQuickScan { get; set; }
    }
}
