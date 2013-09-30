using APTAT.Application.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Telerik.Windows.Controls;

namespace APTAT.Application.Models
{
    public class StartScanDomainViewModel : WizardViewModel
    {
        private readonly IAptService _service = AptService.Instance;
        private readonly IStorageService _storage = new StorageService();
        private Int32 _step;
        private DirectoryItem[] _items;
        private readonly ICollection<Object> _checkedItems;
        private String _domainController;
        private String _administrator;
        private String _password;

      
        public const String SelectDomain = "SelectDomain";
        public const String SelectTargets = "SelectTargets";

        public DirectoryItem[] Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public String DomainController
        {
            get { return _domainController; }
            set
            {
                _domainController = value;

                if (String.IsNullOrWhiteSpace(_domainController))
                {
                    RaiseValidationError("DomainController", "Please Specify Domain Controller");
                }
                else
                {
                    ClearError("DomainController");
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
            set { _password = value; }
        }

        public DelegateCommand ExpandCommand { get; private set; }

        public DelegateCommand CheckItemCommand { get; private set; }

        public DelegateCommand UncheckItemCommand { get; private set; }

        public StartScanDomainViewModel()
        {
            SetSteps(new[] { null, SelectDomain, SelectTargets });

            ExpandCommand = new DelegateCommand(InvokeExpand);
            CheckItemCommand = new DelegateCommand(InvokeCheckItem);
            UncheckItemCommand = new DelegateCommand(InvokeUncheckItem);

            _checkedItems = new Collection<Object>();
        }

        private void InvokeExpand(Object arg)
        {
            ExpandCommandContext commandContext = (ExpandCommandContext)arg;
            _service.GetDirectoryItems(DomainController, Administrator, Password, (DirectoryItem)commandContext.DirectoryItem, commandContext);
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


            if (previousStep == SelectDomain && context.IsForward(CurrentStepIndex))
            {
                try
                {
                    DomainController = DomainController;
                    Administrator = Administrator;
                    Password = Password;
                }
                catch (ValidationException e)
                {
                    return;
                }
               

                IsBusy = true;
                _service.GetDirectoryRoot(DomainController, Administrator, Password, success);
            }
            else if (previousStep == SelectTargets && context.IsForward(CurrentStepIndex))
            {
                if (_checkedItems == null || _checkedItems.Count == 0)
                {
                    Error = "Select at least one item";
                }
                else
                {
                    _storage.SetValue(KnownKeys.CheckedDirectoryItems, _checkedItems);
                    _storage.SetValue(KnownKeys.Login, _administrator);
                    _storage.SetValue(KnownKeys.Password, _password);
                    _storage.SetValue(KnownKeys.Domain, _domainController);
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
            
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            _administrator = _storage.GetValue<String>(KnownKeys.Login, false);
            if (_administrator != null)
            {
                RaisePropertyChanged("Administrator");
            }
            _domainController = _storage.GetValue<String>(KnownKeys.Domain, false);
            if (_domainController != null)
            {
                RaisePropertyChanged("DomainController");
            }

            _service.GetDirectoryItemsCompleted += OnGetDirectoryItemsCompleted;
            _service.GetDirectoryRootCompleted += OnGetDirectoryRootCompleted;
        }

        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _service.GetDirectoryItemsCompleted -= OnGetDirectoryItemsCompleted;
            _service.GetDirectoryRootCompleted -= OnGetDirectoryRootCompleted;
        }


        private void OnGetDirectoryRootCompleted(Object sender, GetDirectoryItemsEventArgs e)
        {
            IsBusy = false;
            Action success = (Action)e.UserState;
            Items = e.Items;
            if (e.Exception == null)
            {
                if (Items != null && Items.Length > 0)
                {
                    _service.GetDirectoryItems(DomainController, Administrator, Password, Items[0], this);
                }

               // _step = commandContext.Step;
                Error = null;
                success();
            }
            else
            {
                Error = e.Exception.Message;
            }
        }

        private void OnGetDirectoryItemsCompleted(Object sender, GetDirectoryItemsEventArgs e)
        {
            if (e.Exception == null)
            {
                e.Parent.Items = new ObservableCollection<DirectoryItem>(e.Items.OrderBy(o => o.Name));
            }
            else
            {
                e.Parent.Items = new ObservableCollection<DirectoryItem>();
                Error = e.Exception.Message;
            }

            ExpandCommandContext context = e.UserState as ExpandCommandContext;
            if (context != null)
            {
                context.RaiseCompleted();
            }

        }

    }
}
