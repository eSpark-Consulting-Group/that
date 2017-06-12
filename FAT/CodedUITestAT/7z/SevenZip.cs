using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CodedUITestFAT._7z
{
    internal static class SevenZip
    {
        private static string pathTo7zipFolder;
        private static string pathTo7zExe;
        static SevenZip()
        {
            pathTo7zipFolder = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramFiles%"), Settings.General.Default.SevenZFolder);
            pathTo7zExe = Path.Combine(pathTo7zipFolder, Settings.General.Default.SevenZExeName);
        }

        internal static bool isInstalled() {            
            if (Directory.Exists(pathTo7zipFolder))
            {
                if (File.Exists(pathTo7zExe))
                {
                    return true;
                }
            }
            return false;
        }

        internal static void install(string pathToInstaller, string installerName)
        {            
            string arguments = @"/S";
            runProcAndWaitForExit(pathToInstaller, installerName, arguments);
        }

        internal static void extract(string pathToArchive, string outputPath)
        {           
            string pattern = @"x {0} -o{1}";
            string arguments = string.Format(pattern, pathToArchive, outputPath);

            execute7zip(arguments);
        }

        private static void execute7zip(string arguments) {
            if (!isInstalled())
            {
                throw new Exception("7zip not installed");
            }
            runProcAndWaitForExit(pathTo7zipFolder, pathTo7zExe, arguments);
        }

        private static void runProcAndWaitForExit(string path, string exeName, string arguments) {
            int waitForExtractionSec = 20;
            string processPath = Path.Combine(path, exeName);
            var procStartInfo = new ProcessStartInfo(processPath);
            procStartInfo.WorkingDirectory = path;
            procStartInfo.Arguments = arguments;
            Process proc = Process.Start(procStartInfo);
            if (proc == null) throw new Exception(string.Format("Failed to start process [{0}]", processPath));
            for (int i = 0; i < waitForExtractionSec; i++)
            {
                proc.Refresh();
                if (proc.HasExited)
                    return;                
                Thread.Sleep(1000);                
            }
            throw new Exception(string.Format("Process [{0}] still working after [{1}] seconds", exeName, waitForExtractionSec));
        }
    }
}
