using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;

using System.Windows.Threading;
using Telerik.Windows.Controls;

using APTAT.Valkyrie;
using FAT.Application.Messages;
using FAT.Application.Extensions;
using FAT.Application.Services;
using APTAT.Common;
using APTAT.DeploymentCore.Directory;
using APTAT.Core.Descent.Apt;
using System.Threading.Tasks;
using FAT.Application.Models.Apt;
using FAT.Application.Models.ModelBase;
using FAT.Application.Models.Valkyrie;


namespace FAT.Application.Models
{
    public sealed class MainViewViewModel : ViewModel, IMainViewModel
    {
        public event EventHandler<Services.ScanCompletedEventArgs> ScanProcessCompleted;
        public event EventHandler<DomainEventArgs> DomainTargetsResolveFailure;

        public event EventHandler<AnalyzeCompletedEventArgs<ValkyrieFile>> ValkyrieAnalyzeCompleted;

        private string _tcpPort = "80";
        private string _udpPort = "53";

        private readonly IAptService _aptService;

        private readonly ITargetsDiscoveryService _targetsDiscoveryService;
        private readonly IStorageService _storage;

        private Guid _operationGuid;
        private DeploymentItem[] _selectedTargets;

        private Boolean _isReportEnabled;
        private Boolean _isValkyrieReportEnabled;
        private Boolean _isValkyrieAnalysCompleted;
        private Boolean? _isScanInProgress = false;
        private Boolean _cancelInProgress = false;

        private Int32 _scannedItemsCount;
        private Int32 _unknownFilesCount;
        private Int32 _malwareFilesCount;
        private Int32 _pendingFilesCount;

        private ThreatInfoItem[] _onlineItems;
        private ThreatInfoItem _selectedItem;

        private DispatcherTimer _scanIteratorTimer;
        private Dictionary<Guid, ScanProgressItem> _scanIterator;

        public string MainUri { get; set; } = "/Views/MainView.xaml";

        public string Uri { get; set; } = "/Views/StartingPage.xaml";

        public string CurrentDomainName { get; set; }
        public string CurrentUser { get; set; }

        public string CurrentDomainNameContent
        {
            get
            {
                return HasCurrentDomainName
                    ? $"Use Current Domain Credentials ({CurrentDomainName})"
                    : $"Use Current User Credentials ({CurrentUser})";

            }
        }
        public bool HasCurrentDomainName
        {
            get { return !string.IsNullOrEmpty(CurrentDomainName); }
        }


        public string Login
        {
            get; set;
        }

        public string Password
        {
            get; set;
        }


        public String TcpPort
        {
            get { return _tcpPort; }
            set
            {
                _tcpPort = value;
                bool hasError = false;
                var errorMessage = value.ValidateTCP(out hasError);
                if (hasError)
                {
                    RaiseValidationError("TcpPort", errorMessage);
                }
                else
                {
                    ClearError("TcpPort");
                    RaisePropertyChanged(nameof(TcpPort));
                }
            }
        }


        public String UdpPort
        {
            get { return _udpPort; }
            set
            {
                _udpPort = value;
                bool hasError = false;
                var errorMessage = value.ValidateUDP(out hasError);
                if (hasError)
                {
                    RaiseValidationError("UdpPort", errorMessage);
                }
                else
                {
                    ClearError("UdpPort");
                    RaisePropertyChanged(nameof(UdpPort));
                }

            }
        }

        public bool Loaded { get; set; }

        #region 4 Tests

        public bool HasMalwares { get; set; }
        public bool HasUnknowns { get; set; }

        private void SimulateCounterActivity()
        {
            var testTimer = new DispatcherTimer();
            testTimer.Interval = TimeSpan.FromSeconds(1);
            testTimer.Tick += (o, e) =>
            {
                if (Percentage == 90)
                    Set(() => Percentage, 15);
                else
                    Set(() => Percentage, Percentage + 15);

                if (!HasMalwares && !HasUnknowns)
                    Set(() => HasMalwares, true);
                else if (HasMalwares)
                {
                    Set(() => HasMalwares, false);
                    Set(() => HasUnknowns, true);
                }
                else if (HasUnknowns)
                    Set(() => HasUnknowns, false);


                _scanCounterManager.AddClean();

                if (Percentage == 30)
                {
                    _scanCounterManager.AddPending();
                }
                if (Percentage == 45)
                {
                    _scanCounterManager.AddPending(-1);
                    _scanCounterManager.AddMalware();
                }
                if (Percentage == 60)
                {  
                    _scanCounterManager.AddFailed();
                }
                if (Percentage == 75)
                {
                    _scanCounterManager.AddPending();     
                }
                if (Percentage == 90)
                {
                    _scanCounterManager.AddPending(-1);
                    _scanCounterManager.AddUnknown();
                }

            };
            testTimer.Start();
        }

        private void SeedComputers()
        {
            Computers.Add(new ThreatInfoItem { IsOnline = true, IpAddress = "10.100.65.11", Name = "EUGENET-PC", IsRootItem = true, ScanStage = ScanStage.OperationCompleted });
            Computers.Add(new ThreatInfoItem
            {
                IsOnline = true,
                IpAddress = "10.100.65.11",
                Name = "EUGENET-PC",
                IsRootItem = true,
                ScanStage = ScanStage.OperationCompleted,
                UnknownFilesCount = 1,
                Items = new ObservableCollection<ThreatInfoItem>(new[]
            {
                new ThreatInfoItem { Name = "application.exe", Clean = true, SizeString = "1 MB", IsOnline = true },
                new ThreatInfoItem{ Name = "application.exe", IsMalware = true, SizeString = "1 MB", IsOnline = true },
                new ThreatInfoItem{ Name = "application.exe", IsUnknown = true, SizeString = "1 MB", IsOnline = true },
                new ThreatInfoItem{ Name = "application.exe", SizeString = "1 MB", IsOnline = true  }
            })
            });
            Computers.Add(new ThreatInfoItem { IsOnline = true, IsRootItem = true, IpAddress = "10.100.65.11", Name = "EUGENET-PC", ScanStage = ScanStage.OperationCompleted, FailedToAnalyzeCount = 1 });
            Computers.Add(new ThreatInfoItem { IsOnline = true, IsRootItem = true, IpAddress = "10.100.65.11", Name = "EUGENET-PC", ScanStage = ScanStage.OperationCompleted, MalwareFilesCount = 1 });
            Computers.Add(new ThreatInfoItem { IsOnline = true, IsRootItem = true, IpAddress = "10.100.65.11", Name = "EUGENET-PC", ScanStage = ScanStage.OperationFailed });
            Computers.Add(new ThreatInfoItem { IsRootItem = true, IpAddress = "10.100.65.11", Name = "EUGENET-PC", });
        }

        private void SeedFiles()
        {
            Files.Add(new ThreatInfoItem { ShortName = "application.exe", Clean = true, SizeString = "1 MB", IsOnline = true, Computers =
                new ObservableCollection<ThreatInfoItem>(new[]
            {
                new ThreatInfoItem { Name = "EUGENET-PC", IpAddress = "10.100.65.11", IsOnline = true, IsRootItem = true,ScanStage = ScanStage.OperationCompleted },
            })});
            Files.Add(new ThreatInfoItem { ShortName = "application.exe", IsMalware = true, SizeString = "1 MB", IsOnline = true });
            Files.Add(new ThreatInfoItem { ShortName = "application.exe", IsUnknown = true, SizeString = "1 MB", IsOnline = true });
            Files.Add(new ThreatInfoItem { ShortName = "application.exe", SizeString = "1 MB", IsOnline = true });
        }

        #endregion

        private async void OnLoaded()
        {
            SimulateCounterActivity();
            SeedComputers();
            SeedFiles();

            Set(() => Loaded, true);

            var name = await TryAction(GetCurrentDomainName());
            CurrentUser = _targetsDiscoveryService.GetLoggedInUser();

            CurrentDomainName = name?.Replace("DC=", "").Replace(",", ".");
            RaisePropertyChanged(nameof(CurrentDomainNameContent));
            RaisePropertyChanged(nameof(HasCurrentDomainName));
        }

        private async Task<string> GetCurrentDomainName()
        {
            var result = await _targetsDiscoveryService.GetCurrentDomainName();

            if (result.Exception != null)
            {
                return null;
            }

            return result.Name;
        }

        public bool IsAnyScanInProgress
        {
            get
            {
                return IsScanInProgress.GetValueOrDefault();
            }
        }

        public Boolean CancelInProgress
        {
            get { return _cancelInProgress; }
            set
            {
                if (_cancelInProgress == value) return;
                _cancelInProgress = value;

                RaisePropertyChanged("Header");

                App.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    StopScanCommand.InvalidateCanExecute();
                });
            }
        }

        public Boolean? IsScanInProgress
        {
            get { return _isScanInProgress; }
            set
            {
                _isScanInProgress = value;
                RaisePropertyChanged("IsScanInProgress");
                RaisePropertyChanged(nameof(IsAnyScanInProgress));


                App.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    StartScanCommand.InvalidateCanExecute();
                    StopScanCommand.InvalidateCanExecute();
                });
            }
        }

        public bool StopScanPredicate
        {
            get
            {
                return IsAnyScanInProgress && IsScanInProgress.HasValue && !CancelInProgress;
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

        public int DistinctFilesCount
        {
            get { return ValkyrieProgressInfo.TotalFileCount; }
        }

        public ProgressInfo<ValkyrieFile> ValkyrieProgressInfo { get; set; }

        public Int32 UnknownFilesCount
        {
            get { return _unknownFilesCount; }
            set
            {
                _unknownFilesCount = value;
                RaisePropertyChanged("UnknownFilesCount");
                RaisePropertyChanged("IsUnknownFound");
            }
        }

        private Int32 _lookupInProgressCount;
        public Int32 LookupInProgressCount
        {
            get { return _lookupInProgressCount; }
            set
            {
                _lookupInProgressCount = value;
                RaisePropertyChanged(nameof(LookupInProgressCount));
                RaisePropertyChanged(nameof(IsErrorsFound));
            }
        }

        public Int32 MalwareFilesCount
        {
            get { return _malwareFilesCount; }
            set
            {
                _malwareFilesCount = value;
                RaisePropertyChanged(nameof(MalwareFilesCount));
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

        public Int32 PendingFilesCount
        {
            get { return _pendingFilesCount; }
            set
            {
                _pendingFilesCount = value;
                RaisePropertyChanged("PendingFilesCount");
                RaisePropertyChanged("IsPendingFound");
            }
        }

        public Double Percentage { get; set; }

        public String Header
        {
            get
            {
                var commonHeaderName = string.Empty;

                string header = "Canceling...";
                if (CancelInProgress)
                    return String.Format("{0} ({1})", commonHeaderName, header);

                if (IsScanInProgress == true)
                {
                    if (Percentage == 0)
                        header = "Starting scan...";
                    else if (Percentage == 100)
                        header = "Finishing scan...";
                    else
                        header = String.Format("{0:0.00}% Scanning...", Percentage).Replace(".00", string.Empty);
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

        public Boolean IsMultipleDetailsEnabled
        {
            get { return SelectedItems != null && SelectedItems.Count == 1 && !SelectedItems[0].IsRootItem; }
        }


        public Boolean IsRestartEnabled
        {
            get
            {
                return SelectedItems != null && SelectedItems.Count > 0 && SelectedItems.Any(_ => _.IsRootItem &&
                  (_.ScanStage == ScanStage.ScanFailed ||
                  _.ScanStage == ScanStage.OperationFailed ||
                  _.ScanStage == ScanStage.OperationCanceled));
            }
        }

        public Boolean IsStopEnabled
        {
            get
            {
                return SelectedItems != null && SelectedItems.Count > 0 && SelectedItems.Any(_ => _.IsRootItem &&
                  (_.ScanStage == ScanStage.ScanInProgress));
            }
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

        private bool _isAnalyseWithValkyrieEnabled;
        public bool IsAnalyseWithValkyrieEnabled
        {
            get { return _isAnalyseWithValkyrieEnabled; }
            set
            {
                _isAnalyseWithValkyrieEnabled = value;
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

                if (_selectedItem == null)
                {
                    _storage.SetValue(KnownKeys.UnknownFile, null);
                }
                else
                {
                    _storage.SetValue(KnownKeys.UnknownFile, Tuple.Create<BadFileItem, ValkyrieFile>(_selectedItem.BadFile, null));
                }

                RaisePropertyChanged(nameof(SelectedItem));
                RaisePropertyChanged(nameof(IsDetailsEnabled));

                SelectCommand.InvalidateCanExecute();
            }
        }

        private List<ThreatInfoItem> _selectedItems = new List<ThreatInfoItem>();
        public List<ThreatInfoItem> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                _selectedItems = value ?? new List<ThreatInfoItem>();

                if (_selectedItems.Count == 1)
                    SelectedItem = _selectedItems[0];

                RaisePropertyChanged(nameof(IsMultipleDetailsEnabled));
                RaisePropertyChanged(nameof(IsRestartEnabled));
                RaisePropertyChanged(nameof(IsStopEnabled));

                SelectMultipleCommand.InvalidateCanExecute();
                RestartComputerScanCommand.InvalidateCanExecute();
                RestartComputerScanWithCredentialsCommand.InvalidateCanExecute();
                StopComputerScanCommand.InvalidateCanExecute();
            }
        }





        public string ValkyrieDetailsLink
        {
            get { return _aptService.GetValkyrieDetailsLink(); }
        }

        public Boolean IsPendingFound
        {
            get { return PendingFilesCount > 0; }
        }

        public bool IsUnknownFound
        {
            get
            {
                return UnknownFilesCount > 0
                    || LookupInProgressCount > 0;
            }
        }

        public bool IsErrorsFound
        {
            get { return LookupInProgressCount > 0; }
        }

        public ObservableCollection<ThreatInfoItem> DiscoveryResults { get; set; } = new ObservableCollection<ThreatInfoItem>();

        public ObservableCollection<ThreatInfoItem> Files { get; set; } = new ObservableCollection<ThreatInfoItem>();

        public ObservableCollection<ThreatInfoItem> Computers { get; set; } = new ObservableCollection<ThreatInfoItem>();


        private ScanCounterManager _scanCounterManager;

        public ScanCounterCollection ScanCounters
        {
            get
            {
                if (_scanCounterManager == null)
                {
                    _scanCounterManager = new ScanCounterManager();
                    _scanCounterManager.SelectionChanged = () => 
                        Set(() => FilterOrders, 
                            _scanCounterManager.Collection.
                                Where(_ => _.IsSelected).
                                    Select(_ => _.Order).ToArray());
                }

                return _scanCounterManager.Collection;
            }
        }


        public int[] FilterOrders { get; set; } 

        public bool IsQuickScan { get; set; }


        private bool _groupByFiles;
        public bool GroupByFiles
        {
            get { return _groupByFiles; }
            set
            {
                Set(() => Uri, "/Views/Scan/ScanResultsWithFiles.xaml");

                _groupByFiles = value;
                _groupByComputers = !value;
                RaisePropertyChanged("GroupByFiles");
                RaisePropertyChanged("GroupByComputers");
                RaisePropertyChanged(nameof(GroupByWatermark));
            }
        }

        private bool _groupByComputers = true;
        public bool GroupByComputers
        {
            get { return _groupByComputers; }
            set
            {
                Set(() => Uri, "/Views/Scan/ScanResultsWithComputers.xaml");

                _groupByFiles = !value;
                _groupByComputers = value;
                RaisePropertyChanged("GroupByFiles");
                RaisePropertyChanged("GroupByComputers");
                RaisePropertyChanged(nameof(GroupByWatermark));
            }
        }

        public string GroupByWatermark
        {
            get
            {
                return GroupByComputers ? "Search By Computers" : "Search By Files";
            }
        }

        public DelegateCommand ChangeScreenCommand { get; private set; }
        public DelegateCommand StartScanCommand { get; private set; }
        public DelegateCommand StopScanCommand { get; private set; }

        public DelegateCommand SelectCommand { get; private set; }

        public DelegateCommand SelectMultipleCommand { get; private set; }


        public DelegateCommand RestartComputerScanCommand { get; private set; }
        public DelegateCommand RestartComputerScanWithCredentialsCommand { get; private set; }
        public DelegateCommand StopComputerScanCommand { get; private set; }


        public MainViewViewModel(ITargetsDiscoveryService targetsDiscoveryService,
            IAptService aptService, IStorageService storage)
        {
            _aptService = aptService;
            _targetsDiscoveryService = targetsDiscoveryService;

            _storage = storage;

            ChangeScreenCommand = new DelegateCommand(o =>
            {
                
                OnLoaded();
                Set(() => Uri, "/Views/Scan/ScanResultsWithComputers.xaml");
            });

            StartScanCommand = new DelegateCommand(o =>
                AppMessages.ScanMessage.Send(ChangeScreenTypes.StartScan, InvokeStartScan),
                o => !IsAnyScanInProgress && IsScanInProgress.HasValue);
            StopScanCommand = new DelegateCommand(o =>
                AppMessages.ScanMessage.Send(ChangeScreenTypes.StopScan, InvokeStopScan),
                o => StopScanPredicate);

            SelectCommand = new DelegateCommand(o => { }, o => IsDetailsEnabled);
            SelectMultipleCommand = new DelegateCommand(o => { }, o => IsMultipleDetailsEnabled);

            RestartComputerScanCommand = new DelegateCommand(o => InvokeRestartComputerScan(o), o => IsRestartEnabled);
            RestartComputerScanWithCredentialsCommand = new DelegateCommand(o => InvokeRestartComputerScan(o, true), o =>
                IsRestartEnabled && SelectedItems.Any(_ => !_.IsLocal));
            StopComputerScanCommand = new DelegateCommand(o => InvokeStopComputerScan(o), o => IsStopEnabled);

            ValkyrieProgressInfo = ProgressInfo<ValkyrieFile>.Empty;
        }

        private void InvokeRestartComputerScan(Object arg, bool showDialog = false)
        {
            if (showDialog)
            {
                SettingsWindow window = new SettingsWindow();
                window.Owner = System.Windows.Application.Current.MainWindow;
                window.DataContext = this;
                window.ShowCredentials = true;
                window.Header = "Scan Credentials";

                bool retry = window.ShowDialog().GetValueOrDefault();

                if (retry)
                {
                    RestartComputerScan(true);
                    return;
                }
            }
            else
                RestartComputerScan();
        }

        private void RestartComputerScan(bool useNewCredentials = false)
        {
            ScanKind scanKind = ScanKind.Quick;
            if (IsQuickScan)
            {
                scanKind = ScanKind.Quick;
            }
            else
            {
                scanKind = ScanKind.Full;
            }

            ScanOptions options;
            CustomScanOptions customScanOptions = null;


            var login = useNewCredentials ? (!UseCurrentDomainForRescan ? (RescanLogin ?? Login) : null) : Login;
            var password = useNewCredentials ? (!UseCurrentDomainForRescan ? (RescanPassword ?? Password) : null) : Password;

            var items = new List<ThreatInfoItem>();
            foreach (var item in SelectedItems)
            {
                if (item.IsRootItem &&
                    (item.ScanStage == ScanStage.ScanFailed ||
                    item.ScanStage == ScanStage.OperationFailed ||
                    item.ScanStage == ScanStage.OperationCanceled))
                {
                    item.RestoreConnectingState();
                    items.Add(item);
                }
            }
            if (_storage.ContainsKey(KnownKeys.CustomScanOptions))
            {
                customScanOptions = _storage.GetValue<CustomScanOptions>(KnownKeys.CustomScanOptions, true);
                scanKind = ScanKind.Custom;
                options = new ScanOptions(_tcpPort, _udpPort, customScanOptions.FolderItems.Select(f => f.FullName).ToArray(),
                    customScanOptions.ScanCriticalAreas, login, password, true);
            }
            else
            {
                options = new ScanOptions(_tcpPort, _udpPort, scanKind, login, password, true);
            }
            _aptService.BeginScan(items.Select(_ => _.DeploymentTarget).ToArray(),
                this, _storage.GetValue<DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, false), options);
            UpdateStatus();
            if (!IsScanInProgress.GetValueOrDefault())
            {
                IsScanInProgress = true;
                Percentage = 0;
                IsReportEnabled = false;
                IsAnalyseWithValkyrieEnabled = false;
                IsValkyrieReportEnabled = false;
            }
        }

        private void InvokeStopComputerScan(Object arg)
        {
            var items = new List<ThreatInfoItem>();
            foreach (var item in SelectedItems)
            {
                if (item.IsRootItem &&
                    (item.ScanStage == ScanStage.ScanInProgress))
                {
                    items.Add(item);
                }
            }

            InvokeStopScan(items.Select(_ => _.DeploymentTarget.Uid).ToArray());
        }

        private void InvokeStartScan(Object arg)
        {
            IsQuickScan = (bool)arg;

            _scanCounterManager.Clear();
            Files = new ObservableCollection<ThreatInfoItem>();
            Computers = new ObservableCollection<ThreatInfoItem>();
            RaisePropertyChanged(nameof(ScanCounters));
            RaisePropertyChanged(nameof(Files));
            RaisePropertyChanged(nameof(Computers));
            

            _onlineItems = new ThreatInfoItem[0];

            SelectedItem = null;
            UnknownFilesCount = 0;
            MalwareFilesCount = 0;
            PendingFilesCount = 0;
            ScannedItemsCount = 0;
            LookupInProgressCount = 0;
            Percentage = 0;
            IsReportEnabled = false;

            IsValkyrieReportEnabled = false;
            RescanLogin = null;
            RescanPassword = null;



            GroupByComputers = true;

            var value = _storage.GetValue<ICollection<Object>>(KnownKeys.CheckedDirectoryItems, true);
            if (value != null)
            {
                _selectedTargets = value.OfType<DeploymentItem>().ToArray();
                if (_selectedTargets.Length > 0)
                {
                    IsBusy = true;
                    IsScanInProgress = null;

                    var model = CompositionHelper.Instance.Resolve<StartScanDomainViewModel>();

                    _targetsDiscoveryService.ResolveDirectoryItems(_selectedTargets,
                        CurrentStep == WizardStepEnum.Domain ? model.ActiveDirectoryLogin : null,
                        CurrentStep == WizardStepEnum.Domain ? model.ActiveDirectoryPassword : null, this);
                }
            }
        }

        private void InvokeStopScan(Object arg)
        {
            if (!IsAnyScanInProgress)
                return;

            var targets = new Guid[0];
            if (arg != null && arg is Guid[])
                targets = (Guid[])arg;
            else
                CancelInProgress = true;

            _aptService.StopScan(targets);

        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            _targetsDiscoveryService.DirectoryItemsResolved += OnDirectoryItemsResolved;

            _aptService.BeginScanCompleted += OnBeginScanCompleted;
            _aptService.ScanStatusChanged += OnScanStatusChanged;
            _aptService.ComputerScanCompleted += OnComputerScanCompleted;
            _aptService.ScanCompleted += OnScanCompleted;
            _aptService.ScanProgressChanged += OnScanProgressChanged;

            _aptService.ValkyrieAnalyzeCompleted += OnValkyrieAnalyzeCompleted;
            _aptService.ValkyrieAnalyzeStarted += OnValkyrieAnalyzeStarted;

        }



        
        void OnValkyrieAnalyzeStarted(object sender, EventArgs e)
        {
            OnBeforeBeginValkyrieAnalysis();

        }

        public string ProxyLogin { get; set; }
        public string ProxyPassword { get; set; }

        public string RescanLogin { get; set; }
        public string RescanPassword { get; set; }

        public bool UseCurrentDomainForRescan { get; set; }

        public void OnPortChanged(int udpPort, int tcpPort)
        {
            _aptService.UpdateFlsPorts(udpPort, tcpPort);
        }

        public IMainViewModel ParentViewModel
        {
            get { return this; }
        }

        public WizardStepEnum CurrentStep { get; set; }

        void OnValkyrieAnalyzeCompleted(object sender, AnalyzeCompletedEventArgs<ValkyrieFile> e)
        {
            if (_isScanInProgress != false) return;
            if (e.Error != null)
            {
                var webException = e.Error as WebException;
                var httpWebResponse = webException?.Response as HttpWebResponse;
                if (httpWebResponse != null)
                {
                    HttpStatusCode code = httpWebResponse.StatusCode;
                    if (code == HttpStatusCode.ProxyAuthenticationRequired)
                    {
                        SettingsWindow window = new SettingsWindow();
                        window.Owner = System.Windows.Application.Current.MainWindow;
                        window.DataContext = this;
                        window.ShowProxyCredentials = true;
                        window.Header = "Proxy Credentials";

                        bool retry = window.ShowDialog().GetValueOrDefault();

                        if (retry)
                        {
                            NetworkCredential cred = new NetworkCredential(ProxyLogin, ProxyPassword, null); 
                            BeginValkyrieAnalysis(cred);
                            return;
                        }
                    }
                }


            }
            else
            {
                if (_isScanInProgress == false)
                {
                    IsValkyrieReportEnabled = true;


                }
            }


            CancelInProgress = false;



            
            IsReportEnabled = true;

            IsValkyrieAnalysCompleted = true;

            RaisePropertyChanged("Header");
            RaisePropertyChanged("ValkyrieDetailsLink");

            ValkyrieAnalyzeCompleted?.Invoke(this, e);
        }




        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _targetsDiscoveryService.DirectoryItemsResolved -= OnDirectoryItemsResolved;
            _aptService.BeginScanCompleted -= OnBeginScanCompleted;
            _aptService.ScanStatusChanged -= OnScanStatusChanged;
            _aptService.ComputerScanCompleted -= OnComputerScanCompleted;
            _aptService.ScanCompleted -= OnScanCompleted;
            _aptService.ScanProgressChanged -= OnScanProgressChanged;
            _aptService.ValkyrieAnalyzeCompleted -= OnValkyrieAnalyzeCompleted;
            _aptService.ValkyrieAnalyzeStarted -= OnValkyrieAnalyzeStarted;
        }

        private void OnScanCompleted(Object sender, Services.ScanCompletedEventArgs e)
        {
            ScanCompleted();
            ScanProcessCompleted?.Invoke(this, e);
        }
        private void OnDirectoryItemsResolved(Object sender, DirectoryItemsResolvedEventArgs e)
        {
            IsBusy = false;
            IsScanInProgress = true;
            ScannedItemsCount = 0;
            if (e.Exception != null)
            {
                DomainTargetsResolveFailure?.Invoke(this, new DomainEventArgs { Exception = e.Exception });
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
            Computers = new ObservableCollection<ThreatInfoItem>(items);


            foreach (var item in Computers)
            {
                if (!item.IsOnline)
                {
                    item.StatusMessageSystem = "This computer is not accessible.";
                }
            }

            if (_onlineItems.Length > 0)
            {


                ScanKind scanKind = ScanKind.Quick;
                if (IsQuickScan)
                {
                    scanKind = ScanKind.Quick;
                }
                else
                {
                    scanKind = ScanKind.Full;
                }

                ScanOptions options;
                CustomScanOptions customScanOptions = null;
                if (_storage.ContainsKey(KnownKeys.CustomScanOptions))
                {
                    customScanOptions = _storage.GetValue<CustomScanOptions>(KnownKeys.CustomScanOptions, true);
                    scanKind = ScanKind.Custom;
                    options = new ScanOptions(_tcpPort, _udpPort, customScanOptions.FolderItems.Select(f => f.FullName).ToArray(),
                        customScanOptions.ScanCriticalAreas, Login, Password);
                }
                else
                {
                    options = new ScanOptions(_tcpPort, _udpPort, scanKind, Login, Password);
                }

                _aptService.BeginScan(e.Items, this, _storage.GetValue<DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, false), options);

            }
            else
            {
                IsScanInProgress = false;
            }
        }

        private void OnBeginScanCompleted(Object sender, BeginScanCompletedEventArgs e)
        {
            _operationGuid = e.OperationGuid;

            _scanIterator = new Dictionary<Guid, ScanProgressItem>();
            _scanIteratorTimer = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
            _scanIteratorTimer.Interval = TimeSpan.FromMilliseconds(500d);
            _scanIteratorTimer.Tick += IterateThroughScan;
            _scanIteratorTimer.Start();

            IsValkyrieAnalysCompleted = false;
        }

        private void ScanCompleted()
        {
            CancelInProgress = false;
            IsScanInProgress = false;
            ScannedItemsCount = _onlineItems.Count(i => i.ScanStage == ScanStage.OperationCompleted);

            _storage.SetValue(KnownKeys.OperationGuid, _operationGuid);
            UpdateStatus();
            RaisePropertyChanged(nameof(Header));
        }

        public void OnBeforeBeginValkyrieAnalysis()
        {
            ValkyrieProgressInfo = ProgressInfo<ValkyrieFile>.CreateFromTotal(DistinctFilesCount);


            IsValkyrieAnalysCompleted = false;



            Keyword = string.Empty;
        }

        public bool BeginValkyrieAnalysis(NetworkCredential networkCredential = null)
        {

                _aptService.BeginValkyrieAnalysis(Login, Password, null, null, null,
                    networkCredential);
                return true;
            
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

        private void OnScanStatusChanged(Object sender, ScanStatusChangedEventArgs e)
        {
            lock (_scanIterator)
            {
                foreach (var scan in e.ScanProgress)
                {
                    if (scan.OperationUid != _operationGuid)
                        continue;
                    _scanIterator[scan.ComputerGuid] = scan;
                }
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
            Int32 lookupInProgressCount = 0;

            foreach (var infoItem in _onlineItems)
            {
                ScanProgressItem scanProgress = null;
                lock (_scanIterator)
                {
                    _scanIterator.TryGetValue(infoItem.ComputerGuid, out scanProgress);
                    _scanIterator.Remove(infoItem.ComputerGuid);
                }
                lock (infoItem)
                {
                    if (infoItem.ScanStage != ScanStage.OperationFailed &&
                        infoItem.ScanStage != ScanStage.OperationCompleted &&
                        infoItem.ScanStage != ScanStage.OperationCanceled)
                    {
                        if (scanProgress != null)
                        {
                            infoItem.TotalFilesCount = scanProgress.TotalFilesCount;
                            infoItem.UnknownFilesCount = scanProgress.UnknownFilesCount;
                            infoItem.MalwareFilesCount = scanProgress.MalwareFilesCount;
                            infoItem.FailedToAnalyzeCount = scanProgress.FailedToAnalyzeFilesCount;
                            infoItem.NoVerdictFilesCount = scanProgress.NoVerdictFilesCount;
                            infoItem.ProgressPercentage = scanProgress.ProgressPercentage;
                            infoItem.StatusMessageSystem = scanProgress.StatusMessage;
                            infoItem.ScanStage = scanProgress.ScanStage;
                        }
                    }
                    if (infoItem.ScanStage == ScanStage.OperationFailed ||
                        infoItem.ScanStage == ScanStage.OperationCompleted ||
                        infoItem.ScanStage == ScanStage.OperationCanceled)
                        completed++;

                    if (infoItem.ScanStage == ScanStage.OperationCompleted)
                        scanned++;

                    if (infoItem.ScanStage == ScanStage.OperationCanceled && !CancelInProgress)
                        continue;

                    unknownFiles += infoItem.UnknownFilesCount;
                    malwareFiles += infoItem.MalwareFilesCount;
                    lookupInProgressCount += infoItem.FailedToAnalyzeCount;
                    lookupInProgressCount += infoItem.NoVerdictFilesCount;
                }
            }

            if (unknownFiles > UnknownFilesCount)
            {
                UnknownFilesCount = unknownFiles;
            }

            if (malwareFiles > MalwareFilesCount)
            {
                MalwareFilesCount = malwareFiles;
            }

            if (lookupInProgressCount > LookupInProgressCount)
            {
                LookupInProgressCount = lookupInProgressCount;
            }

            if (scanned > ScannedItemsCount)
            {
                ScannedItemsCount = scanned;
            }

            if (completed != OnlineItemsCount)
                _scanIteratorTimer.Start();
            else
            {
                if (_scanIteratorTimer.IsEnabled)
                    _scanIteratorTimer.Stop();
            }

            RaisePropertyChanged(nameof(Header));
        }

        private void UpdateStatus()
        {
            Int32 unknownFiles = 0;
            Int32 malwareFiles = 0;
            Int32 lookupInProgressCount = 0;

            foreach (var infoItem in _onlineItems)
            {
                if (infoItem.ScanStage == ScanStage.OperationCanceled)
                    continue;

                unknownFiles += infoItem.UnknownFilesCount;
                malwareFiles += infoItem.MalwareFilesCount;
                lookupInProgressCount += infoItem.FailedToAnalyzeCount;
                lookupInProgressCount += infoItem.NoVerdictFilesCount;
            }


            UnknownFilesCount = unknownFiles;
            MalwareFilesCount = malwareFiles;
            LookupInProgressCount = lookupInProgressCount;

        }

        private void OnComputerScanCompleted(Object sender, GetScanResultCompletedEventArgs e)
        {
            var parentItem = _onlineItems.FirstOrDefault(item => item.ComputerGuid == e.ScanResult.TargetUid);
            if (parentItem != null)
            {
                lock (parentItem)
                {
                    Dictionary<Guid, ThreatInfoItem> computersToUpdate = new Dictionary<Guid, ThreatInfoItem>();
                    parentItem.ScanStage = e.ScanResult.ScanStage;
                    parentItem.StatusMessageSystem = e.ScanResult.StatusMessage;
                    if (e.ScanResult.BadFiles != null)
                    {
                        foreach (var badFile in e.ScanResult.BadFiles)
                        {
                            var item = new ThreatInfoItem(badFile);
                            parentItem.Items.Add(item);
                            if (!computersToUpdate.ContainsKey(parentItem.ComputerGuid))
                            {
                                computersToUpdate.Add(parentItem.ComputerGuid, parentItem);
                            }
                            item = new ThreatInfoItem(badFile);

                            var exist = Files.FirstOrDefault(_ =>
                                _.BadFile.Sha1 == item.BadFile.Sha1 && _.IsUnknown == item.IsUnknown &&
                                _.IsMalware == item.IsMalware);
                            var comp = new ThreatInfoItem
                            {
                                IpAddress = parentItem.IpAddress,
                                ComputerGuid = e.ScanResult.TargetUid,
                                Name = parentItem.Name,
                                ShortName = item.Name,
                                IsRootItem = true
                            };

                            if (exist != null)
                                exist.Computers.Add(comp);
                            else
                            {
                                item.Computers = new Collection<ThreatInfoItem>();
                                item.Computers.Add(comp);
                                Files.Add(item);
                            }
                        }
                        if (e.ScanResult.TotalFilesCount != parentItem.TotalFilesCount)
                        {
                            parentItem.TotalFilesCount = e.ScanResult.TotalFilesCount;
                            if (!computersToUpdate.ContainsKey(parentItem.ComputerGuid))
                            {
                                computersToUpdate.Add(parentItem.ComputerGuid, parentItem);
                            }
                        }
                        foreach (var computer in computersToUpdate.Values)
                        {
                            computer.UpdateStatus();
                        }
                        if (computersToUpdate.Any())
                            UpdateStatus();
                    }


                    parentItem.Items = parentItem.Items;
                }
            }
        }

        public Predicate<object> FilterPredicate
        {
            get
            {
                return _ =>
                {
                    if (_ == null)
                        return false;

                    if (GroupByComputers)
                    {
                        var o = _ as ThreatInfoItem;
                        if (o.IsRootItem)
                        {
                            bool hasOrders = !FilterOrders.Any();
                            foreach (int order in FilterOrders)
                            {
                                if ((!o.HasBadFiles && order == 1 && o.ScanStage == ScanStage.OperationCompleted) ||
                                    (o.HasMalwares && order == 2 && o.ScanStage == ScanStage.OperationCompleted) ||
                                    (o.HasUnknowns && order == 3 && o.ScanStage == ScanStage.OperationCompleted) ||
                                    (o.ScanStage == ScanStage.OperationFailed && order == 4) ||
                                    (o.HasFailed && order == 5))
                                {
                                    hasOrders = true;
                                    break;
                                }
                            }

                            return hasOrders && (string.IsNullOrEmpty(Keyword) || o.NameAndIpAddress.ToLower().Contains(Keyword.ToLower()));
                        }
                        else if (!FilterOrders.Any() && string.IsNullOrEmpty(Keyword))
                            return true;
                    }

                    if (GroupByFiles)
                    {
                        var o = _ as ThreatInfoItem;
                        if (!o.IsRootItem)
                        {
                            bool hasOrders = !FilterOrders.Any();
                            foreach (int order in FilterOrders)
                            { 
                                if ((o.Clean && order == 1) ||
                                    (o.IsMalware && order == 2) ||
                                    (o.IsUnknown && order == 3) ||
                                    (o.Failed && order == 5))
                                {
                                    hasOrders = true;
                                    break;
                                }
                            }

                            return hasOrders && (string.IsNullOrEmpty(Keyword) || o.NameAndIpAddress.ToLower().Contains(Keyword.ToLower()));
                        }
                        else if (!FilterOrders.Any() && string.IsNullOrEmpty(Keyword))
                            return true;
                    }

                    return false;
                };
            }
        }

        public void SetReportKind(ReportKind kind)
        {
            _storage.SetValue(KnownKeys.ReportKind, kind);
        }

        public void OnAppClose()
        {
            IsBusy = true;
            InvokeStopScan(null);
        }


        private async Task<T> TryAction<T>(Task<T> task) where T : class
        {
            T taskResult = default(T);
            try
            {
                taskResult = await task;
            }
            catch (Exception ex)
            {
                TryActionException(ex);
            }
            return taskResult;
        }

        private void TryActionException(Exception ex)
        {


            LogHelper.Trace(TraceEventType.Error, $"TryAction error: {ex.GetType()} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        public bool ValidateCredentials(out string errorMessage)
        {
            errorMessage = null;

            if (String.IsNullOrEmpty(Login))
            {
                errorMessage = "Please specify Login";
                return false;
            }
            if (String.IsNullOrEmpty(Password))
            {
                errorMessage = "Please specify Password";
                return false;
            }

            return true;
        }

        public bool ValidatePorts(out string errorMessage)
        {
            bool hasError;
            errorMessage = TcpPort.ValidateTCP(out hasError);
            if (hasError) return false;
            errorMessage = UdpPort.ValidateUDP(out hasError);
            if (hasError) return false;
            return true;
        }
    }
}
