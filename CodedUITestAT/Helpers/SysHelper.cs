using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Management;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.Win32;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodedUITestAT
{
    /// <summary> Contains methods for work with OS, processes, services, etc. </summary>
    internal static class SysHelper
    {
        /// <summary> Starts the application under test. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        internal static void StartApplication(Computer endpoint)
        {
            if (DataHelper.IsStaging)
            {
                RedirectToValkyrieStaging();
                RedirectToFlsDedicated(endpoint);
            }

            StartProcess(DataHelper.GetWorkingFolder(), DataHelper.ProductExeName, String.Empty);

            CodedUITestEula useEula = new CodedUITestEula();
            UIHelper.WaitForControl(useEula.IAcceptTermsOfThisAgreementCheckBox(), Settings.WaitingTime.Default.LaunchAppWaitTime);        
            UIHelper.WaitForControlIsEnabled(useEula.IAcceptTermsOfThisAgreementCheckBox());
        }

        /// <summary> Stops the application under test. </summary>
        internal static void StopApplication()
        {
            if (IsThereProcess(Settings.General.Default.ATApplicationProcessName))
            { 
                KillProcesses(Settings.General.Default.ATApplicationProcessName);
                Playback.Wait(2000);
            }
        }

        /// <summary> Gets the product version using its executable. </summary>
        /// <returns> Returns the product version or null in the case the product file is not found. </returns>
        internal static string GetProductVersion()
        {
            string filePath = Path.Combine(DataHelper.GetWorkingFolder(), Settings.General.Default.ProductExeName);
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
                return versionInfo.ProductVersion;
            }
            catch (FileNotFoundException) { return null; }
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

        /// <summary> Checks whether a given process is present at an indicated endpoint. </summary>
        /// <param name="procName">Set a process name.</param>
        /// <param name="machine">Set an endpoint or keep default.</param>
        internal static bool IsThereProcess(string procName, Computer machine = null)
        {
            if (machine == null) machine = new Computer();
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(procName), machine.Name);
            if (processes.Length == 0) return false;
            else return true;
        }

        /// <summary> Gets a given process from an indicated endpoint. </summary>
        /// <param name="procName">Set a process name.</param>
        /// <param name="machine">Set an endpoint or keep default.</param>
        internal static Process[] GetProcess(string procName, Computer machine = null)
        {
            if (machine == null) machine = new Computer();
            Process[] processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(procName), machine.Name);
            return processes;
        }

        #endregion

        #region Service-related methods

        /// <summary> Compares an actual service status on a given machine with an expected one. </summary>
        /// <param name="machine">Set a machine.</param>
        /// <param name="serviceName">Set a service name.</param>
        /// <param name="expectedServiceStatus">Set an expected service status.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        internal static void AssertServiceStatus(Computer machine, string serviceName, ServiceControllerStatus expectedServiceStatus, TestContext testContext)
        {
            if (GetServiceStatus(serviceName, machine) == expectedServiceStatus)
                testContext.WriteLine(String.Format("Service '{0}' status is now '{1}' on machine '{2}'.", serviceName, expectedServiceStatus.ToString(), machine.Name));
            else Assert.Fail(String.Format("Service '{0}' status is still not '{1}' on machine '{2}'!", serviceName, expectedServiceStatus.ToString(), machine.Name));
        }

        /// <summary> Waits for a service given status, fails if the status did not appear after all. </summary>
        /// <param name="serviceName">Set a service name.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        /// <param name="status">Set a desired service status.</param>
        /// <param name="machine">Set a machine, on which the service status should be checked.</param>
        /// <param name="waitTime">Set waiting time or keep default.</param>
        internal static void WaitForServiceStatus(string serviceName, TestContext testContext, ServiceControllerStatus serviceStatus = ServiceControllerStatus.Running, 
                                                                                                                        Computer machine = null, int waitTime = DataHelper.DefaultWaitTime)
        {
            if (machine == null) machine = new Computer();
            
            string successMessage = String.Format("Status '{0}' appeared for service '{1}' on machine '{2}'.", serviceStatus.ToString(), serviceName, machine.Name);
            string failureMessage = String.Format("Status '{0}' did not appear for service '{1}' on machine '{2}' after {3} second(s)!", serviceStatus.ToString(), serviceName, 
                                                                                                                                                                        machine.Name, waitTime / 1000);

            var timer = Stopwatch.StartNew();
            for (; ; )
            {
                Playback.Wait(1000);
                if (GetServiceStatus(serviceName, machine) == serviceStatus)
                {
                    testContext.WriteLine(successMessage);
                    break;
                }
                if ((int)timer.ElapsedMilliseconds > waitTime)
                {
                    Assert.Fail(failureMessage);
                    break;
                }
            }
            timer.Stop();
        }

        /// <summary> Gets a given service status on an indicated machine. </summary>
        /// <param name="serviceName">Set a service name.</param>
        /// <param name="machine">Set a machine or keep default.</param>
        internal static ServiceControllerStatus GetServiceStatus(string serviceName, Computer machine = null)
        {
            if (machine == null) machine = new Computer();
            ServiceController serviceController = new ServiceController(serviceName, machine.Name);
            serviceController.Refresh();
            return serviceController.Status;
        }

        /// <summary> Determines whether a given service exists at the indicated machine. </summary>
        /// <param name="serviceName">Set a service name.</param>
        /// <param name="machine">Set a machine.</param>
        internal static bool IsThereService(string serviceName, Computer machine = null)
        {
            if (machine == null) machine = new Computer();
            var services = ServiceController.GetServices(machine.Name);
            var service = services.FirstOrDefault(s => s.ServiceName == serviceName);

            return service != null;
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

        /// <summary> Indicates whether a given remote machine is 64-bit using its registry. </summary>
        /// <param name="machine">Set a machine.</param>
        internal static bool IsOs64Bit(Computer machine)
        {
            string keyName = @"SOFTWARE\Wow6432Node"; // By default is present only for 64-bit OS

            using (RegistryKey regHive = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machine.Name))
            {
                try
                {
                    RegistryKey regKey = regHive.OpenSubKey(keyName, false);
                    regKey.Close();
                    return true;
                }
                catch (Exception) { return false; }
            }
        }

        /// <summary> Edit hosts file on the server VM and adds Valkyrie staging server address. </summary>
        private static void RedirectToValkyrieStaging()
        {
            string hostsFilePath = DataHelper.GetServerRoot() +  Settings.Staging.Default.HostsFile;
            string address = String.Format(@"{0} {1}", Settings.Staging.Default.ValkyrieStagingIpAddress, Settings.Staging.Default.ValkyrieProductionServerAddress);
            string initialFileContent;

            using (var stream = new StreamReader(hostsFilePath, Encoding.Default)) { initialFileContent = stream.ReadToEnd(); }
            
            if (!initialFileContent.Contains(address))
            using (var stream = new StreamWriter(hostsFilePath, true, Encoding.Default))
            {
               stream.WriteLine();
               stream.WriteLine(address);
            }
        }

        /// <summary> Edit hosts file on the endpoint VM and adds FLS staging server address. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        private static void RedirectToFlsDedicated(Computer endpoint)
        {
            string hostsFilePath = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), Settings.Staging.Default.HostsFile);
            string address = String.Format("{0} {1}", Settings.Staging.Default.FlsDedicatedIpAddress, Settings.Staging.Default.FlsProductionServerAddress);
            string initialFileContent;

            using (var stream = new StreamReader(hostsFilePath, Encoding.Default)) { initialFileContent = stream.ReadToEnd(); }

            if (!initialFileContent.Contains(address))
            using (var stream = new StreamWriter(hostsFilePath, true, Encoding.Default))
            {
                stream.WriteLine();
                stream.WriteLine(address);
            }
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
