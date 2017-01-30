using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using Telerik.Windows.Controls;
using APTAT.Core;
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
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace FAT.Application.Models
{
    public sealed class MainViewViewModel : ViewModel, IMainViewModel
    {
        public event EventHandler<Services.ScanCompletedEventArgs> ScanProcessCompleted;
        public event EventHandler<DomainEventArgs> DomainTargetsResolveFailure;
        public event EventHandler ValkyrieAnalyzeStarted;
        public event EventHandler<AnalyzeCompletedEventArgs<ValkyrieFile>> ValkyrieAnalyzeCompleted;



        private string _camLicenseId;
        private string _camUserLogin;
        private string _camUserPassword;
        private string _tcpPort = "80";
        private string _udpPort = "53";

        private readonly IAptService _aptService;

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
        private Int32 _pendingFilesCount;

        private ThreatInfoItem[] _onlineItems;
        private ThreatInfoItem _selectedItem;
        private object _selectedValkyrieItem;

        private ObservableCollection<ThreatInfoItem> _items;
        private ObservableCollection<ThreatInfoItem> _computers;
        private ObservableCollection<ValkyrieFile> _itemsValkyrie;
        private ObservableCollection<ValkyrieComputer> _computersValkyrie;
        private Object _countersLock = new Object();

        private Collection<ThreatInfoItem> _cache;
        private Collection<ThreatInfoItem> _cacheComputers;
        private Collection<ValkyrieFile> _cacheValkyrie;
        private Collection<ValkyrieComputer> _cacheValkyrieComputers;

        private DispatcherTimer _scanIteratorTimer;
        private Dictionary<Guid, ScanProgressItem> _scanIterator;

        public string Uri { get; set; } = "/Views/EULAView.xaml";

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

        private async void OnLoaded()
        {
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
                RaisePropertyChanged(nameof(SearchAvailable));

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

        public bool? IsValkyrieScanInProgress
        {
            get { return _isValkyrieScanInProgress; }
            set
            {
                _isValkyrieScanInProgress = value;
                RaisePropertyChanged("IsValkyrieScanInProgress");
                RaisePropertyChanged(nameof(IsAnyScanInProgress));
                RaisePropertyChanged(nameof(SearchAvailable));

                App.Current.Dispatcher.BeginInvoke((Action)delegate
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
                var commonHeaderName = "Forensic Analysis Tool";

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
                if (IsValkyrieScanInProgress == true && ValkyrieProgressInfo != null)
                {
                    header = String.Format("{0:0.00}% Valkyrie analysis...", Percentage).Replace(".00", string.Empty);
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
            get { return SelectedValkyrieItem != null && SelectedValkyrieItem is ValkyrieFile; }
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
                RaisePropertyChanged("SelectedItem");
                RaisePropertyChanged("IsDetailsEnabled");

                SelectCommand.InvalidateCanExecute();

                if (_selectedItem == null)
                {
                    _storage.SetValue(KnownKeys.UnknownFile, null);
                }
                else
                {
                    _storage.SetValue(KnownKeys.UnknownFile, Tuple.Create<BadFileItem, ValkyrieFile>(_selectedItem.BadFile, null));
                }

            }
        }

        public object SelectedValkyrieItem
        {
            get { return _selectedValkyrieItem; }
            set
            {
                _selectedValkyrieItem = value == null ? null : (value is ValkyrieFile ? value : null);
                RaisePropertyChanged("SelectedValkyrieItem");
                SelectValkyrieCommand.InvalidateCanExecute();

                if (_selectedValkyrieItem == null)
                {
                    _storage.SetValue(KnownKeys.UnknownFile, null);
                }
                else
                {
                    _storage.SetValue(KnownKeys.UnknownFile, Tuple.Create<BadFileItem, ValkyrieFile>(null, (ValkyrieFile)_selectedValkyrieItem));
                }

            }
        }

        public Boolean IsItemsEmpty
        {
            get { return _cacheComputers == null || _cacheComputers.Count == 0; }
        }

        public bool SearchAvailable
        {
            get { return !IsItemsEmpty && !IsAnyScanInProgress; }
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

        public ObservableCollection<ValkyrieFile> ItemsValkyrie
        {
            get { return _itemsValkyrie; }
            set
            {
                _itemsValkyrie = value;
                RaisePropertyChanged(nameof(ItemsValkyrie));
            }
        }

        public ObservableCollection<ValkyrieComputer> ComputersValkyrie
        {
            get { return _computersValkyrie; }
            set
            {
                _computersValkyrie = value;
                RaisePropertyChanged(nameof(ComputersValkyrie));
            }
        }

        public ObservableCollection<ThreatInfoItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged(nameof(Items));
            }
        }

        public ObservableCollection<ThreatInfoItem> Computers
        {
            get { return _computers; }
            set
            {
                _computers = value;
                RaisePropertyChanged(nameof(Computers));
                RaisePropertyChanged(nameof(IsItemsEmpty));
                RaisePropertyChanged(nameof(SearchAvailable));
            }
        }

        public bool ValkyrieSendingFiles { get; set; }
        public bool SelectScan { get; set; }
        public bool SelectValkyrieScan { get; set; }
        public bool IsQuickScan { get; set; }
        public bool ValkyrieEnabled { get; set; }

        private bool _groupingEnabled;
        public bool GroupingEnabled
        {
            get { return _groupingEnabled; }
            set
            {
                _groupingEnabled = value;
                RaisePropertyChanged("GroupingEnabled");
            }
        }

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
        public DelegateCommand ChangeScreenCommand { get; private set; }
        public DelegateCommand StartScanCommand { get; private set; }
        public DelegateCommand StopScanCommand { get; private set; }

        public DelegateCommand SelectCommand { get; private set; }
        public DelegateCommand SelectValkyrieCommand { get; private set; }


        public MainViewViewModel(ITargetsDiscoveryService targetsDiscoveryService,
            IAptService aptService, IStorageService storage)
        {
            _aptService = aptService;
            _targetsDiscoveryService = targetsDiscoveryService;

            _storage = storage;

            ChangeScreenCommand = new DelegateCommand(o =>
            {
                Set(() => Uri, "/Views/MainView.xaml");
                OnLoaded();
            });

            StartScanCommand = new DelegateCommand(o =>
                AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.StartScan, InvokeStartScan),
                o => !IsAnyScanInProgress && IsScanInProgress.HasValue);
            StopScanCommand = new DelegateCommand(o =>
                AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.StopScan, InvokeStopScan),
                o => StopScanPredicate);
            SelectCommand = new DelegateCommand(o => { }, o => IsDetailsEnabled);
            SelectValkyrieCommand = new DelegateCommand(o => { }, o => IsValkyrieDetailsEnabled);

            ValkyrieProgressInfo = ProgressInfo<ValkyrieFile>.Empty;
        }

        public bool ActiveDirectoryStep { get; set; }

        private void InvokeStartScan(Object arg)
        {
            IsQuickScan = (bool)arg;

            Items = new ObservableCollection<ThreatInfoItem>();
            Computers = new ObservableCollection<ThreatInfoItem>();
            ItemsValkyrie = new ObservableCollection<ValkyrieFile>();
            ComputersValkyrie = new ObservableCollection<ValkyrieComputer>();

            _cache = new Collection<ThreatInfoItem>();
            _cacheComputers = new Collection<ThreatInfoItem>();
            _cacheValkyrie = new Collection<ValkyrieFile>();
            _cacheValkyrieComputers = new Collection<ValkyrieComputer>();

            _onlineItems = new ThreatInfoItem[0];

            SelectedItem = null;
            UnknownFilesCount = 0;
            MalwareFilesCount = 0;
            PendingFilesCount = 0;
            ScannedItemsCount = 0;
            LookupInProgressCount = 0;
            Percentage = 0;
            IsReportEnabled = false;
            IsAnalyseWithValkyrieEnabled = false;
            IsValkyrieReportEnabled = false;

            Set(() => SelectScan, true);
            Set(() => ValkyrieEnabled, false);
            GroupingEnabled = false;
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
                        ActiveDirectoryStep ? model.ActiveDirectoryLogin : null,
                        ActiveDirectoryStep ? model.ActiveDirectoryPassword : null, this);

                }
            }
        }

        private void InvokeStopScan(Object arg)
        {
            if (!IsAnyScanInProgress)
                return;

            CancelInProgress = true;

            _aptService.StopScan(new Guid[0]);

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
            _aptService.ValkyrieAnalyzeProgressChanged += OnValkyrieAnalyzeProgressChanged;
            _aptService.ValkyrieAnalyzeCompleted += OnValkyrieAnalyzeCompleted;
            _aptService.ValkyrieAnalyzeStarted += OnValkyrieAnalyzeStarted;
            _aptService.FlsAnalyzeProgressChanged += OnFlsAnalyzeProgressChanged;
        }

        private void OnFlsAnalyzeProgressChanged(object sender, FlsRequestEventArgs e)
        {
            if (e.Exception != null) return;

            var computersToUpdate = new Dictionary<Guid, ThreatInfoItem>();
            var needUpdateStatus = false;
            foreach (var result in e.Results)
            {
                ValkyrieVerdict newVerdict = ValkyrieVerdict.Pending;
                if (result.Cloud == CloudVerdict.FirstSeen || result.Cloud == CloudVerdict.Unknown)
                    newVerdict = ValkyrieVerdict.Pending;
                if (result.Cloud == CloudVerdict.Malware)
                    newVerdict = ValkyrieVerdict.Malware;
                if (result.Cloud == CloudVerdict.Safe)
                    newVerdict = ValkyrieVerdict.Clean;

                var items = _cache.Where(i => String.Equals(i.BadFile.Sha1, result.Sha1, StringComparison.OrdinalIgnoreCase)).ToArray();
                foreach (var threatInfoItem in items)
                {
                    threatInfoItem.UpdateFileVerdict(result.Cloud);
                }


                foreach (ThreatInfoItem computer in _cacheComputers)
                {
                    var files = computer.Items.Where(i =>
                            String.Equals(i.BadFile.Sha1, result.Sha1, StringComparison.OrdinalIgnoreCase)).ToArray();
                    if (files.Any())
                    {
                        if (!computersToUpdate.ContainsKey(computer.ComputerGuid))
                            computersToUpdate.Add(computer.ComputerGuid, computer);
                        var comp = _cacheValkyrieComputers.FirstOrDefault(_ => _.TargetUid == computer.ComputerGuid);
                        comp?.UpdateFileIfPredicate(result.Sha1, new ValkyrieAnalysisResult(result.Sha1, newVerdict), 
                            verdict => verdict == ValkyrieVerdict.PendingFls);
                        foreach (var f in files)
                            f.UpdateFileVerdict(result.Cloud);
                    }
                }
                if (_cacheValkyrie.Any())
                {
                    var file = _cacheValkyrie.FirstOrDefault(_ => String.Equals(_.Sha1, result.Sha1, StringComparison.OrdinalIgnoreCase));
                    if (file != null)
                    {
                        if (!file.PendingFls)
                            continue;
                        
                        file.UpdateValkyrieResult(new ValkyrieAnalysisResult(result.Sha1, newVerdict));
                        Interlocked.Add(ref _lookupInProgressCount, -file.Computers.Count);
                        RaisePropertyChanged(nameof(LookupInProgressCount));
                        RaisePropertyChanged(nameof(IsErrorsFound));

                        if (file.IsUnknown)
                        {
                            Interlocked.Add(ref _unknownFilesCount, file.Computers.Count);
                            RaisePropertyChanged(nameof(UnknownFilesCount));
                        }
                        else if (file.IsMalware)
                        {
                            Interlocked.Add(ref _malwareFilesCount, file.Computers.Count);
                            RaisePropertyChanged(nameof(MalwareFilesCount));
                        }
                        else if (file.Pending)
                        {
                            Interlocked.Add(ref _pendingFilesCount, file.Computers.Count);
                            RaisePropertyChanged(nameof(PendingFilesCount));
                            RaisePropertyChanged(nameof(IsPendingFound));
                        }
                    }
                }
                else
                   needUpdateStatus = true;
            }
            foreach (var computer in computersToUpdate.Values)
            {
                computer.UpdateStatus();
            }
            if (needUpdateStatus)
                UpdateStatus();
        }
        
        void OnValkyrieAnalyzeStarted(object sender, EventArgs e)
        {
            OnBeforeBeginValkyrieAnalysis();
            ValkyrieAnalyzeStarted?.Invoke(this, e);
        }

        public string ProxyLogin { get; set; }
        public string ProxyPassword { get; set; }

        public void OnPortChanged(int udpPort, int tcpPort)
        {
            _aptService.UpdateFlsPorts(udpPort, tcpPort);
        }


        void CopyMalwaresFromScanResults()
        {
            var col = new Collection<ValkyrieFile>();
            foreach (var file in _cache)
            {
                if (!file.IsMalware && !file.Failed)
                    continue;
                var valkyrieFile = MapToValkyrieFile(file);
                col.Add(valkyrieFile);
            }

            MergeResults(col);
        }

        private static ValkyrieFile MapToValkyrieFile(ThreatInfoItem file)
        {
            var valkyrieFile = new ValkyrieFile
            {
                FlsVerdict = file.BadFile.Cloud,
                Size = file.Size,
                SizeSorting = file.SizeSorting,
                Attributes = file.BadFile.Attributes,
                Created = file.BadFile.Created,
                Modified = file.BadFile.Modified,
                Accessed = file.BadFile.Accessed,
                Sha1 = file.BadFile.Sha1.Trim().ToLower(),
                Signer = file.BadFile.Signer,
                Description = file.BadFile.Description,
                Version = file.BadFile.Version,
                Copyright = file.BadFile.Copyright,
                Product = file.BadFile.Product
            };
            valkyrieFile.UpdateValkyrieResult(
                new ValkyrieAnalysisResult(file.BadFile.Sha1, file.IsMalware
                        ? ValkyrieVerdict.Malware
                        : ValkyrieVerdict.PendingFls));
            foreach (var comp in file.Computers)
            {
                var computer = new ValkyrieComputer
                {
                    Name = comp.Name,
                    IP = comp.IpAddress,
                    FilePath = comp.ShortName,
                    TargetUid = comp.ComputerGuid
                };
                valkyrieFile.Path = computer.FilePath;
                valkyrieFile.Computers.Add(computer);
            }
            return valkyrieFile;
        }

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
                        window.ShowCredentials = true;
                        window.Header = "Proxy Credentials";

                        bool retry = window.ShowDialog().GetValueOrDefault();

                        if (retry)
                        {
                            NetworkCredential cred = new NetworkCredential(ProxyLogin, ProxyPassword, null); //no domain?
                            BeginValkyrieAnalysis(cred);
                            return;
                        }
                    }
                }
                Set(() => ValkyrieEnabled, false);
                Set(() => SelectScan, false);
                Set(() => SelectScan, true);
            }
            else
            {
                if (_isScanInProgress == false)
                {
                    IsValkyrieReportEnabled = true;
                    MergeResults(e.Result);
                    CopyMalwaresFromScanResults();
                }
            }
            

            if (e.Cancelled)
                foreach (var comp in _cacheValkyrieComputers)
                    comp.Canceling = true;

            CancelInProgress = false;

            Set(() => ValkyrieSendingFiles, false);
            IsValkyrieScanInProgress = false;
            IsAnalyseWithValkyrieEnabled = false;
            
            IsReportEnabled = true;

            IsValkyrieAnalysCompleted = true;

            RaisePropertyChanged("Header");
            RaisePropertyChanged("ValkyrieDetailsLink");

            ValkyrieAnalyzeCompleted?.Invoke(this, e);
        }

        private void OnValkyrieAnalyzeProgressChanged(object sender, ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieFile> e)
        {
            var info = e.ProgressInfo;

            if (info != null && e.IsUpdate)
            {
                UpdateResults(info.Results);
                return;
            }

            Set(() => ValkyrieSendingFiles, false);

            if (info != null && !CancelInProgress)
            {
                ValkyrieProgressInfo = info;
                Percentage = info.Percentage;
                RaisePropertyChanged("Header");
                MergeResults(info.Results);
            }
        }

        private void UpdateResults(ValkyrieFile[] results)
        {
            foreach (var result in results)
            {
                var file = _cacheValkyrie.FirstOrDefault(_ => _.Equals(result));
                if (file != null)
                {
                    bool wasPending = false;
                    bool wasUnknown = false;
                    if (file.Pending)
                        wasPending = true;
                    if (file.IsUnknown)
                        wasUnknown = true;
                    var verdictChanged = file.UpdateValkyrieResult(result);

                    if (verdictChanged && wasUnknown)
                        Interlocked.Add(ref _unknownFilesCount, -file.Computers.Count);

                    if (verdictChanged && wasPending && !file.Pending)
                        Interlocked.Add(ref _pendingFilesCount, -file.Computers.Count);

                    if (file.IsUnknown)
                        Interlocked.Add(ref _unknownFilesCount, file.Computers.Count);

                    if (file.IsMalware)
                        Interlocked.Add(ref _malwareFilesCount, file.Computers.Count);

                    RaisePropertyChanged(nameof(PendingFilesCount));
                    RaisePropertyChanged(nameof(IsPendingFound));
                    RaisePropertyChanged(nameof(UnknownFilesCount));
                    RaisePropertyChanged(nameof(MalwareFilesCount));

                    foreach (var computer in file.Computers)
                    {
                        var comp = _cacheValkyrieComputers.FirstOrDefault(_ => _.TargetUid == computer.TargetUid);
                        if (comp != null)
                            comp.UpdateFile(result);
                    }
                }
            }
        }

        private void MergeResults(IEnumerable<ValkyrieFile> results)
        {
            if (results == null)
                return;

            if (!_cacheValkyrie.Any())
            {
                UnknownFilesCount = 0;
                MalwareFilesCount = 0;
            }

            var computersDict = _cacheComputers.ToDictionary(_ => _.ComputerGuid);

            foreach (var result in results)
            {
                var file = _cacheValkyrie.FirstOrDefault(_ => _.Equals(result));
                if (file != null)
                {
                    int dummy = 0;
                    if (file.IsUnknown)
                        file.MergeComputers(result.Computers, ref _unknownFilesCount);
                    else if (file.IsMalware)
                        file.MergeComputers(result.Computers, ref _malwareFilesCount);
                    else
                        file.MergeComputers(result.Computers, ref dummy);

                }
                else
                {
                    ItemsValkyrie.Add(result);
                    _cacheValkyrie.Add(result);
                    if (result.IsUnknown)
                        Interlocked.Add(ref _unknownFilesCount, result.Computers.Count);
                    else if (result.IsMalware)
                        Interlocked.Add(ref _malwareFilesCount, result.Computers.Count);
                    else if (result.Pending)
                        Interlocked.Add(ref _pendingFilesCount, result.Computers.Count);
                }

                RaisePropertyChanged(nameof(PendingFilesCount));
                RaisePropertyChanged(nameof(IsPendingFound));
                RaisePropertyChanged(nameof(UnknownFilesCount));
                RaisePropertyChanged(nameof(MalwareFilesCount));

                foreach (var computer in result.Computers)
                {
                    ThreatInfoItem c;
                    if (!computersDict.TryGetValue(computer.TargetUid, out c)) continue;

                    var comp = _cacheValkyrieComputers.FirstOrDefault(_ => _.TargetUid == computer.TargetUid);
                    if (comp != null)
                        comp.MergeFile(result, computer.FilePath);
                    else
                    {
                        comp = new ValkyrieComputer(computer)
                        {
                            TotalDistinctCount = c.DistinctUnknownFilesCount,
                            TotalCount = c.UnknownFilesCount
                        };
                        comp.MergeFile(result, computer.FilePath);
                        ComputersValkyrie.Add(comp);
                        _cacheValkyrieComputers.Add(comp);
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
            _aptService.ComputerScanCompleted -= OnComputerScanCompleted;
            _aptService.ScanCompleted -= OnScanCompleted;
            _aptService.ScanProgressChanged -= OnScanProgressChanged;
            _aptService.ValkyrieAnalyzeProgressChanged -= OnValkyrieAnalyzeProgressChanged;
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
            _cacheComputers = new ObservableCollection<ThreatInfoItem>(items);
            Computers = new ObservableCollection<ThreatInfoItem>(_cacheComputers);


            foreach (var item in _cacheComputers)
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
            //IsReportEnabled = ScannedItemsCount > 0;
            _storage.SetValue(KnownKeys.OperationGuid, _operationGuid);
            UpdateStatus();
            RaisePropertyChanged(nameof(Header));
        }

        public void OnBeforeBeginValkyrieAnalysis()
        {
            ValkyrieProgressInfo = ProgressInfo<ValkyrieFile>.CreateFromTotal(DistinctFilesCount);
            IsAnalyseWithValkyrieEnabled = false;
            IsValkyrieScanInProgress = true;
            IsValkyrieAnalysCompleted = false;

            ItemsValkyrie = new ObservableCollection<ValkyrieFile>();
            ComputersValkyrie = new ObservableCollection<ValkyrieComputer>();
            _cacheValkyrie = new Collection<ValkyrieFile>();
            _cacheValkyrieComputers = new Collection<ValkyrieComputer>();

            Keyword = string.Empty;


            Set(() => ValkyrieEnabled, true);
            Set(() => SelectValkyrieScan, false);
            Set(() => SelectValkyrieScan, true);
            Set(() => ValkyrieSendingFiles, true);
        }

        public bool BeginValkyrieAnalysis(NetworkCredential networkCredential = null)
        {
            var model = CompositionHelper.Instance.Resolve<ValkyrieRegistrationModel>();
            //if (model.ValidateModel())
            {
                _camLicenseId = model.LicenseId;
                _camUserLogin = model.UserName;
                _camUserPassword = model.Password;

                Set(() => ValkyrieEnabled, true);
                Set(() => SelectValkyrieScan, false);
                Set(() => SelectValkyrieScan, true);
                Set(() => ValkyrieSendingFiles, true);

                _aptService.BeginValkyrieAnalysis(Login, Password, _camLicenseId, _camUserLogin, _camUserPassword,
                    networkCredential);
                return true;
            }

            //return false;
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
                unknownFiles += infoItem.UnknownFilesCount;
                malwareFiles += infoItem.MalwareFilesCount;
                lookupInProgressCount += infoItem.FailedToAnalyzeCount;
                lookupInProgressCount += infoItem.NoVerdictFilesCount;
            }


            UnknownFilesCount = unknownFiles;
            MalwareFilesCount = malwareFiles;
            LookupInProgressCount = lookupInProgressCount;

            RaisePropertyChanged(nameof(Header));
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

                            var exist = _cache.FirstOrDefault(_ =>
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
                                _cache.Add(item);
                                Items.Add(item);
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


                    parentItem.ScanStage = parentItem.ScanStage;
                }
            }
        }

        protected override void OnKeywordChanged(string oldValue, string newValue)
        {
            base.OnKeywordChanged(oldValue, newValue);

            if (newValue == oldValue || (newValue != null && newValue.Length < 3))
                return;

            if (_cacheComputers != null && _cacheComputers.Any())
            {
                Computers = new ObservableCollection<ThreatInfoItem>(
                       _cacheComputers.Where(item => item.NameAndIpAddress.ContainsString(Keyword)).ToList());

                SelectedItem = null;
            }

            if (_cache != null && _cache.Any())
            {
                Items = new ObservableCollection<ThreatInfoItem>(
                       _cache.Where(item => item.NameAndIpAddress.ContainsString(Keyword)).ToList());
            }

            if (_cacheValkyrie != null && _cacheValkyrie.Any())
            {
                ItemsValkyrie = new ObservableCollection<ValkyrieFile>(
                       _cacheValkyrie.Where(item => item.Name.ContainsString(Keyword)));
                SelectedValkyrieItem = null;
            }

            if (_cacheValkyrieComputers != null && _cacheValkyrieComputers.Any())
            {
                ComputersValkyrie = new ObservableCollection<ValkyrieComputer>(
                       _cacheValkyrieComputers.Where(item => item.FullName.ContainsString(Keyword)));
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

        private async Task TryAction(Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                TryActionException(ex);
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
