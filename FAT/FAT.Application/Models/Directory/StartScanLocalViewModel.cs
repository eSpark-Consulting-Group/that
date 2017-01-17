using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Telerik.Windows.Controls;
using FAT.Application.Services;
using APTAT.DeploymentCore.Directory;

using FAT.Application.Models.Apt;
using FAT.Application.Models.ModelBase;

using Composition = APTAT.Common.CompositionHelper;

namespace FAT.Application.Models
{
    //[Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class StartScanLocalViewModel : WizardViewModel
    {
        private readonly ITargetsDiscoveryService _targetsDiscoveryService;
        private readonly IFileBrowserService _fileBrowserService;
        private readonly IStorageService _storage;

        private ObservableCollection<FolderItem> _items;
        private readonly ICollection<Object> _checkedItems;
        private bool _scanCriticalAreas;


        public const String SelectTargets = "SelectTargets";
        public const String SelectPort = "SelectPort";

        public Boolean ScanCriticalAreas
        {
            get { return _scanCriticalAreas; }
            set
            {
                _scanCriticalAreas = value;
                RaisePropertyChanged("ScanCriticalAreas");
            }
        }

        public DelegateCommand ExpandCommand { get; private set; }

        public DelegateCommand CheckItemCommand { get; private set; }

        public DelegateCommand UncheckItemCommand { get; private set; }

        public StartScanLocalViewModel(ITargetsDiscoveryService targetsDiscoveryService, IStorageService storage, IFileBrowserService fileBrowserService)
        {
            _fileBrowserService = fileBrowserService;
            _targetsDiscoveryService = targetsDiscoveryService;
            _storage = storage;

            SetSteps(new[] { null, SelectPort, SelectTargets });

            ExpandCommand = new DelegateCommand(InvokeExpand);
            CheckItemCommand = new DelegateCommand(InvokeCheckItem);
            UncheckItemCommand = new DelegateCommand(InvokeUncheckItem);

            _checkedItems = new Collection<Object>();

        }

        public ObservableCollection<FolderItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        private void InvokeExpand(Object arg)
        {
            ExpandCommandContext commandContext = (ExpandCommandContext)arg;
            var parentFolder = (FolderItem) commandContext.DirectoryItem;
            
            _fileBrowserService.GetFolders(parentFolder, new OperationContext
            {
                UserState = this, Success = () => commandContext.RaiseCompleted()
            });
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

        public FolderItem[] GetCheckedFolderItems()
        {
            var items = new List<FolderItem>();
            var stack = new Stack<FolderItem>();
            foreach (var item in Items)
            {
                var node = item;
                while (true)
                {
                    var needCheckChildren = true;
                    if ( node.IsSelected.GetValueOrDefault())
                    {
                        items.Add(node);
                        needCheckChildren = false;
                    }
                    if (needCheckChildren)
                        foreach (var nodeChild in node.Items?? Enumerable.Empty<FolderItem>())
                        {
                            if (nodeChild.IsSelected.GetValueOrDefault())
                                items.Add(nodeChild);
                            else
                                stack.Push(nodeChild);
                        }
                    if (stack.Any())
                    {
                        node = stack.Pop();
                    }
                    else
                        break;
                }
            }
            return items.ToArray();
        }

        protected override void OnStepChanging(Action success, String previousStep, ChangeStepCommandContext context)
        {
            if (previousStep == SelectPort && context.IsForward(CurrentStepIndex))
            {
                Action action = () =>
                {
                    Error = null;

                    if (context.IsNext)
                    {
                        base.OnStepChanging(success, previousStep, context);
                    }
                    else
                    {
                        IsBusy = true;
                        SetSteps(new[] { null, SelectPort });
                        _targetsDiscoveryService.GetByIpAddresses(new List<IpAddressWrapper>
                        {
                            new IpAddressWrapper() {Text = "localhost", IsDnsName = true}
                        }, null, new OperationContext { Success = success, UserState = this });
                    }
                };

                action();


            }
            else if (previousStep == SelectTargets && context.IsForward(CurrentStepIndex))
            {
                if (_checkedItems == null || _checkedItems.Count == 0)
                {
                    Error = "Select at least one folder to scan";
                }
                else
                {
                    var items = GetCheckedFolderItems();
                    if (items.Length == 0)
                    {
                        Error = "Select at least one folder to scan";
                        _checkedItems.Clear();
                        return;
                    }
                    IsBusy = true;
                    _storage.SetValue(KnownKeys.CustomScanOptions, 
                        new CustomScanOptions(items, _scanCriticalAreas));
                    
                    _targetsDiscoveryService.GetByIpAddresses(new List<IpAddressWrapper>
                    {
                        new IpAddressWrapper() {Text = "localhost", IsDnsName = true}
                    }, null, new OperationContext { Success = success, UserState = this });
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

        }

        private bool _availableDrivesCollected;
        protected override void InvokeViewLoaded(object arg)
        {
            base.InvokeViewLoaded(arg);

            if (!_availableDrivesCollected)
            {
                _fileBrowserService.GetAvailableDrives(new OperationContext { UserState = this });
                _availableDrivesCollected = true;
            }

            _targetsDiscoveryService.GetByIpAddressesCompleted -= OnGetByIpAddressesCompleted;
            _targetsDiscoveryService.GetByIpAddressesCompleted += OnGetByIpAddressesCompleted;

            _fileBrowserService.GetAvailableDrivesCompleted -= OnGetAvailableDrivesCompleted;
            _fileBrowserService.GetAvailableDrivesCompleted += OnGetAvailableDrivesCompleted;
            _fileBrowserService.GetFoldersCompleted -= OnGetAvailableDrivesCompleted;
            _fileBrowserService.GetFoldersCompleted += OnGetAvailableDrivesCompleted;

        }

        private void OnGetAvailableDrivesCompleted(object sender, GetFoldersEventArgs e)
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
                    Items = new ObservableCollection<FolderItem>(e.Items);
                else
                    e.Parent.Items = new ObservableCollection<FolderItem>(e.Items);

                context.Success?.Invoke();
            }
            else
            {

                Error = e.Exception.Message;
                context.Success?.Invoke();
            }
        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _targetsDiscoveryService.GetByIpAddressesCompleted -= OnGetByIpAddressesCompleted;
            _fileBrowserService.GetAvailableDrivesCompleted -= OnGetAvailableDrivesCompleted;
            _fileBrowserService.GetFoldersCompleted -= OnGetAvailableDrivesCompleted;

            if (arg != null)
                _checkedItems.Clear();

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
                IsNextEnabled = false;
                IsFinishEnabled = false;
            }

            RaisePropertyChanged(nameof(NextName));
        }


        public override string NextName
        {
            get { return CurrentStepIndex == _steps.Length - 1 ? "Scan" : "Custom Scan"; }
        }

        public string ComputerName
        {
            get { return Environment.MachineName; }
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
                //_storage.SetValue(KnownKeys.Login, null);
                //_storage.SetValue(KnownKeys.Password, null);
                var main = (IMainViewModel) ParentViewModel;
                main.Login = null;
                main.Password = null;
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

            SetSteps(new[] { null, SelectPort, SelectTargets });

        }
    }
}
