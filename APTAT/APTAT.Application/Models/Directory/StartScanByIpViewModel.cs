using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using APTAT.Application.Extensions;
using APTAT.Application.Models.ModelBase;
using APTAT.Application.Services;
using APTAT.Core.Descent;
using APTAT.DeploymentCore.Directory;
using Telerik.Windows.Controls;
using APTAT.Application.Views;
using APTAT.Application.Models;

namespace APTAT.Application.Models.Directory
{
    public class StartScanByIpViewModel : WizardViewModel
    {
        public const String SelectIp = "SelectIp";
        public const String SelectPort = "SelectPort";

        private readonly ITargetsDiscoveryService _targetsDiscoveryService;
        private readonly IStorageService _storage;

        private ObservableCollection<DeploymentItem> _items;

        public ICollection<Object> CheckedItems
        {
            get;
            set;
        }

        public ObservableCollection<DeploymentItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

      

        public ObservableCollection<IpAddressWrapper> IpAddresses { get; private set; }

        private String _inputString;

        public String InputString
        {
            get { return _inputString; }
            set
            {
                _inputString = value;
                RaisePropertyChanged("InputString");
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
                

                RaisePropertyChanged(nameof(Password));
            }
        }


        public DelegateCommand AddIpCommand
        {
            get;
            private set;
        }

        public DelegateCommand DeleteIpCommand
        {
            get;
            private set;
        }

        public DelegateCommand DeleteAllIpCommand
        {
            get;
            private set;
        }



        public StartScanByIpViewModel(ITargetsDiscoveryService targetsDiscoveryService, IStorageService storage)
        {
            _targetsDiscoveryService = targetsDiscoveryService;
            _storage = storage;

            SetSteps(new[] { null, SelectIp, SelectPort });
            IpAddresses = new ObservableCollection<IpAddressWrapper>();
            AddIpCommand = new DelegateCommand(InvokeAddIpCommand);
            DeleteIpCommand = new DelegateCommand(InvokeDeleteIpCommand);
            DeleteAllIpCommand = new DelegateCommand(InvokeDeleteIpCommand);
        }

        private bool _useCurrentNameDeterminied;
        protected override void InvokeViewLoaded(object arg)
        {
            base.InvokeViewLoaded(arg);

            var model = ParentViewModel as MainViewViewModel;

            if (!_useCurrentNameDeterminied)
            {
                UseCurrentDomainForDeploy = model.HasCurrentDomainName;
                RaisePropertyChanged(nameof(UseCurrentCredentials));
                _useCurrentNameDeterminied = true;
            }

            Password = null;

            // restore IP addresses list 
            ObservableCollection<IpAddressWrapper> storedIpAddresses = _storage.GetValue<ObservableCollection<IpAddressWrapper>>(KnownKeys.IpAddresses, false);
            if (storedIpAddresses != null)
            {
                if (IpAddresses.Count != 0)
                {
                    foreach (var item in storedIpAddresses)
                    {
                        if (!IpAddresses.Contains(item))
                        {
                            IpAddresses.Add(item);
                        }
                    }
                }
                else
                {
                    foreach (var item in storedIpAddresses)
                    {
                        IpAddresses.Add(item);
                    }
                }

                RaisePropertyChanged("IpAddresses");
            }

            _targetsDiscoveryService.GetByIpAddressesCompleted -= OnGetByIpAddressesCompleted;
            _targetsDiscoveryService.GetByIpAddressesCompleted += OnGetByIpAddressesCompleted;
        }


        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _targetsDiscoveryService.GetByIpAddressesCompleted -= OnGetByIpAddressesCompleted;
        }


        private void InvokeDeleteIpCommand(Object e)
        {
            CancelItemArgs args = (CancelItemArgs)e;
            var wrappers = args.Item as IEnumerable<Object>;
            if (wrappers != null)
            {
                foreach (var wrapper in wrappers)
                {
                    if (wrapper is IpAddressWrapper)
                    {
                        IpAddresses.Remove((IpAddressWrapper)wrapper);
                    }
                }
            }
        }



        private void InvokeAddIpCommand(Object e)
        {
            Error = null;
            CancelItemArgs args = (CancelItemArgs)e;
            var ipAddress = new IpAddressWrapper();
            try
            {
                ipAddress.Text = (string)args.Item;
            }
            catch (ValidationException exc)
            {
                Error = exc.Message;
                args.Cancel = true;
                return;
            }

            if (ValidateProperty(ipAddress, "Text", IpAddressWrapper.Validator))
            {
                if (IpAddresses.Any(o => o.Text == ipAddress.Text))
                {
                    Error = "The same network address was already added";
                    args.Cancel = true;
                }
                else
                {
                    IpAddresses.Add(ipAddress);
                    RaisePropertyChanged("IpAddresses");
                }
            }
            else
            {
                args.Cancel = true;
            }
        }

        protected override void OnStepChanging(Action success, String previousStep, ChangeStepCommandContext context)
        {
            Error = null;
            
            if (previousStep == SelectIp && context.IsForward(CurrentStepIndex))
            {
                if (IpAddresses.Count == 0)
                {
                    Error = "Please enter at least one network address";
                    return;
                }


                IsBusy = true;
                _storage.SetValue(KnownKeys.IpAddresses, IpAddresses);



                _targetsDiscoveryService.GetByIpAddresses(IpAddresses, CheckIfCanUseCurrentCredentials, new OperationContext { Success = success, UserState = this });

            }
            else if (previousStep == SelectPort && context.IsForward(CurrentStepIndex))
            {


                _storage.SetValue(KnownKeys.CheckedDirectoryItems, CheckedItems);
                //_storage.SetValue(KnownKeys.TcpPort, _tcpPort);
                //_storage.SetValue(KnownKeys.UdpPort, _udpPort);
                _storage.SetValue(KnownKeys.IpAddresses, IpAddresses);
                var ipAddressesString = IpAddresses.Select(elm => elm.FormattedText).ToArray();
                _storage.SetValue(KnownKeys.DevicesSourceDescription,
                    new DevicesSourceDescription()
                    {
                        Type = ETargetsDiscovererType.NetworkAddresses,
                        Description = DevicesSourceDescription.GetNetworkDescription(ipAddressesString)
                    });
                var parent = ParentViewModel as IMainViewModel;
                if (parent != null)
                {
                    string errorMessage;

                    parent.UseCurrentDomainForDeploy = UseCurrentDomainForDeploy;
                    parent.Login = Login;
                    parent.Password = Password;

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



        private void CheckIfCanUseCurrentCredentials(bool local)
        {
            UseCurrentCredentials = local;
        }


        private void OnGetByIpAddressesCompleted(Object sender, GetByIpAddressEventArgs e)
        {
            OperationContext context = (OperationContext)e.UserState;
            if (context.UserState != this)
            {
                return;
            }

            
            IsBusy = false;
            if (e.Exception == null)
            {

                CheckedItems = new Collection<Object>(e.Items.OrderBy(o => o.Name).ToArray());
                _storage.SetValue(KnownKeys.CheckedDirectoryItems, CheckedItems);
                //_storage.SetValue(KnownKeys.TcpPort, _tcpPort);
                //_storage.SetValue(KnownKeys.UdpPort, _udpPort);
                var ipAddressesString = IpAddresses.Select(elm => elm.FormattedText).ToArray();
                _storage.SetValue(KnownKeys.DevicesSourceDescription,
                    new DevicesSourceDescription()
                    {
                        Type = ETargetsDiscovererType.NetworkAddresses,
                        Description = DevicesSourceDescription.GetNetworkDescription(ipAddressesString)
                    });
                context.Success();
            }
            else
            {
                //Items = new ObservableCollection<DeploymentItem>();
                Error = e.Exception.Message;
            }

        }
    }
}
