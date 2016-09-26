using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using APTAT.Application.Extensions;
using APTAT.Application.Services;
using APTAT.Core.Descent;
using APTAT.DeploymentCore.Directory;
using Telerik.Windows.Controls;
using APTAT.Application.Views;

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

        private string _tcpPort = String.Empty;
        public String TcpPort
        {
            get { return _tcpPort; }
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

        private Boolean _enableDefaultButtons;
        public Boolean EnableDefaultButtons
        {
            get { return _enableDefaultButtons; }
            set
            {
                _enableDefaultButtons = value;
                RaisePropertyChanged("EnableDefaultButtons");
            }
        }

        public StartScanByIpViewModel(ITargetsDiscoveryService targetsDiscoveryService, IStorageService storage)
        {
            _targetsDiscoveryService = targetsDiscoveryService;
            _storage = storage;

            SetSteps(new[] { null, SelectIp, SelectPort });
            IpAddresses = new ObservableCollection<IpAddressWrapper>();
            AddIpCommand = new DelegateCommand(InvokeAddIpCommand);
            DeleteIpCommand = new DelegateCommand(InvokeDeleteIpCommand);
            TcpPort = "80";
            UdpPort = "53";
        }

        protected override void InvokeViewLoaded(object arg)
        {
            base.InvokeViewLoaded(arg);

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
                    Error = "The same network address was already added.";
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
            EnableDefaultButtons = false;
            if (previousStep == SelectIp && context.IsForward(CurrentStepIndex))
            {
                if (IpAddresses.Count == 0)
                {
                    Error = "Enter at least one network address";
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
                _storage.SetValue(KnownKeys.IpAddresses, IpAddresses);
                _targetsDiscoveryService.GetByIpAddresses(IpAddresses, new OperationContext { Success = success, UserState = this });
            }
            else if (previousStep == SelectPort && context.IsForward(CurrentStepIndex))
            {


                _storage.SetValue(KnownKeys.CheckedDirectoryItems, CheckedItems);
                _storage.SetValue(KnownKeys.TcpPort, _tcpPort);
                _storage.SetValue(KnownKeys.UdpPort, _udpPort);
                _storage.SetValue(KnownKeys.IpAddresses, IpAddresses);
                var ipAddressesString = IpAddresses.Select(elm => elm.FormattedText).ToArray();
                _storage.SetValue(KnownKeys.DevicesSourceDescription,
                    new DevicesSourceDescription()
                    {
                        Type = ETargetsDiscovererType.NetworkAddresses,
                        Description = DevicesSourceDescription.GetNetworkDescription(ipAddressesString)
                    });
                success();

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

        private void OnGetByIpAddressesCompleted(Object sender, GetByIpAddressEventArgs e)
        {
            OperationContext context = (OperationContext)e.UserState;
            if (context.UserState != this)
            {
                return;
            }

            EnableDefaultButtons = true;
            IsBusy = false;
            if (e.Exception == null)
            {
                CheckedItems = new Collection<Object>(e.Items.OrderBy(o => o.Name).ToArray());
                _storage.SetValue(KnownKeys.CheckedDirectoryItems, CheckedItems);
                _storage.SetValue(KnownKeys.TcpPort, _tcpPort);
                _storage.SetValue(KnownKeys.UdpPort, _udpPort);
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
