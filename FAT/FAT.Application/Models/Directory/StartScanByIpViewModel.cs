using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

using FAT.Application.Models.ModelBase;
using FAT.Application.Services;

using APTAT.DeploymentCore.Directory;
using Telerik.Windows.Controls;
using FAT.Application.Views;

using Composition = APTAT.Common.CompositionHelper;

namespace FAT.Application.Models.Directory
{

    [Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class StartScanByIpViewModel : WizardViewModel
    {
        public const String SelectIp = "SelectIp";
        public const String SelectPort = "SelectPort";

        private readonly ITargetsDiscoveryService _targetsDiscoveryService;
        private readonly IStorageService _storage;



        public ICollection<Object> CheckedItems
        {
            get;
            set;
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



        public override WizardStepEnum Step
        {
            get { return WizardStepEnum.NetworkAddresses; }
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

            _validator = new IpAddressValidator();

            SetSteps(new[] { null, SelectIp, SelectPort });
            IpAddresses = new ObservableCollection<IpAddressWrapper>();
            AddIpCommand = new DelegateCommand(InvokeAddIpCommand);
            DeleteIpCommand = new DelegateCommand(InvokeDeleteIpCommand);
            DeleteAllIpCommand = new DelegateCommand(InvokeDeleteIpCommand);
        }

        private bool _useCurrentNameDetermined;
        protected override void InvokeViewLoaded(object arg)
        {
            base.InvokeViewLoaded(arg);

            if (!_useCurrentNameDetermined)
            {
                UseCurrentDomainForDeploy = HasCurrentDomainName;
                RaisePropertyChanged(nameof(UseCurrentCredentials));
                _useCurrentNameDetermined = true;
            }

            Password = null;

            GetTargetRanges();

            _targetsDiscoveryService.GetByIpAddressesCompleted -= OnGetByIpAddressesCompleted;
            _targetsDiscoveryService.GetByIpAddressesCompleted += OnGetByIpAddressesCompleted;
        }

        private async void GetTargetRanges()
        {
            var range = await _targetsDiscoveryService.GetTargetRanges();
            foreach (var tuple in range)
            {
                var ipAddress = new IpAddressWrapper();
                ipAddress.Text = string.Join("/", tuple.Item1, tuple.Item2);
                string validationMessage;
                if (_validator.Validate1(ipAddress, out validationMessage) &&
                   _validator.Validate2(ipAddress, out validationMessage))
                    IpAddresses.Add(ipAddress);
            }
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

        private IpAddressValidator _validator;

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

            string validationMessage;
            if (_validator.Validate1(ipAddress, out validationMessage) &&
               _validator.Validate2(ipAddress, out validationMessage))
            {
                if (IpAddresses.Any(o => o.FormattedText == ipAddress.FormattedText))
                {
                    Error = "The same network address or range was already added";
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
                Error = validationMessage;
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

                _targetsDiscoveryService.GetByIpAddresses(IpAddresses, CheckIfCanUseCurrentCredentials, new OperationContext { Success = success, UserState = this });

            }
            else if (previousStep == SelectPort && context.IsForward(CurrentStepIndex))
            {


                _storage.SetValue(KnownKeys.CheckedDirectoryItems, CheckedItems);

                var ipAddressesString = IpAddresses.Select(elm => elm.FormattedText).ToArray();
                _storage.SetValue(KnownKeys.DevicesSourceDescription,
                    new DevicesSourceDescription()
                    {
                        Type = ETargetsDiscovererType.NetworkAddresses,
                        Description = DevicesSourceDescription.GetNetworkDescription(ipAddressesString)
                    });
                var parent = ParentViewModel;
                if (parent != null)
                {
                    string errorMessage;

                    parent.Login = Login;
                    parent.Password = Password;

                    if ((UseCurrentDomainForDeploy || parent.ValidateCredentials(out errorMessage)))
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

                Error = e.Exception.Message;
            }

        }
    }
    }
