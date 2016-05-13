using System;
using System.IO;
using System.Management;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodedUITestAT
{
    /// <summary> Contains methods for work with OS, processes, services, etc. </summary>
    internal static class SysHelper
    {
        #region Process-related methods

        /// <summary> Starts a local process. </summary>
        /// <param name="path">Set a path to an executable.</param>
        /// <param name="exeName">Set an executable name</param>
        /// <param name="arguments">Set the process arguments</param>
        internal static void StartProcess(string path, string exeName, string arguments)
        {
            string processPath = Path.Combine(path, exeName);
            var procStartInfo = new ProcessStartInfo(processPath);
            procStartInfo.WorkingDirectory = path;
            procStartInfo.Arguments = arguments;
            Process proc = Process.Start(procStartInfo);

            Playback.Wait(2000);
            if (proc == null) Assert.Fail(String.Format("Process '{0}' was not started!", exeName));
        }

        /// <summary> Ends all local processes with a given name. </summary>
        /// <param name="procName">Set a process name.</param>
        internal static void KillProcesses(string procName)
        {
            foreach (var proc in Process.GetProcessesByName(procName))
            {
                proc.CloseMainWindow();
                if (!proc.WaitForExit(2000)) proc.Kill();
            }
        }

        #endregion

        #region OS-related methods

        /// <summary> Gets the current workgroup name. </summary>
        /// <returns>The current Workgroup name.</returns>
        internal static string GetWorkgroupName()
        {
            var mos = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject mo in mos.Get()) if (mo["Workgroup"] != null) return mo["Workgroup"].ToString().ToUpper();
            return "WORKGROUP";
        }

        #endregion
    }
}
