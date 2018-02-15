using System;
using System.Linq.Expressions;
using System.Reflection;
using Telerik.Windows.Controls;

namespace UFH.Application.Models
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
                if (string.IsNullOrEmpty(_keyword))
                    _keyword = null;
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

        protected bool Set<T>(Expression<Func<T>> propertyExpression, T newValue)
        {
            var inputBody = (MemberExpression)propertyExpression.Body;
            var parameterExpression = Expression.Parameter(typeof(T));

            MemberExpression memberExpression = null;
            if (inputBody.Member is PropertyInfo)
                memberExpression = Expression.Property(inputBody.Expression, (PropertyInfo)inputBody.Member);
            else if (inputBody.Member is FieldInfo)
                memberExpression = Expression.Field(inputBody.Expression, (FieldInfo)inputBody.Member);

            if (memberExpression == null)
                return false;

            var setExpr = Expression.Assign(memberExpression, parameterExpression);
            var result = Expression.Lambda<Action<T>>(setExpr, parameterExpression);
            result.Compile()(newValue);

            var propertyName = ((MemberExpression)propertyExpression.Body).Member.Name;
            RaisePropertyChanged(propertyName);

            return true;
        }
    }
}
