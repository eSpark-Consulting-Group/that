using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        ERR_USER_NOT_FOUND,
    }

    public enum CheckAuthResults
    {
        ERR_NOT_AUTHENTICATED,
        SUCCESS_AUTHENTICATED,
        ERR_MISSING_PARAM,
    }

    public enum OPTAuthResults
    {
        SUCCESS_OTP_AUTHENTICATION,
        ERR_OTP_AUTHENTICATION,
        ERR_OTP_INVALID,
        ERR_OTP_KEY_INVALID,
    }
}
