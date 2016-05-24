using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace APTAT.Application
{
    public static class PortValidators
    {
        public static string ValidateTCP(string value, out bool hasError)
        {            
            hasError = false;
            Int32 i = 0;
            Int32.TryParse(value, out i);

            if (String.IsNullOrWhiteSpace(value))
            {
                hasError = true;
                return "Please Specify TCP port";
            }

            if (i > 65535)
            {
                hasError = true;
                return "TCP port should not be more than 65535";
            }

            return string.Empty;
        }


        public static string ValidateUDP(string value, out bool hasError)
        {
            hasError = false;
            Int32 i = 0;
            Int32.TryParse(value, out i);

            if (String.IsNullOrWhiteSpace(value))
            {
                hasError = true;
                return "Please Specify UDP port";
            }

            if (i > 65535)
            {
                hasError = true;
                return "UDP port should not be more than 65535";
            }

            return string.Empty;
        }
    }
}
