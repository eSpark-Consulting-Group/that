using APTAT.Common;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public interface IDescentImpersonationHelper : IDisposable
    {
        string Impersonate(IRemoteHostInfo hostInfo, CancellationToken cancellationToken, bool impersonateAsNetworkService);
        void SetLogonType(ImpersonationHelper.Logon32LogonType logonType);
    }
    internal class DescentImpersonationHelper : IDescentImpersonationHelper
    {
        private readonly NetworkHelper _networkHelper = new NetworkHelper();
        private readonly INetworkObserver _networkObserver;
        

        public DescentImpersonationHelper(INetworkObserver networkObserver)
        {
            _networkObserver = networkObserver;
            _networkHelper.LogonType = ImpersonationHelper.Logon32LogonType.NewCredentials;
        }

        private static bool ExtractUserNameAndDomain(string login, out string userName, out string domain)
        {
            userName = "";
            domain = "";
            if (login == null) return false;
            string[] splitted = login.Split('\\');
            if (splitted.Length == 2)
            {
                userName = splitted[1];
                domain = splitted[0];
                return true;
            }

            splitted = login.Split('@');
            if (splitted.Length == 2)
            {
                userName = splitted[0];
                domain = splitted[1];
                return true;
            }

            userName = "";
            domain = "";
            return false;
        }

        private Win32Exception _loginException;
        private bool TryLogin(IRemoteHostInfo hostInfo, bool impersonateAsNetworkService, string login, string remoteAdminPath)
        {
            _loginException = null;

            Stopwatch s = new Stopwatch();
            s.Start();

            try
            {
                _networkHelper.GrantNetworkAccessToRemoteComputer(
                    login,
                    hostInfo.Password,
                    remoteAdminPath,
                    impersonateAsNetworkService);
            }
            catch (Win32Exception ex)
            {
                _loginException = ex;
                return false;
            }
            finally
            {
                LogHelper.Trace(TraceEventType.Verbose, $"GrantNetworkAccessToRemoteComputer elapsed {s.Elapsed}");
                s.Stop();
            }
            return true;
        }

        private void InitializeInternal(IRemoteHostInfo hostInfo, CancellationToken cancellationToken
            , bool impersonateAsNetworkService, string remoteAdminPath)
        {
            string userName, domain;
            if (hostInfo.IsLocalComputer && TryLogin(hostInfo, impersonateAsNetworkService, null, remoteAdminPath))
                return;

            if (ExtractUserNameAndDomain(hostInfo.Login, out userName, out domain))
            {
                string login;

                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                login = $"{domain}\\{userName}";
                if (TryLogin(hostInfo, impersonateAsNetworkService, login, remoteAdminPath))
                    return;

                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                login = $"{userName}@{domain}";
                if (TryLogin(hostInfo, impersonateAsNetworkService, login, remoteAdminPath))
                    return;

                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                if (hostInfo.Login != login)
                {
                    if (TryLogin(hostInfo, impersonateAsNetworkService, hostInfo.Login, remoteAdminPath))
                        return;
                }
            }
            else
            {
                //if (TryAlternativeLogin1(impersonateAsNetworkService))
                //    return;

                //if (TryAlternativeLogin2(impersonateAsNetworkService))
                //    return;

                int attemptsCount = 3;
                while (attemptsCount > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();
                    if (TryLogin(hostInfo, impersonateAsNetworkService, hostInfo.Login, remoteAdminPath))
                        return;
                    attemptsCount--;
                    Thread.Sleep(1000);
                    if (cancellationToken.IsCancellationRequested)
                        throw new OperationCanceledException();
                }
            }

            if (_loginException != null)
            {
                throw new DescentException(_loginException);
            }

        }

        public string Impersonate(IRemoteHostInfo hostInfo, CancellationToken cancellationToken, bool impersonateAsNetworkService)
        {
            string remotePath = $@"\\{hostInfo.HostName}\ADMIN$";
            try
            {
                InitializeInternal(hostInfo, cancellationToken, impersonateAsNetworkService, remotePath);
                return hostInfo.HostName;
            }
            catch (DescentException ex)
            {
                if (ex._win32error != NetworkHelper.ERROR_DUP_NAME)
                    throw;
                if (cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException();

                // try to avoid specific network problem
                // Just using IP address instead of computer name
                IPAddress[] ips = _networkObserver.SafeResolveDns(hostInfo.HostName);
                if (ips == null || ips.Length == 0)
                {
                    throw new DescentException(DescentStatus.status_cannot_resolve_remote_hostname,
                        hostInfo.HostName, 0);
                }
                var alternativeHostName = ips[0].ToString();
                remotePath = $@"\\{alternativeHostName}\ADMIN$";
                InitializeInternal(hostInfo, cancellationToken, impersonateAsNetworkService, remotePath);
                return alternativeHostName;
            }
        }

        public void Dispose()
        {
            _networkHelper.Release();
        }

        public void SetLogonType(ImpersonationHelper.Logon32LogonType logonType)
        {
            _networkHelper.LogonType = logonType;
        }
    }
}
