using System;

namespace APTAT.Valkyrie
{
    public class CamRegistration : ICamRegistration
    {
        public CamRegistration(string licenseId, string userName, string password)
        {
            LicenseId = licenseId;
            UserName = userName;
            Password = password;
        }

        public string LicenseId { get; private set; }
        public string UserName { get; private set; }
        public string Password { get; private set; }

        public bool IsEmpty
        {
            get
            {
                return String.IsNullOrWhiteSpace(LicenseId) && String.IsNullOrWhiteSpace(UserName) &&
                       String.IsNullOrWhiteSpace(Password);
            }
        }

        
    }
}