using System;
using System.Linq;
using System.Management;
using Microsoft.Win32;

namespace APTAT.Core.CMC
{
    public class SystemInfoProvider : ISystemInfoProvider
    {
        private SystemInfo _systemInfo;


        private void GetOSVersion(out string majorVersion, out string minorVersion)
        {
            String subKey = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion";
            RegistryKey key = Registry.LocalMachine;
            RegistryKey skey = key.OpenSubKey(subKey);
            if (skey != null)
            {
                string currentMajorVersion = skey.GetValue("CurrentMajorVersionNumber")?.ToString();
                string currentMinorVersion = skey.GetValue("CurrentMinorVersionNumber")?.ToString();
                bool isEmptyMajorMinorVersion = string.IsNullOrEmpty(currentMajorVersion) &&
                                                string.IsNullOrEmpty(currentMinorVersion);

                if (!isEmptyMajorMinorVersion)
                {
                    majorVersion = currentMajorVersion;
                    minorVersion = currentMinorVersion;
                }
                else
                {
                    string[] versionParts = skey.GetValue("CurrentVersion").ToString().Split('.');
                    majorVersion = versionParts[0];
                    minorVersion = versionParts[1];
                }
            }
            else
            {
                majorVersion = minorVersion = String.Empty;
            }
        }

        public SystemInfo GetSystemInfo()
        {
                string osMinorVersion;
                string osMajorVersion;
                GetOSVersion(out osMajorVersion, out osMinorVersion);

                var osname = GetOsFriendlyName(osMajorVersion, osMinorVersion);
                var version = $"{osMajorVersion}.{osMinorVersion}";
                _systemInfo = new SystemInfo()
                {
                    FamilyName = osname,
                    Platform = Environment.Is64BitProcess ? "x64" : "x86",
                    Version = version
                };

            return _systemInfo;
        }

        private bool IsServerVersion()
        {
            var productType = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem")
                .Get().OfType<ManagementObject>().Select(o => (uint)o.GetPropertyValue("ProductType")).First();
            return productType != 1;
        }

        private string GetOsFriendlyName(string osMajorVersion, string osMinorVersion)
        {
            string osName = "Unknown";

            if (osMajorVersion == "6")
            {
                switch (osMinorVersion)
                {
                    case "0":
                        osName = IsServerVersion() ? "Windows Server 2008" : "Windows Vista";
                        break;
                    case "1":
                        osName = IsServerVersion() ? "Windows Server 2008 R2" : "Windows 7";
                        break;
                    case "2":
                        osName = IsServerVersion() ? "Windows Server 2012" : "Windows 8";
                        break;
                    case "3":
                        osName = IsServerVersion() ? "Windows Server 2012 R2" : "Windows 8.1";
                        break;
                }
            }
            else if (osMajorVersion == "10")
            {
                osName = "Windows 10";
            }
            return osName;
        }
    }
}