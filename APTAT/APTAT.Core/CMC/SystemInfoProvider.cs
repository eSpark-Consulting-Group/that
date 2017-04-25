using System;
using System.Linq;
using System.Management;

namespace APTAT.Core.CMC
{
    public class SystemInfoProvider : ISystemInfoProvider
    {
        private SystemInfo _systemInfo;

        public SystemInfo GetSystemInfo()
        {
            OperatingSystem os = Environment.OSVersion;
            var osMajorVersion = os.Version.Major;
            var osMinorVersion = os.Version.Minor;
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

        private string GetOsFriendlyName(int osMajorVersion,int osMinorVersion)
        {
            string osName = "Unknown";

            if (osMajorVersion == 6)
            {
                switch (osMinorVersion)
                {
                    case 0:
                        osName = IsServerVersion() ? "Windows Server 2008" : "Windows Vista";
                        break;
                    case 1:
                        osName = IsServerVersion() ? "Windows Server 2008 R2" : "Windows 7";
                        break;
                    case 2:
                        osName = IsServerVersion() ? "Windows Server 2012" : "Windows 8";
                        break;
                    case 3:
                        osName = IsServerVersion() ? "Windows Server 2012 R2" : "Windows 8.1";
                        break;
                }
            }
            else if (osMajorVersion == 10)
            {
                osName = "Windows 10";
            }
            return osName;
        }
    }
}