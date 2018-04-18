using System;
using System.Collections.Generic;
using System.Text;

namespace APTAT.COT.Login
{
    public enum LoginResults
    {
        SUCCESS_AUTHENTICATION,
        ERR_AUTHENTICATION,
        OTP_REQUIRED, //(2 Factor Authentication is enabled and configured for user, so he/she first needs to pass otp verification.)
        OTP_CONF_REQUIRED, //(2 Factor Authentication is enabled but not configured yet for user, so he/she first needs to configure authentication on Portal.)
        ERR_MISSING_PARAM,
        ALREADY_LOGGED_IN,
    }

    public sealed class LoginHelper
    {
        public LoginResults Login(SecurityUserCredentials userCredentials)
        {
            //TODO: Implement Login
//            string POSTRequest =
//string.Format($"{
//    "email\": "{userCredentials.Login}",
//    "password\": "{ userCredentials.Password.ConvertToString()}",
//    "nameSpace\": "{ userCredentials.Namespace}"
//}";
            return LoginResults.SUCCESS_AUTHENTICATION;
        }

        public bool Logout()
        {
            //TODO: Implement Logout
            return true;
        }
    }
}
