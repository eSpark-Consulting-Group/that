
using System;
using Telerik.Windows.Controls;

namespace DDT.Application.Models
{
    public class SelectableItemWrapper<T> : DomainObject
    {
        protected bool? _isSelected;
        public virtual bool? IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RaisePropertyChanged(nameof(IsSelected));
                SelectionChangedAction?.Invoke();
            }
        }

        protected T _value;
        public virtual T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                RaisePropertyChanged(nameof(Value));
                ValueChangedAction?.Invoke();
            }
        }

        public Action SelectionChangedAction { get; set; }

        public Action ValueChangedAction { get; set; }

        public DelegateCommand RemoveCommand { get; set; }
    }
}
