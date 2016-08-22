using System;

namespace APTAT.DeploymentCore
{
    public static class SystemKindParser
    {
        public static SystemKind TryParse(string osName, string osVersion = null)
        {
            if (osName == null) osName = String.Empty;
            if (osVersion == null) osVersion = String.Empty;

            var result = SystemKind.Unknown;

            osName = osName.ToLowerInvariant();
            osVersion = osVersion.ToLowerInvariant();
            if (osName.Contains("windows"))
            {
                result = SystemKind.Windows;
                if (osVersion.Contains("5.1"))
                {
                    result = SystemKind.WindowsXP;
                    if (osName.Contains("media"))
                        result |= SystemKind.IsWindowsMediaCenter;
                    else if (osName.Contains("tablet"))
                        result |= SystemKind.IsWindowsTabletPC;
                }
                else if (osVersion.Contains("5.2"))
                {
                    if (osName.Contains("2003"))
                    {
                        result = SystemKind.WindowsServer2003;
                        if (osName.Contains("home"))
                            result |= SystemKind.IsWindowsHomeServer;
                        else if (osName.Contains("business"))
                            result |= osName.Contains("r2") ? SystemKind.IsWindowsSBSR2 : SystemKind.IsWindowsSBS;
                        else if (osName.Contains("r2"))
                            result |= SystemKind.IsWindowsServerR2;
                    }
                    else
                    {
                        result = SystemKind.WindowsXP | SystemKind.IsIntelX64Edition;
                    }
                }
                else if (osVersion.Contains("6.0"))
                {
                    if (osName.Contains("server"))
                    {
                        result = SystemKind.WindowsServer2008;
                        if (osName.Contains("business"))
                            result |= SystemKind.IsWindowsSBS;
                    }
                    else
                    {
                        result = SystemKind.WindowsVista;
                    }
                }
                else if (osVersion.Contains("6.1"))
                {
                    if (osName.Contains("2008"))
                    {
                        result = SystemKind.WindowsServer2008;
                        if (osName.Contains("home"))
                            result |= SystemKind.IsWindowsHomeServer;
                        else if (osName.Contains("business"))
                            result |= SystemKind.IsWindowsSBS;
                        else
                            result |= SystemKind.IsWindowsServerR2;
                    }
                    else
                    {
                        result = SystemKind.Windows7;
                    }
                }
                else if (osVersion.Contains("6.2"))
                {
                    if (osName.Contains("2012"))
                    {
                        result = SystemKind.WindowsServer2012;
                    }
                    else
                    {
                        result = SystemKind.Windows8;
                    }
                }
                else if (osVersion.Contains("6.3"))
                {
                    if (osName.Contains("2012"))
                    {
                        result = SystemKind.WindowsServer2012R2;
                    }
                    else
                    {
                        result = SystemKind.Windows81;
                    }
                }
                else if (osVersion.Contains("10"))
                {
                    if (osName.Contains("2016"))
                    {
                        result = SystemKind.WindowsServer2016;
                    }
                    else
                    {
                        result = SystemKind.Windows10;
                    }
                }

                if (osName.Contains("embedded"))
                {
                    result |= SystemKind.IsWindowsEmbedded;
                }
            }
            else if (osName.Contains("ubuntu"))
            {
                result = SystemKind.LinuxUbuntu;
            }
            else if (osName.Contains("redhat"))
            {
                result = SystemKind.LinuxRedhat;
            }
            else if (osName.Contains("fedora"))
            {
                result = SystemKind.LinuxFedora;
            }
            else if (osName.Contains("linux"))
            {
                result = SystemKind.Linux;
            }
            else if (osName.Contains("mac"))
            {
                result = SystemKind.MacOS;
                if (osName.Contains("os x") || osName.Contains("osx"))
                    result = SystemKind.MacOSX;
            }
            else if (osName.Contains("android"))
            {
                result = SystemKind.Android;
            }

            if (result != SystemKind.Unknown && osName.Contains("x64"))
            {
                result |= SystemKind.IsIntelX64Edition;
            }

            return result;
        }
    }
}