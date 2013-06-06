using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using APTAT.Core;

namespace APTAT.Core.Descent
{
	namespace Transport
	{

		public class DescentHelper
		{
			private readonly List<string> _filesToDeleteOnQuit = new List<string>();
			private readonly ServiceHelper _serviceHelper = new ServiceHelper();

            private const string ScannerServiceFileName = "ScannerService.exe";
			private const string ScanStatusFileName = "scan_status.txt";
            private const string ScanResultsFileName = "out.xml";

			private RemoteHostInfo _remoteHostInfo;
			private string _oldRemoteHostName; // old name without changing it to IP address
			private string _remoteAdminPath;
			private string _remoteWorkingDirectory;
			private string _remoteHost;

			private const string AptScannerTempFolderName = "ComodoAptAtScanner";
            private const string RemoteWorkingDirectoryAsLocal = @"%SYSTEMROOT%\ComodoAptAtScanner";

			private readonly NetworkHelper _networkHelper = new NetworkHelper();

			public class RemoteResponse
			{
				public DescentStatus Status;
				public int Win32Error;
				public string ErrorString;
				public string Response;
			};


			RemoteResponse _response = new RemoteResponse();

			public RemoteResponse Response
			{
				get
				{
					return _response;
				}
			}


			public DescentHelper( RemoteHostInfo info )
			{
				_remoteHostInfo = info;
				_oldRemoteHostName = info.HostName;
				InitPaths();
			}

			private void InitPaths()
			{
				_remoteHost = string.Format( @"\\{0}", _remoteHostInfo.HostName );
				_remoteAdminPath = string.Format( @"\\{0}\ADMIN$", _remoteHostInfo.HostName );
				_remoteWorkingDirectory = _remoteAdminPath + @"\" + AptScannerTempFolderName;
			}

			private static bool ExtractUserNameAndDomain( string login, out string userName, out string domain )
			{
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
						_remoteHostInfo.Password,
						_remoteAdminPath,
						impersonateAsNetworkService );
				}
				catch ( Win32Exception ex )
				{
					_loginException = ex;
					return false;
				}

				return true;
			}

			private bool TryAlternativeLogin1(bool impersonateAsNetworkService)
			{
				// Try alternative way
				// HostName\\Login
				string login = _remoteHostInfo.HostName + '\\' + _remoteHostInfo.Login;
				return TryLogin(impersonateAsNetworkService, login);
			}

			private bool TryAlternativeLogin2( bool impersonateAsNetworkService )
			{
				// Try alternative way
				// Login@HostName
				string login = _remoteHostInfo.Login + "@" + _remoteHostInfo.HostName;
				return TryLogin( impersonateAsNetworkService, login );
			}

			private void InitializeInternal( bool impersonateAsNetworkService )
			{
				string userName, domain;
				if ( ExtractUserNameAndDomain( _remoteHostInfo.Login, out userName, out domain ) )
				{
					string login;

					login = string.Format("{0}\\{1}", domain, userName);
					if (TryLogin(impersonateAsNetworkService, login))
						return;

					login = string.Format("{0}@{1}", userName, domain);
					if (TryLogin(impersonateAsNetworkService, login))
						return;

					if ( _remoteHostInfo.Login != login )
					{
						if ( TryLogin( impersonateAsNetworkService, _remoteHostInfo.Login ) )
							return;
					}
				}
				else
				{
					if (TryAlternativeLogin1(impersonateAsNetworkService))
						return;

					if (TryAlternativeLogin2(impersonateAsNetworkService))
						return;

					if(TryLogin( impersonateAsNetworkService, _remoteHostInfo.Login ))
						return;
				}

				if ( _loginException != null )
				{
					throw new DescentException( _loginException );
				}

			}

			private void SetHostName( string hostName )
			{
				_remoteHostInfo.HostName = hostName;
				InitPaths();
			}

			private void InitWithHostName( bool impersonateAsNetworkService )
			{
				bool dnsResolvingNeeded = false;
				try
				{
					InitializeInternal( impersonateAsNetworkService );
					InvokeRemotelyConnected( EventArgs.Empty );
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
					IPAddress[] ips = NetHelp.SafeResolveDns( _remoteHostInfo.HostName );
					if( ips == null || ips.Length == 0 )
					{
						throw new DescentException( DescentStatus.status_cannot_resolve_remote_hostname,
							_remoteHostInfo.HostName, 0 );
					}

					SetHostName( ips[0].ToString() );
					InitializeInternal( impersonateAsNetworkService );
					InvokeRemotelyConnected( EventArgs.Empty );
				}

				InitializeDescentAgentService( ScannerServiceFileName );
			}

			public event EventHandler RemotelyConnected;

			public event EventHandler DescentHelperCopied;

			public event EventHandler DescentHelperParametersCopied;

			public event EventHandler DescentHelperStarted;

			private void InvokeRemotelyConnected( EventArgs e )
			{
				EventHandler handler = RemotelyConnected;
				if( handler != null )
					handler( this, e );
			}

			private void InvokeDescentHelperCopied( EventArgs e )
			{
				EventHandler handler = DescentHelperCopied;
				if( handler != null )
					handler( this, e );
			}

			private void InvokeDescentHelperParametersCopied( EventArgs e )
			{
				EventHandler handler = DescentHelperParametersCopied;
				if( handler != null )
					handler( this, e );
			}
            
			private void InvokeDescentHelperStarted( EventArgs e )
			{
				EventHandler handler = DescentHelperStarted;
				if (handler != null) handler(this, e);
			}

            //======================================================================
            public class AptScanEventArgs : EventArgs
            {
                public string HostName { get; set; }
                public Guid TargetUid { get; set; }
                public int TotalFilesScanned { get; set; }
                public int UnknownFilesFound { get; set; }
            }

            // New events
		    public EventHandler<EventArgs> ScanStarted;
            private void InvokeScanStarted(EventArgs e)
            {
                EventHandler<EventArgs> temp = ScanStarted;
                if (temp != null)
                    temp(this, e);
            }

            public EventHandler<ScanProgressChangedEventArgs> ScanProgressChanged;
            private void InvokeScanProgressChanged(ScanProgressChangedEventArgs e)
            {
                EventHandler<ScanProgressChangedEventArgs> temp = ScanProgressChanged;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanProgressChangedEventArgs : AptScanEventArgs
            {
            }

            public EventHandler<ScanFailedEventArgs> ScanFailed;
            private void InvokeScanFailed(ScanFailedEventArgs e)
            {
                EventHandler<ScanFailedEventArgs> temp = ScanFailed;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanFailedEventArgs : AptScanEventArgs
            {
                public DescentStatus Status { get; set; }
            }

            public EventHandler<ScanCompletedEventArgs> ScanCompleted;
            private void InvokeScanCompleted(ScanCompletedEventArgs e)
            {
                EventHandler<ScanCompletedEventArgs> temp = ScanCompleted;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanCompletedEventArgs : AptScanEventArgs
            {
            }

            public EventHandler<ScanResultsFileReceivedEventArgs> ScanResultsFileReceived;
            private void InvokeScanResultsFileReceived(ScanResultsFileReceivedEventArgs e)
            {
                EventHandler<ScanResultsFileReceivedEventArgs> temp = ScanResultsFileReceived;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanResultsFileReceivedEventArgs : AptScanEventArgs
            {
                public string FileName { get; set; }
            }
            //======================================================================

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
				string hostNameWithoutSuffix = NetHelp.CutDnsSuffixFromHostName( _oldRemoteHostName );
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
				_networkHelper.LogonType = logonType;
				DescentException originalException;
				// First, try traditional impersonation as network service
                try
                {
                    Init(true);
                    return;
                }
                catch (DescentException exception)
                {
                    originalException = exception;
                }

				// We failed. Trying to impersonate as actual user
				try
				{
					Init( false );
				}
                catch (DescentException exception)
                {
                    throw;
                    // TODO: Commented
                    //// Failed. Throw original exception to not to confuse user
                    //throw originalException;
                }
			}


			~DescentHelper()
			{
				Release();
			}

			public void Release()
			{
				try
				{
					foreach ( string targetPath in _filesToDeleteOnQuit )
					{
						File.Delete( targetPath );
					}
				}
				catch ( Exception )
				{
				}

				DeleteDescentAgentService();
				_serviceHelper.Release();
				_networkHelper.Release();
			}
			// ===============================================================
            // TODO: Commented
            //private void PutServerCredentialsOnTheAgentComputer()
            //{
            //    const bool deleteFilesAfterSessionEnded = false; // do not delete for debug purposes

            //    // Get public key
            //    byte[] publicKey, privateKey;
            //    DsaKeysLoader.GetDsaKeys( out privateKey, out publicKey );

            //    // Store server connection settings
            //    FileStream fs = CreateRemoteFile( "server.ini", deleteFilesAfterSessionEnded );
            //    StreamWriter writer = new StreamWriter( fs );

            //    writer.WriteLine( "[General]" );

            //    // Server public key in Base64
            //    writer.WriteLine( "PublicKey={0}", Convert.ToBase64String( publicKey ) );

            //    string[] addresses = NetHelp.GetSupportedExternalAddresses(false);

            //    // Host name
            //    writer.WriteLine( "SERVERHOST={0}", addresses[0] );

            //    // Alternative ip addresses
            //    for (int index = 1; index < addresses.Length; index++)
            //    {
            //        string address = addresses[index];
            //        writer.WriteLine( "Address{0}={1}", index, address );
            //    }
            //    writer.Close();
            //}

			private void InitializeDescentAgentService( string descentAgentPath )
			{
				try
				{
					CopyFile( descentAgentPath, false );
					InvokeDescentHelperCopied( EventArgs.Empty );
                    //PutServerCredentialsOnTheAgentComputer();
					InvokeDescentHelperParametersCopied( EventArgs.Empty );

					_serviceHelper.InitializeService(
						_remoteHost,
						RemoteWorkingDirectoryAsLocal + @"\" + Path.GetFileName( descentAgentPath ),
                        "ScannerService.exe",
						"Comodo APT Scanner"
						);

                    //InvokeDescentHelperStarted( EventArgs.Empty );
				}
				catch ( Win32Exception ex )
				{
					throw new DescentException( DescentStatus.status_remote_cannot_setup_descent_service,
						ex.Message, ex.NativeErrorCode );
				}
			}

			private void DeleteDescentAgentService()
			{
				try
				{
                    try
                    {
                        _serviceHelper.StopService();
                    }
                    catch (Win32Exception exception)
                    {
                    }

					_serviceHelper.DeleteService();
				}
				catch ( Win32Exception ex )
				{
					throw new DescentException( DescentStatus.status_remote_cannot_cleanup_descent_service,
						ex.Message, ex.NativeErrorCode );
				}
			}

			public void ExecuteAction( string actionName, string[] parameters, int timeoutInMs )
			{
				List<string> args = new List<string>();

				const string responseFilePathForRemoteComputer = AptScannerTempFolderName + @"\" + ScanStatusFileName;
				string responseFilePathForCurrentComputer = _remoteWorkingDirectory + @"\" + ScanStatusFileName;
                string resultsFile = _remoteWorkingDirectory + @"\" + ScanResultsFileName;

				// Delete response file if it exists
				if( File.Exists( responseFilePathForCurrentComputer ) )
					File.Delete( responseFilePathForCurrentComputer );

                args.Add(responseFilePathForCurrentComputer);
                args.Add(resultsFile);

				args.Add( ScannerServiceFileName ); // file name
				args.Add( "process_response_action" ); // command
				args.Add( actionName ); // action name
				args.Add( responseFilePathForRemoteComputer ); // response file name
				args.Add( timeoutInMs.ToString() ); // timeout in ms

				foreach( string param in parameters )
				{
					args.Add( param );
				}

				try
				{
					_serviceHelper.RunService( args.ToArray(), true, timeoutInMs );
				}
				catch( Win32Exception ex )
				{
					throw new DescentException( DescentStatus.status_remote_cannot_run_descent_service,
						ex.Message, ex.NativeErrorCode );
				}


                int attemptsCount = 20;
                while (true/*attemptsCount > 0*/)
                {
                    if (attemptsCount > 0)
                        --attemptsCount;

                    try
                    {
                        using(FileStream fs = new FileStream(responseFilePathForCurrentComputer, FileMode.Open, FileAccess.Read, FileShare.Write))
                        {
                            byte[] buf = new byte[fs.Length];
                            fs.Read(buf, 0, (int)fs.Length);
                            string data = Encoding.Unicode.GetString(buf);
                            if(String.IsNullOrEmpty(data))
                                throw new Exception("Response file is empty");

                            string[] temp = data.Split(';');
                            if(temp.Length != 3)
                                throw new Exception("Response file header is not valid");

                            int status;
                            Int32.TryParse(temp[0], out status);

                            int totalFiles;
                            Int32.TryParse(temp[1], out totalFiles);

                            int unknownFiles;
                            Int32.TryParse(temp[2], out unknownFiles);

                            _response.Status = (DescentStatus)status;

                            if(status == 0) // In progress
                            {
                                InvokeScanProgressChanged(new ScanProgressChangedEventArgs() { TargetUid = _remoteHostInfo.TargetUid, HostName = _remoteHostInfo.HostName, TotalFilesScanned = totalFiles, UnknownFilesFound = unknownFiles });
                            }
                            else if(status == 1) // Completed
                            {
                                InvokeScanCompleted(new ScanCompletedEventArgs() { TotalFilesScanned = totalFiles, UnknownFilesFound = unknownFiles });
                                
                                string resultsFileLocal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, String.Concat(_remoteHostInfo.TargetUid.ToString(), ".xml"));
                                File.Copy(resultsFile, resultsFileLocal, true);
                                
                                InvokeScanResultsFileReceived(new ScanResultsFileReceivedEventArgs() { TargetUid = _remoteHostInfo.TargetUid, HostName = _remoteHostInfo.HostName, FileName = resultsFileLocal, TotalFilesScanned = totalFiles, UnknownFilesFound = unknownFiles });
                                break;
                            }
                            else if(status == 2) // Error (>=2)
                            {
                                //InvokeScanFailed(new ScanFailedEventArgs() { Status = (DescentStatus)status });
                                throw new DescentException((DescentStatus)status, "Error returned from a remote computer. See the status for details.", 0);
                                //break;
                            }
                            
                            Thread.Sleep(1000);
                        }
                        

                        //using (TextReader sr = new StreamReader(responseFilePathForCurrentComputer,
                        //    System.Text.Encoding.ASCII))
                        //{
                        //    string line = sr.ReadLine();
                        //    if (line == null)
                        //    {
                        //        throw new Exception("Response file is empty");
                        //    }

                        //    int status;
                        //    Int32.TryParse(line, out status);

                        //    if(status == 100)
                        //    {
                        //        sr.Close();
                        //        break;
                        //    }
                        //    else
                        //    {
                        //        sr.Close();
                        //        Thread.Sleep(1000);
                        //    }

                        //    //string[] splitted = line.Split(';', '#');
                        //    //if (splitted.Length != 3)
                        //    //{
                        //    //    throw new Exception("Response file header is not valid");
                        //    //}

                        //    //_response.Status = (DescentStatus)Int32.Parse(splitted[0]);
                        //    //_response.Win32Error = Int32.Parse(splitted[1]);
                        //    //_response.ErrorString = splitted[2];
                        //    //_response.Response = "";

                        //    //while ((line = sr.ReadLine()) != null)
                        //    //{
                        //    //    _response.Response += line;
                        //    //}
                        //}
                    }
                    catch (FileNotFoundException ex)
                    {
                        // If file is not found, we can sleep and try again
                        if (attemptsCount == 0)
                        {
                            // unless attempts count is zero...
                            const DescentStatus status = DescentStatus.status_remote_invalid_response;
                            InvokeScanFailed(new ScanFailedEventArgs() { Status = status, HostName = _remoteHostInfo.HostName, TargetUid = _remoteHostInfo.TargetUid});
                            throw new DescentException(status, ex.Message, 0);
                        }
                        else
                        {
                            // sleep and try again
                            Thread.Sleep(1000);
                        }
                    }
                    catch (Exception ex)
                    {
                        const DescentStatus status = DescentStatus.status_remote_invalid_response;
                        InvokeScanFailed(new ScanFailedEventArgs() { Status = status, HostName = _remoteHostInfo.HostName, TargetUid = _remoteHostInfo.TargetUid });
                        throw new DescentException(status, ex.Message, 0);
                    }
                }

				// Read the response file contents
                //int attemptsCount = 20;
                //while( attemptsCount > 0 )
                //{
                //    --attemptsCount;
                //    try
                //    {
                //        using( TextReader sr = new StreamReader( responseFilePathForCurrentComputer,
                //            System.Text.Encoding.Unicode ) )
                //        {
                //            string line;
                //            line = sr.ReadLine();
                //            if( line == null )
                //            {
                //                throw new Exception( "Response file is empty" );
                //            }

                //            string[] splitted = line.Split( ';', '#' );
                //            if( splitted.Length != 3 )
                //            {
                //                throw new Exception( "Response file header is not valid" );
                //            }

                //            _response.Status = ( DescentStatus )Int32.Parse( splitted[0] );
                //            _response.Win32Error = Int32.Parse( splitted[1] );
                //            _response.ErrorString = splitted[2];
                //            _response.Response = "";

                //            while( ( line = sr.ReadLine() ) != null )
                //            {
                //                _response.Response += line;
                //            }

                //            sr.Close();
                //            break;
                //        }
                //    }
                //    catch( FileNotFoundException ex )
                //    {
                //        // If file is not found, we can sleep and try again
                //        if( attemptsCount == 0 )
                //        {
                //            // unless attempts count is zero...
                //            throw new DescentException( DescentStatus.status_remote_invalid_response,
                //                ex.Message, 0 );
                //        }
                //        else
                //        {
                //            // sleep and try again
                //            Thread.Sleep( 1000 );
                //        }
                //    }
                //    catch( Exception ex )
                //    {
                //        throw new DescentException( DescentStatus.status_remote_invalid_response,
                //            ex.Message, 0 );
                //    }
                //}
			}

			// Create remote path on remote host (to c:\windows\ComodoESMAgent\)
			public FileStream CreateRemoteFile( string fileName, bool deleteOnQuit )
			{
				try
				{
					// Create directory ESMAgent
					if ( System.IO.Directory.Exists( _remoteWorkingDirectory ) == false )
						System.IO.Directory.CreateDirectory( _remoteWorkingDirectory );

					// Copy file to the target directory
					string targetPath = _remoteWorkingDirectory + @"\" + fileName;
					FileStream fs = new FileStream( targetPath, FileMode.Create );

					if ( deleteOnQuit )
					{
						_filesToDeleteOnQuit.Add( targetPath );
					}

					return fs;
				}
				catch ( Exception ex )
				{
					throw new DescentException( DescentStatus.status_remote_cannot_copy_file,
						ex.Message, 0 );
				}
			}

            public void StopService()
            {
                _serviceHelper.StopService();
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
						if ( System.IO.Directory.Exists( _remoteWorkingDirectory ) == false )
							System.IO.Directory.CreateDirectory( _remoteWorkingDirectory );

						// Copy file to the target directory
						string targetPath = _remoteWorkingDirectory + @"\" + Path.GetFileName( filePath );
						File.Copy( filePath, targetPath, true );

						if ( deleteOnQuit )
						{
							_filesToDeleteOnQuit.Add( targetPath );
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
		}
	}
}
