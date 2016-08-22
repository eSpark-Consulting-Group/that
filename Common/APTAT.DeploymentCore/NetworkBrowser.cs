using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Runtime.InteropServices;

using APTAT.Common;

namespace APTAT.DeploymentCore
{
    public interface INetworkBrowser : IService
    {
        NetServerEnumResults GetNetworkComputers(ServerType serverType, string domainName, List<string> comps);
        /// <summary>
		/// Retrieving available workgroups 
		/// </summary>
		/// <returns>Array, that contains workgroup names</returns>
		string[] EnumerateWorkgroups();

        bool IsWorkgroupName(string workgroupName);
    }

    #region NetworkBrowser CLASS
    /// <summary>
    /// Provides a mechanism for supplying a list of all PC names in the local network.
    /// <br></br>
    /// This class makes use of a DllImport instruction. The purpose of which is as follows:
    /// When a DllImport declaration is made in managed code (C#) it is a call to a legacy
    /// unmanaged code module, normally a C++ Dynamic Link Library. These C++ Dll's are
    /// usually part of the operating system API, or some other vendors API, and must be 
    /// used to carry out operations that are not native within the managed code C# framework. 
    /// This is fairly normal within the windows world. The only thing that needs careful consideration
    /// is the construction of the correct type of STRUCTS, object pointers, and attribute markers,
    /// which all contribute to making the link between managed (C#) and unmanaged code (C++)
    /// more seamless
    /// <br></br>
    /// This class makes use of the following Dll calls
    /// <list type="bullet">
    /// <item>
    /// <description> Netapi32.dll : NetServerEnum, The NetServerEnum function lists all servers
    /// of the specified type that are visible in a domain. For example, an application can call 
    /// NetServerEnum to list all domain controllers only or all SQL servers only.
    ///	You can combine bit masks to list several types. For example, a value of 0x00000003 
    ///	combines the bit masks for SV_TYPE_WORKSTATION (0x00000001) and SV_TYPE_SERVER (0x00000002).
    /// </description>
    /// </item>
    /// <item>
    /// <description> Netapi32.dll : NetApiBufferFree, The NetApiBufferFree function frees 
    /// the memory that the NetApiBufferAllocate function allocates. Call NetApiBufferFree 
    /// to free the memory that other network management functions return.</description>
    /// </item>
    /// </list>
    /// </summary>
    public sealed class NetworkBrowser : INetworkBrowser
    {
        #region Constructor
        /// <summary>
        /// Constructor, simply creates a new NetworkBrowser object
        /// </summary>
        unsafe static NetworkBrowser()
        {
            SERVER_INFO_101_SIZE = sizeof(NetBiosAPI.SERVER_INFO_101);
        }

        #endregion
        
        #region GetNetworkComputers (2)

    	private static int SERVER_INFO_101_SIZE;

        public NetServerEnumResults GetNetworkComputers(ServerType serverType, string domainName, List<string> comps)
        {
            uint level = 101, prefMaxLen = 0xFFFFFFFF, entriesRead = 0, totalEntries = 0;
            IntPtr serverInfoPtr = IntPtr.Zero;
            NetServerEnumResults result = NetServerEnumResults.NERR_Success;
            try
            {
				DateTime start = DateTime.UtcNow;
				LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network,
					"NetServerEnum started{0} serverType: {1}{0} domainName: {2}{0}",
					Environment.NewLine, serverType, domainName );
                result = (NetServerEnumResults)NetBiosAPI.NetServerEnum(
                    IntPtr.Zero,            // Server Name: Reserved; must be NULL. 		
                    level,                  // Return server names, types, and associated software. The bufptr parameter points to an array of SERVER_INFO_101 structures.			
                    ref serverInfoPtr,	    // Pointer to the buffer that receives the data.			
                    prefMaxLen,		        // Specifies the preferred maximum length of returned data, in bytes.			
                    ref entriesRead,	    // count of elements actually enumerated.			
                    ref totalEntries,	    // total number of visible servers and workstations on the network			
                    (uint)serverType,	    // value that filters the server entries to return from the enumeration			
                    domainName,				// Pointer to a constant string that specifies the name of the domain for which a list of servers is to be returned.			
                    IntPtr.Zero);		    // Reserved; must be set to zero.

                for (int i = 0; i < entriesRead; i++)
                {
                    long newOffset = serverInfoPtr.ToInt64() + SERVER_INFO_101_SIZE * i;
                    NetBiosAPI.SERVER_INFO_101 si = (NetBiosAPI.SERVER_INFO_101)Marshal.PtrToStructure(new IntPtr(newOffset), typeof(NetBiosAPI.SERVER_INFO_101));
                    string name = Marshal.PtrToStringAuto(si.lpszServerName);
                    name = string.IsNullOrEmpty(name) ? "Unknown" : name;
                    comps.Add(name);
                }

				LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network,
					"NetServerEnum finished{0} serverType: {3}{0} domainName: {4}{0} result: {1}{0} found computers: {2}{0} elapsed: {5}{0}",
					Environment.NewLine, result, comps.Count, serverType, domainName, DateTime.UtcNow - start);
            }
            finally
            {
                if (!serverInfoPtr.Equals(IntPtr.Zero))
                {
                    NetBiosAPI.NetApiBufferFree(serverInfoPtr);
                    serverInfoPtr = IntPtr.Zero;
                }
            }
            return result;
        }
        #endregion GetNetworkComputers (2)

        public bool IsWorkgroupName(string name)
        {
            return (NetBiosAPI.NetValidateName(null, name, null, null, NETSETUP_NAME_TYPE.NetSetupWorkgroup) == 0);
        }
        public string[] EnumerateWorkgroups()
        {
            // Retrieving list of workgroups
            List<string> workgroups = new List<string>();

            DateTime start = DateTime.UtcNow;
            LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network, "Workgroups enumeration started");

            using (DirectoryEntry de = new DirectoryEntry("WinNT:"))
            {
                foreach (DirectoryEntry d in de.Children)
                {
                    string wgName = d.Name;
                    if (IsWorkgroupName(wgName))
                    {
                        workgroups.Add(wgName);
                    }
                }
            }

            LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Network,
                "Workgroups enumeration finished{0} found workgroups: {1}{0} elapsed: {2}",
                Environment.NewLine, workgroups.Count, DateTime.UtcNow - start);

            return workgroups.ToArray();
        }
        #region NetBiosAPI
        internal class NetBiosAPI
        {
            #region Win32 API Interfaces
            [DllImport("netapi32.dll", EntryPoint = "NetApiBufferFree")]
            internal static extern void NetApiBufferFree(IntPtr bufptr);

            [DllImport("netapi32.dll", EntryPoint = "NetServerEnum")]
            internal static extern uint NetServerEnum(
                                        IntPtr ServerName,
                                        uint level,
                                        ref IntPtr siPtr,
                                        uint prefmaxlen,
                                        ref uint entriesread,
                                        ref uint totalentries,
                                        uint servertype,
                                        [MarshalAs(UnmanagedType.LPWStr)] string domain,
                                        IntPtr resumeHandle);

            [DllImport("netapi32.dll", EntryPoint = "NetValidateName")]
            internal static extern uint NetValidateName(
                                        [MarshalAs(UnmanagedType.LPWStr)] string server,
                                        [MarshalAs(UnmanagedType.LPWStr)] string name,
                                        [MarshalAs(UnmanagedType.LPWStr)] string account,
                                        [MarshalAs(UnmanagedType.LPWStr)] string password,
                                        NETSETUP_NAME_TYPE nameType);

            [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Auto)]
            internal struct SERVER_INFO_101
            {
                public int dwPlatformID;
                public IntPtr lpszServerName;
                public int dwVersionMajor;
                public int dwVersionMinor;
                public int dwType;
                public IntPtr lpszComment;
            }

            #endregion
        }
        #endregion NetBiosAPI

    }
    #endregion



    #region ServerType
    /// <summary>      
    /// Значения флагов, которые определяют типы серверов, информацию о      
    /// которых необходимо получить. (см lmserver.h)	
    /// </summary>	
    [Flags]
    public enum ServerType : uint
    {
        /// <summary>		
        /// Не возвращать информации не об одном из серверов.		
        /// </summary>		
        None = 0x00000000,
        /// <summary>		/// 
        ///	Все рабочие станции		
        ///	</summary>		
        Workstation = 0x00000001,
        /// <summary>		
        /// Все сервера		
        /// </summary>		
        Server = 0x00000002,
        /// <summary>		
        /// Любой из серверов, на котором запущенна служба MS SQL Server		
        /// </summary>		
        SQLServer = 0x00000004,
        /// <summary>		
        /// Основной контролер домена		
        /// </summary>		
        DomainController = 0x00000008,
        /// <summary>		
        /// Резервный контролер домена		
        /// </summary>		
        DomainBackupController = 0x00000010,
        /// <summary>		
        /// Сервера поддерживающие Timesource service		
        /// </summary>		
        TimeSource = 0x00000020,
        /// <summary>		
        /// Сервера поддерживающие Apple File Protocol 		
        /// </summary>		
        AFP = 0x00000040,
        /// <summary>		
        /// Novell сервера		
        /// </summary>		
        Novell = 0x00000080,
        ///<summary>		
        ///Член LAN Manager 2.x domain		
        ///</summary>		
        DomainMember = 0x00000100,
        ///<summary>		
        ///Сервера поддерживающие очередь печати
        ///</summary>		
        PrintQueue = 0x00000200,
        /// <summary>		
        /// Сервера, на которых запущен dial-in service		
        /// </summary>		
        Dialin = 0x00000400,
        ///<summary>		
        ///Xenix сервера		
        ///</summary>		
        Xenix = 0x00000800,
        /// <summary>		
        /// Unix сервера ?            
        /// Xenix и Unix сервера считаются равнозначными!!!		
        /// </summary>		
        Unix = Xenix,
        /// <summary>		
        /// Сервера и рабочие станции Windows NT		
        /// </summary>		
        NT = 0x00001000,
        /// <summary>		
        /// Сервера Windows for Workgroups		
        /// </summary>		
        WFW = 0x00002000,
        /// <summary>		
        /// Microsoft File and Print for NetWare		
        /// </summary>		
        MFPN = 0x00004000,
        /// <summary>		
        /// Сервера, не являющиеся контролерами домена		
        /// </summary>		
        NTServer = 0x00008000,
        /// <summary>		
        /// Сервера, которые поддерживают запуск «browser service»		
        /// </summary>		
        PotentialBrowser = 0x00010000,
        /// <summary>	
        /// Сервера резервной поддержки «browser service»		
        /// </summary>		
        BackupBrowser = 0x00020000,
        /// <summary>		
        /// Сервера запущенные как «master browser service»		
        /// </summary>		
        MasterBrowser = 0x00040000,
        /// <summary>		
        /// Сервера запущенные как «domain master browser»		
        /// </summary>		
        DomainMaster = 0x00080000,
        /// <summary>		
        /// Не документирован.		
        /// </summary>		
        OSF = 0x00100000,
        /// <summary>		
        /// VMS		
        /// </summary>		
        VMS = 0x00200000,
        /// <summary>
        /// Windows 95 or later
        /// </summary>		
        Windows = 0x00400000,
        ///<summary>
        ///Distributed File System
        ///</summary>		
        DFS = 0x00800000,
        /// <summary>
        /// Не документирован.
        /// </summary>		
        ClusterNT = 0x01000000,
        /// <summary>
        /// Terminal Server		
        /// </summary>		
        TerminalServer = 0x02000000,
        /// <summary>
        /// Не документирован.
        /// </summary>		
        DCE = 0x10000000,
        /// <summary>
        /// Не документирован
        /// </summary>		
        AlternateXPort = 0x20000000,
        /// <summary>
        /// Сервера поддержки  «browser service»
        /// </summary>		
        ListOnly = 0x40000000,
        /// <summary>
        ///  Список доменов
        ///  </summary>		
        DomainEnum = 0x80000000,
        /// <summary>
        /// Все сервера.
        /// </summary>		
        All = 0xFFFFFFFF
    }
    #endregion ServerType

    #region NetServerEnumResults
    public enum NetServerEnumResults
    {
        NERR_Success = 0,
        ERROR_NO_BROWSER_SERVERS_FOUND = 6118,
        ERROR_MORE_DATA = 234,
        ERROR_INVALID_PARAMETER = 87,
        UNKNOWN_ERROR = 1326
    }
    #endregion NetServerEnumResults

    public enum NETSETUP_NAME_TYPE 
    {
        NetSetupUnknown = 0, // The nametype is is unknown. If this value is used, the NetValidateName function fails with ERROR_INVALID_PARAMETER.
        NetSetupMachine = 1, // Verify that the NetBIOS computer name is valid and that it is not in use.
        NetSetupWorkgroup = 2, // Verify that the workgroup name is valid.
        NetSetupDomain = 3, // Verify that the domain name exists and that it is a domain.
        NetSetupNonExistentDomain = 4, // Verify that the domain name is not in use.
        NetSetupDnsMachine = 5, // Verify that the DNS computer name is valid. This value is supported on Windows 2000 and later. The application must be compiled with _WIN32_WINNT >= 0x0500 to use this value.
    }
}