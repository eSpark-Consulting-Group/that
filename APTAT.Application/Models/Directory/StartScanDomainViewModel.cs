using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Telerik.Windows.Controls;
using APTAT.Application.Extensions;
using APTAT.Application.Services;
using APTAT.Core.Descent;

namespace APTAT.Application.Models
{
    public class StartScanDomainViewModel : WizardViewModel
    {
        ITargetsDiscoveryService _targetsDiscoveryService; 
        IStorageService _storage;
  
        private DirectoryItem[] _items;
        private readonly ICollection<Object> _checkedItems;
        private String _domainController;
        private String _administrator;
        private String _password;

      
        public const String SelectDomain = "SelectDomain";
        public const String SelectTargets = "SelectTargets";

        public DirectoryItem[] Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public String DomainController
        {
            get { return _domainController; }
            set
            {
                _domainController = value;

                if (String.IsNullOrWhiteSpace(_domainController))
                {
                    RaiseValidationError("DomainController", "Please Specify Domain Controller");
                }
                else
                {
                    ClearError("DomainController");
                }
            }
        }

        public String Administrator
        {
            get { return _administrator; }
            set
            {
                _administrator = value;

                if (String.IsNullOrWhiteSpace(_administrator))
                {
                    RaiseValidationError("Administrator", "Please Specify Administrator Login");
                }
                else
                {
                    ClearError("Administrator");
                }
            }
        }

        public String Password
        {
            get { return _password; }
            set
            {
                _password = value;
              
            }
        }
        private string _tcpPort = String.Empty;
        public String TcpPort
        {            
            get
            {
                return _tcpPort;
            }
            set
            {
                _tcpPort = value;
                bool hasError;
                var errorMessage = value.ValidateTCP(out hasError);
                if (hasError)
                {
                    RaiseValidationError("TcpPort", errorMessage);
                }
                if (!hasError)
                {
                    ClearError("TcpPort");                
                }            
            }
        }

        private string _udpPort = String.Empty;
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
                if (!hasError)
                {
                    ClearError("UdpPort");
                }
            }
        }



        public DelegateCommand ExpandCommand { get; private set; }

        public DelegateCommand CheckItemCommand { get; private set; }

        public DelegateCommand UncheckItemCommand { get; private set; }

        public StartScanDomainViewModel(ITargetsDiscoveryService targetsDiscoveryService,IStorageService storage)
        {
            _targetsDiscoveryService = targetsDiscoveryService;
            _storage = storage;

            SetSteps(new[] { null, SelectDomain, SelectTargets });

            ExpandCommand = new DelegateCommand(InvokeExpand);
            CheckItemCommand = new DelegateCommand(InvokeCheckItem);
            UncheckItemCommand = new DelegateCommand(InvokeUncheckItem);

            _checkedItems = new Collection<Object>();
            TcpPort = "80";
            UdpPort = "53";
        }

        private void InvokeExpand(Object arg)
        {
            ExpandCommandContext commandContext = (ExpandCommandContext)arg;
            _targetsDiscoveryService.GetDirectoryItems(DomainController, Administrator, Password, (DirectoryItem)commandContext.DirectoryItem, new OperationContext { UserState = this, Arg = commandContext });
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
                if (String.IsNullOrEmpty(DomainController))
                {
                    Error = "Please Specify Domain Controller";
                    return;
                }

                if (String.IsNullOrEmpty(Administrator))
                {
                    Error = "Please Specify Administrator Login";
                    return;
                }

                if (String.IsNullOrEmpty(Password))
                {
                    Error = "Please Specify Password";
                    return;
                }

                bool isTcpPortHasError = false;
                var msgTcp = TcpPort.ValidateTCP(out isTcpPortHasError);
                if (isTcpPortHasError)
                {
                    Error = msgTcp;
                    return;
                }

                bool isUdpPortHasError = false;
                var msgUdp = UdpPort.ValidateUDP(out isUdpPortHasError);
                if (isUdpPortHasError)
                {
                    Error = msgUdp;
                    return;
                }

                IsBusy = true;
                _storage.SetValue(KnownKeys.Domain, DomainController);
                _targetsDiscoveryService.GetDirectoryRoot(DomainController, Administrator, Password, new OperationContext { UserState = this, Success = success });
            }
            else if (previousStep == SelectTargets && context.IsForward(CurrentStepIndex))
            {
                if (_checkedItems == null || _checkedItems.Count == 0)
                {
                    Error = "Select at least one item";
                }
                else
                {
                    _storage.SetValue(KnownKeys.CheckedDirectoryItems, _checkedItems);
                    _storage.SetValue(KnownKeys.Login, _administrator);
                    _storage.SetValue(KnownKeys.Password, _password);
                    _storage.SetValue(KnownKeys.Domain, _domainController);
                    _storage.SetValue(KnownKeys.TcpPort, _tcpPort);
                    _storage.SetValue(KnownKeys.UdpPort, _udpPort);
                    _storage.SetValue(KnownKeys.DevicesSourceDescription,
                        new DevicesSourceDescription() { Type = ETargetsDiscovererType.ActiveDirectory, Description = _domainController });
                    success();
                }
            }
            else
            {
                if (context.IsBackward(CurrentStepIndex))
                {
                    Error = null;
                }

                _storage.SetValue(KnownKeys.Login, _administrator);
                _storage.SetValue(KnownKeys.Domain, _domainController);

                base.OnStepChanging(success, previousStep, context);
            }
        }

        protected override void OnStepChanged()
        {
            
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            _administrator = _storage.GetValue<String>(KnownKeys.Login, false);
            //if (_administrator != null)
            {
                RaisePropertyChanged("Administrator");
            }
            _domainController = _storage.GetValue<String>(KnownKeys.Domain, false);
            //if (_domainController != null)
            {
                RaisePropertyChanged("DomainController");
            }

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
            
            Items = e.Items;
            if (e.Exception == null)
            {
                if (Items != null && Items.Length > 0)
                {
                    _targetsDiscoveryService.GetDirectoryItems(DomainController, Administrator, Password, Items[0], new OperationContext { UserState = this, Arg = this });
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
