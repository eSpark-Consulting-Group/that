using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace APTAT.Extractor
{
    public static class Extractor
    {
        
        public static string ExtractId(string processName)
        {
            var process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.ToLower().StartsWith(processName.ToLower()));
            if (process == null) return "";

            var processPath = process.MainModule.FileName;

            var reg = new Regex(processName + "_[a-zA-Z0-9]+[=]{0,2}", RegexOptions.IgnoreCase);
            var match = reg.Match(processPath); 
            if (match.Success)
            {
                return match.Value;
            }

            return "";
        }
    }
}
