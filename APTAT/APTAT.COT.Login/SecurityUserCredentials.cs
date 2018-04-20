using System;
using System.Security;

namespace APTAT.COT.Login
{
    public class SecurityUserCredentials
    {
        public bool CanApplay { get; private set; } = true;
        public SecurityUserCredentials()
        {
            Email = string.Empty;
            Password = string.Empty.ConvertToSecureString();
            Namespace = string.Empty;
            LicenseID = string.Empty;
            CanApplay = false;
        }
        public SecurityUserCredentials(string email, string password) : this()
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                CanApplay = false;
            }

            Email = email;
            Password = password.ConvertToSecureString();
        }

        public SecurityUserCredentials(string email, string password, string licenseID)
        {
            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
            {
                Email = email;
                Password = password.ConvertToSecureString();
                CanApplay = true;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(licenseID))
                {
                    CanApplay = false;
                }
                LicenseID = licenseID;
            }
        }        

        public string Email { get; private set; }
        public SecureString Password { get; private set; }

        public string Namespace { get; private set; }

        public string LicenseID { get; private set; }
    }
}
