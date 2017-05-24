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
using FAT.Application.Models.Directory;
using System.Text.RegularExpressions;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using FAT.Application.Helpers;
using ReportKind = FAT.Application.Services.ReportKind;
using static FAT.Application.Messages.AppMessages;

namespace FAT.Application.Models
{
    public sealed class MainViewModel : ViewModel, IMainViewModel
    {
        public event EventHandler ScanProcessCompleted;
        public event EventHandler<DomainEventArgs> ErrorOccurred;

        private readonly IAptService _aptService;
        private readonly ITargetsDiscoveryService _targetsDiscoveryService;
        private readonly IStorageService _storage;
        private readonly IScanCounterManager _scanCounterManager;
        private readonly IScanOperationsManager _scanOperationsManager;
        private readonly IScanTimer _scanTimer;
        private readonly IStatisticsService _statisticsService;
        private ObservableCollection<IThreatInfoItem> _files;
        private ObservableCollection<IThreatInfoItem> _computers;

        private bool _groupEnabled;
        private bool _groupByFiles;
        private bool _groupByComputers;
        private bool _isFailed;
        private bool _isSearchInitiated;
        private bool _raisedHasUnknowns;
        private bool _raisedHasMalwares;
        private bool _isReportEnabled;
        private bool _isAutoDiscoveryInProgress;
        private bool _isAutoDiscoveryCompleted;
        private bool _cancelInProgress;
        private bool _submitingEmail;
        private bool _hasEmail;
        private bool _previousScansAvailable;
        private bool _valkyrieDetailsLinkAvailable;

        private bool? _isScanInProgress = false;

        private TimeSpan? _lastModifiesComputerDate;
        private DateTime? _lastModifiesFlsVerdictDate;
        private DateTime? _lastModifiesValkyrieVerdictDate;

        private IThreatInfoItem _selectedItem;
        private List<IThreatInfoItem> _selectedItems = new List<IThreatInfoItem>();
        private IThreatInfoItem[] _onlineItems;

        private Guid _operationGuid;
        private int _scannedItemsCount;
        private double _percentage;
        private string _tcpPort = "80";
        private string _udpPort = "53";
        private string _email = string.Empty;
        private string _scanKeyword;
        private string _valkyrieDetailsLink;


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

        public ViewModel ParentViewModel
        {
            get { return this; }
        }

        public MainViewModel PreviousScansViewModel { get; private set; }

        public MainViewModel DataContextSwitcher
        {
            get { return PreviousScansViewModel ?? this; }
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

                if (App.Current != null)
                    App.Current.Dispatcher.BeginInvoke((Action)delegate
                {
                    StopScanCommand.InvalidateCanExecute();
                });
            }
        }

        public Boolean IsScanCancelled { get; set; }
        public Boolean IsValkyrieError { get; set; }

        public Boolean IsScanFailed { get; set; }

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

                if (App.Current != null)
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
                    Percentage = 0;

                    _targetsDiscoveryService.DeploymentTargetsAutoResolving -= OnDeploymentTargetsAutoResolving;

                    if (!DiscoveryResults.Any(_ => _.IsSelected))
                        Set(() => SelectDiscoveryResults, true);
                }

                StartScanCommand.InvalidateCanExecute();
                RaisePropertyChanged(nameof(IsAutoDiscoveryCompleted));
            }
        }

        public int ScannedItemsCount
        {
            get { return _scannedItemsCount; }
            set
            {
                _scannedItemsCount = value;

                RaisePropertyChanged(nameof(ScannedItemsCount));
            }
        }

        public int TotalItemsCount { get; set; }

        public bool RaisedHasMalwares
        {
            get { return _raisedHasMalwares; }
            set
            {
                _raisedHasMalwares = value;
                RaisePropertyChanged(nameof(RaisedHasMalwares));
            }
        }

        public bool RaisedHasUnknowns
        {
            get { return _raisedHasUnknowns; }
            set
            {
                _raisedHasUnknowns = value;
                RaisePropertyChanged(nameof(RaisedHasUnknowns));
            }
        }

        public bool ShowResult
        {
            get
            {
                return !IsScanCancelled && !IsScanFailed && (!RaisedHasUnknowns || (_scanCounterManager.HasUnknowns || _scanCounterManager.HasInProgress));
            }
        }

        public bool NotRaisedHasMalwaresOrUnknowns
        {
            get { return !RaisedHasMalwares && !RaisedHasUnknowns; }
        }

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
                if (!IsAutoDiscoveryInProgress && !CancelInProgress && !IsScanCancelled && !IsScanFailed && Percentage > 0 && Percentage <= 100)
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
                    return "Searching for Available Devices...";
                if (IsAutoDiscoveryInProgress)
                {
                    var percentage = Math.Min(Math.Max(Math.Round(Percentage), 1), 99);
                    header = String.Format("{0}% Searching for Available Devices...", percentage);
                    return header;
                }
                if (IsScanInProgress == true)
                {
                    if (Percentage == 0)
                        header = "Starting Scan...";
                    else if (Percentage == 100)
                        header = "Finishing Scan...";
                    else
                    {
                        var percentage = Math.Min(Math.Max(Math.Round(Percentage), 1), 99);
                        header = String.Format("{0}% Scan In Progress...", percentage);
                    }
                    return header;
                }
                else
                {
                    if (IsScanCancelled || (Computers?.Any(_ => _.ScanStage == ScanStage.OperationCanceled) ?? false))
                        return "Scan Canceled";
                    if (IsScanFailed)
                        return "Scan Failed";
                    if (Percentage > 0)
                        return "Scan Completed";
                    else
                        return "Discovery Completed";
                }
            }
        }

        public string SubmitHeader
        {
            get { return _submitingEmail ? "Please Wait..." : "Submit"; }
        }

        public bool NotSubmitingEmail
        {
            get { return !_submitingEmail; }
        }

        public bool IsDetailsEnabled
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

        /*public Boolean IsRestartEnabled
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
        }*/

        public Boolean IsReportEnabled
        {
            get { return _isReportEnabled && !CurrentScanAvailable; }
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
                /*RestartComputerScanCommand.InvalidateCanExecute();
                RestartComputerScanWithCredentialsCommand.InvalidateCanExecute();
                StopComputerScanCommand.InvalidateCanExecute();*/
            }
        }

        public string ValkyrieDetailsLink
        {
            get { return string.IsNullOrEmpty(_valkyrieDetailsLink) ? _aptService.GetValkyrieDetailsLink() : _valkyrieDetailsLink; }
            set { _valkyrieDetailsLink = value; }
        }

        public bool ValkyrieDetailsLinkAvailable
        {
            get { return _valkyrieDetailsLinkAvailable; }
            set
            {
                if (value && Computers.All(_ => _.ScanStage == ScanStage.OperationFailed || _.ScanStage == ScanStage.OperationCanceled || !_.IsOnline))
                    value = false;

                _valkyrieDetailsLinkAvailable = value;
                RaisePropertyChanged(nameof(ValkyrieDetailsLinkAvailable));
            }
        }

        public bool? SelectDiscoveryResults { get; set; } = false;

        public ObservableCollection<DeploymentTarget> DiscoveryResults { get; set; }

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

        public IScanCounterCollection ScanCounters
        {
            get
            {
                return _scanCounterManager.CounterCollection;
            }
        }

        public IScanOperationCollection ScanOperations
        {
            get
            {
                return _scanOperationsManager.OperationCollection;
            }
        }

        public bool? FilterReset { get; set; }

        public int[] FilterOrders { get; set; }

        public bool IsQuickScan { get; set; }

        public bool GroupEnabled
        {
            get { return _groupEnabled; }
            set
            {
                _groupEnabled = value;
                RaisePropertyChanged(nameof(GroupEnabled));
            }
        }

        public bool GroupByFiles
        {
            get { return _groupByFiles; }
            set
            {
                if (value)
                {
                    Set(() => Uri, "/Views/Scan/ScanResultsWithFiles.xaml");
                    SelectedItem = null;
                    _groupByComputers = !value;
                }
                _groupByFiles = value;
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
                if (value)
                {
                    GroupEnabled = true;
                    Set(() => Uri, "/Views/Scan/ScanResultsWithComputers.xaml");
                    SelectedItem = null;
                    _groupByFiles = !value;
                }
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
                return !GroupByFiles ? "Search by Computers" : "Search by Files";
            }
        }

        public bool CurrentScanAvailable
        {
            get { return PreviousScansViewModel != null; }
        }

        public string Email
        {
            get { return _email; }
            set
            {
                _email = value;

                RaisePropertyChanged(nameof(Email));
                SubmitEmailCommand.InvalidateCanExecute();
            }
        }

        
        public bool HideSubmitEmail { get; set; }

        public string ScanKeyword
        {
            get { return _scanKeyword; }
            set
            {
                _scanKeyword = value;
                RaisePropertyChanged(nameof(ScanKeyword));
                RaisePropertyChanged(nameof(HasScanKeyword));
            }
        }

        public bool HasScanKeyword
        {
            get
            {
                return !string.IsNullOrEmpty(ScanKeyword);
            }
        }

        #endregion

        #region Commands
        public DelegateCommand OnLoadedCommand { get; private set; }
        public DelegateCommand StartScanCommand { get; private set; }
        public DelegateCommand StopScanCommand { get; private set; }
        public DelegateCommand SelectCommand { get; private set; }
        public DelegateCommand StopAutoDiscoveryCommand { get; private set; }
        public DelegateCommand StartScanWithDiscoveryResultsCommand { get; private set; }
        public DelegateCommand SubmitEmailCommand { get; private set; }
        public DelegateCommand EditEmailCommand { get; private set; }
        public DelegateCommand StartAutoDiscoveryCommand { get; private set; }
        /*public DelegateCommand RestartComputerScanCommand { get; private set; }
        public DelegateCommand RestartComputerScanWithCredentialsCommand { get; private set; }
        public DelegateCommand StopComputerScanCommand { get; private set; }*/
        public DelegateCommand OpenPreviousScansCommand { get; private set; }
        public DelegateCommand OpenCurrentScanCommand { get; private set; }
        public DelegateCommand OpenPreviousScanDetailsCommand { get; private set; }
        public DelegateCommand SearchPreviousScansCommand { get; private set; }
        public DelegateCommand ClearScanKeywordCommand { get; private set; }
        #endregion

        public MainViewModel(
            ITargetsDiscoveryService targetsDiscoveryService,
            IAptService aptService, 
            IStorageService storage, 
            IScanCounterManager scanCounterManager, 
            IScanOperationsManager scanOperationsManager,
            IScanTimer scanTimer,
            IStatisticsService statisticsService)
        {
            _targetsDiscoveryService = targetsDiscoveryService;
            _aptService = aptService;
            _storage = storage;
            _scanCounterManager = scanCounterManager;
            _scanOperationsManager = scanOperationsManager;
            _scanTimer = scanTimer;
            _statisticsService = statisticsService;

            InitCommands();
        }

        private void InitCommands()
        {
            OnLoadedCommand = new DelegateCommand(OnLoaded);

            StartScanCommand = new DelegateCommand(o =>
                AppMessages.ScanMessages.Send(MessageTypes.StartScan, InvokeStartScan), o => !IsAnyScanInProgress);
            StopScanCommand = new DelegateCommand(o =>
                AppMessages.ScanMessages.Send(MessageTypes.StopScan, InvokeStopScan),
                o => IsGlobalStopEnabled);

            SelectCommand = new DelegateCommand(o => { },
                o => IsDetailsEnabled);

            SubmitEmailCommand = new DelegateCommand(InvokeSubmitEmail,
                o => IsValidEmail(Email) && !_submitingEmail);
            EditEmailCommand = new DelegateCommand(o =>
                Set(() => HideSubmitEmail, false));

            /*RestartComputerScanCommand = new DelegateCommand(o => InvokeRestartComputerScan(o),
                o => IsRestartEnabled);
            RestartComputerScanWithCredentialsCommand = new DelegateCommand(o => InvokeRestartComputerScan(o, true),
                o => IsRestartEnabled && SelectedItems.Any(_ => !_.IsLocal));
            StopComputerScanCommand = new DelegateCommand(InvokeStopComputerScan,
                o => IsStopEnabled);*/

            StopAutoDiscoveryCommand = new DelegateCommand(InvokeStopAutoDiscovery);
            StartScanWithDiscoveryResultsCommand = new DelegateCommand(InvokeScanWithDiscoveryResults,
                o => DiscoveryResults != null && DiscoveryResults.Any(_ => _.IsSelected));
            StartAutoDiscoveryCommand = new DelegateCommand(InvokeStartAutoDiscovery);

            OpenPreviousScansCommand = new DelegateCommand(InvokeOpenPreviousScans,
                o => _previousScansAvailable);
            OpenCurrentScanCommand = new DelegateCommand(InvokeOpenCurrentScan);
            OpenPreviousScanDetailsCommand = new DelegateCommand(InvokeOpenPreviousScanDetails);
            SearchPreviousScansCommand = new DelegateCommand(InvokeSearchPreviousScans);
            ClearScanKeywordCommand = new DelegateCommand(InvokeCleanScanKeyword);
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            _targetsDiscoveryService.DirectoryItemsResolved += OnDirectoryItemsResolved;

            _aptService.FatScanStarted += OnScanStarted;
            _aptService.FatScanCompleted += OnScanCompleted;
            _aptService.CancelScanStarted += OnCancelScanStarted;

            _scanCounterManager.SelectedOrdersChanged = orders => Set(() => FilterOrders, orders);
            _scanTimer.ScanIterationHandler = IterateThroughScan;
        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _targetsDiscoveryService.DirectoryItemsResolved -= OnDirectoryItemsResolved;
            _targetsDiscoveryService.DeploymentTargetsAutoResolving -= OnDeploymentTargetsAutoResolving;

            _aptService.FatScanStarted -= OnScanStarted;
            _aptService.FatScanCompleted -= OnScanCompleted;
            _aptService.CancelScanStarted -= OnCancelScanStarted;
        }

        private async void OnLoaded(object o)
        {
            Set(() => Loaded, true);
            
            InvokeStartAutoDiscovery(null);
            InvokeLoadInitialData();
            
            var name = await TryAction(GetCurrentDomainName());

            CurrentUser = _targetsDiscoveryService.GetLoggedInUser();
            CurrentDomainName = name?.Replace("DC=", "").Replace(",", ".");
            RaisePropertyChanged(nameof(CurrentDomainNameContent));
            RaisePropertyChanged(nameof(HasCurrentDomainName));

            CMCStatisticsMessages<IApplicationLaunchedRequest>.Send();
        }

        private void InvokeLoadInitialData()
        {
            ThreadHelper.Instance.RunOnThread(() =>
            {
                var email = _scanOperationsManager.GetEmail();
                if (!string.IsNullOrEmpty(email))
                {
                    _aptService.InitEmail(new[] { email });
                    App.Current?.Dispatcher.Invoke(() =>
                    {
                        Email = email;
                        Set(() => HideSubmitEmail, true);
                        _hasEmail = true;
                    });
                }
            });

            ThreadHelper.Instance.RunOnThread(() =>
            {
                _previousScansAvailable = _scanOperationsManager.HasPreviousScans();
                App.Current?.Dispatcher.Invoke(() => OpenPreviousScansCommand.InvalidateCanExecute());
            });
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

        #region Auto Discovery

        private void InvokeStartAutoDiscovery(Object arg)
        {
            if (_scanTimer.ScanActive)
            {
                _scanTimer.Stop();
                _aptService.StopScan(new Guid[0]);
            }

            _scanCounterManager.Clear();

            Set(() => ScanWasOnceStarted, false);
            Set(() => SelectDiscoveryResults, false);
            GroupEnabled = false;
            IsScanCancelled = false;
            IsScanFailed = false;
            IsReportEnabled = false;
            RaisedHasUnknowns = false;
            RaisedHasMalwares = false;
            ValkyrieDetailsLinkAvailable = false;
            IsAutoDiscoveryCompleted = false;
            Percentage = 0;
            IsAutoDiscoveryInProgress = true;
            GroupByComputers = false;
            GroupByFiles = false;
            Keyword = null;

            DiscoveryResults = new ObservableCollection<DeploymentTarget>();
            RaisePropertyChanged(nameof(DiscoveryResults));
            StartScanCommand.InvalidateCanExecute();
            StartScanWithDiscoveryResultsCommand.InvalidateCanExecute();

            Set(() => Uri, "/Views/DiscoveryResults.xaml");

            _targetsDiscoveryService.DeploymentTargetsAutoResolving += OnDeploymentTargetsAutoResolving;
            _targetsDiscoveryService.StartTargetsAutoDiscovery();
        }

        private void InvokeScanWithDiscoveryResults(Object arg)
        {
            var model = CompositionHelper.Instance.Resolve<StartScanCredentialsViewModel>();
            model.ViewLoadedCommand.Execute(null);
            model.CheckIfCanUseCurrentCredentials(DiscoveryResults.Where(_ => _.IsSelected));

            SettingsWindow window = new SettingsWindow();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.DataContext = model;
            window.ShowCredentials = true;
            window.ShowScanModes = true;
            window.Header = "Scan Credentials";

            if (window.ShowDialog().GetValueOrDefault())
                StartScanWithDiscoveryResults();
        }

        private void StartScanWithDiscoveryResults()
        {
            _scanCounterManager.Clear();

            var items = DiscoveryResults.Where(_ => _.IsSelected).Select(AddComputer).ToArray();
            Set(() => TotalItemsCount, items.Length);
            ScannedItemsCount = 0;
            _onlineItems = items.Where(item => item.IsOnline).ToArray();
            Computers = new ObservableCollection<IThreatInfoItem>(items);
            Files = new ObservableCollection<IThreatInfoItem>();

            GroupByComputers = true;
            Set(() => ScanWasOnceStarted, true);
            Keyword = null;

            if (_onlineItems.Length > 0)
            {
                ScanKind scanKind = ScanKind.Quick;
                IsQuickScan = true;
                ScanOptions options = new ScanOptions(_tcpPort, _udpPort, scanKind, Login, Password, true);  

                var ipAddressesString = DiscoveryResults.Where(_ => _.IsSelected).Select(_ => _.IPAddress).ToArray();
                _storage.SetValue(KnownKeys.DevicesSourceDescription,
                new DevicesSourceDescription()
                {
                    Type = ETargetsDiscovererType.AutoDiscovery,
                    Description = DevicesSourceDescription.GetNetworkDescription(ipAddressesString)
                });
                _lastModifiesComputerDate = null;
                options = new ScanOptions(_tcpPort, _udpPort, scanKind, Login, Password, true);

             _aptService.BeginScan(DiscoveryResults.Where(_ => _.IsSelected).ToArray(),
                    _storage.GetValue<DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, false), options);

                if (!IsScanInProgress.GetValueOrDefault())
                {
                    _lastModifiesComputerDate = null;
                    IsScanInProgress = true;
                    IsScanCancelled = false;
                    IsScanFailed = false;
                    IsReportEnabled = false;
                    Percentage = 0;
                }
            }
        }

        private void InvokeStopAutoDiscovery(Object arg)
        {
            IsAutoDiscoveryCompleted = true;
            _targetsDiscoveryService.StopTargetsAutoDiscovery();

            CMCStatisticsMessages<IAutoDiscoveryFinishedRequest>.Send(request =>
            {
                request.CountOfPotentiallyScanableDiscoveredComputers = DiscoveryResults.Count;
                request.DiscoveryStatus = DiscoveryStatus.Skipped;
            });
        }

        private void OnDeploymentTargetsAutoResolving(object sender, DeploymentTargetResolvingEventArgs e)
        {
            if (e.Error != null)
            {
                HandleNetworkConnectionError(e.Error);
                CMCStatisticsMessages<IAutoDiscoveryFinishedRequest>.Send(request =>
                {
                    request.CountOfPotentiallyScanableDiscoveredComputers = DiscoveryResults.Count;
                    request.DiscoveryStatus = DiscoveryStatus.Failed;
                });
                return;
            }

            if (e.Target != null)
            {
                if (!e.Target.IsLocalComputer || !DiscoveryResults.Any(_ => _.IsLocalComputer))
                {
                    e.Target.SelectAction = _ => StartScanWithDiscoveryResultsCommand.InvalidateCanExecute();
                    DiscoveryResults.Add(e.Target);
                    _scanCounterManager.AddComputer();
                    if (SelectDiscoveryResults.GetValueOrDefault())
                        Set(() => SelectDiscoveryResults, null);
                    StartScanWithDiscoveryResultsCommand.InvalidateCanExecute();
                }
            }

            Percentage = e.Progress * 100d;

            if (Math.Abs(Percentage - 100) < 0.0001)
            {
                IsAutoDiscoveryCompleted = true;

                CMCStatisticsMessages<IAutoDiscoveryFinishedRequest>.Send(request =>
                {
                    request.CountOfPotentiallyScanableDiscoveredComputers = DiscoveryResults.Count;
                    request.DiscoveryStatus = DiscoveryStatus.Success;
                });
            }
        }

        #endregion

        #region Previous Scans

        private void InvokeOpenPreviousScans(Object arg)
        {
            if (PreviousScansViewModel != null && arg == null)
            {
                PreviousScansViewModel.GroupEnabled = false;
                PreviousScansViewModel.Uri = "/Views/Scan/PreviousScanResults.xaml";
                RaisePropertyChanged(nameof(PreviousScansViewModel));
                return;
            }

            IsBusy = true;

            var op = ThreadHelper.Instance.RunOnThreadDelayed(
                () => _scanOperationsManager.GetPreviousScans(arg as string));
            op.Completed += DispatcherOperationCompleted;
            op.Priority = DispatcherPriority.Normal;
        }

        private void InvokeSearchPreviousScans(Object arg)
        {
            PreviousScansViewModel._isSearchInitiated = PreviousScansViewModel.HasScanKeyword;
            InvokeOpenPreviousScans(PreviousScansViewModel.ScanKeyword ?? string.Empty);
        }

        private void InvokeCleanScanKeyword(Object arg)
        {
            PreviousScansViewModel.ScanKeyword = null;
            if (PreviousScansViewModel._isSearchInitiated)
                InvokeSearchPreviousScans(arg);
        }

        private void DispatcherOperationCompleted(object sender, EventArgs e)
        {
            if (PreviousScansViewModel == null)
            {
                var model = CompositionHelper.Instance.Resolve<MainViewModel>();
                model.Uri = "/Views/Scan/PreviousScanResults.xaml";

                PreviousScansViewModel = model;
                PreviousScansViewModel.ViewLoadedCommand.Execute(null);
                RaisePropertyChanged(nameof(PreviousScansViewModel));
                RaisePropertyChanged(nameof(CurrentScanAvailable));
                RaisePropertyChanged(nameof(IsReportEnabled));
            }
            else
            {
                var operation = sender as DispatcherOperation;

                if (operation != null && operation.Result != null)
                    MergeScanOperationResult(operation.Result as IScanOperationResult);
                else
                    RaisePropertyChanged(nameof(ScanOperations));
            }

            IsBusy = false;
        }

        private void InvokeOpenCurrentScan(Object arg)
        {
            PreviousScansViewModel.ViewUnloadedCommand.Execute(null);
            PreviousScansViewModel = null;
            RaisePropertyChanged(nameof(PreviousScansViewModel));
            RaisePropertyChanged(nameof(CurrentScanAvailable));
            RaisePropertyChanged(nameof(IsReportEnabled));
        }

        private void InvokeOpenPreviousScanDetails(Object arg)
        {
            var operation = arg as ScanOperation;
            if (operation != null && operation.LoadDetailsFunc != null)
            {
                IsBusy = true;

                var op = ThreadHelper.Instance.RunOnThreadDelayed(
                    () => operation.LoadDetailsFunc(operation));
                op.Completed += DispatcherOperationCompleted;
                op.Priority = DispatcherPriority.Normal;
            }
        }

        private void MergeScanOperationResult(IScanOperationResult result)
        {
            var diff = new ScanCounterDiff();
            diff.MalwareDiff = result.ScanOperation.MaliciousCount;
            diff.UnknownDiff = result.ScanOperation.UnknownsCount;
            diff.NotAnalyzedDiff = result.ScanOperation.NotAnalyzedCount;
            diff.InProgressDiff = result.ScanOperation.InAnalysisCount;
            diff.CleanDiff = result.ScanOperation.CleanCount;

            App.Current.Dispatcher.BeginInvoke((Action)delegate
            {
                PreviousScansViewModel._scanCounterManager.Clear();
                PreviousScansViewModel._scanCounterManager.UpdateCounters(diff);

                PreviousScansViewModel.RaisedHasMalwares = PreviousScansViewModel._scanCounterManager.HasMalwares;
                if (!PreviousScansViewModel.RaisedHasMalwares)
                    PreviousScansViewModel.RaisedHasUnknowns =
                        PreviousScansViewModel._scanCounterManager.HasUnknowns ||
                        PreviousScansViewModel._scanCounterManager.HasNotAnalyzed ||
                        PreviousScansViewModel._scanCounterManager.HasInProgress ||
                        PreviousScansViewModel.IsScanFailed ||
                        PreviousScansViewModel.IsScanCancelled;
            });

            PreviousScansViewModel.ScannedItemsCount = result.ScanOperation.ComputersScanned;
            PreviousScansViewModel.TotalItemsCount = result.ScanOperation.TotalComputersScanned;
            PreviousScansViewModel.IsScanCancelled = result.ScanOperation.IsCancelled;
            PreviousScansViewModel.IsScanFailed = result.ScanOperation.IsFailed;
            PreviousScansViewModel.ValkyrieDetailsLink = result.ScanOperation.ValkyrieOperationInfo?.DetailedLink;

            MergeScanComputerResults(result.ComputerResults);

            PreviousScansViewModel.Keyword = null;
            PreviousScansViewModel.Percentage = 100;
            PreviousScansViewModel.GroupByComputers = true;
            PreviousScansViewModel.ValkyrieDetailsLinkAvailable = !string.IsNullOrEmpty(
                result.ScanOperation.ValkyrieOperationInfo?.DetailedLink);
        }

        private void MergeScanComputerResults(IFatScanComputerResult[] results)
        {
            var computers = results.ToList();
            computers.Sort(new ScanComputerResultComparer());

            PreviousScansViewModel.Computers = new ObservableCollection<IThreatInfoItem>();
            PreviousScansViewModel.Files = new ObservableCollection<IThreatInfoItem>();

            foreach (var res in computers)
            {
                var item = new ThreatInfoItem();
                item.IsComputer = true;
                item.IsOnline = res.IsOnline;
                item.IsLocal = res.IsLocal;
                item.Name = res.Name;
                item.IpAddress = res.IpAddress;
                item.TotalFilesCount = res.TotalFilesCount;
                item.UnknownFilesCount = res.UnknownFilesCount;
                item.MalwareFilesCount = res.MalwareFilesCount;
                item.CleanFilesCount = res.CleanFilesCount;
                item.PendingFilesCount = res.InAnalysisFilesCount;
                item.NotAnalyzedFilesCount = res.NotAnalyzedFilesCount;
                item.ValkyrieProgressPercentage = 100;
                item.RemoteScanningPercentage = 100;
                item.ScanStage = res.ScanStage;
                item.StatusMessageSystem = res.StatusMessage;

                foreach (var file in res.BadFiles)
                {
                    var badFile = new BadFileItem();
                    badFile.Name = file.Path;
                    badFile.Size = file.Size;
                    badFile.Sha1 = file.Sha1;
                    badFile.Product = file.Product;
                    badFile.Accessed = file.Accessed;
                    badFile.Copyright = file.Copyright;
                    badFile.Description = file.Description;
                    badFile.Created = file.Created;
                    badFile.Modified = file.Modified;
                    badFile.Cloud = file.Cloud;
                    badFile.Version = file.Version;
                    badFile.Signer = file.Signer;
                    badFile.Attributes = file.Attributes;

                    Func<BadFileItem, IFatFileResult, ThreatInfoItem> createFile = (b, f) =>
                    {
                        var t = new ThreatInfoItem(b);
                        if (f.ValkyrieVerdictValue != null)
                        {
                            t.ValkyrieVerdict = f.ValkyrieVerdictValue.Value;
                            t.OperationStatus = f.OperationStatus;
                            t.MalwareProbabilityValue = f.MalwareProbabilityValue;
                            t.SkipReason = f.SkipReason;
                            t.IsWhiteListed = f.IsWhiteListed;
                        }
                        return t;
                    };

                    var ti = createFile(badFile, file);
                    if (ti.ShouldCheckOnValkyrie && ti.ValkyrieVerdict == APTAT.Core.Valkyrie.ValkyrieVerdict.PendingFls)
                    {
                        item.ScanStage = ScanStage.OperationCanceled;
                        item.StatusMessageSystem = "Scan operation was canceled.";
                    }
                    item.AddFile(ti);

                    ti = createFile(badFile, file);
                    var exist = PreviousScansViewModel.Files.FirstOrDefault(_ =>
                        String.Equals(_.BadFile.Sha1, ti.BadFile.Sha1, StringComparison.OrdinalIgnoreCase));

                    var comp = new ThreatInfoItem
                    {
                        IpAddress = res.IpAddress,
                        ComputerGuid = res.TargetUid,
                        Name = res.Name,
                        ShortName = ti.Name,
                        IsComputer = true
                    };

                    if (exist != null)
                    {
                        comp.Parent = exist;
                        exist.Computers.Add(comp);
                    }
                    else
                    {
                        comp.Parent = ti;
                        ti.Computers = new ObservableCollection<IThreatInfoItem> { comp };
                        PreviousScansViewModel.Files.Add(ti);
                    }
                }

                PreviousScansViewModel.Computers.Add(item);
            }
        }

        #endregion

        /*private void InvokeRestartComputerScan(Object arg, bool showDialog = false)
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
                    RestartComputerScan(IsQuickScan);
            }
            else
                RestartComputerScan(IsQuickScan);
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
        
        private void RestartComputerScan(bool isQuickScan)
        {
        }*/

        private void InvokeStartScan(Object arg)
        {
            IsQuickScan = (bool)arg;

            _scanCounterManager.Clear();

            Set(() => FilterReset, null);

            Files = new ObservableCollection<IThreatInfoItem>();
            Computers = new ObservableCollection<IThreatInfoItem>();


            RaisedHasUnknowns = false;
            RaisedHasMalwares = false;

            ValkyrieDetailsLinkAvailable = false;

            _onlineItems = new IThreatInfoItem[0];

            SelectedItem = null;
            Percentage = 0;
            IsScanCancelled = false;
            IsScanFailed = false;
            IsReportEnabled = false;

            GroupByComputers = true;

            var value = _storage.GetValue<ICollection<Object>>(KnownKeys.CheckedDirectoryItems, true);
            if (value != null)
            {
                var targets = value.OfType<DeploymentItem>().ToArray();
                if (targets.Length > 0)
                {
                    IsBusy = true;
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
                targets = (Guid[])arg;

            _aptService.StopScan(targets);
        }

        private void OnScanCompleted(Object sender, ErrorEventArgs e)
        {
            if (e.Error != null)
            {
                _isFailed = true;
                ErrorOccurred?.Invoke(this, new DomainEventArgs(null, e.Error));
            }
        }

        private void OnScanCompleted()
        {
            if (IsScanInProgress.GetValueOrDefault())
            {
                IsScanInProgress = false;
                _storage.SetValue(KnownKeys.OperationGuid, _operationGuid);

                if (ScannedItemsCount == 0)
                {
                    IsScanFailed = true;
                    RaisePropertyChanged(nameof(Header));
                    RaisePropertyChanged(nameof(ComputersHeader));
                }

                IsReportEnabled = (!IsScanCancelled || Computers.Any(_ => _.ScanStage == ScanStage.OperationCompleted)) && !IsScanFailed;

                ValkyrieDetailsLinkAvailable = _aptService.GetValkyrieDetailsLink() != null;

                CancelInProgress = false;

                if (!_previousScansAvailable)
                {
                    _previousScansAvailable = _scanOperationsManager.HasPreviousScans();
                    OpenPreviousScansCommand.InvalidateCanExecute();
                }

                CMCStatisticsMessages<IScanFinishedRequest>.Send(request =>
                {
                    request.CountOfSuccessScans = ScannedItemsCount;
                    request.CountOfUnknownFiles = _scanCounterManager.UnknownsCount;
                    request.CountOfMaliciousFiles = _scanCounterManager.MalwaresCount;
                    request.CountOfFailedScans = _computers.Count(c => c.ScanStatusAsEnum == ScanStatusEnum.Failed);
                    if (IsScanCancelled)
                        request.ScanStatus = APTAT.Core.CMC.ScanStatus.Interrupted;
                    else if (IsScanFailed)
                        request.ScanStatus = APTAT.Core.CMC.ScanStatus.Failed;
                    else
                        request.ScanStatus = APTAT.Core.CMC.ScanStatus.Success;
                });
                ScanProcessCompleted?.Invoke(this, new DomainEventArgs());
            }
        }

        private IThreatInfoItem AddComputer(IComputer target)
        {
            var item = new ThreatInfoItem(target);
            item.ScanStatusUpdated = () => Set(() => FilterReset, !FilterReset.GetValueOrDefault());

            return item;
        }

        private void OnDirectoryItemsResolved(Object sender, DirectoryItemsResolvedEventArgs e)
        {
            IsBusy = false;
            IsScanInProgress = true;
            if (e.Exception != null)
            {
                HandleNetworkConnectionError(e.Exception);
                return;
            }
            if (e.Items == null)
            {
                return;
            }

            var items = e.Items.Select(AddComputer).ToArray();


            var local = items.FirstOrDefault(item => item.IsLocal);
            if (local != null)
                _onlineItems = (new[] { local }).Union(items.Where(item => item.IsOnline && !item.IsLocal)).ToArray();
            else
                _onlineItems = items.Where(item => item.IsOnline).ToArray();

            items = _onlineItems.Union(items.Where(item => !item.IsOnline)).ToArray();
            Computers = new ObservableCollection<IThreatInfoItem>(items);

            Set(() => TotalItemsCount, items.Length);
            ScannedItemsCount = 0;

            foreach (var item in Computers)
            {
                if (!item.IsOnline)
                {
                    item.StatusMessageSystem = "This computer is not accessible.";
                }
            }

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
            _IsCustomScan = false;
            if (_storage.ContainsKey(KnownKeys.CustomScanOptions))
            {
                var customScanOptions = _storage.GetValue<CustomScanOptions>(KnownKeys.CustomScanOptions, true);
                _IsCustomScan = true;
                scanKind = ScanKind.Custom;
                options = new ScanOptions(_tcpPort, _udpPort, customScanOptions.FolderItems.Select(f => f.FullName).ToArray(),
                    customScanOptions.ScanCriticalAreas, Login, Password);
            }
            else
            {
                options = new ScanOptions(_tcpPort, _udpPort, scanKind, Login, Password);
            }

            _lastModifiesComputerDate = null;
            _aptService.BeginScan(items.Select(item => item.DeploymentTarget).ToArray(), _storage.GetValue<DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, false), options);

            if (_onlineItems.Length == 0)
            {
                IsScanInProgress = false;
            }
        }

        private void OnScanStarted(Object sender, ScanStartedEventArgs e)
        {
            _operationGuid = e.OperationUid;
            _lastModifiesComputerDate = null;
            _scanCounterManager.Clear();
            _scanTimer.Start();

            CMCStatisticsMessages<IScanStartedRequest>.Send(request =>
            {
                request.TotalCountOfDiscoveredComputers = TotalItemsCount;
                request.CountOfScannedComputers = _onlineItems?.Length ?? 0;
                var targetsDiscovererType = _storage.GetValue<DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, false);
                ETargetsDiscovererTypeRequest targetDiscoverTypeMapped;
                
                switch (targetsDiscovererType.Type)
                {
                    case ETargetsDiscovererType.NetworkAddresses:
                        targetDiscoverTypeMapped = ETargetsDiscovererTypeRequest.NetworkAddresses;
                        break;

                    case ETargetsDiscovererType.Workgroup:
                        targetDiscoverTypeMapped = ETargetsDiscovererTypeRequest.Workgroup;
                        break;

                    case ETargetsDiscovererType.ActiveDirectory:
                        targetDiscoverTypeMapped = ETargetsDiscovererTypeRequest.ActiveDirectory;
                        break;

                    case ETargetsDiscovererType.LocalComputer:
                        targetDiscoverTypeMapped = ETargetsDiscovererTypeRequest.LocalComputer;
                        break;

                    case ETargetsDiscovererType.AutoDiscovery:
                        targetDiscoverTypeMapped = ETargetsDiscovererTypeRequest.AutoDiscovery;
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException(nameof(targetsDiscovererType));
                        
                }
                request.ScanTarget = targetDiscoverTypeMapped;
                ScanType scanType = IsQuickScan ? ScanType.Quick : ScanType.Full;
                if (_IsCustomScan)
                    scanType = ScanType.Custom;

                request.ScanType = scanType;                
                request.ScanTriggerType = targetsDiscovererType.Type == ETargetsDiscovererType.AutoDiscovery ? ScanTriggerType.ScanNow : ScanTriggerType.CustomScan;            
            
            });
        }

        private void OnCancelScanStarted(object sender, CancelEventArgs e)
        {
            if (_scanTimer.ScanActive)
            {
                CancelInProgress = true;
                IsScanCancelled = true;
                IsValkyrieError = e.IsProxyError;
            }
        }

        private void IterateThroughScan()
        {
            if (!_scanTimer.ScanActive)
                return;

            _scanTimer.Stop();

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
                IScanResultItem scanProgress = null;
                computers.TryGetValue(infoItem.ComputerGuid, out scanProgress);
                if (infoItem.ScanStage != ScanStage.OperationFailed &&
                        infoItem.ScanStage != ScanStage.OperationCompleted &&
                        infoItem.ScanStage != ScanStage.OperationCanceled)
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
                                    var exist = Files.FirstOrDefault(_ => String.Equals(_.BadFile.Sha1, item.BadFile.Sha1, StringComparison.OrdinalIgnoreCase));
                                    var comp = new ThreatInfoItem
                                    {
                                        IpAddress = infoItem.IpAddress,
                                        ComputerGuid = infoItem.ComputerGuid,
                                        Name = infoItem.Name,
                                        ShortName = item.Name,
                                        IsComputer = true
                                    };
                                    if (exist != null)
                                    {
                                        comp.Parent = exist;
                                        exist.Computers.Add(comp);
                                    }
                                    else
                                    {
                                        comp.Parent = item;
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
                    if (infoItem.ScanStage == ScanStage.ScanInProgress)
                        infoItem.ScanStage = ScanStage.OperationCancelling;
                    else
                        infoItem.TrySetCancelledStatus(IsValkyrieError);


                    counterDiff.MalwareDiff += (-1) * infoItem.MalwareFilesCount;
                    counterDiff.CleanDiff += (-1) * infoItem.CleanFilesCount;
                    counterDiff.InProgressDiff += (-1) * infoItem.PendingFilesCount;
                    counterDiff.UnknownDiff += (-1) * infoItem.UnknownFilesCount;
                    counterDiff.NotAnalyzedDiff += (-1) * infoItem.NotAnalyzedFilesCount;

                    _scanCounterManager.UpdateCounters(counterDiff);
                    RaisedHasMalwares = _scanCounterManager.HasMalwares;

                    counterDiff = new ScanCounterDiff();       
                }
                if (_isFailed && localPercentage < 100)
                {
                    infoItem.TrySetFailedStatus();
                    localPercentage = infoItem.TotalProgressPercentage;
                }
                if (localPercentage == 100 && infoItem.ScanStage == ScanStage.OperationCompleted)
                    scanned++;
                percentages += localPercentage;
            }
            if (Math.Abs(Percentage - 100) > 0.0001 && _onlineItems.Length > 0)
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
            if (IsScanInProgress == false && (counterDiff.MalwareDiff > 0 || counterDiff.UnknownDiff > 0))
            {
                CMCStatisticsMessages<IScanUpdatedRequest>.Send(request =>
                {
                    request.CountOfNewMaliciousFilesAfterTheScanWasFinished = counterDiff.MalwareDiff;
                    request.CountOfNewUnknownFilesAfterTheScanWasFinished = counterDiff.UnknownDiff;
                });
            }

            ScannedItemsCount = scanned;

            if (!RaisedHasMalwares)
            {
                RaisedHasMalwares = _scanCounterManager.HasMalwares;

                if (!RaisedHasMalwares)
                    RaisedHasUnknowns =
                        _scanCounterManager.HasUnknowns ||
                        _scanCounterManager.HasNotAnalyzed ||
                        _scanCounterManager.HasInProgress ||
                        IsScanFailed ||
                        IsScanCancelled;
                else
                    RaisedHasUnknowns = false;
            }

            if (Math.Abs(Percentage - 100) < 0.0001)
            {
                OnScanCompleted();
            }

            _scanTimer.Start();
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
                            return FilterFileByOrders(o) && o.Parent.NameAndIpAddress.ContainsString(Keyword);
                    }
                    else if (GroupByFiles)
                    {
                        var o = item as IThreatInfoItem;
                        if (!o.IsComputer)
                            return FilterFileByOrders(o) && o.NameAndIpAddress.ContainsString(Keyword);
                        else
                            return FilterFileByOrders(o.Parent) && o.Parent.NameAndIpAddress.ContainsString(Keyword);
                    }
                    else
                    {
                        var o = item as DeploymentTarget;
                        return o.DnsName.ContainsString(Keyword) || o.Name.ContainsString(Keyword) || o.IPAddress.ContainsString(Keyword);
                    }
                };
            }
        }

        private bool _IsCustomScan;

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

        public void OnPortChanged(int udpPort, int tcpPort)
        {
            _aptService.UpdateFlsPorts(udpPort, tcpPort);
        }

        private async void InvokeSubmitEmail(Object arg)
        {
            _submitingEmail = true;
            SubmitEmailCommand.InvalidateCanExecute();
            RaisePropertyChanged(nameof(SubmitHeader));
            RaisePropertyChanged(nameof(NotSubmitingEmail));

            var result = await _aptService.SubmitEmailAsync(Email.Split(new[] { ',', ';' }));

            if (result.IsSucceeded)
            {
                var editAvailable = _hasEmail;
                CMCStatisticsMessages<IEmailActionRequest>.Send(request =>
                {
                    request.EmailAction = _hasEmail ? EmailAction.Edit : EmailAction.Save;
                });
                Set(() => HideSubmitEmail, true);
                _hasEmail = true;
            }
            else
                ErrorOccurred?.Invoke(this, new DomainEventArgs(null, new InvalidOperationException(result.ErrorMessage)));

            _submitingEmail = false;
            SubmitEmailCommand.InvalidateCanExecute();
            RaisePropertyChanged(nameof(SubmitHeader));
            RaisePropertyChanged(nameof(NotSubmitingEmail));
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
                    await _statisticsService.OnAppClosing();
                    action?.Invoke();
                }
                else
                    InvokeStopScan(null);
            }
            else
            {
                IsBusy = true;

                await _aptService.OnAppClosing();
                await _statisticsService.OnAppClosing();
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
                var domainException = ex as DirectoryQueryException;
                if (domainException != null && domainException.Error == DirectoryError.DefaultDomainNotFound)
                {
                    ;
                }
                else
                {
                    TryActionException(ex);
                    ErrorOccurred?.Invoke(this, new DomainEventArgs(null, ex));
                }
            }
            return taskResult;
        }

        private void TryActionException(Exception ex)
        {
            LogHelper.Trace(TraceEventType.Error, $"TryAction error: {ex.GetType()} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        private void HandleNetworkConnectionError(Exception exception)
        {
            if (exception != null)
            {
                var msg =
                    @"The setting 'SupportedDnsNames' in the application configuration file should contain at least one external (not loopback) address.";
                var agrEx = exception as AggregateException;

                if (exception.Message == msg || (agrEx != null && agrEx.InnerExceptions.Any(x => x.Message == msg)))
                {
                    InvokeStopAutoDiscovery(null);

                    ErrorOccurred?.Invoke(this,
                        new DomainEventArgs { Exception = new InvalidOperationException("No network connection") });
                }
                else
                {
                    ErrorOccurred?.Invoke(this, new DomainEventArgs { Exception = exception });
                }

                IsScanInProgress = false;
            }
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

        private bool IsValidEmail(string email)
        {
            if (String.IsNullOrEmpty(email))
                return false;

            return Regex.IsMatch(email,
                      @"^(([^<>()[\]\\.,;:\s@\""]+"
                    + @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
                    + @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
                    + @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
                    + @"[a-zA-Z]{2,}))$",
                      RegexOptions.IgnoreCase);
        }
    }
}