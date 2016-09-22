using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using APTAT.Common;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public class NetworkHelper : INetworkHelper
    {
        ImpersonationHelper _impersonation = null;
        private string _remotePath = null;

        ~NetworkHelper()
        {
            Release();
        }

        public void Release()
        {
            if (!String.IsNullOrEmpty(_remotePath))
                CancelConnection(_remotePath);
            if (_impersonation != null)
            {
                _impersonation.Dispose();
                _impersonation = null;
            }
        }
        // ===============================================================



        #region Win32
        public const int CONNECT_UPDATE_PROFILE = 0x1;
        public const int NO_ERROR = 0;

        public const int ERROR_EXTENDED_ERROR = 1208;
        public const int ERROR_SESSION_CREDENTIAL_CONFLICT = 1219;
        public const int ERROR_NO_LOGON_SERVERS = 1311;
        public const int ERROR_DUP_NAME = 52;

        public struct NETRESOURCE
        {
            public int dwScope;
            public int dwType;
            public int dwDisplayType;
            public int dwUsage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        public enum ResourceScope
        {
            RESOURCE_CONNECTED = 1,
            RESOURCE_GLOBALNET,
            RESOURCE_REMEMBERED,
            RESOURCE_RECENT,
            RESOURCE_CONTEXT
        };


        public enum ResourceType
        {
            RESOURCETYPE_ANY,
            RESOURCETYPE_DISK,
            RESOURCETYPE_PRINT,
            RESOURCETYPE_RESERVED
        };

        public enum ResourceUsage
        {
            RESOURCEUSAGE_CONNECTABLE = 0x00000001,
            RESOURCEUSAGE_CONTAINER = 0x00000002,
            RESOURCEUSAGE_NOLOCALDEVICE = 0x00000004,
            RESOURCEUSAGE_SIBLING = 0x00000008,
            RESOURCEUSAGE_ATTACHED = 0x00000010,
            RESOURCEUSAGE_ALL = (RESOURCEUSAGE_CONNECTABLE | RESOURCEUSAGE_CONTAINER | RESOURCEUSAGE_ATTACHED),
        };

        public enum ResourceDisplayType
        {
            RESOURCEDISPLAYTYPE_GENERIC,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
            RESOURCEDISPLAYTYPE_FILE,
            RESOURCEDISPLAYTYPE_GROUP,
            RESOURCEDISPLAYTYPE_NETWORK,
            RESOURCEDISPLAYTYPE_ROOT,
            RESOURCEDISPLAYTYPE_SHAREADMIN,
            RESOURCEDISPLAYTYPE_DIRECTORY,
            RESOURCEDISPLAYTYPE_TREE,
            RESOURCEDISPLAYTYPE_NDSCONTAINER
        };

        [DllImport( "mpr.dll" )]
        public static extern int WNetAddConnection2( ref NETRESOURCE netResource,
                                                     string password, string username, int flags );

        [DllImport( "mpr.dll" )]
        static extern int WNetCancelConnection2( string lpName, Int32 dwFlags, bool bForce );

        [DllImport( "mpr.dll" )]
        static extern int WNetGetLastError( ref int lpError, StringBuilder lpErrorBuf, int nErrorBufSize, StringBuilder lpNameBuf, int nNameBufSize );
 
        #endregion

        public ImpersonationHelper.Logon32LogonType LogonType { get; set; }

        public void GrantNetworkAccessToRemoteComputer( string userName, string password, string remotePath, bool impersonateAsNetworkService )
        {
            _remotePath = remotePath;
            if (_impersonation != null)
            {
                _impersonation = null;
            }

            int error = NO_ERROR;

            // Try to impersonate under network service
            // If we can not do it, it is not fatal... At least yet.
            try
            {
			
                if ( impersonateAsNetworkService )
                {
                    _impersonation = new ImpersonationHelper( "NetworkService", "NT AUTHORITY", "", LogonType );
                }
                else
                {
                    _impersonation = new ImpersonationHelper( userName, "", password, LogonType );
                }
            }
            catch (Win32Exception)
            {
                // ignore this error for now
            }


            NETRESOURCE nr = new NETRESOURCE();
            nr.dwScope = (int)ResourceScope.RESOURCE_GLOBALNET;
            nr.dwUsage = (int)ResourceUsage.RESOURCEUSAGE_CONNECTABLE;
            nr.dwType = (int)ResourceType.RESOURCETYPE_ANY;
            nr.LocalName = null;
            nr.RemoteName = remotePath;
            nr.Provider = null;

            int iRetriesCount = 0;
            string extendedErrorMessage = null;
            while (true)
            {
                error = WNetAddConnection2(ref nr, password, userName, CONNECT_UPDATE_PROFILE);
                LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
                                "Conct {0} ({1} {2}\t{3} err: {4})",
                                error == 0 ? "SUCC" : "FAIL", impersonateAsNetworkService ? "NSV" : "USR", nr.RemoteName, userName, error);

                int code = -1;
                StringBuilder errorMsg = new StringBuilder( 4096 );
                StringBuilder providerName = new StringBuilder( 4096 );

                if( error == ERROR_EXTENDED_ERROR )
                {
                    // When any WNet function (which is WNetAddConnection2 too) returns ERROR_EXTENDED_ERROR (1208) code
                    // then this means that a specific error details should be obtained using WNetGetLastError function,
                    // which returns specific error message, error code and error source provider name.
                    int getErrorResult = WNetGetLastError( ref code, errorMsg, errorMsg.Capacity, providerName, providerName.Capacity );
                    if( getErrorResult == NO_ERROR )
                    {
                        LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent,
                                         "WNetAddConnection2 set an extended error:{0} code: {1}{0} message: {2}{0} provider: {3}{0}", 
                                         Environment.NewLine, code, errorMsg, providerName );

                        error = code;
                        extendedErrorMessage = String.Format( "{0}{2}Error source: {1}", errorMsg, providerName, Environment.NewLine );
                        break;
                    }
                }

                if (error != ERROR_SESSION_CREDENTIAL_CONFLICT)
                    break;

                if (++iRetriesCount > 1)
                    break; // only one retry is allowed

                error = WNetCancelConnection2(remotePath, CONNECT_UPDATE_PROFILE, false);
            }

            // Finished

            // Bug #4801 fix
            // Commented this out because this didn't work
            //if (error == ERROR_NO_LOGON_SERVERS)
            //{
            //    // This error is sometimes occurred when we are connecting
            //    // to the local computer that is in Domain
            //    // Just ignore it. It must solve the problem.
            //    return;
            //}

            if (error != NO_ERROR)
            {
                if( extendedErrorMessage != null )
                {
                    // Extended network error occurred
                    throw new Win32Exception( error, extendedErrorMessage );
                }

                // General error occurred
                throw new Win32Exception(error);
            }
        }
        private void CancelConnection(string remotePath)
        {
            try
            {
                var error = WNetCancelConnection2(remotePath, CONNECT_UPDATE_PROFILE, true);
            }
            catch (Exception)
            {
            }
        }
    }
}