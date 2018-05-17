using System;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Text;

namespace APTAT.COT.Login
{
    public static class SecureStringExtender
    {
        public static SecureString ConvertToSecureString(this string password)
        {
            var res = new SecureString();
            if (!string.IsNullOrEmpty(password))
            {
                //return new NetworkCredential(string.Empty, password).SecurePassword;
                
                foreach (char c in password)
                    res.AppendChar(c);

                res.MakeReadOnly();
            }
            return res;
        }

        public static string ConvertToString(this SecureString secureString)
        {
            if (secureString.Length == 0) return null;
            return new NetworkCredential(string.Empty, secureString).Password;
        }
    }
}
