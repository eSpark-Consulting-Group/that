using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace APTAT.Application.Models
{
    public class MainViewViewModel : ViewModel
    {
        private IAptService _atpService;
        private IStorageService _storage;
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

            ICollection<Object> value = _storage.GetValue<ICollection<Object>>(KnownKeys.CheckedDirectoryItems);
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
        }

        private void OnDirectoryItemsResolved(Object sender, DirectoryItemsResolvedEventArgs e)
        {
            Items = new ObservableCollection<ThreatInfoItem>(e.Items);
            

            IsBusy = false;
        }

      
    }
}
