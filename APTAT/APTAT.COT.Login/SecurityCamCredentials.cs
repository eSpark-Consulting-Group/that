using System;
using System.Security;

namespace APTAT.COT.Login
{
    public sealed class SecurityCamCredentials
    {
        private static readonly object _syncOobject = new object();
        private static SecurityCamCredentials _instance;

        public bool CanApplay { get; private set; } = true;        
        
        public static SecurityCamCredentials Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncOobject)
                    {
                        if (_instance == null)
                        {
                            _instance = new SecurityCamCredentials();
                        }
                    }
                }
                return _instance;
            }
        }
        public static SecurityCamCredentials SetCredentials(string login, string password, string licenseId)
        {
            var inst = Instance;
            SetSecurityCamCredentials(login, password, licenseId);
            return inst;
        }

        public static void Reset()
        {
            _instance = null;
        }

        private SecurityCamCredentials()
        {
            Login = null;
            Password = string.Empty.ConvertToSecureString();
            Namespace = null;
            LicenseID = null;
            CanApplay = false;
        }        

        private static void SetSecurityCamCredentials(string login, string password, string licenseID = null)
        {
            var inst = Instance;
            if (!string.IsNullOrWhiteSpace(login) && !string.IsNullOrWhiteSpace(password))
            {
                inst.Login = login;
                inst.Password = password.ConvertToSecureString();
                inst.CanApplay = true;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(licenseID))
                {
                    inst.CanApplay = false;
                }
                inst.LicenseID = licenseID;
            }
        }

        public string Login { get; private set; }
        public SecureString Password { get; private set; }

        public string Namespace { get; private set; }

        public string LicenseID { get; private set; }
    }
}
