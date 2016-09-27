using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using APTAT.Application.Extensions;
using Telerik.Windows.Controls;
using APTAT.Application.Services;
using APTAT.DeploymentCore.Directory;
using APTAT.Application.Messages;

namespace APTAT.Application.Models
{
    public class StartScanLocalViewModel : WizardViewModel
    {
        ITargetsDiscoveryService _targetsDiscoveryService;
        IStorageService _storage;

        private DirectoryItem[] _items;
        private readonly ICollection<Object> _checkedItems;

   

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


        public DelegateCommand ExpandCommand { get; private set; }

        public DelegateCommand CheckItemCommand { get; private set; }

        public DelegateCommand UncheckItemCommand { get; private set; }

        public StartScanLocalViewModel(ITargetsDiscoveryService targetsDiscoveryService, IStorageService storage)
        {
            _targetsDiscoveryService = targetsDiscoveryService;
            _storage = storage;

            SetSteps(new[] { null, SelectPort, SelectTargets });

            ExpandCommand = new DelegateCommand(InvokeExpand);
            CheckItemCommand = new DelegateCommand(InvokeCheckItem);
            UncheckItemCommand = new DelegateCommand(InvokeUncheckItem);

            _checkedItems = new Collection<Object>();

        }



        private void InvokeExpand(Object arg)
        {
            ExpandCommandContext commandContext = (ExpandCommandContext)arg;
            
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
            if (previousStep == SelectPort && context.IsForward(CurrentStepIndex))
            {

                if (!context.IsNext)
                {
                    IsBusy = true;
                    SetSteps(new[] { null, SelectPort });
                    _targetsDiscoveryService.GetByIpAddresses(new List<IpAddressWrapper>
                    {
                        new IpAddressWrapper() {Text = "localhost", IsDnsName = true}
                    }, new OperationContext { Success = success, UserState = this });
                }
            }
            else if (previousStep == SelectTargets && context.IsForward(CurrentStepIndex))
            {
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

        protected override void OnStepChanged()
        {

        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);
            _targetsDiscoveryService.GetByIpAddressesCompleted -= OnGetByIpAddressesCompleted;
            _targetsDiscoveryService.GetByIpAddressesCompleted += OnGetByIpAddressesCompleted;


        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);
            _targetsDiscoveryService.GetByIpAddressesCompleted -= OnGetByIpAddressesCompleted;

        }

        protected override void UpdateState()
        {
            if (CurrentStepIndex == 0 && CurrentStepIndex == _steps.Length - 1)
            {
                IsBackEnabled = false;
                IsNextEnabled = false;
                IsFinishEnabled = true;
            }
            else if (CurrentStepIndex == 0)
            {
                IsBackEnabled = false;
                IsNextEnabled = false;
                IsFinishEnabled = true;
            }
            else if (CurrentStepIndex == _steps.Length - 1)
            {
                IsBackEnabled = true;
                IsNextEnabled = true;
                IsFinishEnabled = false;
            }
            else
            {
                IsBackEnabled = true;
                IsNextEnabled = true;
                IsFinishEnabled = true;
            }

            RaisePropertyChanged(nameof(NextName));
        }


        public override string NextName
        {
            get { return CurrentStepIndex == _steps.Length - 1 ? "Scan" : "Custom Scan"; }
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
                var items = new Collection<Object>(e.Items.OrderBy(o => o.Name).ToArray());
                _storage.SetValue(KnownKeys.CheckedDirectoryItems, items);
                _storage.SetValue(KnownKeys.Login, null);
                _storage.SetValue(KnownKeys.Password, null);
                _storage.SetValue(KnownKeys.DevicesSourceDescription,
                new DevicesSourceDescription()
                {
                    Type = ETargetsDiscovererType.NetworkAddresses,
                    Description = DevicesSourceDescription.GetNetworkDescription(new[] { "localhost" })
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
