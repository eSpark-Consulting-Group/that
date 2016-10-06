using APTAT.Application.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Telerik.Windows.Controls;
using APTAT.Application.Extensions;
using APTAT.Core.Descent;
using APTAT.DeploymentCore.Directory;
using System.Threading.Tasks;
using APTAT.Common;
using System.Diagnostics;
using APTAT.Application.Models.ModelBase;

namespace APTAT.Application.Models
{

    public class StartScanWorkgroupViewModel : WizardViewModel
    {
        public const String SelectWorkgroup = "SelectWorkgroup";
        public const String SelectTargets = "SelectTargets";
        public const String SelectPort = "SelectPort";

        ITargetsDiscoveryService _targetsDiscoveryService;
        IStorageService _storage;





        private readonly ICollection<Object> _checkedItems;
        public ICollection<Object> CheckedItems
        {
            get;
            set;
        }

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

            //_workgroup = _storage.GetValue<String>(KnownKeys.Workgroup, false);
            //RaisePropertyChanged("Workgroup");

            _targetsDiscoveryService.GetWorkgroupCompleted -= OnGetWorkgroupCompleted;
            _targetsDiscoveryService.GetWorkgroupCompleted += OnGetWorkgroupCompleted;

        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _targetsDiscoveryService.GetWorkgroupCompleted -= OnGetWorkgroupCompleted;
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

        protected override async void OnStepChanging(Action success, String previousStep, ChangeStepCommandContext context)
        {
            if (previousStep == SelectWorkgroup && context.IsForward(CurrentStepIndex))
            {
                if (String.IsNullOrEmpty(WorkgroupName) && !EnumerateWorkgroups)
                {
                    Error = "Please Specify Workgroup";
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
                            Error = "No Workgroups Found";
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
                    var selectedItems = Items.GetSelectedItemsWithItemsPriority().OfType<DirectoryItem>().ToArray();
                    CheckIfCanUseCurrentCredentials(selectedItems);
                    _storage.SetValue(KnownKeys.CheckedDirectoryItems, selectedItems);
                   
                    _storage.SetValue(KnownKeys.DevicesSourceDescription,
                        new DevicesSourceDescription() { Type = ETargetsDiscovererType.Workgroup, Description = WorkgroupName });
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

                //_storage.SetValue(KnownKeys.Workgroup, _workgroup);

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
            bool onlyLocal = true;
            if (selectedItems.Length > 1) return false;
            foreach (var address in selectedItems)
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

                // _storage.SetValue(KnownKeys.Workgroup, _workgroup);

                Error = null;
                context.Success();
            }
            else
            {
                //  Items = new ObservableCollection<DeploymentItem>();
                Error = "No Workgroups Found";
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
