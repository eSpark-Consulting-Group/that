using System;
using Telerik.Windows.Controls;

namespace APTAT.Application.Models
{
    public class ViewModel : DomainObject
    {
        private Boolean _isBusy;
   
        public Boolean IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                RaisePropertyChanged("IsBusy");
            }
        }

        public DelegateCommand ViewLoadedCommand { get; private set; }

        public DelegateCommand ViewUnloadedCommand { get; private set; }

        public ViewModel()
        {
            ViewLoadedCommand = new DelegateCommand(InvokeViewLoaded);
            ViewUnloadedCommand = new DelegateCommand(InvokeViewUnloaded);
        }

        protected virtual void InvokeViewLoaded(Object arg)
        {
            
        }

        protected virtual void InvokeViewUnloaded(Object arg)
        {
            
        }

    

    }
}
