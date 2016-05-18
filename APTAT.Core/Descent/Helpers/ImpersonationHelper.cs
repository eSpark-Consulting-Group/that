using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.ComponentModel;
using System.Diagnostics;

namespace APTAT.Core.Descent
{
    public class ImpersonationHelper : IDisposable
    {

        /// <summary>
        /// Constructor. Starts the impersonation with the given credentials.
        /// Please note that the account that instantiates the ImpersonationHelper class
        /// needs to have the 'Act as part of operating system' privilege set.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domainName">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="logonType">Type of impersonation used for impersonating.</param>
        public ImpersonationHelper(
            string userName,
            string domainName,
            string password,
            Logon32LogonType logonType )
        {
            ImpersonateValidUser(userName, domainName, password, logonType);
        }


        // ===============================================================
        // DISPOSING
        // ===============================================================
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize( this );
        }

        protected bool disposed = false;
        ~ImpersonationHelper()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // get rid of managed resources
                }

                // get rid of unmanaged resources
                Release();
                disposed = true;
            }
        }
        private void Release()
        {
            UndoImpersonation();
        }
        // ===============================================================

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern int LogonUser(
            string lpszUserName,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int DuplicateToken(
            IntPtr hToken,
            int impersonationLevel,
            ref IntPtr hNewToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool CloseHandle(
            IntPtr handle);

        private const int LOGON32_LOGON_INTERACTIVE = 2;
        private const int LOGON32_LOGON_NETWORK = 3;
        private const int LOGON32_LOGON_SERVICE = 5;
        private const int LOGON32_LOGON_NEW_CREDENTIALS = 9;
        private const int LOGON32_PROVIDER_DEFAULT = 0;
        private const int LOGON32_PROVIDER_WINNT50 = 3;

        public enum Logon32LogonType
        {
            Network = LOGON32_LOGON_NETWORK,
            Service = LOGON32_LOGON_SERVICE,
            NewCredentials = LOGON32_LOGON_NEW_CREDENTIALS,
        }

        public Logon32LogonType LogonType
        {
            get;
            set;
        }

        // ------------------------------------------------------------------

        /// <summary>
        /// Does the actual impersonation.
        /// </summary>
        /// <param name="userName">The name of the user to act as.</param>
        /// <param name="domain">The domain name of the user to act as.</param>
        /// <param name="password">The password of the user to act as.</param>
        /// <param name="logonType">Type of impersonation used for impersonating.</param>
        private void ImpersonateValidUser(
            string userName,
            string domain,
            string password,
            Logon32LogonType logonType)
        {
            WindowsIdentity tempWindowsIdentity = null;
            IntPtr token = IntPtr.Zero;
            IntPtr tokenDuplicate = IntPtr.Zero;

            try
            {
                if ( RevertToSelf() )
                {
                    LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, "Using LogonType = {0}", logonType);
                    if (LogonUser(
                        userName,
                        domain,
                        password,
                        ( int )logonType,
                        logonType == Logon32LogonType.NewCredentials ? LOGON32_PROVIDER_WINNT50 : LOGON32_PROVIDER_DEFAULT,
                        ref token) != 0)
                    {
                        if (DuplicateToken(token, 2, ref tokenDuplicate) != 0)
                        {
                            tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                            if( !tempWindowsIdentity.IsGuest )
                            {
                                impersonationContext = tempWindowsIdentity.Impersonate();
                                LogHelper.Trace( TraceEventType.Information, TraceCategory.Descent, "Impersonated successfully as '{0}'", userName );
                            }
                            else
                            {
                                LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent, "LogonUser has been given Guest account for login '{0}'. Skipping impersonation to Guest.", userName );
                            }
                        }
                        else
                        {
                            Win32Exception ex = new Win32Exception( Marshal.GetLastWin32Error() );
                            LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent, "DuplicateToken failed: {0}", ex );
                            throw ex;
                        }
                    }
                    else
                    {
                        Win32Exception ex = new Win32Exception( Marshal.GetLastWin32Error() );
                        LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent, "LogonUser failed: {0}", ex );
                        throw ex;
                    }
                }
                else
                {
                    Win32Exception ex = new Win32Exception( Marshal.GetLastWin32Error() );
                    LogHelper.Trace( TraceEventType.Verbose, TraceCategory.Descent, "RevertToSelf failed: {0}", ex );
                    throw ex;
                }
            }
            finally
            {
                if (token != IntPtr.Zero)
                {
                    CloseHandle(token);
                }
                if (tokenDuplicate != IntPtr.Zero)
                {
                    CloseHandle(tokenDuplicate);
                }
            }
        }

        /// <summary>
        /// Reverts the impersonation.
        /// </summary>
        private void UndoImpersonation()
        {
            if (impersonationContext != null)
            {
                impersonationContext.Undo();
                impersonationContext = null;
            }
        }

        private WindowsImpersonationContext impersonationContext = null;

    }
}
