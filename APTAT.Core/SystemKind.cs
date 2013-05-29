using System;

namespace APTAT.Core
{
    [Serializable]
    public enum SystemKind : uint
    {
        Unknown = 0,

        Windows = 0x01000000,
        WindowsXP = 0x01000501,
        WindowsServer2003 = 0x01800502,
        WindowsVista = 0x01000600,
        WindowsServer2008 = 0x01800600,
        Windows7 = 0x01000601,
        WindowsServer2008R2 = 0x01800601,
        Windows8 = 0x01000602,
        WindowsServer2012 = 0x01800602,

        Linux = 0x02000000,
        LinuxDebian = 0x02000100,
        LinuxUbuntu = 0x02000101,
        LinuxRedhat = 0x02000200,
        LinuxFedora = 0x02000201,

        MacOS = 0x04000000,
        MacOSX = 0x04000100,

        Android = 0x08000000,

        IsIntelX64Edition = 0x00400000,
        IsWindowsServer = 0x00800000,

        FamilyMask = 0xFF000000,
    };
}
