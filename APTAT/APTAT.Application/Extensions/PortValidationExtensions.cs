using System;


namespace APTAT.Application.Extensions
{
    public static class PortValidationExtensions
    {
        public static string ValidateTCP(this string value, out bool hasError)
        {
            hasError = false;
            ulong i = 0;
            ulong.TryParse(value, out i);

            if (String.IsNullOrEmpty(value))
            {
                hasError = true;
                return "Please specify TCP port";
            }

            if (i > 65535)
            {
                hasError = true;
                return "TCP port should be less or equal to 65535";
            }

            return string.Empty;
        }

       
        public static string ValidateUDP(this string value, out bool hasError)
        {
            hasError = false;
            ulong i = 0;
            ulong.TryParse(value, out i);

            if (String.IsNullOrEmpty(value))
            {
                hasError = true;
                return "Please specify UDP port";
            }

            if (i > 65535)
            {
                hasError = true;
                return "UDP port should be less or equal to 65535";
            }

            return string.Empty;
        }
    }
}
