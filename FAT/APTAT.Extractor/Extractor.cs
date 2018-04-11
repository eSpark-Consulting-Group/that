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
        const string PROCESS_NAME = "UnknownFileHunter";
        public static void ExtractId(string processName = PROCESS_NAME)
        {
            processName = string.IsNullOrWhiteSpace(processName) ? PROCESS_NAME : processName;
            var process = Process.GetProcesses().FirstOrDefault(p => p.ProcessName.StartsWith(processName));
            var processPath = process.MainModule.FileName;
            var processFolder = Path.GetDirectoryName(processPath);

            var curPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            
            var reg = new Regex(processName + "_[a-zA-Z0-9]+[=]{0,2}", RegexOptions.Compiled);
            var match = reg.Match(processPath);  //math.Value == Path.GetFileNameWithoutExtension(processPath); //????????????
            if (match.Success)
            {
                File.WriteAllBytes(Path.Combine(curPath, "originalfile"), Encoding.UTF8.GetBytes(match.Value));
            }

            Process.Start(Path.Combine(curPath, processName + ".exe"));
        }
    }
}
