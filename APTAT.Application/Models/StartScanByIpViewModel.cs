using APTAT.Application.Services;
using APTAT.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Telerik.Windows.Controls;

namespace APTAT.Application.Models
{
 

    public class StartScanByIpViewModel : WizardViewModel
    {
        public const String SelectIp = "SelectIp";
        public const String SelectTargets = "SelectTargets";

        private readonly IAptService _service = AptService.Instance;
        private readonly IStorageService _storage = new StorageService();

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

        public StartScanByIpViewModel()
        {
            SetSteps(new[] { null, SelectIp });
            IpAddresses = new ObservableCollection<IpAddressWrapper>();
            AddIpCommand = new DelegateCommand(InvokeAddIpCommand);
            DeleteIpCommand = new DelegateCommand(InvokeDeleteIpCommand);

            _storage.GetValue<DevicesSourceDescription>(KnownKeys.DevicesSourceDescription, true);
        }

        protected override void InvokeViewLoaded(object arg)
        {
            base.InvokeViewLoaded(arg);
            
            _administrator = _storage.GetValue<String>(KnownKeys.Login, false);
            RaisePropertyChanged("Administrator");

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

            _service.GetByIpAddressesCompleted += OnGetByIpAddressesCompleted;
        }


        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _service.GetByIpAddressesCompleted -= OnGetByIpAddressesCompleted;
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
                return;
            }
            
            if ( ValidateProperty(ipAddress, "Text", IpAddressWrapper.Validator ))
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
            EnableDefaultButtons = false;
            if (previousStep == SelectIp && context.IsForward(CurrentStepIndex))
            {
                if (IpAddresses.Count == 0)
                {
                    Error = "Enter at least one network address";
                    return;
                }

                if(String.IsNullOrEmpty(Administrator))
                {
                    Error = "Please Specify Administrator Login";
                    return;
                }

                if (String.IsNullOrEmpty(Password))
                {
                    Error = "Please Specify Password";
                    return;
                }


                IsBusy = true;
                _service.GetByIpAddresses(IpAddresses, new OperationContext { Success = success, UserState = this });
            }
            else if (previousStep == SelectTargets && context.IsForward(CurrentStepIndex))
            {
             
                if (CheckedItems == null || CheckedItems.Count == 0)
                {
                    Error = "Select at least one item";
                }
                else
                {
                    _storage.SetValue(KnownKeys.CheckedDirectoryItems, CheckedItems);
                    _storage.SetValue(KnownKeys.Login, _administrator);
                    _storage.SetValue(KnownKeys.Password, _password);
                    _storage.SetValue(KnownKeys.IpAddresses, IpAddresses);

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
                _storage.SetValue(KnownKeys.IpAddresses, IpAddresses);

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
                _storage.SetValue(KnownKeys.Login, _administrator);
                _storage.SetValue(KnownKeys.Password, _password);

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
