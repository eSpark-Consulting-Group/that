using System;
using System.Security;

namespace APTAT.COT.Login
{
    public class SecurityUserCredentials
    {
        private SecurityUserCredentials()
        {
            Email = string.Empty;
            Password = string.Empty.ConvertToSecureString();
            Namespace = string.Empty;
            LicenseNumber = string.Empty;
        }
        public SecurityUserCredentials(string email, string password) : this()
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ArgumentException is Invalid", email);
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("ArgumentException is Invalid", password);

            Email = email;
            Password = password.ConvertToSecureString();
        }

        public SecurityUserCredentials(string email, string password, string licenseID) : this(email, password)
        {
            LicenseNumber = licenseID;
        }

        public SecurityUserCredentials(string licenseNumber) : this()
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
                throw new ArgumentException("ArgumentException is Invalid", licenseNumber);            
        }

        public string Email { get; private set; }
        public SecureString Password { get; private set; }

        public string Namespace { get; private set; }

        public string LicenseNumber { get; private set; }
    }
}
