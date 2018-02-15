using System;


namespace UFH.Application.Extensions
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
                return "Please specify TCP Port";
            }

            if (i > 65535)
            {
                hasError = true;
                return "TCP Port should not be greater than 65535";
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
                return "Please specify UDP Port";
            }

            if (i > 65535)
            {
                hasError = true;
                return "UDP Port should not be greater than 65535";
            }

            return string.Empty;
        }
    }
}
