using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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


        public string DomainControllerMask
        {
            get { return "Enter Domain Name"; }
        }


        private string _domainController;
        public string DomainController
        {
            get { return UseCurrentDomainForActiveDirectory ? null : _domainController; }
            set
            {
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
                _useCurrentDomainForActiveDirectory = value;
                RaisePropertyChanged(nameof(UseCurrentDomainForActiveDirectory));
                RaisePropertyChanged(nameof(DomainController));
                RaisePropertyChanged(nameof(ActiveDirectoryLogin));
                RaisePropertyChanged(nameof(ActiveDirectoryPassword));

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
            Error = null;
        }

        private void InvokeUncheckItem(Object arg)
        {
            _checkedItems.Remove(arg);
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



                IsBusy = true;
                //_storage.SetValue(KnownKeys.Domain, DomainController);
                _targetsDiscoveryService.GetDirectoryRoot(DomainController, ActiveDirectoryLogin, ActiveDirectoryPassword, new OperationContext { UserState = this, Success = success });
            }
            else if (previousStep == SelectTargets && context.IsForward(CurrentStepIndex))
            {
                if (_checkedItems == null || _checkedItems.Count == 0)
                {
                    Error = "Please select at least one item";
                }
                else
                {
                    CheckIfCanUseCurrentCredentials();
                //    var item = (DeploymentItem)_checkedItems.ToArray()[0];
                    _storage.SetValue(KnownKeys.CheckedDirectoryItems, _checkedItems);
                    //storage.SetValue(KnownKeys.Login, ActiveDirectoryLogin);
                    //_storage.SetValue(KnownKeys.Password, ActiveDirectoryPassword);
                    //_storage.SetValue(KnownKeys.Domain, _domainController);
                    _storage.SetValue(KnownKeys.DevicesSourceDescription,
                        new DevicesSourceDescription() { Type = ETargetsDiscovererType.ActiveDirectory, Description = _domainController });
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

                //_storage.SetValue(KnownKeys.Login, ActiveDirectoryLogin);
                //_storage.SetValue(KnownKeys.Domain, _domainController);

                base.OnStepChanging(success, previousStep, context);
            }
        }

        private void CheckIfCanUseCurrentCredentials()
        {
            var parentView = ((MainViewViewModel)base.ParentViewModel);
            parentView.UseCurrentCredentials = IsOnlyLocalComputer();
        }

        private bool IsOnlyLocalComputer()
        {
            bool onlyLocal = true;
            foreach (var address in _checkedItems)
            {
                var depItem = (DirectoryItem)address;
                if (depItem.Type == DirectoryItemType.Root) return false;
                if (!_targetsDiscoveryService.IsLocalAddress(depItem.Name))
                {
                    onlyLocal = false;
                    break;
                }
            }

            return onlyLocal;
        }

        protected override void OnStepChanged()
        {

        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            var model = ParentViewModel as MainViewViewModel;
            UseCurrentDomainForActiveDirectory =  model.HasCurrentDomainName;
      //      this.ParentViewModel

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
                
                    _targetsDiscoveryService.GetDirectoryItems(DomainController, ActiveDirectoryLogin, ActiveDirectoryPassword, Items[0], new OperationContext { UserState = this, Arg = this });
                }

                Error = null;
                context.Success();
            }
            else
            {
                Error = e.Exception.Message;
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
