using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.IO;

namespace APTAT.Common.MachineKey
{
    public static class MachineKeyHelper
    {
        #region DLL_IMPORTS
        enum STORAGE_QUERY_TYPE
        {
            /// <summary>Instructs the driver to return an appropriate descriptor.</summary>
            PropertyStandardQuery = 0,
            /// <summary>Instructs the driver to report whether the descriptor is supported.</summary>
            PropertyExistsQuery = 1,
            /// <summary>Used to retrieve a mask of writeable fields in the descriptor. Not currently supported. Do not use.</summary>
            PropertyMaskQuery = 2,
            /// <summary>Specifies the upper limit of the list of query types. This is used to validate the query type.</summary>
            PropertyQueryMaxDefined = 3
        }

        [StructLayout(LayoutKind.Sequential)]
        struct STORAGE_PROPERTY_QUERY
        {
            public STORAGE_PROPERTY_ID PropertyId;
            public STORAGE_QUERY_TYPE QueryType;
            public byte[] AdditionalParameters;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct STORAGE_DEVICE_DESCRIPTOR
        {
            public uint Version;
            public uint Size;
            public byte DeviceType;
            public byte DeviceTypeModifier;
            [MarshalAs(UnmanagedType.U1)]
            public bool RemovableMedia;
            [MarshalAs(UnmanagedType.U1)]
            public bool CommandQueueing;
            public uint VendorIdOffset;
            public uint ProductIdOffset;
            public uint ProductRevisionOffset;
            public uint SerialNumberOffset;
            public STORAGE_BUS_TYPE BusType;
            public uint RawPropertiesLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x16)]
            public byte[] RawDeviceProperties;
        }
        enum STORAGE_BUS_TYPE
        {
            BusTypeUnknown = 0x00,
            BusTypeScsi = 0x1,
            BusTypeAtapi = 0x2,
            BusTypeAta = 0x3,
            BusType1394 = 0x4,
            BusTypeSsa = 0x5,
            BusTypeFibre = 0x6,
            BusTypeUsb = 0x7,
            BusTypeRAID = 0x8,
            BusTypeiScsi = 0x9,
            BusTypeSas = 0xA,
            BusTypeSata = 0xB,
            BusTypeSd = 0xC,
            BusTypeMmc = 0xD,
            BusTypeVirtual = 0xE,
            BusTypeFileBackedVirtual = 0xF,
            BusTypeMax = 0x10,
            BusTypeMaxReserved = 0x7F
        }
        enum STORAGE_PROPERTY_ID
        {
            StorageDeviceProperty = 0,
            StorageAdapterProperty = 1,
            StorageDeviceIdProperty = 2,
            StorageDeviceUniqueIdProperty = 3,
            StorageDeviceWriteCacheProperty = 4,
            StorageMiniportProperty = 5,
            StorageAccessAlignmentProperty = 6,
            StorageDeviceSeekPenaltyProperty = 7,
            StorageDeviceTrimProperty = 8,
            StorageDeviceWriteAggregationProperty = 9,
            StorageDeviceDeviceTelemetryProperty = 10, // 0xA
            StorageDeviceLBProvisioningProperty = 11, // 0xB
            StorageDevicePowerProperty = 12, // 0xC
            StorageDeviceCopyOffloadProperty = 13, // 0xD
            StorageDeviceResiliencyProperty = 14 // 0xE
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern SafeFileHandle CreateFile(string lpFileName, Int32 dwDesiredAccess, Int32 dwShareMode, IntPtr lpSecurityAttributes, Int32 dwCreationDisposition, Int32 dwFlagsAndAttributes, IntPtr hTemplateFile);

        [Flags]
        enum IOMethod : uint
        {
            Buffered = 0,
            InDirect = 1,
            OutDirect = 2,
            Neither = 3
        }

        [Flags]
        enum IOFileDevice : uint
        {
            Beep = 0x00000001,
            CDRom = 0x00000002,
            CDRomFileSytem = 0x00000003,
            Controller = 0x00000004,
            Datalink = 0x00000005,
            Dfs = 0x00000006,
            Disk = 0x00000007,
            DiskFileSystem = 0x00000008,
            FileSystem = 0x00000009,
            InPortPort = 0x0000000a,
            Keyboard = 0x0000000b,
            Mailslot = 0x0000000c,
            MidiIn = 0x0000000d,
            MidiOut = 0x0000000e,
            Mouse = 0x0000000f,
            MultiUncProvider = 0x00000010,
            NamedPipe = 0x00000011,
            Network = 0x00000012,
            NetworkBrowser = 0x00000013,
            NetworkFileSystem = 0x00000014,
            Null = 0x00000015,
            ParallelPort = 0x00000016,
            PhysicalNetcard = 0x00000017,
            Printer = 0x00000018,
            Scanner = 0x00000019,
            SerialMousePort = 0x0000001a,
            SerialPort = 0x0000001b,
            Screen = 0x0000001c,
            Sound = 0x0000001d,
            Streams = 0x0000001e,
            Tape = 0x0000001f,
            TapeFileSystem = 0x00000020,
            Transport = 0x00000021,
            Unknown = 0x00000022,
            Video = 0x00000023,
            VirtualDisk = 0x00000024,
            WaveIn = 0x00000025,
            WaveOut = 0x00000026,
            Port8042 = 0x00000027,
            NetworkRedirector = 0x00000028,
            Battery = 0x00000029,
            BusExtender = 0x0000002a,
            Modem = 0x0000002b,
            Vdm = 0x0000002c,
            MassStorage = 0x0000002d,
            Smb = 0x0000002e,
            Ks = 0x0000002f,
            Changer = 0x00000030,
            Smartcard = 0x00000031,
            Acpi = 0x00000032,
            Dvd = 0x00000033,
            FullscreenVideo = 0x00000034,
            DfsFileSystem = 0x00000035,
            DfsVolume = 0x00000036,
            Serenum = 0x00000037,
            Termsrv = 0x00000038,
            Ksec = 0x00000039,
            // From Windows Driver Kit 7
            Fips = 0x0000003A,
            Infiniband = 0x0000003B,
            Vmbus = 0x0000003E,
            CryptProvider = 0x0000003F,
            Wpd = 0x00000040,
            Bluetooth = 0x00000041,
            MtComposite = 0x00000042,
            MtTransport = 0x00000043,
            Biometric = 0x00000044,
            Pmi = 0x00000045,
            Volume = 0x00000056
        }

        [Flags]
        enum IOControlCode : uint
        {
            // STORAGE
            StorageCheckVerify = (IOFileDevice.MassStorage << 16) | (0x0200 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            StorageCheckVerify2 = (IOFileDevice.MassStorage << 16) | (0x0200 << 2) | IOMethod.Buffered | (0 << 14),
            StorageMediaRemoval = (IOFileDevice.MassStorage << 16) | (0x0201 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            StorageEjectMedia = (IOFileDevice.MassStorage << 16) | (0x0202 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            StorageLoadMedia = (IOFileDevice.MassStorage << 16) | (0x0203 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            StorageLoadMedia2 = (IOFileDevice.MassStorage << 16) | (0x0203 << 2) | IOMethod.Buffered | (0 << 14),
            StorageReserve = (IOFileDevice.MassStorage << 16) | (0x0204 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            StorageRelease = (IOFileDevice.MassStorage << 16) | (0x0205 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            StorageFindNewDevices = (IOFileDevice.MassStorage << 16) | (0x0206 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            StorageEjectionControl = (IOFileDevice.MassStorage << 16) | (0x0250 << 2) | IOMethod.Buffered | (0 << 14),
            StorageMcnControl = (IOFileDevice.MassStorage << 16) | (0x0251 << 2) | IOMethod.Buffered | (0 << 14),
            StorageGetMediaTypes = (IOFileDevice.MassStorage << 16) | (0x0300 << 2) | IOMethod.Buffered | (0 << 14),
            StorageGetMediaTypesEx = (IOFileDevice.MassStorage << 16) | (0x0301 << 2) | IOMethod.Buffered | (0 << 14),
            StorageResetBus = (IOFileDevice.MassStorage << 16) | (0x0400 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            StorageResetDevice = (IOFileDevice.MassStorage << 16) | (0x0401 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            StorageGetDeviceNumber = (IOFileDevice.MassStorage << 16) | (0x0420 << 2) | IOMethod.Buffered | (0 << 14),
            StoragePredictFailure = (IOFileDevice.MassStorage << 16) | (0x0440 << 2) | IOMethod.Buffered | (0 << 14),
            StorageObsoleteResetBus = (IOFileDevice.MassStorage << 16) | (0x0400 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            StorageObsoleteResetDevice = (IOFileDevice.MassStorage << 16) | (0x0401 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            StorageQueryProperty = (IOFileDevice.MassStorage << 16) | (0x0500 << 2) | IOMethod.Buffered | (0 << 14),
            // DISK
            DiskGetDriveGeometry = (IOFileDevice.Disk << 16) | (0x0000 << 2) | IOMethod.Buffered | (0 << 14),
            DiskGetDriveGeometryEx = (IOFileDevice.Disk << 16) | (0x0028 << 2) | IOMethod.Buffered | (0 << 14),
            DiskGetPartitionInfo = (IOFileDevice.Disk << 16) | (0x0001 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskGetPartitionInfoEx = (IOFileDevice.Disk << 16) | (0x0012 << 2) | IOMethod.Buffered | (0 << 14),
            DiskSetPartitionInfo = (IOFileDevice.Disk << 16) | (0x0002 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskSetPartitionInfoEx = (IOFileDevice.Disk << 16) | (0x0013 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskGetDriveLayout = (IOFileDevice.Disk << 16) | (0x0003 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskGetDriveLayoutEx = (IOFileDevice.Disk << 16) | (0x0014 << 2) | IOMethod.Buffered | (0 << 14),
            DiskSetDriveLayout = (IOFileDevice.Disk << 16) | (0x0004 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskSetDriveLayoutEx = (IOFileDevice.Disk << 16) | (0x0015 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskVerify = (IOFileDevice.Disk << 16) | (0x0005 << 2) | IOMethod.Buffered | (0 << 14),
            DiskFormatTracks = (IOFileDevice.Disk << 16) | (0x0006 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskReassignBlocks = (IOFileDevice.Disk << 16) | (0x0007 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskPerformance = (IOFileDevice.Disk << 16) | (0x0008 << 2) | IOMethod.Buffered | (0 << 14),
            DiskIsWritable = (IOFileDevice.Disk << 16) | (0x0009 << 2) | IOMethod.Buffered | (0 << 14),
            DiskLogging = (IOFileDevice.Disk << 16) | (0x000a << 2) | IOMethod.Buffered | (0 << 14),
            DiskFormatTracksEx = (IOFileDevice.Disk << 16) | (0x000b << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskHistogramStructure = (IOFileDevice.Disk << 16) | (0x000c << 2) | IOMethod.Buffered | (0 << 14),
            DiskHistogramData = (IOFileDevice.Disk << 16) | (0x000d << 2) | IOMethod.Buffered | (0 << 14),
            DiskHistogramReset = (IOFileDevice.Disk << 16) | (0x000e << 2) | IOMethod.Buffered | (0 << 14),
            DiskRequestStructure = (IOFileDevice.Disk << 16) | (0x000f << 2) | IOMethod.Buffered | (0 << 14),
            DiskRequestData = (IOFileDevice.Disk << 16) | (0x0010 << 2) | IOMethod.Buffered | (0 << 14),
            DiskControllerNumber = (IOFileDevice.Disk << 16) | (0x0011 << 2) | IOMethod.Buffered | (0 << 14),
            DiskSmartGetVersion = (IOFileDevice.Disk << 16) | (0x0020 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskSmartSendDriveCommand = (IOFileDevice.Disk << 16) | (0x0021 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskSmartRcvDriveData = (IOFileDevice.Disk << 16) | (0x0022 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskUpdateDriveSize = (IOFileDevice.Disk << 16) | (0x0032 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskGrowPartition = (IOFileDevice.Disk << 16) | (0x0034 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskGetCacheInformation = (IOFileDevice.Disk << 16) | (0x0035 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskSetCacheInformation = (IOFileDevice.Disk << 16) | (0x0036 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskDeleteDriveLayout = (IOFileDevice.Disk << 16) | (0x0040 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskFormatDrive = (IOFileDevice.Disk << 16) | (0x00f3 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskSenseDevice = (IOFileDevice.Disk << 16) | (0x00f8 << 2) | IOMethod.Buffered | (0 << 14),
            DiskCheckVerify = (IOFileDevice.Disk << 16) | (0x0200 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskMediaRemoval = (IOFileDevice.Disk << 16) | (0x0201 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskEjectMedia = (IOFileDevice.Disk << 16) | (0x0202 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskLoadMedia = (IOFileDevice.Disk << 16) | (0x0203 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskReserve = (IOFileDevice.Disk << 16) | (0x0204 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskRelease = (IOFileDevice.Disk << 16) | (0x0205 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskFindNewDevices = (IOFileDevice.Disk << 16) | (0x0206 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskCreateDisk = (IOFileDevice.Disk << 16) | (0x0016 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            DiskGetLengthInfo = (IOFileDevice.Disk << 16) | (0x0017 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            DiskGetDiskAttributes = (IOFileDevice.Disk << 16) | (0x003c << 2) | IOMethod.Buffered | (0 << 14),
            DiskSetDiskAttributes = (IOFileDevice.Disk << 16) | (0x003d << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            // CHANGER
            ChangerGetParameters = (IOFileDevice.Changer << 16) | (0x0000 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            ChangerGetStatus = (IOFileDevice.Changer << 16) | (0x0001 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            ChangerGetProductData = (IOFileDevice.Changer << 16) | (0x0002 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            ChangerSetAccess = (IOFileDevice.Changer << 16) | (0x0004 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            ChangerGetElementStatus = (IOFileDevice.Changer << 16) | (0x0005 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            ChangerInitializeElementStatus = (IOFileDevice.Changer << 16) | (0x0006 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            ChangerSetPosition = (IOFileDevice.Changer << 16) | (0x0007 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            ChangerExchangeMedium = (IOFileDevice.Changer << 16) | (0x0008 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            ChangerMoveMedium = (IOFileDevice.Changer << 16) | (0x0009 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            ChangerReinitializeTarget = (IOFileDevice.Changer << 16) | (0x000A << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            ChangerQueryVolumeTags = (IOFileDevice.Changer << 16) | (0x000B << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            // FILESYSTEM
            FsctlRequestOplockLevel1 = (IOFileDevice.FileSystem << 16) | (0 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlRequestOplockLevel2 = (IOFileDevice.FileSystem << 16) | (1 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlRequestBatchOplock = (IOFileDevice.FileSystem << 16) | (2 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlOplockBreakAcknowledge = (IOFileDevice.FileSystem << 16) | (3 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlOpBatchAckClosePending = (IOFileDevice.FileSystem << 16) | (4 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlOplockBreakNotify = (IOFileDevice.FileSystem << 16) | (5 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlLockVolume = (IOFileDevice.FileSystem << 16) | (6 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlUnlockVolume = (IOFileDevice.FileSystem << 16) | (7 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlDismountVolume = (IOFileDevice.FileSystem << 16) | (8 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlIsVolumeMounted = (IOFileDevice.FileSystem << 16) | (10 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlIsPathnameValid = (IOFileDevice.FileSystem << 16) | (11 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlMarkVolumeDirty = (IOFileDevice.FileSystem << 16) | (12 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlQueryRetrievalPointers = (IOFileDevice.FileSystem << 16) | (14 << 2) | IOMethod.Neither | (0 << 14),
            FsctlGetCompression = (IOFileDevice.FileSystem << 16) | (15 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlSetCompression = (IOFileDevice.FileSystem << 16) | (16 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            FsctlMarkAsSystemHive = (IOFileDevice.FileSystem << 16) | (19 << 2) | IOMethod.Neither | (0 << 14),
            FsctlOplockBreakAckNo2 = (IOFileDevice.FileSystem << 16) | (20 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlInvalidateVolumes = (IOFileDevice.FileSystem << 16) | (21 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlQueryFatBpb = (IOFileDevice.FileSystem << 16) | (22 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlRequestFilterOplock = (IOFileDevice.FileSystem << 16) | (23 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlFileSystemGetStatistics = (IOFileDevice.FileSystem << 16) | (24 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlGetNtfsVolumeData = (IOFileDevice.FileSystem << 16) | (25 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlGetNtfsFileRecord = (IOFileDevice.FileSystem << 16) | (26 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlGetVolumeBitmap = (IOFileDevice.FileSystem << 16) | (27 << 2) | IOMethod.Neither | (0 << 14),
            FsctlGetRetrievalPointers = (IOFileDevice.FileSystem << 16) | (28 << 2) | IOMethod.Neither | (0 << 14),
            FsctlGetRetrievalPointerBase = (IOFileDevice.FileSystem << 16) | (141 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlMoveFile = (IOFileDevice.FileSystem << 16) | (29 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlIsVolumeDirty = (IOFileDevice.FileSystem << 16) | (30 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlGetHfsInformation = (IOFileDevice.FileSystem << 16) | (31 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlAllowExtendedDasdIo = (IOFileDevice.FileSystem << 16) | (32 << 2) | IOMethod.Neither | (0 << 14),
            FsctlReadPropertyData = (IOFileDevice.FileSystem << 16) | (33 << 2) | IOMethod.Neither | (0 << 14),
            FsctlWritePropertyData = (IOFileDevice.FileSystem << 16) | (34 << 2) | IOMethod.Neither | (0 << 14),
            FsctlFindFilesBySid = (IOFileDevice.FileSystem << 16) | (35 << 2) | IOMethod.Neither | (0 << 14),
            FsctlDumpPropertyData = (IOFileDevice.FileSystem << 16) | (37 << 2) | IOMethod.Neither | (0 << 14),
            FsctlSetObjectId = (IOFileDevice.FileSystem << 16) | (38 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlGetObjectId = (IOFileDevice.FileSystem << 16) | (39 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlDeleteObjectId = (IOFileDevice.FileSystem << 16) | (40 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlSetReparsePoint = (IOFileDevice.FileSystem << 16) | (41 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlGetReparsePoint = (IOFileDevice.FileSystem << 16) | (42 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlDeleteReparsePoint = (IOFileDevice.FileSystem << 16) | (43 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlEnumUsnData = (IOFileDevice.FileSystem << 16) | (44 << 2) | IOMethod.Neither | (0 << 14),
            FsctlSecurityIdCheck = (IOFileDevice.FileSystem << 16) | (45 << 2) | IOMethod.Neither | (FileAccess.Read << 14),
            FsctlReadUsnJournal = (IOFileDevice.FileSystem << 16) | (46 << 2) | IOMethod.Neither | (0 << 14),
            FsctlSetObjectIdExtended = (IOFileDevice.FileSystem << 16) | (47 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlCreateOrGetObjectId = (IOFileDevice.FileSystem << 16) | (48 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlSetSparse = (IOFileDevice.FileSystem << 16) | (49 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlSetZeroData = (IOFileDevice.FileSystem << 16) | (50 << 2) | IOMethod.Buffered | (FileAccess.Write << 14),
            FsctlQueryAllocatedRanges = (IOFileDevice.FileSystem << 16) | (51 << 2) | IOMethod.Neither | (FileAccess.Read << 14),
            FsctlEnableUpgrade = (IOFileDevice.FileSystem << 16) | (52 << 2) | IOMethod.Buffered | (FileAccess.Write << 14),
            FsctlSetEncryption = (IOFileDevice.FileSystem << 16) | (53 << 2) | IOMethod.Neither | (0 << 14),
            FsctlEncryptionFsctlIo = (IOFileDevice.FileSystem << 16) | (54 << 2) | IOMethod.Neither | (0 << 14),
            FsctlWriteRawEncrypted = (IOFileDevice.FileSystem << 16) | (55 << 2) | IOMethod.Neither | (0 << 14),
            FsctlReadRawEncrypted = (IOFileDevice.FileSystem << 16) | (56 << 2) | IOMethod.Neither | (0 << 14),
            FsctlCreateUsnJournal = (IOFileDevice.FileSystem << 16) | (57 << 2) | IOMethod.Neither | (0 << 14),
            FsctlReadFileUsnData = (IOFileDevice.FileSystem << 16) | (58 << 2) | IOMethod.Neither | (0 << 14),
            FsctlWriteUsnCloseRecord = (IOFileDevice.FileSystem << 16) | (59 << 2) | IOMethod.Neither | (0 << 14),
            FsctlExtendVolume = (IOFileDevice.FileSystem << 16) | (60 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlQueryUsnJournal = (IOFileDevice.FileSystem << 16) | (61 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlDeleteUsnJournal = (IOFileDevice.FileSystem << 16) | (62 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlMarkHandle = (IOFileDevice.FileSystem << 16) | (63 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlSisCopyFile = (IOFileDevice.FileSystem << 16) | (64 << 2) | IOMethod.Buffered | (0 << 14),
            FsctlSisLinkFiles = (IOFileDevice.FileSystem << 16) | (65 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            FsctlHsmMsg = (IOFileDevice.FileSystem << 16) | (66 << 2) | IOMethod.Buffered | (FileAccess.ReadWrite << 14),
            FsctlNssControl = (IOFileDevice.FileSystem << 16) | (67 << 2) | IOMethod.Buffered | (FileAccess.Write << 14),
            FsctlHsmData = (IOFileDevice.FileSystem << 16) | (68 << 2) | IOMethod.Neither | (FileAccess.ReadWrite << 14),
            FsctlRecallFile = (IOFileDevice.FileSystem << 16) | (69 << 2) | IOMethod.Neither | (0 << 14),
            FsctlNssRcontrol = (IOFileDevice.FileSystem << 16) | (70 << 2) | IOMethod.Buffered | (FileAccess.Read << 14),
            FsctlQueryOnDiskVolumeInfo = (IOFileDevice.FileSystem << 16) | (79 << 2) | IOMethod.Buffered | (0 << 14),
            // VIDEO
            VideoQuerySupportedBrightness = (IOFileDevice.Video << 16) | (0x0125 << 2) | IOMethod.Buffered | (0 << 14),
            VideoQueryDisplayBrightness = (IOFileDevice.Video << 16) | (0x0126 << 2) | IOMethod.Buffered | (0 << 14),
            VideoSetDisplayBrightness = (IOFileDevice.Video << 16) | (0x0127 << 2) | IOMethod.Buffered | (0 << 14),
            // Volume (Disk Volumes)
            VolumeGetVolumeDiskExtents = (IOFileDevice.Volume << 16) | (0 << 2) | IOMethod.Buffered | (0 << 14)
        }

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            IOControlCode IoControlCode,
            [MarshalAs(UnmanagedType.AsAny)]
            [In] object InBuffer,
            uint nInBufferSize,
            [MarshalAs(UnmanagedType.AsAny)]
            [Out] object OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            [In] IntPtr Overlapped
            );


        #endregion
        #region constansts
        private const Int32 OPEN_EXISTING = 3;
        private const Int32 GENERIC_READ = unchecked((int)0x80000000);
        private const Int32 GENERIC_WRITE = 0x40000000;
        private const Int32 FILE_SHARE_READ = 0x1;
        private const Int32 FILE_SHARE_WRITE = 0x2;
        private const Int32 FILE_SHARE_DELETE = 0x4;
        private const Int32 SMART_GET_VERSION = 0x74080;
        private const Int32 SMART_RCV_DRIVE_DATA = 0x7C088;
        private const Int32 ID_CMD = 0xEC;
        private const Int32 IDENTIFY_BUFFER_SIZE = 512;
        private const Int32 CAP_SMART_CMD = 0x4;
        private const Int32 IOCTL_STORAGE_QUERY_PROPERTY = 0x2D1400;
        private const Int32 PropertyStandardQuery = 0;
        private const Int32 StorageDeviceProperty = 0;
        #endregion
        static SafeFileHandle OpenDisk(int diskNumber)
        {
            SafeFileHandle hDevice = CreateFile(string.Format(@"\\.\PhysicalDrive{0}", diskNumber), GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE | FILE_SHARE_DELETE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
            if (!hDevice.IsInvalid)
                return hDevice;
            else
                throw CreateWin32Exception(Marshal.GetLastWin32Error(), "CreateFile");
        }

        static byte[] InvokeIoControlUnknownSize<V>(SafeFileHandle handle, IOControlCode controlCode,
            V input, uint increment = 128)
        {
            uint returnedBytes = 0;

            uint inputSize = (uint)Marshal.SizeOf(typeof(V));
            uint outputLength = increment;

            do
            {
                byte[] output = new byte[outputLength];
                bool success = DeviceIoControl(handle, controlCode, input, inputSize, output, outputLength, ref returnedBytes, IntPtr.Zero);

                if (!success)
                {
                    int lastError = Marshal.GetLastWin32Error();

                    if (lastError == 234)
                    {
                        // More data
                        outputLength += increment;
                        continue;
                    }

                    throw CreateWin32Exception(Marshal.GetLastWin32Error(), "DeviceIoControl(IOCTL_STORAGE_QUERY_PROPERTY)");
                }

                // Return the result
                if (output.Length == returnedBytes)
                    return output;

                byte[] res = new byte[returnedBytes];
                Array.Copy(output, res, (int)returnedBytes);

                return res;
            } while (true);
        }

        public static string GetSerialNumberUsingStorageQuery(int diskNumber)
        {
            try
            {
                var serial = String.Empty;
                var productId = String.Empty;
                var productRevision = String.Empty;
                using (SafeFileHandle hDisk = OpenDisk(diskNumber))
                {
                    STORAGE_PROPERTY_QUERY query = new STORAGE_PROPERTY_QUERY();
                    query.QueryType = STORAGE_QUERY_TYPE.PropertyStandardQuery;
                    query.PropertyId = STORAGE_PROPERTY_ID.StorageDeviceProperty;
                    byte[] result = InvokeIoControlUnknownSize(hDisk, IOControlCode.StorageQueryProperty, query);
                    IntPtr handle = Marshal.AllocHGlobal(result.Length);
                    Marshal.Copy(result, 0, handle, result.Length);
                    var desc = Marshal.PtrToStructure<STORAGE_DEVICE_DESCRIPTOR>(handle);
                    serial = Marshal.PtrToStringAnsi(handle + (int)desc.SerialNumberOffset);
                    productId = Marshal.PtrToStringAnsi(handle + (int)desc.ProductIdOffset);
                    productRevision = Marshal.PtrToStringAnsi(handle + (int)desc.ProductRevisionOffset);
                    Marshal.FreeHGlobal(handle);
                }
                return productId + productRevision + serial;
            }
            catch(Exception )
            {
                return null;
            }
        }
        static Win32Exception CreateWin32Exception(Int32 errorCode, string context)
        {
            var win32Exception = new Win32Exception(errorCode);
            win32Exception.Data["Context"] = context;
            return win32Exception;
        }

    }
}
