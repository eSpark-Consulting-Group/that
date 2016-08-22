using System;

namespace APTAT.DeploymentCore
{
    public static class SystemKindExtetions
    {
        public static SystemFamilyKind ToSystemFamilyKind(this SystemKind systemKind)
        {
            var family = systemKind & SystemKind.FamilyMask;

            switch (family)
            {
                case SystemKind.Windows:
                    return SystemFamilyKind.Windows;
                case SystemKind.MacOS:
                    return SystemFamilyKind.MacOS;
                case SystemKind.Linux:
                    return SystemFamilyKind.Linux;
                case SystemKind.Android:
                    return SystemFamilyKind.Android;
                case SystemKind.Unknown:
                    return SystemFamilyKind.Unknown;
                default:
                    throw new ArgumentException(string.Format("{0} system kind is not supported.", systemKind));
            }
        }

        public static bool IsServerOs(this SystemKind systemKind)
        {
            return (systemKind & SystemKind.IsWindowsServer) != 0;
        }
    }
    [Serializable]
    public enum SystemKind : uint
    {
        Unknown = 0,

        Windows = 0x01000000,
        WindowsXP = 0x01000501,
        WindowsXPMediaCenter = 0x01030501,
        WindowsXPTabletPC = 0x01040501,
        WindowsServer2003 = 0x01800502,
        WindowsServer2003R2 = 0x01810502,
        WindowsHomeServer2003 = 0x01820502,
        WindowsSBS2003 = 0x01850502,
        WindowsSBS2003R2 = 0x01860502,
        WindowsVista = 0x01000600,
        WindowsServer2008 = 0x01800600,
        WindowsSBS2008 = 0x01850600,
        Windows7 = 0x01000601,
        WindowsServer2008R2 = 0x01800601,
        WindowsHomeServer2011 = 0x01820601,
        WindowsSBS2011 = 0x01850601,
        Windows8 = 0x01000602,
        WindowsServer2012 = 0x01800602,
        Windows81 = 0x01000603,
        WindowsServer2012R2 = 0x01810603,
        Windows10 = 0x01000A00,
        WindowsServer2016 = 0x01800A00,

        Linux = 0x02000000,
        LinuxDebian = 0x02000100,
        LinuxUbuntu = 0x02000101,
        LinuxRedhat = 0x02000200,
        LinuxFedora = 0x02000201,

        MacOS = 0x04000000,
        MacOSX = 0x04000100,

        Android = 0x08000000,

        // Bits 23-21 are bit field, these can be tested as usual: (value & <bit>) != 0
        WindowsEditionMask1 = 0x00E00000,
        IsIntelX64Edition = 0x00400000,
        IsWindowsEmbedded = 0x00200000,
        IsWindowsServer = 0x00800000,

        // Bits 20-16 contain integer in a range 0-31. The value should be tested against enum values directly: (value & SystemKind.WindowsEditionMask2) == Is<...>        
        WindowsEditionMask2 = 0x001F0000,
        IsWindowsServerR2 = 0x00010000,
        IsWindowsHomeServer = 0x00020000,
        IsWindowsMediaCenter = 0x00030000,
        IsWindowsTabletPC = 0x00040000,
        IsWindowsSBS = 0x00050000,
        IsWindowsSBSR2 = 0x00060000,

        FamilyMask = 0xFF000000,
        EditionMask = 0x00FF0000,
        SubKindMask = 0x0000FF00,
        FlavourMask = 0x000000FF,
    };

    public enum SystemFamilyKind : uint
    {
        Unknown = SystemKind.Unknown,
        Windows = SystemKind.Windows,
        Linux = SystemKind.Linux,
        MacOS = SystemKind.MacOS,
        Android = SystemKind.Android,
    };
}
