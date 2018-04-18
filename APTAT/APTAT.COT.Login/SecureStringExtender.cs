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
            //return new NetworkCredential(string.Empty, password).SecurePassword;
            var res = new SecureString();            
            foreach (char c in password)
                res.AppendChar(c);

            res.MakeReadOnly();

            return res;
        }

        public static string ConvertToString(this SecureString secureString)
        {
            return new NetworkCredential(string.Empty, secureString).Password;
        }
    }
}
