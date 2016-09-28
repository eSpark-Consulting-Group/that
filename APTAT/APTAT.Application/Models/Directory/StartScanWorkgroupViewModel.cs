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

namespace APTAT.Application.Models
{

    public class StartScanWorkgroupViewModel : WizardViewModel
    {
        public const String SelectWorkgroup = "SelectWorkgroup";
        public const String SelectTargets = "SelectTargets";
        public const String SelectPort = "SelectPort";

        ITargetsDiscoveryService _targetsDiscoveryService;
        IStorageService _storage;


        

        

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

        private String _workgroup;
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

        public DelegateCommand ExpandCommand { get; private set; }


        public DelegateCommand CheckItemCommand { get; private set; }

        public DelegateCommand UncheckItemCommand { get; private set; }

        private string _workgroupName;
        public string WorkgroupName
        {
            get { return EnumerateWorkgroups ? null : _workgroupName; }
            set
            {
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
                _enumerateWorkgroups = value;
                RaisePropertyChanged(nameof(EnumerateWorkgroups));
                RaisePropertyChanged(nameof(WorkgroupName));
            }
        }

        public string WorkgroupMask
        {
            get { return "Enter Workgroup Name"; }
        }

        public StartScanWorkgroupViewModel(ITargetsDiscoveryService targetsDiscoveryService, IStorageService storage)
        {
            _targetsDiscoveryService = targetsDiscoveryService;
            _storage = storage;

            SetSteps(new[] { null, SelectWorkgroup, SelectTargets, SelectPort });


        }


        protected override void InvokeViewLoaded(object arg)
        {
            base.InvokeViewLoaded(arg);

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
                if (String.IsNullOrEmpty(Workgroup) && !EnumerateWorkgroups)
                {
                    Error = "Please Specify Workgroup";
                    return;
                }



                IsBusy = true;


                if (EnumerateWorkgroups)
                {
                    var groups = await TryAction(GetWorkgroups());
                    Items = new ObservableCollection<DirectoryItem>(groups.Items);
                    IsBusy = false;
                    base.OnStepChanging(success, previousStep, context);
                }
                else
                {
                    //_storage.SetValue(KnownKeys.Workgroup, Workgroup);
                    _targetsDiscoveryService.GetWorkgroup(Workgroup, new OperationContext { Success = success, UserState = this });
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
                CheckedItems = new Collection<Object>(e.Items.OrderBy(o => o.Name).ToArray());
                _storage.SetValue(KnownKeys.CheckedDirectoryItems, CheckedItems);
               // _storage.SetValue(KnownKeys.Workgroup, _workgroup);
                _storage.SetValue(KnownKeys.DevicesSourceDescription,
                    new DevicesSourceDescription() { Type = ETargetsDiscovererType.Workgroup, Description = _workgroup });


                context.Success();
            }
            else
            {
                //  Items = new ObservableCollection<DeploymentItem>();
                Error = e.Exception.Message;
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
