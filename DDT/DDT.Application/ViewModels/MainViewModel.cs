using System;
using DDT.Application.Messages;
using DDT.Application.Transitions;
using Telerik.Windows.Controls;
using DDT.Application.Services;
using DDT.Application.Models;
using System.Threading.Tasks;
using APTAT.Common;
using System.Diagnostics;

namespace DDT.Application.ViewModels
{
    public partial class MainViewModel : DomainObject
    {
        public MainViewModel(ITargetsDiscoveryService discoveryService, IDeploymentService deploymentService, IReportService reportService)
        {
            _discoveryService = discoveryService;
            _deploymentService = deploymentService;
            _reportService = reportService;

            Initialize();
            InitializeActiveDirectory();
            InitializeWorkgroup();
            InitializeNetworkAddresses();
            InitializeObtainResults();
            InitializeReport();
        }

        private void Initialize()
        {
            OnLoadedCommand = new DelegateCommand(OnLoaded);
            ChangeScreenCommand = new DelegateCommand(ChangeScreen, CanChangeScreen);
            ChangeScreenPreviousCommand = new DelegateCommand(ChangeScreenPrevious);
            AddMoreTargetsCommand = new DelegateCommand(AddMoreTargets);
            DeployCommand = new DelegateCommand(Deploy, CanDeploy);
            StopDeployCommand = new DelegateCommand(StopDeploy);
            ClearErrorMessageCommand = new DelegateCommand(ClearErrorMessage);
            ClearDisclaimerMessageCommand = new DelegateCommand(ClearDisclaimerMessage);
        }

        partial void InitializeActiveDirectory();
        partial void InitializeWorkgroup();
        partial void InitializeNetworkAddresses();
        partial void InitializeObtainResults();
        partial void InitializeReport();

        private readonly ITargetsDiscoveryService _discoveryService;
        private readonly IDeploymentService _deploymentService;
        private readonly IReportService _reportService;

        private ChangeScreenTypes _previousScreen, _previousLocalScreen, _currentScreen;

        #region Commands

        public DelegateCommand OnLoadedCommand { get; private set; }
        public DelegateCommand ChangeScreenCommand { get; private set; }
        public DelegateCommand ChangeScreenPreviousCommand { get; private set; }
        public DelegateCommand AddMoreTargetsCommand { get; private set; }
        public DelegateCommand DeployCommand { get; private set; }
        public DelegateCommand StopDeployCommand { get; private set; }
        public DelegateCommand ClearErrorMessageCommand { get; private set; }
        public DelegateCommand ClearDisclaimerMessageCommand { get; private set; }

        #endregion

        #region Properties

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                RaisePropertyChanged(nameof(Progress));
                RaisePropertyChanged(nameof(HasProgress));
                RaisePropertyChanged(nameof(ButtonsAvailable));
            }
        }

        private double _internalProgress;
        public double InternalProgress
        {
            get { return _internalProgress; }
            set
            {
                _internalProgress = value;
                RaisePropertyChanged(nameof(InternalProgress));
                RaisePropertyChanged(nameof(HasInternalProgress));
                RaisePropertyChanged(nameof(SearchAvailable));
            }
        }

        public bool HasProgress
        {
            get { return Progress > 0 && Progress <= 100; }
        }

        public bool HasInternalProgress
        {
            get { return InternalProgress > 0 && InternalProgress <= 100; }
        }

        private string _progressMessage = "Please Wait...";
        public string ProgressMessage
        {
            get { return _progressMessage; }
            set
            {
                _progressMessage = value;
                RaisePropertyChanged(nameof(ProgressMessage));
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value; 
                RaisePropertyChanged(nameof(ErrorMessage));

                if (!string.IsNullOrEmpty(value) && DisclaimerMessage != null)
                    DisclaimerMessage = string.Empty;
            }
        }

        private const string _disclaimerText = @"You are going to deploy EXE package which will be installed silently. If the installation process requires any user interaction, it may fail due to no response.";
        private string _disclaimerMessage;
        public string DisclaimerMessage
        {
            get { return _disclaimerMessage; }
            set
            {
                _disclaimerMessage = value;
                RaisePropertyChanged(nameof(DisclaimerMessage));
            }
        }

        private bool _stopAvailable;
        public bool StopAvailable
        {
            get { return _stopAvailable; }
            set
            {
                _stopAvailable = value;
                RaisePropertyChanged(nameof(StopAvailable));
            }
        }

        private bool _generateReportAvailable;
        public bool GenerateReportAvailable
        {
            get { return _generateReportAvailable; }
            set
            {
                _generateReportAvailable = value;
                RaisePropertyChanged(nameof(GenerateReportAvailable));
                GenerateReportCommand.InvalidateCanExecute();
            }
        }

        private bool _buttonsAvailable = true;
        public bool ButtonsAvailable
        {
            get { return HasProgress ? false : _buttonsAvailable; }
            set
            {
                _buttonsAvailable = value;
                RaisePropertyChanged(nameof(ButtonsAvailable));
            }
        }

        public bool Redeploy { get; set; } = true;

        public bool RunAsUser { get; set; }

        public string ParametersExample
        {
            get { return "Example: -p -s"; }
        }

        private bool _isInstallationParametersSpecified;
        public bool IsInstallationParametersSpecified
        {
            get { return _isInstallationParametersSpecified; }
            set
            {
                _isInstallationParametersSpecified = value;
                RaisePropertyChanged(nameof(IsInstallationParametersSpecified));

                ChangeScreenCommand.InvalidateCanExecute();
            }
        }
        public string InstallationParameters { get; set; }

        private bool _isInstallationParameters64Specified;
        public bool IsInstallationParameters64Specified
        {
            get { return _isInstallationParameters64Specified; }
            set
            {
                _isInstallationParameters64Specified = value;
                RaisePropertyChanged(nameof(IsInstallationParameters64Specified));
            }
        }
        public string InstallationParameters64 { get; set; }

        private bool _separatePackages;
        public bool SeparatePackages
        {
            get { return _separatePackages; }
            set
            {
                _separatePackages = value;
                RaisePropertyChanged(nameof(SeparatePackages));
                RaisePropertyChanged(nameof(PackageHeader));

                ChangeScreenCommand.InvalidateCanExecute();
            }
        }

        public string PackageHeader
        {
            get { return SeparatePackages ? "32-bit Package" : "Package"; }
        }

        private string _package = "Select the package you want to deploy over discovered endpoints";
        public object Package
        {
            get { return _package; }
            set
            {
                _package = value == null ? "Select the package you want to deploy over discovered endpoints" : value as string;
                RaisePropertyChanged(nameof(Package));
                RaisePropertyChanged(nameof(HasPackage));

                ChangeScreenCommand.InvalidateCanExecute();

                DisclaimerMessage = null;
                if (value != null)
                {
                    if (_package.ToLower().EndsWith("exe"))
                        DisclaimerMessage = _disclaimerText;
                }
            }
        }

        public bool HasPackage
        {
            get { return _package.StartsWith("["); }
        }

        private string _package64 = "Select the package you want to deploy over discovered endpoints";
        public object Package64
        {
            get { return _package64; }
            set
            {
                _package64 = value == null ? "Select the package you want to deploy over discovered endpoints" : value as string;
                RaisePropertyChanged(nameof(Package64));
                RaisePropertyChanged(nameof(HasPackage64));

                ChangeScreenCommand.InvalidateCanExecute();
            }
        }

        public bool HasPackage64
        {
            get { return _package64.StartsWith("["); }
        }

        private string _transform = "Select the MST transform for the MSI package";
        public object Transform
        {
            get { return _transform; }
            set
            {
                _transform = value == null ? "Select the MST transform for the MSI package" : value as string;
                RaisePropertyChanged(nameof(Transform));
                RaisePropertyChanged(nameof(HasTransform));
            }
        }

        public bool HasTransform
        {
            get { return _transform.StartsWith("["); }
        }

        private string _transform64 = "Select the MST transform for the MSI package";
        public object Transform64
        {
            get { return _transform64; }
            set
            {
                _transform64 = value == null ? "Select the MST transform for the MSI package" : value as string;
                RaisePropertyChanged(nameof(Transform64));
                RaisePropertyChanged(nameof(HasTransform64));
            }
        }

        public bool HasTransform64
        {
            get { return _transform64.StartsWith("["); }
        }

        public string LoginMask
        {
            get { return "Enter Login"; }
        }

        public string PasswordMask
        {
            get { return "Enter Password"; }
        }

        private string _login;
        public string Login
        {
            get { return UseCurrentDomainForDeploy ? null : _login; }
            set
            {
                _login = value;
                RaisePropertyChanged(nameof(Login));

                DeployCommand.InvalidateCanExecute();
            }
        }

        private string _password;
        public string Password
        {
            get { return UseCurrentDomainForDeploy ? null : _password; }
            set
            {
                _password = value;
                RaisePropertyChanged(nameof(Password));

                DeployCommand.InvalidateCanExecute();
            }
        }

        public bool SearchAvailable
        {
            get { return !HasInternalProgress; }
        }

        public Predicate<object> SearchPredicate
        {
            get
            {
                return _ =>
                {
                    if (_ == null)
                        return false;

                    switch (_currentScreen)
                    {
                        case ChangeScreenTypes.Workgroup:
                            return SearchWorkgroup(_);
                        case ChangeScreenTypes.ActiveDirectory:
                            return SearchActiveDirectory(_);
                        case ChangeScreenTypes.DeploymentResults:
                            return SearchResults(_);
                    }

                    return true;
                };
            }
        }

        public string CurrentDomainName { get; set; }

        public bool HasCurrentDomainName
        {
            get { return !string.IsNullOrEmpty(CurrentDomainName); }
        }

        private bool _useCurrentDomainForDeploy;
        public bool UseCurrentDomainForDeploy
        {
            get { return _useCurrentDomainForDeploy; }
            set
            {
                _useCurrentDomainForDeploy = value;
                RaisePropertyChanged(nameof(UseCurrentDomainForDeploy));
                RaisePropertyChanged(nameof(Login));
                RaisePropertyChanged(nameof(Password));

                DeployCommand.InvalidateCanExecute();
            }
        }

        #endregion

        private async void OnLoaded(object o)
        {
            var name = await TryAction(GetCurrentDomainName());

            CurrentDomainName = name?.Replace("DC=", "").Replace(",", ".");

            if (HasCurrentDomainName)
            {
                CurrentDomainName = $"Use Current Domain Credentials ({CurrentDomainName})";
                UseCurrentDomainForActiveDirectory = true;
                UseCurrentDomainForDeploy = true;
            }

            RaisePropertyChanged(nameof(CurrentDomainName));
            RaisePropertyChanged(nameof(HasCurrentDomainName));
        }

        private void ChangeScreenPrevious(object o = null)
        {
            if (HasProgress || HasInternalProgress)
                return;

            switch (_currentScreen)
            {
                case ChangeScreenTypes.NetworkAddresses:
                    NetworkAddress = null;
                    DispatcherAction(() => RestoreNetworkAddressMemento());
                    break;
                case ChangeScreenTypes.WorkgroupCredentials:
                case ChangeScreenTypes.Workgroup:
                    if ((ChangeScreenTypes)o != ChangeScreenTypes.WorkgroupCredentials)
                        DispatcherAction(() => RestoreWorkgroupMemento());
                    break;
                case ChangeScreenTypes.ActiveDirectoryCredentials:
                case ChangeScreenTypes.ActiveDirectory:
                    if ((ChangeScreenTypes)o != ChangeScreenTypes.ActiveDirectoryCredentials)
                        DispatcherAction(() => RestoreActiveDirectoryMemento());
                    break;
                case ChangeScreenTypes.About:
                case ChangeScreenTypes.Options:
                    if (o != null && (ChangeScreenTypes)o == ChangeScreenTypes.Packages)
                    {
                        switch (_previousScreen)
                        {
                            case ChangeScreenTypes.NetworkAddresses:
                                DispatcherAction(() => RestoreNetworkAddressMemento());
                                break;
                            case ChangeScreenTypes.WorkgroupCredentials:
                            case ChangeScreenTypes.Workgroup:
                                DispatcherAction(() => RestoreWorkgroupMemento());
                                break;
                            case ChangeScreenTypes.ActiveDirectoryCredentials:
                            case ChangeScreenTypes.ActiveDirectory:
                                DispatcherAction(() => RestoreActiveDirectoryMemento());
                                break;
                        }
                    }
                    break;
            }

            if (o != null)
            {
                var screen = (ChangeScreenTypes)o;
                if (screen == _currentScreen)
                    return;

                switch (screen)
                {
                    case ChangeScreenTypes.Packages:
                        if (DisclaimerMessage == string.Empty)
                            DisclaimerMessage =_disclaimerText;
                        break;
                    case ChangeScreenTypes.Options:
                    case ChangeScreenTypes.About:
                        if (DisclaimerMessage != null)
                            DisclaimerMessage = string.Empty;
                        break;
                }

                AppMessages.ChangeScreenMessage.Send(screen, _ => SetCurrentScreen(_),
                    _currentScreen != ChangeScreenTypes.EULA && (int)_currentScreen < 11 ? Direction.RightToLeft : (Direction?)null);
                ButtonsAvailable = false;
                ErrorMessage = null;
            }
            else
            {
                if (_currentScreen == _previousScreen)
                    _previousScreen = _previousLocalScreen;

                switch (_previousScreen)
                {
                    case ChangeScreenTypes.Packages:
                        if (DisclaimerMessage == string.Empty)
                            DisclaimerMessage = _disclaimerText;
                        break;
                    case ChangeScreenTypes.NetworkAddresses:
                        DispatcherAction(SaveNetworkAddressMemento);
                        break;
                    case ChangeScreenTypes.Workgroup:
                        DispatcherAction(SaveWorkgroupMemento);
                        break;
                    case ChangeScreenTypes.ActiveDirectory:
                        DispatcherAction(SaveActiveDirectoryMemento);
                        break;
                }

                AppMessages.ChangeScreenMessage.Send(_previousScreen, _ => SetCurrentScreen(_));
                ButtonsAvailable = false;
                ErrorMessage = null;
            }
        }

        private void ChangeScreen(object o = null)
        {
            if (HasProgress || HasInternalProgress)
                return;

            switch (_currentScreen)
            {
                case ChangeScreenTypes.NetworkAddresses:
                    NetworkAddress = null;
                    string validationMessage = null;
                    if (o != null && !ValidateNetworkAddresses(out validationMessage))
                    {
                        ErrorMessage = validationMessage;
                        return;
                    }
                    RaisePropertyChanged(nameof(NetworkAddressHeader));
                    break;
                case ChangeScreenTypes.WorkgroupCredentials:
                case ChangeScreenTypes.Workgroup:
                    RaisePropertyChanged(nameof(WorkgroupHeader));
                    break;
                case ChangeScreenTypes.ActiveDirectoryCredentials:
                case ChangeScreenTypes.ActiveDirectory:
                    RaisePropertyChanged(nameof(ActiveDirectoryHeader)); 
                    break;
            }

            if (o != null)
            {
                var screen = (ChangeScreenTypes)o;
                if (screen == _currentScreen)
                    return;

                switch (screen)
                {
                    case ChangeScreenTypes.NetworkAddresses:
                        DispatcherAction(SaveNetworkAddressMemento);
                        break;
                    case ChangeScreenTypes.WorkgroupCredentials:
                        DispatcherAction(SaveWorkgroupMemento);
                        if (HasWorkgroup)
                            screen = ChangeScreenTypes.Workgroup;
                        break;
                    case ChangeScreenTypes.ActiveDirectoryCredentials:
                        DispatcherAction(SaveActiveDirectoryMemento);
                        if (HasActiveDirectory)
                            screen = ChangeScreenTypes.ActiveDirectory;
                        break;
                }

                AppMessages.ChangeScreenMessage.Send(screen, _ => SetCurrentScreen(_), null);
                ButtonsAvailable = false;
                ErrorMessage = null;

                if (DisclaimerMessage != null)
                    DisclaimerMessage = string.Empty;
            }
        }

        private bool CanChangeScreen(object o)
        {
            switch (_currentScreen)
            {
                case ChangeScreenTypes.Packages:
                    return HasPackage && (SeparatePackages ? HasPackage64 : true) && (!IsInstallationParametersSpecified || !string.IsNullOrEmpty(InstallationParameters));
                case ChangeScreenTypes.DiscoveryTargets:
                    return HasActiveDirectory || HasWorkgroup || HasNetworkAddress;
                case ChangeScreenTypes.NetworkAddresses:
                    return HasNetworkAddress;
                case ChangeScreenTypes.Workgroup:
                    return HasWorkgroup;
                case ChangeScreenTypes.ActiveDirectory:
                    return HasActiveDirectory;
            }

            return true;
        }

        private void SetCurrentScreen(ChangeScreenTypes screen, Action action = null)
        {
            if ((int)_currentScreen < 11)
            {
                if ((int)screen >= 11)
                    _previousLocalScreen = _previousScreen;
                _previousScreen = _currentScreen;
            }
            _currentScreen = screen;

            Action a = () => { ButtonsAvailable = true; ChangeScreenCommand.InvalidateCanExecute(); };
            if (action != null)
                a = (Action)Delegate.Combine(a, action);

            if (TransitionManager.CurrentStrategy is NoneTransitionStrategy)
                a();
            else
                Task.Delay(1000).ContinueWith(_ => DispatcherAction(a));
        }

        private void AddMoreTargets(object o)
        {
            ChangeScreenCommand.Execute(ChangeScreenTypes.DiscoveryTargets);
        }

        private void Deploy(object o)
        {
            ButtonsAvailable = false;

            AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.DeploymentResults, async _ =>
            {
                SetCurrentScreen(_);

                await TryAction(ResolveAggregatedDeploymentItems());
            });
        }

        private bool CanDeploy(object o)
        {
            return UseCurrentDomainForDeploy || (!string.IsNullOrEmpty(Login) && !string.IsNullOrEmpty(Password));
        }

        private void StopDeploy(object o)
        {
            if (StopAvailable)
            {
                if (Results.Count == 0)
                    return;

                AppMessages.ChangeScreenMessage.Send<bool?>(ChangeScreenTypes.StopConfirmation, _ =>
                {
                    if (_ == true)
                    {
                        StopAvailable = false;

                        if (_previousScreen == ChangeScreenTypes.DeploymentResults)
                            AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.DeploymentResults, __ => SetCurrentScreen(__));

                        _deploymentService.StopDeployment(true);

                        SetStoppingStatuses();

                        if (o != null)
                            ((Action)o).Invoke();
                    }
                });
            }
            else if (o != null)
                ((Action)o).Invoke();
        }

        private void ClearErrorMessage(object o)
        {
            ErrorMessage = null;
        }

        private void ClearDisclaimerMessage(object o)
        {
            DisclaimerMessage = null;
        }

        private void DispatcherAction(Action action) => App.Current.Dispatcher.BeginInvoke(action);

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

        private async Task<T> TryAction<T>(Task<T> task) where T: class
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
            switch (_currentScreen)
            {
                case ChangeScreenTypes.ActiveDirectory:
                    AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.ActiveDirectoryCredentials, _ =>
                    {
                        SetCurrentScreen(_, () =>
                        {
                            Progress = 0;
                            ErrorMessage = "Domain does not exist or could not be contacted under the specified credentials. Please try another domain or credentials.";
                        });
                    }, Direction.RightToLeft);
                    break;
                case ChangeScreenTypes.Workgroup:
                    AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.WorkgroupCredentials, _ =>
                    {
                        SetCurrentScreen(_, () =>
                        {
                            Progress = 0;
                            ErrorMessage = "No workgroups found. Please try another name.";
                        });
                    }, Direction.RightToLeft);
                    break;
                default:
                    StopAvailable = false;
                    Progress = 0;
                    InternalProgress = 0;
                    break;
            }

            LogHelper.Trace(TraceEventType.Error, $"TryAction error: {ex.GetType()} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }
    }
}