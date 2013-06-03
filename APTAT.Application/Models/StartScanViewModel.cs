using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Telerik.Windows.Controls;

namespace APTAT.Application.Models
{

    public class ExpandCommandContext
    {
        public Object DirectoryItem { get; set; }

        public Action<ExpandCommandContext> Completed { get; set; }

        public Object TreeItem { get; set; }

        public void RaiseCompleted()
        {
            if(Completed != null)
            {
                Completed(this);
            }
        }
    }

    public class ChangeStepCommandContext
    {
        public Int32 Step { get; set; }

        public Action<ChangeStepCommandContext> Completed { get; set; }

        public Boolean Cancel { get; set; }

        public void RaiseCompleted()
        {
            if(Completed != null)
            {
                Completed(this);
            }
        }
    }

    public class StartScanViewModel : ViewModel
    {
        private IAptService _service = new AptService();
        private IStorageService _storage = new StorageService();
        private Int32 _step;
        private DirectoryItem[] _items;
        private ICollection<Object> _chekedItems;
        private String _domainController;
        private String _administrator;
        private String _password;


        public DirectoryItem[] Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged("Items");
            }
        }

        public Int32 Step
        {
            get { return _step; }
        }

        public String DomainController
        {
            get { return _domainController; }
            set
            {
                _domainController = value;

                if(String.IsNullOrWhiteSpace(_domainController))
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

                if(String.IsNullOrWhiteSpace(_administrator))
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

        public DelegateCommand ChangeStepCommand { get; private set; }

        public DelegateCommand ExpandCommand { get; private set; }

        public DelegateCommand CheckItemsCommand { get; private set; }

        public StartScanViewModel()
        {
            ExpandCommand = new DelegateCommand(InvokeExpand);
            CheckItemsCommand = new DelegateCommand(InvokeCheckItems);
            ChangeStepCommand = new DelegateCommand(InvokeChangeStep);
        }

        private void InvokeChangeStep(Object arg)
        {
            Error = null;
            ChangeStepCommandContext commandContext = (ChangeStepCommandContext) arg;
            if(commandContext.Step == 0)
            {
                _step = commandContext.Step;
                commandContext.RaiseCompleted();
            }
            else if (commandContext.Step == 1)
            {
                IsBusy = true;
                _service.GetDirectoryRoot(DomainController, Administrator, Password, commandContext);
            }
            else if(commandContext.Step == 2)
            {
                if(_chekedItems == null || _chekedItems.Count == 0)
                {
                    Error = "Select at least one item";
                }
                else
                {
                    _storage.SetValue(KnownKeys.CheckedDirectoryItems, _chekedItems);
                    _step = commandContext.Step;
                    commandContext.RaiseCompleted();
                }
            }
        }

        private void InvokeExpand(Object arg)
        {
            ExpandCommandContext commandContext = (ExpandCommandContext) arg;
            _service.GetDirectoryItems(DomainController, Administrator, Password, (DirectoryItem)commandContext.DirectoryItem, commandContext);    
        }

        private void InvokeCheckItems(Object arg)
        {
            _chekedItems = (ICollection<Object>) arg;
            Error = null;
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

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
            ChangeStepCommandContext commandContext = (ChangeStepCommandContext)e.UserState;
            Items = e.Items;
            if(e.Error == null)
            {
                if (Items != null && Items.Length > 0)
                {
                    _service.GetDirectoryItems(DomainController, Administrator, Password, Items[0], this);
                }

                _step = commandContext.Step;   
                Error = null;
            }
            else
            {
                Error = e.Error;
            }
            commandContext.RaiseCompleted();
            
        }

        private void OnGetDirectoryItemsCompleted(Object sender, GetDirectoryItemsEventArgs e)
        {
            e.Parent.Items = new ObservableCollection<DirectoryItem>(e.Items);

            ExpandCommandContext context = e.UserState as ExpandCommandContext;
            if(context != null)
            {
                context.RaiseCompleted();    
            }
        }



      
    }
}
