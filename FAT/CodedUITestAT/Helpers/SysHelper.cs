using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Management;
using System.Diagnostics;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net.NetworkInformation;
using System.DirectoryServices.ActiveDirectory;
using Microsoft.Win32;
using Microsoft.VisualStudio.TestTools.UITesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodedUITestFAT
{
    /// <summary> Contains common methods for work with OS, processes, services, etc. </summary>
    internal static class SysHelper
    {
        #region The product-related methods

        /// <summary> Starts the application under test. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        internal static void StartProduct(Computer endpoint)
        {
            if (!DataHelper.IsProductExtracted)
            {
                ExtractProduct();
                DataHelper.IsProductExtracted = true;
            }

            if (DataHelper.IsFlsStaging() && !DataHelper.IsRedirectedToFlsStaging)
            {
                RedirectToFlsStaging(endpoint);
                DataHelper.IsRedirectedToFlsStaging = true;
            }

            if (DataHelper.IsValkyrieStaging() && !DataHelper.IsRedirectedToValkyrieStaging)
            {
                RedirectToValkyrieStaging();
                DataHelper.IsRedirectedToValkyrieStaging = true;
            }

            StartProcess(DataHelper.ExtractedProductPath, DataHelper.ProductExeName, String.Empty);

            CodedUITestEula useEula = new CodedUITestEula();
            UIHelper.WaitForControl(useEula.IAcceptButton(), Settings.WaitingTime.Default.LaunchAppWaitTime);        
            UIHelper.WaitForControlIsEnabled(useEula.IAcceptButton());
        }

        /// <summary> Stops the application under test. </summary>
        internal static void StopProduct()
        {
            string procName = DataHelper.ProductExeName.WithoutExtension();

            if (IsThereProcess(procName))
            { 
                KillProcesses(procName);
                Playback.Wait(2000);
            }
        }

        /// <summary> Extracts the product archive for further testing. </summary>
        private static void ExtractProduct()
        {
            string pathToZippedFat = Path.Combine(DataHelper.WorkingFolder, DataHelper.ProductExeName);            
            string pathToExtracetdFat = DataHelper.ExtractedProductPath;

            string sevenZArgument = String.Format(@"x {0} -o{1}", pathToZippedFat, pathToExtracetdFat);
            string sevenZPath;

            // Installs 7z
            Install7z();

            // Uses 7z to extract the product archive           
            sevenZPath = Path.Combine(Environment.ExpandEnvironmentVariables("%ProgramFiles%"), Settings.General.Default.SevenZFolder);
            StartProcess(sevenZPath, Settings.General.Default.SevenZExeName, sevenZArgument);
        }

        /// <summary> Installs 7-zip at the local machine. </summary>
        private static void Install7z()
        {
            string workingFolder = DataHelper.WorkingFolder;
            string argument = @"/S";
            
            StartProcess(workingFolder, Settings.General.Default.SevenZInstallerNameX32, argument);
        }

        /// <summary> Edits the product configuration file and adds Valkyrie and CAM staging servers addresses. </summary>
        private static void RedirectToValkyrieStaging()
        {
            string configPath = Path.Combine(DataHelper.ExtractedProductPath, Settings.General.Default.ApplicationBinFolder);

            XmlHelper.AddConfigSetting(configPath, Settings.Configuration.Default.CamStagingSettingNameValue, Settings.Configuration.Default.CamStagingSettingValue);
            XmlHelper.AddConfigSetting(configPath, Settings.Configuration.Default.ValkyrieStagingSettingNameValue, Settings.Configuration.Default.ValkyrieStagingSettingValue);
        }

        /// <summary> Edits hosts file on the given endpoint and adds FLS staging server address. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        private static void RedirectToFlsStaging(Computer endpoint)
        {
            string hostsFilePath = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), Settings.Staging.Default.HostsFile);
            string address = String.Format("{0} {1}", Settings.Staging.Default.FlsStagingIpAddress, Settings.Staging.Default.FlsProductionServerAddress);
            string initialFileContent;

            using (var stream = new StreamReader(hostsFilePath, Encoding.Default))
            {
                initialFileContent = stream.ReadToEnd();
                stream.Close();
            }

            if (!initialFileContent.Contains(address))
                using (var stream = new StreamWriter(hostsFilePath, true, Encoding.Default))
                {
                    stream.WriteLine();
                    stream.WriteLine(address);
                    stream.Close();
                }
        }

        #endregion

        /// <summary> Stops the application scanner at a given computer. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        internal static void StopScanner(Computer endpoint)
        {
            string procName;
            if (IsOs64Bit(endpoint)) procName = Settings.Scanner.Default.ExeName64;
            else procName = Settings.Scanner.Default.ExeName86;

            if (IsThereProcess(procName, endpoint)) KillProcessesRemotely(procName, endpoint);
        }

        /// <summary> Gets the product version using its executable. </summary>
        /// <returns> Returns the product version or null in the case the product file is not found. </returns>
        internal static string GetProductVersion()
        {
            string filePath = Path.Combine(DataHelper.WorkingFolder, DataHelper.ProductExeName);
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(filePath);
                return versionInfo.ProductVersion;
            }
            catch (FileNotFoundException) { return null; }
        }

        /// <summary> Looks for an offline machine IP address in local subnet 10.100.65. </summary>
        internal static IPAddress FindOfflineIp()
        {
            Ping ping = new Ping();
            IPAddress ipAddress;
            string ipAddressStr = null;

            for (int i = 1; i < 255; i++)
            {
                string ip = String.Format("10.100.65.{0}", i);
                PingReply pingReply = ping.Send(ip);

                if (pingReply.Status != IPStatus.Success)
                {
                    ipAddressStr = ip;
                    break;
                }
            }

            if (!IPAddress.TryParse(ipAddressStr, out ipAddress)) throw new Exception("No valid offline IP found!");
            else return ipAddress;
        }

        /// <summary> Gets IPv4-address of the machine running the tests. </summary>        
        ///<param name="isInternal">Determine whether the required IPv4-address should be internal or not.</param>
        internal static IPAddress GetLocalIpAddress(bool isInternal = false)
        {
            IPAddress ipAddress = null;
            NetworkInterface net;

            var nets = NetworkInterface.GetAllNetworkInterfaces();
            if (!isInternal) net = nets.FirstOrDefault(n => n.GetIPProperties().GatewayAddresses.Any()); // A network interface with a Gateway, corresponds to external local IP for network isolated VM
            else net = nets.FirstOrDefault(n => !n.GetIPProperties().GatewayAddresses.Any()); // A network interface without a Gateway, corresponds to internal IP for network isolated VM
            var addresses = net.GetIPProperties().UnicastAddresses; // A list of addresses of the above network interface
            foreach (var address in addresses)
            {
                if (address.Address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipAddress = address.Address;
                    break;
                }
            }

            return ipAddress;
        }

        /// <summary> Changes the last byte in a given IPv4-address to the provided new value. </summary>
        /// <param name="ipAddress">Set IPv4-address.</param>
        /// <param name="newByteValue">Set a new value for the last byte.</param>
        internal static IPAddress ChangeLastByte(this IPAddress ipAddress, byte newByteValue)
        {
            List<byte> bytes = ipAddress.GetAddressBytes().ToList();
            bytes.Remove(bytes.Last());
            bytes.Add(newByteValue);
            return new IPAddress(bytes.ToArray());
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

        /// <summary> Kills all the given processes on a remote machine using "pskill" Sysinternals tool. </summary>
        /// <param name="procName">Set a process name.</param>
        /// <param name="machine">Set a computer.</param>
        internal static void KillProcessesRemotely(string procName, Computer machine = null)
        {
            if (machine == null) machine = new Computer();
            StartProcess(DataHelper.WorkingFolder, Settings.General.Default.KillProcToolExe, String.Format(@"\\{0} {1}", machine.Name, procName));
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

        /// <summary> Gets a given local process location. </summary>
        /// <param name="processName">Set a process name.</param>
        internal static string GetProcessLocation(string processName)
        {
            string location = null;
            string wmiQuery = "SELECT ProcessId, ExecutablePath FROM Win32_Process";

            using (var searcher = new ManagementObjectSearcher(wmiQuery))
            using (var results = searcher.Get())
            {
                var query = from proc in Process.GetProcesses()
                            join mo in results.Cast<ManagementObject>()
                            on proc.Id equals (int) (uint) mo["ProcessId"]
                            select new
                            {
                                Process = proc,
                                Path = (string) mo["ExecutablePath"]
                            };

                foreach (var item in query) if (item.Process.ProcessName == processName) location = item.Path;
            }

            return location;
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

        /// <summary> Gets a current workgroup name. </summary>
        /// <returns>The current Workgroup name.</returns>
        internal static string GetWorkgroupName()
        {
            var mos = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            foreach (ManagementObject mo in mos.Get()) if (mo["Workgroup"] != null) return mo["Workgroup"].ToString().ToUpper();
            return "WORKGROUP";
        }

        /// <summary> Gets either absolute or relative domain name. </summary>
        ///<param name="isRelative">Indicate whether the requested domain name should be relative.</param>
        /// <returns>A current domain name.</returns>
        internal static string GetDomainName(bool isRelative = false)
        {
            Domain domain = Domain.GetComputerDomain();
            string name = domain.Name;
            if (isRelative) name = name.Remove(name.IndexOf("."));

            return name;
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

        #endregion

        #region File- and directory-related methods

        /// <summary> Infects a given endpoint for a scan. </summary>
        /// <param name="endpoint">Set an endpoint to infect.</param>
        /// <param name="scanType">Set a scan type.</param>
        /// <returns>Returns a malware.</returns>
        internal static Malware InfectForScan(Computer endpoint, ScanType scanType = ScanType.Quick)
        {
            string threatName = Settings.General.Default.ThreatName;
            string fileToBeScanned;

            switch (scanType)
            {
                case ScanType.Quick:
                    fileToBeScanned = String.Format(Settings.Scanner.Default.StartupFolder + "{1}", DataHelper.Username, threatName);
                    break;
                case ScanType.Full:
                    fileToBeScanned = String.Format(TextHelper.GenerateName() + @"\{0}", threatName);
                    break;
                default: throw new Exception(String.Format("Unexpected scan type '{0}'!", scanType));
            }

            string source = String.Format(@"{0}\{1}", DataHelper.GetMalwaresFolder(true), threatName);
            string destination = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), fileToBeScanned);
            if (!Directory.Exists(Path.GetDirectoryName(destination))) Directory.CreateDirectory(Path.GetDirectoryName(destination));
            File.Copy(source, destination, true);
            Malware malware = new Malware(String.Format(@"{0}{1}", DataHelper.GetEndpointRoot(), fileToBeScanned), endpoint);

            return malware;
        }

        /// <summary> Gets SHA1 hash of a given file. </summary>
        /// <param name="fullFilePath">Set a full path to a file.</param>        
        internal static string GetFileSha1(string fullFilePath)
        {
            string checkSumString = null;

            using (FileStream stream = File.OpenRead(fullFilePath))
            {
                SHA1Managed sha1 = new SHA1Managed();
                byte[] checkSum = sha1.ComputeHash(stream);
                checkSumString = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                stream.Close();
            }

            return checkSumString;
        }

        /// <summary>Copies an unknown file to a given endpoint for a scan. </summary>
        /// <param name="endpoint">Set an endpoint.</param>
        /// <param name="scanType">Set a scan type.</param>
        /// <returns>Returns an unknown file.</returns>
        internal static UnknownFile AddUnknownFileForScan(Computer endpoint, ScanType scanType = ScanType.Quick)
        {
            string newUnknownPath = GenerateUnknownFile();
            string fileToBeScanned;

            switch (scanType)
            {
                case ScanType.Quick:
                     fileToBeScanned = String.Format(Settings.Scanner.Default.StartupFolder + "{1}", DataHelper.Username, Path.GetFileName(newUnknownPath));
                     break;
                case ScanType.Full:
                     fileToBeScanned = String.Format(TextHelper.GenerateName() + @"\{0}", Path.GetFileName(newUnknownPath));
                    break;
                default: throw new Exception(String.Format("Unexpected scan type '{0}'!", scanType));
            }
            
            string destination = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), fileToBeScanned);
            if (!Directory.Exists(Path.GetDirectoryName(destination))) Directory.CreateDirectory(Path.GetDirectoryName(destination));
            if (!File.Exists(destination)) File.Move(newUnknownPath, destination);
            UnknownFile unknown = new UnknownFile(String.Format(@"{0}{1}", DataHelper.GetEndpointRoot(), fileToBeScanned), endpoint);

            return unknown;
        }

        /// <summary>Removes files from Startup folder of a given endpoint, if there are any.</summary>
        /// <param name="endpoint">Set an endpoint.</param>
        /// <param name="testContext">Set a current TestContext.</param>
        internal static void DisinfectForQuickScan(Computer endpoint, TestContext testContext)
        {
            string folderToBecleaned = String.Format(Settings.Scanner.Default.StartupFolder, DataHelper.Username);
            string remoteDirPath = String.Format(@"\\{0}\{1}\{2}", endpoint.Name, DataHelper.GetEndpointRoot(true), folderToBecleaned);
            DirectoryInfo remoteDir = new DirectoryInfo(remoteDirPath);
            foreach (FileInfo file in remoteDir.GetFiles())
            {
                file.Delete();
                testContext.WriteLine(String.Format("File '{0}' deleted from the endpoint: '{1}'.", file.FullName, endpoint.Name));
            }
        }

        /// <summary> Generates a unique unknown file from an existing one. </summary>
        /// <returns>Returns a local path to it.</returns>
        private static string GenerateUnknownFile()
        {
            string initialFilePath = Path.Combine(DataHelper.GetMalwaresFolder(true), Settings.General.Default.UnknownName);
            string newFileName = TextHelper.GenerateName() + ".exe";
            string newFilePath = Path.Combine(DataHelper.GetMalwaresFolder(true), newFileName);
            if (!File.Exists(newFilePath)) File.Copy(initialFilePath, newFilePath);

            byte[] data = Encoding.ASCII.GetBytes(newFileName);
            using (FileStream appendingStream = new FileStream(newFilePath, FileMode.Append, FileAccess.Write))
            {
                appendingStream.Write(data, 0, data.Length);
                appendingStream.Close();
            }

            return newFilePath;
        }

        /// <summary> Gets a directory containing the product files. </summary>
        internal static string GetProductDirectory()
        {
            string procName = DataHelper.ProductExeName.WithoutExtension();
            string productProcessLocation = SysHelper.GetProcessLocation(procName);

            if (String.IsNullOrEmpty(productProcessLocation)) throw new Exception(String.Format("The product process '{0}' and directory not found!", procName));
            else return Path.GetDirectoryName(productProcessLocation);
        }

        #endregion
    }
}
