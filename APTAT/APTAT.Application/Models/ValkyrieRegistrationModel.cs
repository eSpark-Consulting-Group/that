

using Composition = APTAT.Common.CompositionHelper;

namespace APTAT.Application.Models
{
    [Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class ValkyrieRegistrationModel : ViewModel
    {
        private string _licenseId;
        private string _userName;
        private string _passwordBuffer;
        private string _errorMessage;

        public string LicenseId
        {
            get { return _licenseId; }
            set
            {
                _licenseId = value;
                RaisePropertyChanged("LicenseId");
                ErrorMessage = null;
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                RaisePropertyChanged("UserName");
                ErrorMessage = null;
            }
        }

        public string Password
        {
            get;
            set;
        }

        public string PasswordBuffer
        {
            get { return _passwordBuffer; }
            set
            {
                _passwordBuffer = value;

                if (string.IsNullOrEmpty(_passwordBuffer))
                    Password = null;
                else
                {
                    for (int i = 0; i < _passwordBuffer.Length; i++)
                    {
                        if (_passwordBuffer[i] != '*')
                        {
                            if (string.IsNullOrEmpty(Password))
                                Password = string.Empty;
                            Password += _passwordBuffer[i];
                        }
                    }

                    if (Password.Length > _passwordBuffer.Length)
                        Password = Password.Substring(0, _passwordBuffer.Length);

                    _passwordBuffer = string.Empty;
                    for (int i = 0; i < Password.Length; i++)
                        _passwordBuffer += '*';
                }

                RaisePropertyChanged("PasswordBuffer");
                ErrorMessage = null;
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
                            errorMessage = "Given license id is invalid.";
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

            return !string.IsNullOrEmpty(LicenseId) && LicenseId.Length == 36;
        }

        public bool ValidateModel()
        {
            ErrorMessage = this["UserName"] ?? this["Password"] ?? this["LicenseId"];

            return ErrorMessage == null;
        }

    }
}
