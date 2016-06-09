using System;
using System.ComponentModel;

using Composition = APTAT.Application.Helpers.CompositionHelper;

namespace APTAT.Application.Models
{
    [Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class ValkyrieRegistrationModel : ViewModel
    {
        private string _licenseId;
        private string _userName;
        private string _password;
        private string _errorMessage;

        public string LicenseId
        {
            get { return _licenseId; }
            set
            {
                _licenseId = value;
                RaisePropertyChanged("LicenseId");
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                RaisePropertyChanged("ErrorMessage");
            }
        }

        public string this[string columnName]
        {
            get
            {
                string errorMessage = null;

                switch (columnName)
                {
                    case "UserName":
                        if (!ValidateUserName())
                            errorMessage = "User Name is required.";
                        break;

                    case "Password":
                        if (!ValidatePassword())
                            errorMessage = "Password is required.";
                        break;
                    case "LicenseId":
                        if (!ValidateLicenseId())
                            errorMessage = "CAM Credentials are required.";
                        break;
                }

                return errorMessage;
            }
        }

        private bool ValidateUserName()
        {
            if (!string.IsNullOrEmpty(LicenseId)) return true;

            return !string.IsNullOrEmpty(UserName);
        }

        private bool ValidatePassword()
        {
            if (!string.IsNullOrEmpty(LicenseId)) return true;

            return !string.IsNullOrEmpty(Password);
        }

        private bool ValidateLicenseId()
        {
            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password)) return true;

            return !string.IsNullOrEmpty(LicenseId);
        }

        public bool ValidateModel()
        {
            ErrorMessage = this["UserName"] ?? this["Password"] ?? this["LicenseId"];

            return ErrorMessage == null;
        }

    }
}
