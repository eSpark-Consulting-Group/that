using System;
using System.Linq;

namespace APTAT.Core
{
    public static class StringExt
    {
        public static string TrimTo(this string str, int maxLength = 80)
        {
            if (String.IsNullOrWhiteSpace(str))
            {
                return String.Empty;
            }

            if (str.Length <= maxLength)
            {
                return str;
            }
            return str.Substring(0, maxLength);
        }
    }
}
