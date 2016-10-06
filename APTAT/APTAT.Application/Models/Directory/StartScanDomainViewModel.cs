using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using APTAT.Application.Extensions;
using Telerik.Windows.Controls;
using APTAT.Application.Services;
using APTAT.DeploymentCore.Directory;
using APTAT.Application.Messages;
using APTAT.Application.Models.ModelBase;

namespace APTAT.Application.Models
{
    public class StartScanDomainViewModel : WizardViewModel
    {
        ITargetsDiscoveryService _targetsDiscoveryService;
        IStorageService _storage;

        private DirectoryItem[] _items;
        private readonly ICollection<Object> _checkedItems;


        public const String SelectDomain = "SelectDomain";
        public const String SelectTargets = "SelectTargets";
        public const String SelectPort = "SelectPort";

        public DirectoryItem[] Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }




        private bool _domainCredentialsChanged = true;
        
        private string _domainController;
        public string DomainController
        {
            get { return UseCurrentDomainForActiveDirectory ? null : _domainController; }
            set
            {
                if (_domainController != value)
                    _domainCredentialsChanged = true;

                _domainController = value;
                RaisePropertyChanged(nameof(DomainController));
            }
        }

        private string _activeDirectoryLogin;
        public string ActiveDirectoryLogin
        {
            get { return UseCurrentDomainForActiveDirectory ? null : _activeDirectoryLogin; }
            set
            {
                if (_activeDirectoryLogin != value)
                    _domainCredentialsChanged = true;

                _activeDirectoryLogin = value;
                RaisePropertyChanged(nameof(ActiveDirectoryLogin));

            }
        }

        private string _activeDirectoryPassword;
        public string ActiveDirectoryPassword
        {
            get { return UseCurrentDomainForActiveDirectory ? null : _activeDirectoryPassword; }
            set
            {
                if (_activeDirectoryPassword != value)
                    _domainCredentialsChanged = true;

                _activeDirectoryPassword = value;
                RaisePropertyChanged(nameof(ActiveDirectoryPassword));

            }
        }

        private bool _useCurrentDomainForActiveDirectory;
        public bool UseCurrentDomainForActiveDirectory
        {
            get { return _useCurrentDomainForActiveDirectory; }
            set
            {
                if (_useCurrentDomainForActiveDirectory != value)
                    _domainCredentialsChanged = true;

                _useCurrentDomainForActiveDirectory = value;
                RaisePropertyChanged(nameof(UseCurrentDomainForActiveDirectory));
                RaisePropertyChanged(nameof(DomainController));
                RaisePropertyChanged(nameof(ActiveDirectoryLogin));
                RaisePropertyChanged(nameof(ActiveDirectoryPassword));

            }
        }

        private bool _useCurrentCredentials;
        public bool UseCurrentCredentials
        {
            get { return _useCurrentCredentials || ((IMainViewModel)ParentViewModel).HasCurrentDomainName; }

            set
            {
                if (UseCurrentCredentials == value) return;
                if (((IMainViewModel)ParentViewModel).HasCurrentDomainName) return;
                _useCurrentCredentials = value;

                UseCurrentDomainForDeploy = UseCurrentCredentials;
                RaisePropertyChanged(nameof(UseCurrentCredentials));
            }
        }

        private bool _useCurrentDomainForDeploy;
        public bool UseCurrentDomainForDeploy
        {
            get { return _useCurrentDomainForDeploy; }
            set
            {
                _useCurrentDomainForDeploy = value;
                ((IMainViewModel)ParentViewModel).UseCurrentDomainForDeploy = value;

                RaisePropertyChanged(nameof(UseCurrentDomainForDeploy));
                RaisePropertyChanged(nameof(Login));
                RaisePropertyChanged(nameof(Password));
            }
        }

        private string _login;
        public string Login
        {
            get { return UseCurrentDomainForDeploy ? null : _login; }
            set
            {
                _login = value;
                ((IMainViewModel)ParentViewModel).Login = value;

                RaisePropertyChanged(nameof(Login));
            }
        }

        private string _password;
        public string Password
        {
            get { return UseCurrentDomainForDeploy ? null : _password; }
            set
            {
                _password = value;
                ((IMainViewModel)ParentViewModel).Password = value;

                RaisePropertyChanged(nameof(Password));
            }
        }

        public DelegateCommand ExpandCommand { get; private set; }

        public DelegateCommand CheckItemCommand { get; private set; }

        public DelegateCommand UncheckItemCommand { get; private set; }

        public StartScanDomainViewModel(ITargetsDiscoveryService targetsDiscoveryService, IStorageService storage)
        {
            _targetsDiscoveryService = targetsDiscoveryService;
            _storage = storage;

            SetSteps(new[] { null, SelectDomain, SelectTargets, SelectPort });

            ExpandCommand = new DelegateCommand(InvokeExpand);
            CheckItemCommand = new DelegateCommand(InvokeCheckItem);
            UncheckItemCommand = new DelegateCommand(InvokeUncheckItem);

            _checkedItems = new Collection<Object>();


        }

        private void InvokeExpand(Object arg)
        {
            ExpandCommandContext commandContext = (ExpandCommandContext)arg;
            _targetsDiscoveryService.GetDirectoryItems(DomainController, ActiveDirectoryLogin, ActiveDirectoryPassword, (DirectoryItem)commandContext.DirectoryItem, new OperationContext { UserState = this, Arg = commandContext });
        }

        private void InvokeCheckItem(Object arg)
        {
            _checkedItems.Add(arg);
            var item = arg as ISelectableItem;
            if (item != null)
            {
                item.IsSelected = true;
            }
            Error = null;
        }

        private void InvokeUncheckItem(Object arg)
        {
            _checkedItems.Remove(arg);
            var item = arg as ISelectableItem;
            if (item != null)
            {
                item.IsSelected = false;
            }
        }

        protected override void OnStepChanging(Action success, String previousStep, ChangeStepCommandContext context)
        {
            if (previousStep == SelectDomain && context.IsForward(CurrentStepIndex))
            {          
                if (String.IsNullOrEmpty(DomainController) && !UseCurrentDomainForActiveDirectory)
                {
                    Error = "Please Specify Domain Name";
                    return;
                }

                if (String.IsNullOrEmpty(ActiveDirectoryLogin) && !UseCurrentDomainForActiveDirectory)
                {
                    Error = "Please Specify Domain Administrator";
                    return;
                }

                if (String.IsNullOrEmpty(ActiveDirectoryPassword) && !UseCurrentDomainForActiveDirectory)
                {
                    Error = "Please Specify Password";
                    return;
                }



                if (_domainCredentialsChanged)
                {
                    IsBusy = true;
                    _checkedItems.Clear();
                    _targetsDiscoveryService.GetDirectoryRoot(DomainController, ActiveDirectoryLogin, ActiveDirectoryPassword, new OperationContext { UserState = this, Success = success });
                    _domainCredentialsChanged = false;
                }
                else
                    success();
                
            }
            else if (previousStep == SelectTargets && context.IsForward(CurrentStepIndex))
            {
                if (_checkedItems == null || _checkedItems.Count == 0)
                {
                    Error = "Please select at least one item";
                }
                else
                {
                    var selectedItems = Items.GetSelectedItemsWithItemsPriority().OfType<DirectoryItem>().ToArray();
                    var isOnlyLocal = CheckIfCanUseCurrentCredentials(selectedItems);
                    _storage.SetValue(KnownKeys.CheckedDirectoryItems, selectedItems);
                    _storage.SetValue(KnownKeys.DevicesSourceDescription,
                        new DevicesSourceDescription() { Type = ETargetsDiscovererType.ActiveDirectory, Description = _domainController });

                    
                    if (!_credentialsDetermined)
                    {
                        if (!UseCurrentDomainForActiveDirectory)
                        {
                            if (!isOnlyLocal)
                                UseCurrentDomainForDeploy = false;
                            Login = ActiveDirectoryLogin;
                            Password = ActiveDirectoryPassword;

                        }
                        else if (!isOnlyLocal)
                            UseCurrentDomainForDeploy = true;

                        _credentialsDetermined = true;
                    }

                    success();
                }
            }
            else if (previousStep == SelectPort && context.IsForward(CurrentStepIndex))
            {
                var parent = ParentViewModel as IMainViewModel;
                if (parent != null)
                {
                    string errorMessage;
                    if (parent.ValidateCredentials(out errorMessage) && parent.ValidatePorts(out errorMessage))
                    {
                        success();
                    }
                    else
                        Error = errorMessage;
                }
                else
                {
                    success();
                }
            }
            else
            {
                if (context.IsBackward(CurrentStepIndex))
                {
                    Error = null;
                }

                base.OnStepChanging(success, previousStep, context);
            }
        }

        private bool CheckIfCanUseCurrentCredentials(DirectoryItem[] selectedItems)
        {
            return (UseCurrentCredentials = IsOnlyLocalComputer(selectedItems));
        }

        private bool IsOnlyLocalComputer(DirectoryItem[] selectedItems)
        {
            if (selectedItems.Length > 1) return false;
            foreach (var address in selectedItems)
            {
                var depItem = (DirectoryItem)address;
                if (depItem.Type != DirectoryItemType.Computer) return false;
                if (!_targetsDiscoveryService.IsLocalAddress(depItem.Name))
                {
                    return false;
                }
            }

            return true;
        }

        protected override void OnStepChanged()
        {

        }

        private bool _useCurrentNameDeterminied, _credentialsDetermined;
        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            var model = ParentViewModel as MainViewViewModel;

            if (!_useCurrentNameDeterminied)
            {
                UseCurrentDomainForDeploy = UseCurrentDomainForActiveDirectory = model.HasCurrentDomainName;
                RaisePropertyChanged(nameof(UseCurrentCredentials));
                _useCurrentNameDeterminied = true;
            }

            ActiveDirectoryPassword = null;
            Password = null;

            _targetsDiscoveryService.GetDirectoryItemsCompleted += OnGetDirectoryItemsCompleted;
            _targetsDiscoveryService.GetDirectoryRootCompleted += OnGetDirectoryRootCompleted;
        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _targetsDiscoveryService.GetDirectoryItemsCompleted -= OnGetDirectoryItemsCompleted;
            _targetsDiscoveryService.GetDirectoryRootCompleted -= OnGetDirectoryRootCompleted;


        }


        private void OnGetDirectoryRootCompleted(Object sender, GetDirectoryItemsEventArgs e)
        {
            OperationContext context = (OperationContext)e.UserState;
            if (context.UserState != this)
            {
                return;
            }

            IsBusy = false;

            
            if (e.Exception == null)
            {
                if (e.Items != null && e.Items.Length > 0)
                { 
                    e.Items[0].Name = e.Items[0].Name.Replace("DC=","");

                Items = e.Items;
                
                    //_targetsDiscoveryService.GetDirectoryItems(DomainController, ActiveDirectoryLogin, ActiveDirectoryPassword, Items[0], new OperationContext { UserState = this, Arg = this });
                }

                Error = null;
                context.Success();
            }
            else
            {
                Error = e.Exception.Message;
                _domainCredentialsChanged = true;
            }
        }

        private void OnGetDirectoryItemsCompleted(Object sender, GetDirectoryItemsEventArgs e)
        {
            OperationContext context = (OperationContext)e.UserState;
            if (context.UserState != this)
            {
                return;
            }

            if (e.Exception == null)
            {
                e.Parent.Items = new ObservableCollection<DirectoryItem>(e.Items.OrderBy(o => o.Name));
            }
            else
            {
                e.Parent.Items = new ObservableCollection<DirectoryItem>();
                Error = e.Exception.Message;
            }

            ExpandCommandContext cmdContext = context.Arg as ExpandCommandContext;
            if (cmdContext != null)
            {
                cmdContext.RaiseCompleted();
            }

        }

    }
}
