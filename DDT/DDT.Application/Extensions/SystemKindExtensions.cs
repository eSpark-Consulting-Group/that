using APTAT.DeploymentCore;

namespace DDT.Application.Extensions
{
    public static class SystemKindHelpers
    {
        public static string ToFormattedString(this SystemKind? systemKindNullable, bool? is64)
        {
            var result = string.Empty;
            if (systemKindNullable == null) return "Unknown";
            var systemKind = (SystemKind) systemKindNullable;
            var isEmbedded = (systemKind & SystemKind.IsWindowsEmbedded) != 0;
            systemKind &= ~SystemKind.IsWindowsEmbedded;
            systemKind &= ~SystemKind.IsIntelX64Edition;

            if (systemKind == SystemKind.WindowsXP)
            {
                result = "Windows XP";
            }
            else if (systemKind == SystemKind.WindowsXPMediaCenter)
            {
                result = "Windows XP Media Center";
            }
            else if (systemKind == SystemKind.WindowsXPTabletPC)
            {
                result = "Windows XP Tablet PC";
            }
            else if (systemKind == SystemKind.WindowsVista)
            {
                result = "Windows Vista";
            }
            else if (systemKind == SystemKind.Windows7)
            {
                result = "Windows 7";
            }
            else if (systemKind == SystemKind.Windows8)
            {
                result = "Windows 8";
            }
            else if (systemKind == SystemKind.Windows81)
            {
                result = "Windows 8.1";
            }
            else if (systemKind == SystemKind.Windows10)
            {
                result = "Windows 10";
            }
            else if (systemKind == SystemKind.WindowsServer2003)
            {
                result = "Windows Server 2003";
            }
            else if (systemKind == SystemKind.WindowsServer2003R2)
            {
                result = "Windows Server 2003 R2";
            }
            else if (systemKind == SystemKind.WindowsHomeServer2003)
            {
                result = "Windows Home Server 2003";
            }
            else if (systemKind == SystemKind.WindowsSBS2003)
            {
                result = "Windows SBS 2003";
            }
            else if (systemKind == SystemKind.WindowsSBS2003R2)
            {
                result = "Windows SBS 2003 R2";
            }
            else if (systemKind == SystemKind.WindowsServer2008)
            {
                result = "Windows Server 2008";
            }
            else if (systemKind == SystemKind.WindowsSBS2008)
            {
                result = "Windows SBS 2008";
            }
            else if (systemKind == SystemKind.WindowsServer2008R2)
            {
                result = "Windows Server 2008 R2";
            }
            else if (systemKind == SystemKind.WindowsHomeServer2011)
            {
                result = "Windows Home Server 2011";
            }
            else if (systemKind == SystemKind.WindowsSBS2011)
            {
                result = "Windows SBS 2011";
            }
            else if (systemKind == SystemKind.WindowsServer2012)
            {
                result = "Windows Server 2012";
            }
            else if (systemKind == SystemKind.WindowsServer2012R2)
            {
                result = "Windows Server 2012 R2";
            }
            else if (systemKind == SystemKind.WindowsServer2016)
            {
                result = "Windows Server 2016";
            }
            else if ((systemKind & SystemKind.Windows) != 0)
            {
                result = "Windows";
            }
            else if (systemKind == SystemKind.MacOSX)
            {
                result = "Mac OS X";
            }
            else if ((systemKind & SystemKind.MacOS) != 0)
            {
                result = "Mac OS";
            }
            else if (systemKind == SystemKind.LinuxDebian)
            {
                result = "Debian";
            }
            else if (systemKind == SystemKind.LinuxUbuntu)
            {
                result = "Ubuntu";
            }
            else if (systemKind == SystemKind.LinuxRedhat)
            {
                result = "Red Hat";
            }
            else if (systemKind == SystemKind.LinuxFedora)
            {
                result = "Fedora";
            }
            else if ((systemKind & SystemKind.Linux) != 0)
            {
                result = "Linux";
            }
            else if (systemKind == SystemKind.Unknown)
            {
                return "Unknown";
            }
            if (isEmbedded)
                result += " Embedded";
            if (is64.HasValue)
            {
                if (is64.Value)
                    result += " x64";
                else
                    result += " x86";
            }
            return result;
        }
    }
}
