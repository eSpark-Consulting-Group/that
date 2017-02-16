using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

using System.Windows.Threading;
using Telerik.Windows.Controls;

using FAT.Application.Messages;
using FAT.Application.Extensions;
using FAT.Application.Services;
using APTAT.Common;
using APTAT.DeploymentCore.Directory;
using APTAT.Core.Descent.Apt;
using System.Threading.Tasks;
using APTAT.Application.Services.EventArgs;
using APTAT.Core;
using FAT.Application.Models.Apt;
using FAT.Application.Models.ModelBase;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace FAT.Application.Models
{
    public sealed class MainViewViewModel : ViewModel, IMainViewModel
    {
        public event EventHandler ScanProcessCompleted;
        public event EventHandler<DomainEventArgs> ErrorOccurred;


        private string _tcpPort = "80";
        private string _udpPort = "53";
        private Double _percentage;
        private IThreatInfoItem _selectedItem;
        private readonly IAptService _aptService;
        private readonly ITargetsDiscoveryService _targetsDiscoveryService;
        private readonly IStorageService _storage;
        private Boolean _isReportEnabled;
        private ObservableCollection<IThreatInfoItem> _files;
        private ObservableCollection<IThreatInfoItem> _computers;
        private readonly ScanCounterManager _scanCounterManager;
        private bool _groupByFiles;
        private bool _groupByComputers;

        private TimeSpan? _lastModifiesComputerDate = null;
        private DateTime? _lastModifiesFlsVerdictDate = null;
        private DateTime? _lastModifiesValkyrieVerdictDate = null;

        private List<IThreatInfoItem> _selectedItems = new List<IThreatInfoItem>();
        private List<IComputer> _selectedDiscoveryResults = new List<IComputer>();

        private Guid _operationGuid;
        private Boolean? _isScanInProgress = false;
        private Boolean _isAutoDiscoveryInProgress;
        private Boolean _isAutoDiscoveryCompleted;
        private Boolean _cancelInProgress;
        private IThreatInfoItem[] _onlineItems;
        private readonly DispatcherTimer _scanIteratorTimer;

        #region Props
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


        public string Login { get; set; }
        public string Password { get; set; }

        public string RescanLogin { get; set; }
        public string RescanPassword { get; set; }

        public bool UseCurrentDomainForRescan { get; set; }

        public IMainViewModel ParentViewModel
        {
            get { return this; }
        }

        public WizardStepEnum CurrentStep { get; set; }


        
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

        public bool ScanWasOnceStarted { get; set; }

        public bool IsAnyScanInProgress
        {
            get
            {
                return IsScanInProgress.GetValueOrDefault(true) || IsAutoDiscoveryInProgress;
            }
        }

        public Boolean CancelInProgress
        {
            get { return _cancelInProgress; }
            set
            {
                if (_cancelInProgress == value) return;
                _cancelInProgress = value;

                RaisePropertyChanged(nameof(Header));
                RaisePropertyChanged(nameof(ComputersHeader));

                App.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    StopScanCommand.InvalidateCanExecute();
                });
            }
        }

        public Boolean IsScanCancelled { get; set; }

        public Boolean? IsScanInProgress
        {
            get { return _isScanInProgress; }
            set
            {
                _isScanInProgress = value;
                RaisePropertyChanged(nameof(IsScanInProgress));
                RaisePropertyChanged(nameof(IsAnyScanInProgress));
                RaisePropertyChanged(nameof(Header));
                RaisePropertyChanged(nameof(ComputersHeader));
                App.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    StartScanCommand.InvalidateCanExecute();
                    StopScanCommand.InvalidateCanExecute();
                });
            }
        }

        public Boolean IsAutoDiscoveryInProgress
        {
            get { return _isAutoDiscoveryInProgress; }
            set
            {
                _isAutoDiscoveryInProgress = value;
                RaisePropertyChanged(nameof(IsAutoDiscoveryInProgress));
                RaisePropertyChanged(nameof(IsAnyScanInProgress));
                RaisePropertyChanged(nameof(Header));
            }
        }

        public Boolean IsAutoDiscoveryCompleted
        {
            get { return _isAutoDiscoveryCompleted; }
            set
            {
                _isAutoDiscoveryCompleted = value;
                if (value)
                {
                    IsAutoDiscoveryInProgress = false;
                    _targetsDiscoveryService.DeploymentTargetsAutoResolving -= OnDeploymentTargetsAutoResolving;
                    Percentage = 0;
                    Set(() => SelectDiscoveryResults, true);
                }

                RaisePropertyChanged(nameof(IsAutoDiscoveryCompleted));
            }
        }


        private int _scannedItemsCount;
        public int ScannedItemsCount
        {
            get { return _scannedItemsCount; }
            set
            {
                _scannedItemsCount = value;
                RaisePropertyChanged(nameof(ScannedItemsCount));
                RaisePropertyChanged(nameof(ComputersHeader));
            }
        }

        public int TotalItemsCount { get; set; }

        public bool RaisedHasMalwares { get; set; }

        public bool RaisedHasUnknowns { get; set; }
        
        public Double Percentage    
        {
            get { return _percentage; }
            set
            {
                _percentage = value;
                RaisePropertyChanged(nameof(Percentage));
                RaisePropertyChanged(nameof(Header));
                RaisePropertyChanged(nameof(ComputersHeader));
            }
        }

        public String ComputersHeader
        {
            get
            {
                if (!IsAutoDiscoveryInProgress && !CancelInProgress && !IsScanCancelled && Percentage > 0 && Percentage <= 100)
                {
                    return $"(Files: {_scanCounterManager.Total} | Computers: {ScannedItemsCount} of {TotalItemsCount})";
                }

                return string.Empty;
            }
        }

        public String Header
        {
            get
            {
                string header = string.Empty;

                if (CancelInProgress)
                    return "Canceling...";
                if (IsScanInProgress == null)
                    return "Discovering Computers...";
                if (IsAutoDiscoveryInProgress)
                {
                    if (Percentage > 0)
                        header = String.Format("{0:0.00}% Discovering Computers...", Percentage).Replace(".00", string.Empty);
                    return header;
                }
                if (IsScanInProgress == true)
                {
                    if (Percentage == 0)
                        header = "Starting scan...";
                    else if (Percentage == 100)
                        header = "Finishing scan...";
                    else
                        header = String.Format("{0:0.00}% Scanning...", Percentage).Replace(".00", string.Empty);
                    return header;
                }
                else
                {
                    if (IsScanCancelled)
                        return "Scan Canceled";
                    if (Percentage > 0)
                        return "Scan Completed";
                }
                

                return string.Empty;
            }
        }

        public Boolean IsDetailsEnabled
        {
            get { return SelectedItem != null && !SelectedItem.IsComputer; }
        }

        public bool IsGlobalStopEnabled
        {
            get
            {
                return IsAnyScanInProgress && IsScanInProgress.HasValue && !CancelInProgress;
            }
        }

        public Boolean IsRestartEnabled
        {
            get
            {
                return SelectedItems != null && SelectedItems.Count > 0 && SelectedItems.Any(_ => _.IsComputer &&
                  (_.ScanStage == ScanStage.ScanFailed ||
                  (_.ScanStage == ScanStage.OperationFailed) ||
                  _.ScanStage == ScanStage.OperationCanceled));
            }
        }

        public Boolean IsStopEnabled
        {
            get
            {
                return SelectedItems != null && SelectedItems.Count > 0 && SelectedItems.Any(_ => _.IsComputer && _.ScanStage == ScanStage.ScanInProgress);
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
        
        public IThreatInfoItem SelectedItem
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
                    _storage.SetValue(KnownKeys.UnknownFile, _selectedItem);
                }

                RaisePropertyChanged(nameof(SelectedItem));
            }
        }

        public List<IThreatInfoItem> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                _selectedItems = value ?? new List<IThreatInfoItem>();

                if (_selectedItems.Count == 1)
                    SelectedItem = _selectedItems[0];

                SelectCommand.InvalidateCanExecute();
                RestartComputerScanCommand.InvalidateCanExecute();
                RestartComputerScanWithCredentialsCommand.InvalidateCanExecute();
                StopComputerScanCommand.InvalidateCanExecute();
            }
        }

        public string ValkyrieDetailsLink
        {
            get { return _aptService.GetValkyrieDetailsLink(); }
        }

        public bool ValkyrieDetailsLinkAvailable { get; set; }

        public bool SelectDiscoveryResults { get; set; }

        public ObservableCollection<DeploymentTarget> DiscoveryResults { get; set; }

        public List<IComputer> SelectedDiscoveryResults
        {
            get { return _selectedDiscoveryResults; }
            set
            {
                _selectedDiscoveryResults = value ?? new List<IComputer>();
                StartScanWithDiscoveryResultsCommand.InvalidateCanExecute();
            }
        }

        public ObservableCollection<IThreatInfoItem> Files
        {
            get { return _files; }
            set
            {
                _files = value;
                RaisePropertyChanged(nameof(Files));
            }
        }

        public ObservableCollection<IThreatInfoItem> Computers
        {
            get { return _computers; }
            set
            {
                _computers = value;
                RaisePropertyChanged(nameof(Computers));
            }
        }

        public ScanCounterCollection ScanCounters
        {
            get
            {
                return _scanCounterManager.CounterCollection;
            }
        }


        public int[] FilterOrders { get; set; } 

        public bool IsQuickScan { get; set; }


        public bool GroupEnabled { get; set; }

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

        public bool GroupByComputers
        {
            get { return _groupByComputers; }
            set
            {
                Set(() => GroupEnabled, true);
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
                return !GroupByFiles ? "Search By Computers" : "Search By Files";
            }
        }

        public string Email { get; set; } = string.Empty;

        public bool HideSubmitEmail { get; set; }

        #endregion

        #region Commands
        public DelegateCommand OnLoadedCommand { get; private set; }
        public DelegateCommand StartScanCommand { get; private set; }
        public DelegateCommand StopScanCommand { get; private set; }
        public DelegateCommand SelectCommand { get; private set; }
        public DelegateCommand StopAutoDiscoveryCommand { get; private set; }
        public DelegateCommand StartScanWithDiscoveryResultsCommand { get; private set; }
        public DelegateCommand SubmitEmailCommand { get; private set; }
        public DelegateCommand RestartComputerScanCommand { get; private set; }
        public DelegateCommand RestartComputerScanWithCredentialsCommand { get; private set; }
        public DelegateCommand StopComputerScanCommand { get; private set; }
        #endregion

        public MainViewViewModel(ITargetsDiscoveryService targetsDiscoveryService,
            IAptService aptService, IStorageService storage)
        {
            _targetsDiscoveryService = targetsDiscoveryService;
            _aptService = aptService;
            _storage = storage;

            _scanCounterManager = new ScanCounterManager();
            _scanCounterManager.SelectedOrdersChanged = orders => Set(() => FilterOrders, orders);

            _scanIteratorTimer = new DispatcherTimer(DispatcherPriority.Normal, App.Current.Dispatcher);
            _scanIteratorTimer.Interval = TimeSpan.FromMilliseconds(500d);
            _scanIteratorTimer.Tick += IterateThroughScan;

            InitCommands();  
        }

        private void InitCommands()
        {
            OnLoadedCommand = new DelegateCommand(OnLoaded);
            StartScanCommand = new DelegateCommand(o =>
                AppMessages.ScanMessage.Send(MessageTypes.StartScan, InvokeStartScan),
                o => !IsAnyScanInProgress);
            StopScanCommand = new DelegateCommand(o =>
                AppMessages.ScanMessage.Send(MessageTypes.StopScan, InvokeStopScan),
                o => IsGlobalStopEnabled);
            StopAutoDiscoveryCommand = new DelegateCommand(InvokeStopAutoDiscovery);
            StartScanWithDiscoveryResultsCommand = new DelegateCommand(InvokeScanWithDiscoveryResults, 
                o => SelectedDiscoveryResults.Any());
            SelectCommand = new DelegateCommand(o => { }, 
                o => IsDetailsEnabled);
            SubmitEmailCommand = new DelegateCommand(InvokeSubmitEmail);
            RestartComputerScanCommand = new DelegateCommand(o => InvokeRestartComputerScan(o), 
                o => IsRestartEnabled);
            RestartComputerScanWithCredentialsCommand = new DelegateCommand(o => InvokeRestartComputerScan(o, true), 
                o => IsRestartEnabled && SelectedItems.Any(_ => !_.IsLocal));
            StopComputerScanCommand = new DelegateCommand(InvokeStopComputerScan, 
                o => IsStopEnabled);
        }

        private async void OnLoaded(object o)
        {
            IsAutoDiscoveryInProgress = true;
            DiscoveryResults = new ObservableCollection<DeploymentTarget>();
            RaisePropertyChanged(nameof(DiscoveryResults));

            Set(() => Uri, "/Views/DiscoveryResults.xaml");
            Set(() => Loaded, true);

            _targetsDiscoveryService.DeploymentTargetsAutoResolving += OnDeploymentTargetsAutoResolving;
            _targetsDiscoveryService.StartTargetsAutoDiscovery();

            var name = await TryAction(GetCurrentDomainName());
            CurrentUser = _targetsDiscoveryService.GetLoggedInUser();
            CurrentDomainName = name?.Replace("DC=", "").Replace(",", ".");
            UseCurrentDomainForRescan = HasCurrentDomainName;

            RaisePropertyChanged(nameof(CurrentDomainNameContent));
            RaisePropertyChanged(nameof(HasCurrentDomainName));
        }

        private void InvokeSubmitEmail(Object arg)
        {
            if (_aptService.SubmitEmail(Email.Split(new[] { ',', ';' })).IsSucceeded)
                Set(() => HideSubmitEmail, true);
            else
            { }
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
                    RestartComputerScan(true, IsQuickScan);
                    return;
                }
            }
            else
                RestartComputerScan(false, IsQuickScan);
        }

        private void InvokeScanWithDiscoveryResults(Object arg)
        {
            SettingsWindow window = new SettingsWindow();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.DataContext = this;
            window.ShowCredentials = true;
            window.ShowScanModes = true;
            window.Header = "Scan Credentials";

            if (window.ShowDialog().GetValueOrDefault())
                RestartComputerScan(true, window.IsQuickScan, true);
        }

        private void RestartComputerScan(bool useNewCredentials, bool isQuickScan, bool useDiscoveryResults = false)
        {
            if (useDiscoveryResults)
            {
                _scanCounterManager.Clear();

                var items = SelectedDiscoveryResults.Select(t => new ThreatInfoItem(t)).ToArray();
                Set(() => TotalItemsCount, items.Length);
                ScannedItemsCount = 0;
                _onlineItems = items.Where(item => item.IsOnline).ToArray();
                Computers = new ObservableCollection<IThreatInfoItem>(items);
                Files = new ObservableCollection<IThreatInfoItem>();

                GroupByComputers = true;
                Set(() => ScanWasOnceStarted, true);
            }

            if (_onlineItems.Length > 0)
            {
                ScanKind scanKind = ScanKind.Quick;
                if (isQuickScan)
                {
                    scanKind = ScanKind.Quick;
                }
                else
                {
                    scanKind = ScanKind.Full;
                }

                var login = useNewCredentials ? (!UseCurrentDomainForRescan ? (RescanLogin ?? Login) : null) : Login;
                var password = useNewCredentials ? (!UseCurrentDomainForRescan ? (RescanPassword ?? Password) : null) : Password;

                ScanOptions options;
                CustomScanOptions customScanOptions = null;
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

                if (!useDiscoveryResults)
                {
                    var items = new List<IThreatInfoItem>();
                    foreach (var item in SelectedItems)
                    {
                        if (item.IsComputer &&
                            (item.ScanStage == ScanStage.ScanFailed ||
                            item.ScanStage == ScanStage.OperationFailed ||
                            item.ScanStage == ScanStage.OperationCanceled))
                        {
                            item.RestoreConnectingState();
                            items.Add(item);
                        }
                    }

                    _lastModifiesComputerDate = null;
                    _aptService.BeginScan(items.Select(_ => _.DeploymentTarget).ToArray(),
                        _storage.GetValue<DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, false), options);
                }
                else
                {
                    _lastModifiesComputerDate = null;
                    options = new ScanOptions(_tcpPort, _udpPort, scanKind, login, password, true);
                    _aptService.BeginScan(SelectedDiscoveryResults.ToArray(),
                        _storage.GetValue<DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, false), options);
                }
                if (!IsScanInProgress.GetValueOrDefault())
                {
                    _lastModifiesComputerDate = null;
                    IsScanInProgress = true;
                    IsScanCancelled = false;
                    IsReportEnabled = false;
                    Percentage = 0;
                }
            }
        }

        private void InvokeStopAutoDiscovery(Object arg)
        {
            IsAutoDiscoveryCompleted = true;
            _targetsDiscoveryService.StopTargetsAutoDiscovery();
        }

        private void InvokeStopComputerScan(Object arg)
        {
            var items = new List<IThreatInfoItem>();
            foreach (var item in SelectedItems)
            {
                if (item.IsComputer && item.ScanStage == ScanStage.ScanInProgress)
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

            Files = new ObservableCollection<IThreatInfoItem>();
            Computers = new ObservableCollection<IThreatInfoItem>();


            Set(() => RaisedHasUnknowns, false);
            Set(() => RaisedHasMalwares, false);

            Set(() => ValkyrieDetailsLinkAvailable, false);

            _onlineItems = new IThreatInfoItem[0];

            SelectedItem = null;
            Percentage = 0;
            IsScanCancelled = false;
            IsReportEnabled = false;
            
            RescanLogin = null;
            RescanPassword = null;

            GroupByComputers = true;

            var value = _storage.GetValue<ICollection<Object>>(KnownKeys.CheckedDirectoryItems, true);
            if (value != null)
            {
                var targets = value.OfType<DeploymentItem>().ToArray();
                if (targets.Length > 0)
                {
                    IsScanInProgress = null;
                    Set(() => ScanWasOnceStarted, true);
                    Keyword = null;

                    var model = CompositionHelper.Instance.Resolve<StartScanDomainViewModel>();

                    _targetsDiscoveryService.ResolveDirectoryItems(targets,
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
                targets = (Guid[]) arg;

            _aptService.StopScan(targets);

        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            _targetsDiscoveryService.DirectoryItemsResolved += OnDirectoryItemsResolved;


            _aptService.FatScanStarted += OnScanStarted;
            _aptService.FatScanCompleted += OnScanCompleted;
            _aptService.CancelScanStarted += OnCancelScanStarted;

        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _targetsDiscoveryService.DirectoryItemsResolved -= OnDirectoryItemsResolved;
            _targetsDiscoveryService.DeploymentTargetsAutoResolving -= OnDeploymentTargetsAutoResolving;

            _aptService.CancelScanStarted -= OnCancelScanStarted;
            _aptService.FatScanStarted -= OnScanStarted;
            _aptService.FatScanCompleted -= OnScanCompleted;
        }


        public void OnPortChanged(int udpPort, int tcpPort)
        {
            _aptService.UpdateFlsPorts(udpPort, tcpPort);
        }

        private void OnScanCompleted(Object sender, ErrorEventArgs e)
        {
            
            if (e.Error != null)
            {
                ErrorOccurred?.Invoke(this, new DomainEventArgs(null, e.Error));
            }
            ScanProcessCompleted?.Invoke(this, new DomainEventArgs());
        }

        private void OnScanCompleted()
        {
            if (IsScanInProgress.GetValueOrDefault())
            {
                IsScanInProgress = false;
                _storage.SetValue(KnownKeys.OperationGuid, _operationGuid);
                IsReportEnabled = true;

                Set(() => ValkyrieDetailsLinkAvailable, true);

                CancelInProgress = false;
            }
        }

        private void OnDeploymentTargetsAutoResolving(object sender, DeploymentTargetResolvingEventArg e)
        {
            if (e.Target != null)
            {
                if (!e.Target.IsLocalComputer || !DiscoveryResults.Any(_ => _.IsLocalComputer))
                {
                    DiscoveryResults.Add(e.Target);
                    _scanCounterManager.AddComputer();
                }
            }

            Percentage = e.Progress * 100d;

            if (Math.Abs(Percentage - 100) < 0.0001)
                IsAutoDiscoveryCompleted = true;
        }

        private void OnDirectoryItemsResolved(Object sender, DirectoryItemsResolvedEventArgs e)
        {

            IsScanInProgress = true;
            if (e.Exception != null)
            {
                ErrorOccurred?.Invoke(this, new DomainEventArgs { Exception = e.Exception });
                IsScanInProgress = false;
                return;
            }
            if (e.Items == null)
            {
                return;
            }

            var list = new ObservableCollection<IThreatInfoItem>();
            foreach (var item in e.Items)
            {
                list.Add(new ThreatInfoItem(item));
            }

            Set(() => TotalItemsCount, list.Count);
            ScannedItemsCount = 0;
            _onlineItems = list.Where(item => item.IsOnline).ToArray();
            Computers = list;


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
                if (_storage.ContainsKey(KnownKeys.CustomScanOptions))
                {
                    var customScanOptions = _storage.GetValue<CustomScanOptions>(KnownKeys.CustomScanOptions, true);
                    scanKind = ScanKind.Custom;
                    options = new ScanOptions(_tcpPort, _udpPort, customScanOptions.FolderItems.Select(f => f.FullName).ToArray(),
                        customScanOptions.ScanCriticalAreas, Login, Password);
                }
                else
                {
                    options = new ScanOptions(_tcpPort, _udpPort, scanKind, Login, Password);
                }

                _lastModifiesComputerDate = null;
                _aptService.BeginScan(e.Items, _storage.GetValue<DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, false), options);

            }
            else
            {
                IsScanInProgress = false;
            }
        }

        private void OnScanStarted(Object sender, ScanStartedEventArgs e)
        {
            _operationGuid = e.OperationUid;

            _scanCounterManager.Clear();

            if (!_scanIteratorTimer.IsEnabled)
                _scanIteratorTimer.Start();
        }

        private void OnCancelScanStarted(object sender, EventArgs e)
        {
            CancelInProgress = true;
            IsScanCancelled = true;
        }

        private void IterateThroughScan(object sender, EventArgs e)
        {
            if (_scanIteratorTimer.IsEnabled)
                _scanIteratorTimer.Stop();
            else
                return;
            var operationResult = _aptService.GetModifiedResultsForOperation(_operationGuid, _lastModifiesComputerDate,
                _lastModifiesFlsVerdictDate, _lastModifiesValkyrieVerdictDate);
            var computers = operationResult.Computers.ToDictionary(c => c.TargetUid);
            

            var files = operationResult.Files;
            if (files.Any())
            {
                var maxDate = files.Max(v => v.ValkyrieModifiedDate);
                if (maxDate != null)
                    _lastModifiesValkyrieVerdictDate = maxDate;
                maxDate = files.Max(v => v.FlsVerdictModifiedDate);
                if (maxDate != null)
                    _lastModifiesFlsVerdictDate = maxDate;
            }
            if (computers.Any())
            {
                var maxDate = computers.Values.Max(c => c.Modified);
                _lastModifiesComputerDate = maxDate;
            }

            Int32 completed = 0;
            Int32 scanned = 0;
            double percentages = 0;
            IScanCounterDiff counterDiff = new ScanCounterDiff();
            foreach (var infoItem in _onlineItems)
            {
                ScanResultItem scanProgress = null;
                computers.TryGetValue(infoItem.ComputerGuid, out scanProgress);
                if (infoItem.ScanStage != ScanStage.OperationFailed &&
                        infoItem.ScanStage != ScanStage.OperationCompleted &&
                        infoItem.ScanStage != ScanStage.OperationCanceled )
                {
                    if (scanProgress != null)
                    {
                        if (scanProgress.ScanStage == ScanStage.OperationCompleted)
                        {
                            if (scanProgress.BadFiles.Any() && infoItem.Items.Count == 0)
                            {
                                foreach (var badFile in scanProgress.BadFiles)
                                {
                                    var item = new ThreatInfoItem(badFile);
                                    infoItem.AddFile(item);
                                    item = new ThreatInfoItem(badFile);
                                    var exist = Files.FirstOrDefault(_ =>
                                        _.BadFile.Sha1 == item.BadFile.Sha1 && _.IsUnknown == item.IsUnknown &&
                                        _.IsMalware == item.IsMalware && _.Failed == item.Failed);
                                    var comp = new ThreatInfoItem
                                    {
                                        IpAddress = infoItem.IpAddress,
                                        ComputerGuid = infoItem.ComputerGuid,
                                        Name = infoItem.Name,
                                        ShortName = item.Name,
                                        IsComputer = true
                                    };
                                    if (exist != null)
                                        exist.Computers.Add(comp);
                                    else
                                    {
                                        item.Computers = new ObservableCollection<IThreatInfoItem> { comp };
                                        Files.Add(item);
                                    }

                                }
                            }
                            infoItem.RemoteScanCompleted();
                        }
                        infoItem.UpdateCounters(scanProgress, counterDiff);
                    }
                }
                if (infoItem.ScanStage == ScanStage.OperationFailed ||
                    infoItem.ScanStage == ScanStage.OperationCompleted ||
                    infoItem.ScanStage == ScanStage.OperationCanceled)
                    completed++;
                if (files.Any())
                    infoItem.UpdateVerdictForFiles(files, counterDiff);
                var localPercentage = infoItem.TotalProgressPercentage;
                if (IsScanCancelled && localPercentage < 100 && infoItem.ScanStage != ScanStage.OperationCancelling
                    && infoItem.ScanStage != ScanStage.OperationCanceled)
                {
                    infoItem.TrySetCancelledStatus();
                }
                if (localPercentage == 100 && infoItem.ScanStage == ScanStage.OperationCompleted)
                    scanned++;
                percentages += localPercentage;
            }
            if (Math.Abs(Percentage - 100) > 0.01 && _onlineItems.Length > 0)
                Percentage = percentages / (double)_onlineItems.Length;

            foreach (var result in files)
            {
                var items = Files.Where(i => String.Equals(i.BadFile.Sha1, result.Sha1, StringComparison.OrdinalIgnoreCase)).ToArray();

                foreach (var IThreatInfoItem in items)
                {
                    IThreatInfoItem.UpdateFileVerdict(result);
                }
            }

            _scanCounterManager.UpdateCounters(counterDiff);

            ScannedItemsCount = scanned;
            if (!RaisedHasMalwares)
            {
                Set(() => RaisedHasMalwares, 
                    _scanCounterManager.HasMalwares);

                if (!RaisedHasMalwares)
                    Set(() => RaisedHasUnknowns, 
                        _scanCounterManager.HasUnknowns || 
                        _scanCounterManager.HasNotAnalyzed || 
                        _scanCounterManager.HasInProgress);
            }

            if (Math.Abs(Percentage - 100) < 0.01)
                OnScanCompleted();

            if (!_scanIteratorTimer.IsEnabled)
                _scanIteratorTimer.Start();
        }

        public Predicate<object> FilterPredicate
        {
            get
            {
                return item =>
                {
                    if (item == null)
                        return false;

                    if (GroupByComputers)
                    {
                        var o = item as IThreatInfoItem;
                        if (o.IsComputer)
                            return FilterComputerByOrders(o) && o.NameAndIpAddress.ContainsString(Keyword);
                        else
                            return FilterFileByOrders(o) && string.IsNullOrEmpty(Keyword);
                    }
                    else if (GroupByFiles)
                    {
                        var o = item as IThreatInfoItem;
                        if (!o.IsComputer)
                            return FilterFileByOrders(o) && o.NameAndIpAddress.ContainsString(Keyword);
                        else
                            return false;
                    }
                    else
                    {
                        var o = item as DeploymentTarget;
                        return o.DnsName.ContainsString(Keyword) || o.Name.ContainsString(Keyword);
                    }
                };
            }
        }

        private bool FilterComputerByOrders(IThreatInfoItem item)
        {
            if (FilterOrders == null || !FilterOrders.Any()) return true;
            int[] orders = FilterOrders.ToArray();
            foreach (int order in orders)
            {
                if ((item.HasClean && order == (int)CounterOrder.Clean) ||
                    (item.HasMalwares && order == (int)CounterOrder.Malware) ||
                    (item.HasUnknowns && order == (int)CounterOrder.Unknown) ||
                    (item.HasFailed && item.ScanStage == ScanStage.OperationCompleted && order == (int)CounterOrder.NotAnalyzed) ||
                    (item.HasPending && order == (int)CounterOrder.InProgress))
                    return true;
            }
            return false;
        }

        private bool FilterFileByOrders(IThreatInfoItem item)
        {
            if (FilterOrders == null || !FilterOrders.Any()) return true;
            int[] orders = FilterOrders.ToArray();
            foreach (int order in orders)
            {
                if ((item.Clean && order == (int)CounterOrder.Clean) ||
                    (item.IsMalware && order == (int)CounterOrder.Malware) ||
                    (item.IsUnknown && order == (int)CounterOrder.Unknown) ||
                    (item.Failed && order == (int)CounterOrder.NotAnalyzed) ||
                    (item.Pending && order == (int)CounterOrder.InProgress))
                    return true;
            }
            return false;
        }

        public void SetReportKind(ReportKind kind)
        {
            _storage.SetValue(KnownKeys.ReportKind, kind);
        }

        public async void OnAppClose(Action action)
        {
            if (IsAnyScanInProgress)
            {
                if (IsAutoDiscoveryInProgress)
                {
                    InvokeStopAutoDiscovery(null);
                    action?.Invoke();
                }
                else
                    InvokeStopScan(null);
            }
            else
            {
                await _aptService.OnAppClosing();
                action?.Invoke();
            }
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
