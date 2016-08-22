
using System;
using System.ComponentModel;

namespace DDT.Application.Models
{

    public class DomainObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(String name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class DomainObject<T> : DomainObject
    {
        public virtual T BaseEntity
        {
            get;
            set;
        }
    }
}