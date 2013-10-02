
using FluentValidation.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace APTAT.Application.Models
{
    public class Error
    {
        public String Message { get; set; }

        public String Property { get; set; }
    }

    public class DomainObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Int64 Id { get; set; }
        private Boolean _hasErrors;
        private List<Error> _errors;

        private String _error;

        public Boolean HasErrors
        {
            get { return _hasErrors; }
            set
            {
                _hasErrors = value;
                RaisePropertyChanged("HasErrors");
            }
        }

        public String Error
        {
            get { return _error; }
            set
            {
                _error = value;
                RaisePropertyChanged("Error");
               if(String.IsNullOrWhiteSpace(_error) == false)
               {
                   HasErrors = true;
               }
               else
               {
                   HasErrors = false;
               }
            }
        }

        protected virtual void RaisePropertyChanged(String name)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }


        protected void RaiseValidationError(String propertyName, String errorMessage)
        {
            if (_errors == null)
            {
                _errors = new List<Error>();
            }

            _errors.Add(new Error { Message = errorMessage, Property = propertyName });

            if(Error == null)
            {
                Error = _errors.First().Message;
            }

            throw new ValidationException(errorMessage);
        }

        protected void ClearError(String propertyName)
        {

            if(_errors != null)
            {
                for (Int32 i = _errors.Count - 1; i >= 0; --i)
                {
                    Error e = _errors[i];
                    if (e.Property == propertyName)
                    {
                        _errors.Remove(e);
                    }
                }

                Error err = _errors.FirstOrDefault();
                if (err != null)
                {
                    Error = err.Message;
                }
                else
                {
                    Error = null;
                } 
            }
            else
            {
                Error = null;
            }
            
        }

        protected void ValidatePropertyThrow(String propertyName, FluentValidation.IValidator validator)
        {
            FluentValidation.Results.ValidationResult result = validator.Validate(new FluentValidation.ValidationContext(this, new PropertyChain(), new MemberNameValidatorSelector(new[] { propertyName })));
            if (result.IsValid == false)
            {
                throw new ValidationException(result.Errors[0].ErrorMessage);
            }

                
        }

        protected Boolean ValidateProperty(Object instance, String propertyName, FluentValidation.IValidator validator)
        {
            FluentValidation.Results.ValidationResult result = validator.Validate(new FluentValidation.ValidationContext(instance, new PropertyChain(), new MemberNameValidatorSelector(new[] { propertyName })));
            if (result.IsValid == false)
            {
                Error = result.Errors[0].ErrorMessage;
                return false;
            }

            return true;
        }

  
    }
}
