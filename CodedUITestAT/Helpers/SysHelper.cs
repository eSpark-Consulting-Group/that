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
        /// <summary> Starts the application under test. </summary>
        internal static void StartApplication()
        {
            StartProcess(DataHelper.GetWorkingFolder(), DataHelper.ProductExeName, String.Empty);

            CodedUITestEula useEula = new CodedUITestEula();
            UIHelper.WaitForControlIsEnabled(useEula.IAcceptTermsOfThisAgreementCheckBox(), Settings.WaitingTime.Default.LaunchAppWaitTime);        
        }

        /// <summary> Stops the application under test. </summary>
        internal static void StopApplication()
        {
            if (IsThereProcess(Settings.General.Default.ATApplicationProcessName)) KillProcesses(Settings.General.Default.ATApplicationProcessName);
        }

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

        /// <summary> Checks whether a given process is present at a given endpoint. </summary>
        /// <param name="procName">Set a process name.</param>
        /// <param name="machineName">Set an endpoint name.</param>
        internal static bool IsThereProcess(string procName, string machineName = "localhost")
        {
            if (machineName == "localhost") machineName = Environment.MachineName;
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(procName), machineName);
            if (processes.Length == 0) return false;
            else return true;
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

        #region File- and directory-related methods

        /// <summary> Infects a given endpoint for a Quick Scan. </summary>
        /// <param name="endpoint">Set an endpoint to infect.</param>
        /// <returns>Returns a malware object.</returns>
        internal static Malware InfectForQuickScan(Computer endpoint)
        {
            string threatName = Settings.General.Default.ThreatName;
            string fileToBeScanned = String.Format(@"Users\{0}\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\{1}", DataHelper.Username, threatName);
            string source = String.Format(@"{0}\{1}", DataHelper.GetMalwaresFolder(true), threatName);
            string destination = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), fileToBeScanned);
            File.Copy(source, destination, true);
            Malware malware = new Malware(String.Format(@"{0}{1}", DataHelper.GetEndpointRoot(), fileToBeScanned));

            return malware;
        }

        #endregion
    }
}
