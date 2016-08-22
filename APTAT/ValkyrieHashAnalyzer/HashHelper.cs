using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ValkyrieHashAnalyzer
{
    public static class HashHelper
    {
        public static bool IsValidSHA1(this String s)
        {
            string pat = @"[a-fA-F0-9]{40}";
            bool isValid = Regex.IsMatch(s, pat, RegexOptions.IgnoreCase);
            return isValid;            
        }
    }
}
