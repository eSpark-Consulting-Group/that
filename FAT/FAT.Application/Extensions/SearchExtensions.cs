using System;

namespace APTAT.Application.Extensions
{
    public static class SearchExtensions
    {
        public static bool ContainsString(this string target, string value)
        {
            if (string.IsNullOrEmpty(value))
                return true;

            if (string.IsNullOrEmpty(target))
                return false;

            return target.ToLower().Contains(value.ToLower());
        }
    }
}
