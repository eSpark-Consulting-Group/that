using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APTAT.Extractor
{
    class Program
    {
        const string DEFAULT_PROCESS_NAME = "UnknownFileHunter";

        static void Main(string[] args)
        {

            var curPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var processName = args.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(processName)) processName = DEFAULT_PROCESS_NAME;

            var id = Extractor.ExtractId(processName);
            SaveIDToFile(curPath, id);
            
            Process.Start(Path.Combine(curPath, processName + ".exe"));
        }

        public static bool SaveIDToFile(string directoryToWrite, string id)
        {
            //nothing to save
            if (string.IsNullOrWhiteSpace(id)) return true;

            //we have ID, try to save it to originalfile in application folder
            try
            {
                File.WriteAllBytes(Path.Combine(directoryToWrite, "originalfile"), Encoding.UTF8.GetBytes(id));
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }
    }
}
