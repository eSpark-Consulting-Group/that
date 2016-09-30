using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using APTAT.Common;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;

namespace APTAT.DeploymentCore.Descent.Helpers
{
	namespace Transport
	{

		public abstract class DescentHelper
		{
			private readonly List<string> _filesToDeleteOnQuit = new List<string>();
            protected readonly ServiceHelper ServiceHelper = new ServiceHelper();
            private bool _objectDestroyed = false;
            protected RemoteHostInfo RemoteHostInfo;
			private readonly string _oldRemoteHostName; // old name without changing it to IP address
			protected string RemoteAdminPath;
            public string RemoteWorkingDirectory;
            protected string RemoteHost;
			public string TempFolderName = "ComodoRemoteDeployment";
            private readonly NetworkHelper _networkHelper = new NetworkHelper();
		    protected string PackageFileName;
		    protected readonly INetworkObserver NetworkObserver;

            public class RemoteResponse
			{
				public DescentStatus Status;
				public int Win32Error;
				public string ErrorString;
				public string Response;
			};


		    protected readonly RemoteResponse _response = new RemoteResponse();

			public RemoteResponse Response
			{
				get
				{
					return _response;
				}
			}

		    protected virtual string[] GetAdditionalFilesToCopy(bool is64)
		    {
		        return new string[0];
		    }

		    protected virtual string ServiceDisplayName
		    {
		        get { return "Comodo Deployment Helper"; }
            }
            protected virtual string ServiceName { get { return "Helper.exe"; } }

            private string RemoteWorkingDirectoryAsLocal
		    {
                get
                {
                    return @"%SYSTEMROOT%\" + TempFolderName;
                }
		    }


            public DescentHelper(RemoteHostInfo info)
		    {
		        NetworkObserver = CompositionHelper.Instance.Resolve<INetworkObserver>();
				RemoteHostInfo = info;
				_oldRemoteHostName = info.HostName;
				InitPaths();
			}

			protected virtual void InitPaths()
			{
                RemoteHost = string.Format(@"\\{0}", RemoteHostInfo.HostName);
                RemoteAdminPath = string.Format(@"\\{0}\ADMIN$", RemoteHostInfo.HostName);
                RemoteWorkingDirectory = RemoteAdminPath + @"\" + TempFolderName;
            }

			private static bool ExtractUserNameAndDomain( string login, out string userName, out string domain )
			{
                userName = "";
                domain = "";
                if (login == null) return false;
				string[] splitted = login.Split( '\\' );
				if ( splitted.Length == 2 )
				{
					userName = splitted[1];
					domain = splitted[0];
					return true;
				}

				splitted = login.Split( '@' );
				if ( splitted.Length == 2 )
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
			private bool TryLogin( bool impersonateAsNetworkService, string login)
			{
				_loginException = null;

				try
				{
					_networkHelper.GrantNetworkAccessToRemoteComputer(
						login,
						RemoteHostInfo.Password,
						RemoteAdminPath,
						impersonateAsNetworkService );
				}
				catch ( Win32Exception ex )
				{
					_loginException = ex;
					return false;
				}

				return true;
			}

			private void InitializeInternal( bool impersonateAsNetworkService )
			{
				string userName, domain;
                if (RemoteHostInfo.IsLocalComputer && TryLogin(impersonateAsNetworkService, null))
                    return;

                if ( ExtractUserNameAndDomain( RemoteHostInfo.Login, out userName, out domain ) )
				{
					string login;

					login = string.Format("{0}\\{1}", domain, userName);
					if (TryLogin(impersonateAsNetworkService, login))
						return;

					login = string.Format("{0}@{1}", userName, domain);
					if (TryLogin(impersonateAsNetworkService, login))
						return;

					if ( RemoteHostInfo.Login != login )
					{
						if ( TryLogin( impersonateAsNetworkService, RemoteHostInfo.Login ) )
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
                        if (TryLogin(impersonateAsNetworkService, RemoteHostInfo.Login))
                            return;
                        attemptsCount--;
                        Thread.Sleep(1000);
                    }
				}

				if ( _loginException != null )
				{
					throw new DescentException( _loginException );
				}

			}

			private void SetHostName( string hostName )
			{
				RemoteHostInfo.HostName = hostName;
				InitPaths();
			}

			private void InitWithHostName( bool impersonateAsNetworkService )
			{
				bool dnsResolvingNeeded = false;
				try
				{
					InitializeInternal( impersonateAsNetworkService );
					InvokeRemotelyConnected( new DescentHelperEventArgs(RemoteHostInfo.TargetUid) );
				}
				catch( DescentException ex )
				{
				    if( ex._win32error != NetworkHelper.ERROR_DUP_NAME )
						throw;
				    dnsResolvingNeeded = true;
				}

			    if( dnsResolvingNeeded )
				{
					// try to avoid specific network problem
					// Just using IP address instead of computer name
					IPAddress[] ips = NetworkObserver.SafeResolveDns( RemoteHostInfo.HostName );
					if( ips == null || ips.Length == 0 )
					{
						throw new DescentException( DescentStatus.status_cannot_resolve_remote_hostname,
							RemoteHostInfo.HostName, 0 );
					}

					SetHostName( ips[0].ToString() );
					InitializeInternal( impersonateAsNetworkService );
					InvokeRemotelyConnected(new DescentHelperEventArgs(RemoteHostInfo.TargetUid));
				}


			    bool isx64 = System.IO.Directory.Exists(Path.Combine(RemoteAdminPath, "SysWOW64"));
			    PackageFileName = GetPackageFileName(isx64);

                InitializeDescentAgentService(PackageFileName);
			}

			public event EventHandler<DescentHelperEventArgs> RemotelyConnected;

			public event EventHandler<DescentHelperEventArgs> DescentHelperCopied;

			public event EventHandler<DescentHelperEventArgs> DescentHelperParametersCopied;

            public event EventHandler<DescentHelperEventArgs> DescentHelperStarted;

            private void InvokeRemotelyConnected(DescentHelperEventArgs e )
			{
                EventHandler<DescentHelperEventArgs> handler = RemotelyConnected;
				if( handler != null )
					handler( this, e );
			}

			private void InvokeDescentHelperCopied(DescentHelperEventArgs e )
			{
                EventHandler<DescentHelperEventArgs> handler = DescentHelperCopied;
				if( handler != null )
					handler( this, e );
			}

			private void InvokeDescentHelperParametersCopied(DescentHelperEventArgs e )
			{
                EventHandler<DescentHelperEventArgs> handler = DescentHelperParametersCopied;
				if( handler != null )
					handler( this, e );
			}
            private void InvokeDescentHelperStarted(DescentHelperEventArgs e)
            {
                var handler = DescentHelperStarted;
                if (handler != null) handler(this, e);
            }

            private void Init( bool impersonateAsNetworkService )
			{
				SetHostName( _oldRemoteHostName );
				DescentException originalException;
				try
				{
					InitWithHostName( impersonateAsNetworkService );
					return;
				}
				catch ( DescentException ex )
				{
					if( ex._status == DescentStatus.status_remote_cannot_copy_file )
					{
						// File copy IO failure - no need try any more
						throw;
					}

					originalException = ex;
				}

				// try to use dns name without domain suffix
				string hostNameWithoutSuffix = NetworkObserver.CutDnsSuffixFromHostName( _oldRemoteHostName );
				if( hostNameWithoutSuffix != _oldRemoteHostName )
				{
					SetHostName( hostNameWithoutSuffix );
					InitWithHostName( impersonateAsNetworkService );
				}
				else
					throw originalException;
			}

			public void Init( ImpersonationHelper.Logon32LogonType logonType )
			{
                // Impersonation logic has been inverted as opposed to the code below.
                _networkHelper.LogonType = logonType;
                DescentException originalException;
                // First, try impersonate as actual user
                try
                {
                    Init(false);
                    return;
                }
                catch (DescentException exception)
                {
                    originalException = exception;
                }

                // We failed. Trying to impersonate as network service
                try
                {
                    Init(true);
                }
                catch (DescentException)
                {
                    //throw;
                    // TODO: Commented
                    // Failed. Throw original exception to not to confuse user
                    throw originalException;
                }
			}


			~DescentHelper()
			{
                if (!_objectDestroyed)
				    Release();
			}

		    public void Release()
			{
		        try
		        {
                    DeleteDescentAgentService();
                }
                catch (Exception)
		        {
		        }
				ServiceHelper.Release();
				
                // Remove file system content
                try
                {
                    CleanUpRemoteContent();

                    //foreach ( string targetPath in _filesToDeleteOnQuit )
                    //{
                    //    if(File.Exists(targetPath))
                    //        File.Delete( targetPath );
                    //}
                }
                catch (Exception)
                {
                }

                _networkHelper.Release();

                _objectDestroyed = true;
            }


			private void InitializeDescentAgentService( string descentAgentPath)
			{
				try
				{
                    bool isx64 = System.IO.Directory.Exists(Path.Combine(RemoteAdminPath, "SysWOW64"));
                    CleanUpRemoteContent();
					CopyFile( descentAgentPath, true ); // "true" means remove after operation is completed
				    foreach (var filePath in GetAdditionalFilesToCopy(isx64))
				    {
                        CopyFile(filePath, true);
                    }
					InvokeDescentHelperCopied(new DescentHelperEventArgs(RemoteHostInfo.TargetUid));
                    //PutServerCredentialsOnTheAgentComputer();
					InvokeDescentHelperParametersCopied(new DescentHelperEventArgs(RemoteHostInfo.TargetUid));

					ServiceHelper.InitializeService(
						RemoteHost,
						RemoteWorkingDirectoryAsLocal + @"\" + Path.GetFileName( descentAgentPath ),
                        ServiceName,
						ServiceDisplayName,
                        GetServiceParams()
						);

                    InvokeDescentHelperStarted( new DescentHelperEventArgs(RemoteHostInfo.TargetUid) );
				}
				catch ( Win32Exception ex )
				{
					throw new DescentException( DescentStatus.status_remote_cannot_setup_descent_service,
						ex.Message, ex.NativeErrorCode );
				}
			}

            protected virtual string[] GetServiceParams()
            {
                List<string> serviceParams = new List<string>();

                return serviceParams.ToArray();
            }

		    protected abstract string GetPackageFileName(bool is64);

            private void CleanUpRemoteContent()
            {
                try
                {
                    if (System.IO.Directory.Exists(RemoteWorkingDirectory))
                        System.IO.Directory.Delete(RemoteWorkingDirectory, true);
                }
                catch (Exception)
                {
                }
            }

			private void DeleteDescentAgentService()
			{
				try
				{
                    try
                    {
                        ServiceHelper.StopService();
                    }
                    catch (Win32Exception ex)
                    {
                        LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent,$"Cannot stop remote service ({ex.NativeErrorCode})");
                    }

					ServiceHelper.DeleteService();
				}
				catch ( Win32Exception ex )
                {
                    LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, $"Cannot delete remote service ({ex.NativeErrorCode})");
                    throw new DescentException( DescentStatus.status_remote_cannot_cleanup_descent_service,
						ex.Message, ex.NativeErrorCode );
				}
			}

		    protected virtual void CleanUpBeforeActionStarted()
		    {
		        
		    }

		    public virtual void ExecuteAction(string actionName, string[] parameters, int timeoutInMs)
		    {
		        List<string> args = new List<string>();
                CleanUpBeforeActionStarted();
                
		        foreach (string param in parameters)
		        {
		            args.Add(param);
		        }

		        try
		        {
		            ServiceHelper.RunService(args.ToArray(), true, timeoutInMs);
		        }
		        catch (Win32Exception ex)
		        {
		            throw new DescentException(DescentStatus.status_remote_cannot_run_descent_service, ex.Message,
		                ex.NativeErrorCode);
		        }
		    }


		    // Copy file to remote host (to c:\windows\ComodoESMAgent\)
            public string CopyFile( string filePath, bool deleteOnQuit )
			{
				int attemptsCount = 5;

				while ( true )
				{
					try
					{
						// Create directory ESMAgent
						if ( System.IO.Directory.Exists( RemoteWorkingDirectory ) == false )
							System.IO.Directory.CreateDirectory( RemoteWorkingDirectory );

						// Copy file to the target directory
						string targetPath = RemoteWorkingDirectory + @"\" + Path.GetFileName( filePath );
						File.Copy( filePath, targetPath, true );

						if ( deleteOnQuit )
						{
                            AddFileToRemoveList(targetPath);
						}

						return targetPath;
					}
					catch ( Exception ex )
					{
						--attemptsCount;
						if ( attemptsCount == 0 )
						{
							throw new DescentException( DescentStatus.status_remote_cannot_copy_file,
								ex.Message, 0 );
						}

						Thread.Sleep( 2000 );
					}
				}
			}

            private void AddFileToRemoveList(string fileName)
            {
                _filesToDeleteOnQuit.Add(fileName);
            }
		}
	}
}
