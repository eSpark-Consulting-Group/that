using System;
using Telerik.Windows.Controls;

namespace APTAT.Application.Models
{
    public class ViewModel : DomainObject
    {
        private String _keyword;
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

        public String Keyword
        {
            get { return _keyword; }
            set
            {
                String oldValue = _keyword;
                _keyword = value;
                RaisePropertyChanged("Keyword");
                OnKeywordChanged(oldValue, _keyword);
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

        protected virtual void OnKeywordChanged(String oldValue, String newValue)
        {
            
        }

    

    }
}
