using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Forms;
using APTAT.Application.Services;

namespace APTAT.Application.Models
{
    public class MainViewViewModel : ViewModel
    {
        private readonly IAptService _atpService;
        private readonly IStorageService _storage;
        private DirectoryItem[] _selectedTargets;

        private Boolean _isScanInProgress;
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
        }

        private void OnDirectoryItemsResolved(Object sender, DirectoryItemsResolvedEventArgs e)
        {
            Items = new ObservableCollection<ThreatInfoItem>(e.Items);
            IsBusy = false;

            String login = _storage.GetValue<String>(KnownKeys.Login, true);
            String password = _storage.GetValue<String>(KnownKeys.Password, true);
            _atpService.BeginScan(e.Items.Where(item => item.IsOnline).ToArray(), login, password, this);
        }

        private void OnBeginScanCompleted(Object sender, BeginScanCompletedEventArgs e)
        {
            if(e.Exception != null)
            {
                MessageBox.Show(String.Format("Exception: {0}", e.Exception.Message));    
            }
            
        }
      
    }
}
