using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using APTAT.Application.Services;
using Telerik.Windows.Controls;

namespace APTAT.Application.Models
{
    public class MainViewViewModel : ViewModel
    {
        private readonly IAptService _atpService;
        private readonly IStorageService _storage;
        private Guid _operationGuid;
        private DirectoryItem[] _selectedTargets;


        private Boolean _isScanInProgress;
        private ThreatInfoItem[] _onlineItems;
        private ThreatInfoItem _selectedItem;
        private ObservableCollection<ThreatInfoItem> _threatInfoItems;

        public Boolean IsScanInProgress
        {
            get { return _isScanInProgress; }
            set
            {
                _isScanInProgress = value;
                RaisePropertyChanged("IsScanInProgress");
            }
        }

        public Boolean IsDetailsEnabled
        {
            get
            {
                return SelectedItem != null &&
                       (SelectedItem.ScanStage == ScanStage.OperationCompleted ||
                        SelectedItem.ScanStage == ScanStage.OperationFailed);
            }
       
        }

        public Int32 ItemsCount
        {
            get
            {
               if(Items == null)
               {
                   return 0;
               }

                return Items.Count;
            }
        }

        public ThreatInfoItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");
                RaisePropertyChanged("IsDetailsEnabled");
            }
        }

        public ObservableCollection<ThreatInfoItem> Items
        {
            get { return _threatInfoItems; }
            set
            {
                _threatInfoItems = value;
                RaisePropertyChanged("Items");
                RaisePropertyChanged("ItemsCount");
            }
        }

        
        
        public MainViewViewModel()
        {
            _atpService = new AptService();
            _storage = new StorageService();
        }

        protected override void InvokeViewLoaded(Object arg)
        {
            base.InvokeViewLoaded(arg);

            _atpService.DirectoryItemsResolved += OnDirectoryItemsResolved;
            _atpService.BeginScanCompleted += OnBeginScanCompleted;
            _atpService.ScanStatusChanged += OnScanStatusChanged;

            ICollection<Object> value = _storage.GetValue<ICollection<Object>>(KnownKeys.CheckedDirectoryItems, true);
            if(value != null)
            {
                _selectedTargets = value.OfType<DirectoryItem>().ToArray();
                if (_selectedTargets.Length > 0)
                {
                    IsScanInProgress = true;
                    IsBusy = true;
                    _atpService.ResolveDirectoryItems(_selectedTargets, this);
                }    
            }
        }


        protected override void InvokeViewUnloaded(Object arg)
        {
            base.InvokeViewUnloaded(arg);

            _atpService.DirectoryItemsResolved -= OnDirectoryItemsResolved;
            _atpService.BeginScanCompleted -= OnBeginScanCompleted;
            _atpService.ScanStatusChanged -= OnScanStatusChanged;
        }

        private void OnDirectoryItemsResolved(Object sender, DirectoryItemsResolvedEventArgs e)
        {
            Items = new ObservableCollection<ThreatInfoItem>(e.Items);
            IsBusy = false;

            String login = _storage.GetValue<String>(KnownKeys.Login, false);
            String password = _storage.GetValue<String>(KnownKeys.Password, true);

            _onlineItems = e.Items.Where(item => item.IsOnline).ToArray();

            foreach (ThreatInfoItem item in Items)
            {
                if(item.IsOnline == false)
                {
                    item.StatusMessage = null;
                }
            }

            _atpService.BeginScan(_onlineItems, login, password, this);
        }

        private void OnBeginScanCompleted(Object sender, BeginScanCompletedEventArgs e)
        {
            if(e.Exception != null)
            {
              //  MessageBox.Show(String.Format("Exception: {0}", e.Exception.Message));    
            }
            else
            {
                _operationGuid = e.OperationGuid;
            }
        }

        private void ScanCompleted()
        {
            IsScanInProgress = false;
        }

        private void OnScanStatusChanged(Object sender, ScanStatusChangedEventArgs e)
        {
            for(Int32 i = 0; i < e.ScanProgress.Length; ++i)
            {
               ScanProgressItem scanProgress = e.ScanProgress[i];
               if (scanProgress.OperationUid != _operationGuid)
               {
                   continue;
               }
               for (Int32 j = 0; j < _onlineItems.Length; ++j)
               {
                   ThreatInfoItem infoItem = Items[j];
                   if(infoItem.ComputerGuid == scanProgress.ComputerGuid)
                   {
                       infoItem.TotalFilesCount = scanProgress.TotalFilesCount;
                       infoItem.UnknownFilesCount = scanProgress.UnknownFilesCount;
                       infoItem.StatusMessage = scanProgress.StatusMessage;
                       infoItem.ScanStage = scanProgress.ScanStage;
                       break;
                   }
               }
            }

            Int32 completed = 0;
            for (Int32 j = 0; j < _onlineItems.Length; ++j)
            {
                ThreatInfoItem infoItem = _onlineItems[j];
                if(infoItem.ScanStage == ScanStage.OperationFailed || infoItem.ScanStage == ScanStage.OperationCompleted)
                {
                    completed++;
                }
            }

            if (completed == _onlineItems.Length)
            {
                ScanCompleted();
            }
        }
      
    }
}
