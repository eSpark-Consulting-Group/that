using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using APTAT.Application.Annotations;
using APTAT.Application.Services;
using Telerik.Windows.Data;
using ValidationContext = FluentValidation.ValidationContext;

namespace APTAT.Application.Models
{
    public class ValkyrieRegistrationModel : IDataErrorInfo, INotifyPropertyChanged
    {
        private string _licenseId;
        private string _userName;
        private string _password;
        private string _errorMessage;

        //    private readonly IAptService _service = AptService.Instance;
        public string LicenseId
        {
            get { return _licenseId; }
            set
            {
                _licenseId = value;
                OnPropertyChanged("LicenseId");
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        //public void ValidateModel()
        //{
        //  //  Error = null;

        //    if (IsEmpty)
        //    {
        //     //   Error = "Please specify CAM LicenseID or CAM Credentials";
        //    }

        //    if (String.IsNullOrWhiteSpace(LicenseId))
        //    {
        //        if (String.IsNullOrWhiteSpace(UserName))
        //        {

        //            //var Error = "Please specify UserName";

        //            return;
        //        }

        //        if (string.IsNullOrWhiteSpace(Password))
        //        {
        //            //Error = "Please specify Password";
        //            return;
        //        }
        //    }
        //}


        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrWhiteSpace(LicenseId) && String.IsNullOrWhiteSpace(UserName) &&
                       String.IsNullOrWhiteSpace(Password);
            }
        }

        public bool IsValidationMode { get; set; }
        public string this[string columnName]
        {
            get
            {
                if (!IsValidationMode) return String.Empty;

                string errorMessage = String.Empty;
                switch (columnName)
                {
                    case "UserName":
                        if (!ValidateUserName())
                            errorMessage = "UserName should not be empty";

                        break;

                    case "Password":
                        if (!ValidatePassword())
                            errorMessage = "Password should not be empty";

                        break;
                    case "LicenseId":
                        if (IsEmpty)
                            errorMessage = "CAM LicenseID or CAM Credentials are required";

                        break;
                }


                return errorMessage;
            }
        }

        #region ValidateDate
        public bool ValidatePassword()
        {
            if (!String.IsNullOrWhiteSpace(LicenseId)) return true;

            return !String.IsNullOrWhiteSpace(this.Password);
        }


        private bool ValidateUserName()
        {
            if (!String.IsNullOrWhiteSpace(LicenseId)) return true;
            return !String.IsNullOrWhiteSpace(this.UserName);
        }

        private bool FormValidation()
        {
            if (!String.IsNullOrWhiteSpace(LicenseId)) return true; 

            if (!String.IsNullOrWhiteSpace(this.UserName) && !String.IsNullOrWhiteSpace(this.Password)) return true;
            
            return false;
        }

        public bool ValidateModel()
        {
            IsValidationMode = true;
            OnPropertyChanged("UserName");
            OnPropertyChanged("Password");
            OnPropertyChanged("LicenseId");
            IsValidationMode = false;
            return FormValidation();
        }
        
        #endregion
        public string Error
        {
            get { return String.Empty; }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
