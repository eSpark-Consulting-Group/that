using System;
using System.Security;

namespace APTAT.COT.Login
{
    public class SecurityUserCredentials
    {
        public SecurityUserCredentials(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login))
                throw new ArgumentException("ArgumentException is Invalid", login);
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("ArgumentException is Invalid", password);

            Login = login;
            Password = password.ConvertToSecureString();
        }

        public SecurityUserCredentials(string login, string password, string namspace) : this(login, password)
        {
            if (string.IsNullOrWhiteSpace(namspace))
                throw new ArgumentException("ArgumentException is Invalid", namspace);
            Namespace = namspace;
        }

        public SecurityUserCredentials(string licenseNumber)
        {
            if (string.IsNullOrWhiteSpace(licenseNumber))
                throw new ArgumentException("ArgumentException is Invalid", licenseNumber);

            LicenseNumber = licenseNumber;
        }

        public string Login { get; private set; }
        public SecureString Password { get; private set; }

        public string Namespace { get; private set; }

        public string LicenseNumber { get; private set; }
    }
}
