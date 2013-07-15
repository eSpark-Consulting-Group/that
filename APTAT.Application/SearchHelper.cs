using System;

namespace APTAT.Application
{
    public static class SearchHelper
    {
        public static Boolean ContainsString(this String s, String value)
        {
            if(value == null)
            {
                return true;
            }

            if(s == null)
            {
                return false;
            }

            return s.Contains(value);
        }
    }
}
