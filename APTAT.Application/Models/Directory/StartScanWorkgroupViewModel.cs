using APTAT.Application.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Telerik.Windows.Controls;
using APTAT.Application.Extensions;
using APTAT.Core.Descent;


namespace APTAT.Application.Models
{

    public class StartScanWorkgroupViewModel : WizardViewModel
    {
        public const String SelectWorkgroup = "SelectWorkgroup";

        ITargetsDiscoveryService _targetsDiscoveryService;
        IStorageService _storage;


        private String _workgroup;
        private String _administrator;
        private String _password;

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

        public String Workgroup
        {
            get { return _workgroup; }
            set
            {
                _workgroup = value;

                if (String.IsNullOrWhiteSpace(_workgroup))
                {
                    RaiseValidationError("Workgroup", "Please Specify Workgroup");
                }
                else
                {
                    ClearError("Workgroup");
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

        public DelegateCommand CheckItemCommand { get; private set; }

        public DelegateCommand UncheckItemCommand { get; private set; }

        public StartScanWorkgroupViewModel(ITargetsDiscoveryService targetsDiscoveryService, IStorageService storage)
        {
            _targetsDiscoveryService = targetsDiscoveryService;
            _storage = storage;
            
            SetSteps(new[] { null, SelectWorkgroup });
            
            TcpPort = "80";
            UdpPort = "53";
        }

        private string _tcpPort = String.Empty;
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
        protected override void InvokeViewLoaded(object arg)
        {
            base.InvokeViewLoaded(arg);
            _administrator = _storage.GetValue<String>(KnownKeys.Login, false);
            //if (_administrator != null)
            {
                RaisePropertyChanged("Administrator");
            }
            _workgroup = _storage.GetValue<String>(KnownKeys.Workgroup, false);
            //if (_workgroup != null)
            {
                RaisePropertyChanged("Workgroup");
            }

            _targetsDiscoveryService.GetWorkgroupCompleted += OnGetWorkgroupCompleted;
           
        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _targetsDiscoveryService.GetWorkgroupCompleted -= OnGetWorkgroupCompleted;
        }

        protected override void OnStepChanging(Action success, String previousStep, ChangeStepCommandContext context)
        {
            if (previousStep == SelectWorkgroup && context.IsForward(CurrentStepIndex))
            {
                if (String.IsNullOrEmpty(Workgroup))
                {
                    Error = "Please Specify Workgroup";
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
                _storage.SetValue(KnownKeys.Workgroup, Workgroup);
                _targetsDiscoveryService.GetWorkgroup(Workgroup, new OperationContext { Success = success, UserState = this });
            }
            else
            {
                if (context.IsBackward(CurrentStepIndex))
                {
                    Error = null;
                }

                _storage.SetValue(KnownKeys.Login, _administrator);
                _storage.SetValue(KnownKeys.Workgroup, _workgroup);

                base.OnStepChanging(success, previousStep, context);
            }
        }

        protected override void OnStepChanged()
        {

        }

        private void OnGetWorkgroupCompleted(Object sender, GetWorkgroupEventArgs e)
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
                _storage.SetValue(KnownKeys.Login, _administrator);
                _storage.SetValue(KnownKeys.Password, _password);
                _storage.SetValue(KnownKeys.Workgroup, _workgroup);
                _storage.SetValue(KnownKeys.DevicesSourceDescription,
                    new DevicesSourceDescription() { Type = ETargetsDiscovererType.Workgroup, Description = _workgroup });              
                _storage.SetValue(KnownKeys.TcpPort, _tcpPort);
                _storage.SetValue(KnownKeys.UdpPort, _udpPort);

                context.Success();
            }
            else
            {
                //  Items = new ObservableCollection<DeploymentItem>();
                Error = e.Exception.Message;
            }


        }
    }
}
