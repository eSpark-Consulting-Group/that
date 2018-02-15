using UFH.Application.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Telerik.Windows.Controls;
using UFH.Application.Extensions;

using APTAT.DeploymentCore.Directory;
using System.Threading.Tasks;
using APTAT.Common;
using System.Diagnostics;
using UFH.Application.Models.ModelBase;

using Composition = APTAT.Common.CompositionHelper;

namespace UFH.Application.Models
{
    [Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class StartScanWorkgroupViewModel : WizardViewModel
    {
        public const String SelectWorkgroup = "SelectWorkgroup";
        public const String SelectTargets = "SelectTargets";
        public const String SelectPort = "SelectPort";

        ITargetsDiscoveryService _targetsDiscoveryService;
        IStorageService _storage;


        private readonly ICollection<Object> _checkedItems;


        private ObservableCollection<DirectoryItem> _items;
        public ObservableCollection<DirectoryItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public override WizardStepEnum Step
        {
            get { return WizardStepEnum.Workgroup; }
        }

        public DelegateCommand ExpandCommand { get; private set; }


        public DelegateCommand CheckItemCommand { get; private set; }

        public DelegateCommand UncheckItemCommand { get; private set; }

        private bool _workgroupCredentialsChanged = true;

        private string _workgroupName;
        public string WorkgroupName
        {
            get { return EnumerateWorkgroups ? null : _workgroupName; }
            set
            {
                if (_workgroupName != value)
                    _workgroupCredentialsChanged = true;

                _workgroupName = value;
                RaisePropertyChanged(nameof(WorkgroupName));
            }
        }

        private bool _enumerateWorkgroups = true;
        public bool EnumerateWorkgroups
        {
            get { return _enumerateWorkgroups; }
            set
            {
                if (_enumerateWorkgroups != value)
                    _workgroupCredentialsChanged = true;

                _enumerateWorkgroups = value;
                RaisePropertyChanged(nameof(EnumerateWorkgroups));
                RaisePropertyChanged(nameof(WorkgroupName));
            }
        }



        public StartScanWorkgroupViewModel(ITargetsDiscoveryService targetsDiscoveryService, IStorageService storage)
        {
            _targetsDiscoveryService = targetsDiscoveryService;
            _storage = storage;

            SetSteps(new[] { null, SelectWorkgroup, SelectTargets, SelectPort });

            ExpandCommand = new DelegateCommand(InvokeExpand);
            CheckItemCommand = new DelegateCommand(InvokeCheckItem);
            UncheckItemCommand = new DelegateCommand(InvokeUncheckItem);

            _checkedItems = new Collection<Object>();
        }

        private void InvokeExpand(Object arg)
        {
            ExpandCommandContext commandContext = (ExpandCommandContext)arg;
            var item = (DirectoryItem)commandContext.DirectoryItem;
            if (item != null && item.Items != null && !item.Items.Any())
                _targetsDiscoveryService.GetWorkgroup(item.Name, item, new OperationContext
                { UserState = this, Success = () => commandContext.RaiseCompleted() });
            else
                commandContext.RaiseCompleted();
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

        private bool _useCurrentNameDetermined;
        protected override void InvokeViewLoaded(object arg)
        {
            base.InvokeViewLoaded(arg);

            if (!_useCurrentNameDetermined)
            {

                UseCurrentDomainForDeploy = HasCurrentDomainName;
                RaisePropertyChanged(nameof(UseCurrentCredentials));
                _useCurrentNameDetermined = true;

                GetCurrentWorkgroupName();
            }

            Password = null;
            _workgroupCredentialsChanged = true;

            _targetsDiscoveryService.GetWorkgroupCompleted -= OnGetWorkgroupCompleted;
            _targetsDiscoveryService.GetWorkgroupCompleted += OnGetWorkgroupCompleted;

        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _targetsDiscoveryService.GetWorkgroupCompleted -= OnGetWorkgroupCompleted;

            if (arg != null)
                _checkedItems.Clear();
        }

        private async Task<GetDirectoryItemsEventArgs> GetWorkgroups()
        {
            var result = await _targetsDiscoveryService.GetWorkgroups(this);

            if (result.Exception != null)
            {
                return null;
            }

            return result;
        }

        private void GetCurrentWorkgroupName()
        {
            WorkgroupName = _targetsDiscoveryService.GetCurrentWorkgroupName();
        }

        protected override async void OnStepChanging(Action success, String previousStep, ChangeStepCommandContext context)
        {
            if (previousStep == SelectWorkgroup && context.IsForward(CurrentStepIndex))
            {
                Error = null;

                if (String.IsNullOrEmpty(WorkgroupName) && !EnumerateWorkgroups)
                {
                    Error = "Please specify Workgroup";
                    return;
                }

                if (_workgroupCredentialsChanged)
                {
                    IsBusy = true;
                    _checkedItems.Clear();
                    if (EnumerateWorkgroups)
                    {
                        var groups = await TryAction(GetWorkgroups());
                        Items = new ObservableCollection<DirectoryItem>(groups.Items);
                        IsBusy = false;

                        if (Items.Any())
                        {
                            base.OnStepChanging(success, previousStep, context);
                            _workgroupCredentialsChanged = false;
                        }
                        else
                            Error = "No Workgroups found";
                    }
                    else
                    {
                        _targetsDiscoveryService.GetWorkgroup(WorkgroupName, null, new OperationContext { Success = success, UserState = this });
                        _workgroupCredentialsChanged = false;
                    }

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
                    var items = Items.GetSelectedItemsWithItemsPriority().OfType<DirectoryItem>().ToArray();
                    if (items.Length == 0)
                    {
                        Error = "Please select at least one item";
                        _checkedItems.Clear();
                        return;
                    }

                    CheckIfCanUseCurrentCredentials(items);
                    _storage.SetValue(KnownKeys.CheckedDirectoryItems, items);

                    var workGroups = DevicesSourceDescription.GetNetworkDescription(
                        items.GroupBy(i => i.LdapPath).Select(i => i.Key).Distinct().ToArray());
                    _storage.SetValue(KnownKeys.DevicesSourceDescription,

                            new DevicesSourceDescription() { Type = ETargetsDiscovererType.Workgroup, Description = workGroups });
                    success();
                }
            }
            else if (previousStep == SelectPort && context.IsForward(CurrentStepIndex))
            {
                var parent = ParentViewModel as IMainViewModel;
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

        protected override void OnStepChanged()
        {
            Error = null;
        }

        private void CheckIfCanUseCurrentCredentials(DirectoryItem[] selectedItems)
        {
            UseCurrentCredentials = IsOnlyLocalComputer(selectedItems);
        }

        private bool IsOnlyLocalComputer(DirectoryItem[] selectedItems)
        {
            if (selectedItems.Length > 1) return false;
            foreach (var address in selectedItems)
            {
                if (address.Type != DirectoryItemType.Computer || !address.IsLocalComputer)
                    return false;
            }

            return true;
        }

        private void OnGetWorkgroupCompleted(Object sender, GetDirectoryItemsEventArgs e)
        {
            OperationContext context = (OperationContext)e.UserState;
            if (context.UserState != this)
            {
                return;
            }

            IsBusy = false;
            if (e.Exception == null)
            {
                if (e.Parent == null)
                    Items = new ObservableCollection<DirectoryItem>(e.Items.OrderBy(o => o.Name));
                else
                    e.Parent.Items = new ObservableCollection<DirectoryItem>(e.Items.OrderBy(o => o.Name));

                Error = null;
                context.Success();
            }
            else
            {

                Error = "No Workgroups found";
                _workgroupCredentialsChanged = true;
            }


        }

        private async Task<T> TryAction<T>(Task<T> task) where T : class
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


            LogHelper.Trace(TraceEventType.Error, $"TryAction error: {ex.GetType()} {ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }
    }
}
