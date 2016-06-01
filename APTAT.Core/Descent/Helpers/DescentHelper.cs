using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using APTAT.Core.Descent.Helpers;

namespace APTAT.Core.Descent
{
	namespace Transport
	{

		public class DescentHelper
		{
			private readonly List<string> _filesToDeleteOnQuit = new List<string>();
			private readonly ServiceHelper _serviceHelper = new ServiceHelper();

            //private const string ScannerServiceX86FileName = "ScannerService.exe";
            //private const string ScannerServiceX64FileName = "ScannerService64.exe";
            private const string ScannerServiceX86FileName = "cmdapt86.exe";
            private const string ScannerServiceX64FileName = "cmdapt64.exe";
            
			private const string ScanStatusFileName = "scan_status.txt";
            private const string ScanResultsFileName = "out.xml";
            private const string TvlFileName = "tvl.txt";
            private const string TrlFileName = "trl.txt";

            private const string ScopeParamFormat = "--scope \"{0}\"";
            private const string OutputFileParamFormat = "--output \"{0}\"";
            private const string StatusFileParamFormat = "--status \"{0}\"";
            private const string TvlParamFormat = "--tvl \"{0}\"";
            private const string TrlParamFormat = "--trl \"{0}\"";
            private const string TcpPortParamFormat = "--flsTcpPort {0}";
            private const string UdpPortParamFormat = "--flsUdpPort {0}";
            private const string SkipGacParam = "--skipGAC";

			private RemoteHostInfo _remoteHostInfo;
			private readonly string _oldRemoteHostName; // old name without changing it to IP address
			private string _remoteAdminPath;
			private string _remoteWorkingDirectory;
			private string _remoteHost;
            private string _responseFilePathForCurrentComputer;
            private string _resultsFile;
		    private string _tvlFile;
            private string _trlFile;

			private const string AptScannerTempFolderName = "ComodoAptAtScanner";
            private const string RemoteWorkingDirectoryAsLocal = @"%SYSTEMROOT%\ComodoAptAtScanner";

			private readonly NetworkHelper _networkHelper = new NetworkHelper();

            private readonly string _scanScope;

			public class RemoteResponse
			{
				public DescentStatus Status;
				public int Win32Error;
				public string ErrorString;
				public string Response;
			};


		    readonly RemoteResponse _response = new RemoteResponse();

			public RemoteResponse Response
			{
				get
				{
					return _response;
				}
			}


			public DescentHelper( RemoteHostInfo info, string scanScope, string tcpPort, string udpPort )
			{
			    _scanScope = scanScope;
			    _tcpPort = tcpPort;
			    _udpPort = udpPort;
				_remoteHostInfo = info;
				_oldRemoteHostName = info.HostName;
				InitPaths();
			}

			private void InitPaths()
			{
				_remoteHost = string.Format( @"\\{0}", _remoteHostInfo.HostName );
				_remoteAdminPath = string.Format( @"\\{0}\ADMIN$", _remoteHostInfo.HostName );
				_remoteWorkingDirectory = _remoteAdminPath + @"\" + AptScannerTempFolderName;
                _responseFilePathForCurrentComputer = _remoteWorkingDirectory + @"\" + ScanStatusFileName;
                _resultsFile = _remoteWorkingDirectory + @"\" + ScanResultsFileName;
                _tvlFile = _remoteWorkingDirectory + @"\" + TvlFileName;
                _trlFile = _remoteWorkingDirectory + @"\" + TrlFileName;
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
                    //if (TryAlternativeLogin1(impersonateAsNetworkService))
                    //    return;

                    //if (TryAlternativeLogin2(impersonateAsNetworkService))
                    //    return;

				    int attemptsCount = 3;
                    while (attemptsCount > 0)
                    {
                        if (TryLogin(impersonateAsNetworkService, _remoteHostInfo.Login))
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

                //========================================================================
                // Define remote OS platform (x86/x64)
			    string scannerFileName = ScannerServiceX86FileName;
                if (System.IO.Directory.Exists(Path.Combine(_remoteAdminPath, "SysWOW64")))
                    scannerFileName = ScannerServiceX64FileName;
                //========================================================================

                InitializeDescentAgentService(scannerFileName, TvlFileName, TrlFileName);
			}

			public event EventHandler RemotelyConnected;

			public event EventHandler DescentHelperCopied;

			public event EventHandler DescentHelperParametersCopied;

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

            //======================================================================
            public class AptEventArgs : EventArgs
            {
                public string HostName { get; set; }
                public Guid TargetUid { get; set; }
                public int EnumeratedFilesCount { get; set; }
                public long EnumeratedFilesSize { get; set; }
            }

            public class AptScanningEventArgs : AptEventArgs
            {
                public int TotalFilesScanned { get; set; }
                public int UnknownFilesFound { get; set; }
                public int SafeFilesFound { get; set; }
                public int MalwaresFound { get; set; }
                public int EmptyFilesFound { get; set; }
                public int SkippedFilesCount { get; set; }
                public int FailedToAnalyzeFilesCount { get; set; }
                public long TotalFilesSize { get; set; }
                public int NoVerdictFilesCount { get; set; }
                public int Percentage
                {
                    get
                    {
                        return (int)((double)TotalFilesScanned / EnumeratedFilesCount * 100);
                    }
                }
            }

            public EventHandler<ScanProgressChangedEventArgs> ScanProgressChanged;
            private void InvokeScanProgressChanged(ScanProgressChangedEventArgs e)
            {
                EventHandler<ScanProgressChangedEventArgs> temp = ScanProgressChanged;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanProgressChangedEventArgs : AptScanningEventArgs
            {
                
            }

            public EventHandler<ScanFailedEventArgs> ScanFailed;
            private void InvokeScanFailed(ScanFailedEventArgs e)
            {
                EventHandler<ScanFailedEventArgs> temp = ScanFailed;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanFailedEventArgs : AptScanningEventArgs
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
            public class ScanCompletedEventArgs : AptScanningEventArgs
            {
            }

            public EventHandler<ScanResultsFileReceivedEventArgs> ScanResultsFileReceived;
            private void InvokeScanResultsFileReceived(ScanResultsFileReceivedEventArgs e)
            {
                EventHandler<ScanResultsFileReceivedEventArgs> temp = ScanResultsFileReceived;
                if (temp != null)
                    temp(this, e);
            }
            public class ScanResultsFileReceivedEventArgs : AptScanningEventArgs
            {
                public string FileName { get; set; }
            }

            public EventHandler<EventArgs> ReceivingResults;
            private void InvokeReceivingResults(EventArgs e)
            {
                EventHandler<EventArgs> temp = ReceivingResults;
                if (temp != null)
                    temp(this, e);
            }

            public EventHandler<AptEnumeratingEventArgs> EnumerationProgressChanged;
            private void InvokeEnumerationProgressChanged(AptEnumeratingEventArgs e)
            {
                EventHandler<AptEnumeratingEventArgs> temp = EnumerationProgressChanged;
                if (temp != null)
                    temp(this, e);
            }

            public class AptEnumeratingEventArgs : AptEventArgs
            {
                public EnAptStage Stage { get; set; }
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
                catch (DescentException exception)
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

		    private bool _objectDestroyed = false;
		    private readonly string _udpPort;
		    private readonly string _tcpPort;

		    public void Release()
			{
				DeleteDescentAgentService();
				_serviceHelper.Release();
				
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


			private void InitializeDescentAgentService( string descentAgentPath, string tvlFilePath, string trlFilePath )
			{
				try
				{
				    CleanUpRemoteContent();
					CopyFile( descentAgentPath, true ); // "true" means remove after operation is completed
                    CopyFile( tvlFilePath, true ); // "true" means remove after operation is completed
                    CopyFile( trlFilePath, true ); // "true" means remove after operation is completed
					InvokeDescentHelperCopied( EventArgs.Empty );
                    //PutServerCredentialsOnTheAgentComputer();
					InvokeDescentHelperParametersCopied( EventArgs.Empty );

					_serviceHelper.InitializeService(
						_remoteHost,
						RemoteWorkingDirectoryAsLocal + @"\" + Path.GetFileName( descentAgentPath ),
                        "ScannerService.exe",
						"Comodo APT Scanner",
                        GetServiceParams()
						);

                    //InvokeDescentHelperStarted( EventArgs.Empty );
				}
				catch ( Win32Exception ex )
				{
					throw new DescentException( DescentStatus.status_remote_cannot_setup_descent_service,
						ex.Message, ex.NativeErrorCode );
				}
			}

            private string[] GetServiceParams()
            {
                List<string> serviceParams = new List<string>();
                serviceParams.Add(String.Format(ScopeParamFormat, _scanScope));
                serviceParams.Add(String.Format(StatusFileParamFormat, _responseFilePathForCurrentComputer));
                serviceParams.Add(String.Format(OutputFileParamFormat, _resultsFile));
                serviceParams.Add(String.Format(TvlParamFormat, _tvlFile));
                serviceParams.Add(String.Format(TrlParamFormat, _trlFile));
                serviceParams.Add("--filter \"*\"");
                serviceParams.Add("--scanPeOnly on");
                serviceParams.Add(String.Format(UdpPortParamFormat, _udpPort));
                serviceParams.Add(String.Format(TcpPortParamFormat, _tcpPort));
                
                if (Properties.Settings.Default.SkipGac)
                    serviceParams.Add(SkipGacParam);

                return serviceParams.ToArray();
            }

            private void CleanUpRemoteContent()
            {
                try
                {
                    if (System.IO.Directory.Exists(_remoteWorkingDirectory))
                        System.IO.Directory.Delete(_remoteWorkingDirectory, true);
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
                        _serviceHelper.StopService();
                    }
                    catch (Win32Exception)
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

			public void ExecuteAction( int timeoutInMs )
			{
				List<string> args = new List<string>();

				//const string responseFilePathForRemoteComputer = AptScannerTempFolderName + @"\" + ScanStatusFileName;
				
                //string responseFilePathForCurrentComputer = @"d:\" + ScanStatusFileName;
                //string resultsFile = @"d:\" + ScanResultsFileName;

                // Delete response file if it exists
				if( File.Exists( _responseFilePathForCurrentComputer ) )
					File.Delete( _responseFilePathForCurrentComputer );

                // Delete response file if it exists
                if (File.Exists(_resultsFile))
                    File.Delete(_resultsFile);

                //args.Add(scanScope);
                //args.Add(responseFilePathForCurrentComputer);
                //args.Add(resultsFile);

				try
				{
					_serviceHelper.RunService( args.ToArray(), true, timeoutInMs );
				}
				catch( Win32Exception ex )
				{
					throw new DescentException( DescentStatus.status_remote_cannot_run_descent_service,
						ex.Message, ex.NativeErrorCode );
				}


                int findFileAttemptsCount = 20;
                int readFileAttemptsCount = 20;
			    int checkFileHeaderAttemptsCount = 20;
                while (true/*attemptsCount > 0*/)
                {
                    if (findFileAttemptsCount > 0)
                        --findFileAttemptsCount;

                    try
                    {
                        using (FileStream fs = new FileStream(_responseFilePathForCurrentComputer, FileMode.Open,
                                FileAccess.Read, FileShare.ReadWrite))
                        {
                            byte[] buf;
                            try
                            {
                                buf = new byte[fs.Length];
                                fs.Read(buf, 0, (int) fs.Length);
                            }
                            catch (ArgumentException)
                            {
                                buf = new byte[0];
                            }

                            string data = Encoding.Unicode.GetString(buf);
                            if (String.IsNullOrEmpty(data))
                            {
                                if (readFileAttemptsCount > 0)
                                    --readFileAttemptsCount;
                                throw new StatusFileEmptyException();
                            }
                            readFileAttemptsCount = 20;

                            string[] temp = data.Split(';');
                            if (temp.Length != 14 && temp.Length != 13)
                            {
                                if (checkFileHeaderAttemptsCount > 0)
                                    --checkFileHeaderAttemptsCount;
                                throw new ResponseFileHeaderInvalidException();
                                //throw new Exception("Response file header is not valid");
                            }
                            checkFileHeaderAttemptsCount = 20;

                            int statusInt;
                            Int32.TryParse(temp[0], out statusInt);

                            int enumeratedCount;
                            Int32.TryParse(temp[1], out enumeratedCount);

                            long enumeratedSize;
                            Int64.TryParse(temp[2], out enumeratedSize);

                            int totalFiles;
                            Int32.TryParse(temp[3], out totalFiles); //scanned count

                            long totalFilesSize;
                            Int64.TryParse(temp[4], out totalFilesSize); //scanned size

                            int unknownFiles;
                            Int32.TryParse(temp[5], out unknownFiles);

                            int newFiles;
                            Int32.TryParse(temp[6], out newFiles); //nonExistentCount

                            int errorFiles;
                            Int32.TryParse(temp[7], out errorFiles); //failed to analyze count

                            int emptyFiles;
                            Int32.TryParse(temp[8], out emptyFiles); //inaccessibleCount

                            int safeFiles;
                            Int32.TryParse(temp[9], out safeFiles);

                            int malwares;
                            Int32.TryParse(temp[10], out malwares);

                            int skipped;
                            Int32.TryParse(temp[11], out skipped);

                            string error = String.Empty;

                            int failedAtNodeCount = 0;
                            if (temp.Length == 14) //next FLS version
                            {
                                Int32.TryParse(temp[12], out failedAtNodeCount);
                                error = temp[13];
                            }
                            else if (temp.Length == 13)
                            {
                                error = temp[12];
                            }

                            EnAptStage status = (EnAptStage) statusInt;

                            if (!_serviceHelper.IsServiceRunning() && status != EnAptStage.EnApt_Complete &&
                                status != EnAptStage.EnApt_Failed)
                                throw new DescentException(DescentStatus.status_error, "scanner was crashed.", 0);

                            int unknownFilesTotal = unknownFiles + newFiles;
                            

                            if (status == EnAptStage.EnApt_Pending)
                            {

                            }
                            else if (status == EnAptStage.EnApt_EnumeratingProcesses)
                            {
                                InvokeEnumerationProgressChanged(new AptEnumeratingEventArgs()
                                {
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    HostName = _remoteHostInfo.HostName,
                                    TargetUid = _remoteHostInfo.TargetUid,
                                    Stage = status
                                });
                            }
                            else if (status == EnAptStage.EnApt_EnumeratingDrivers)
                            {
                                InvokeEnumerationProgressChanged(new AptEnumeratingEventArgs()
                                {
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    HostName = _remoteHostInfo.HostName,
                                    TargetUid = _remoteHostInfo.TargetUid,
                                    Stage = status
                                });
                            }
                            else if (status == EnAptStage.EnApt_EnumeratingAutoruns)
                            {
                                InvokeEnumerationProgressChanged(new AptEnumeratingEventArgs()
                                {
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    HostName = _remoteHostInfo.HostName,
                                    TargetUid = _remoteHostInfo.TargetUid,
                                    Stage = status
                                });
                            }
                            else if (status == EnAptStage.EnApt_EnumeratingFolder)
                            {
                                InvokeEnumerationProgressChanged(new AptEnumeratingEventArgs()
                                {
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    HostName = _remoteHostInfo.HostName,
                                    TargetUid = _remoteHostInfo.TargetUid,
                                    Stage = status
                                });
                            }
                            else if (status == EnAptStage.EnApt_ScaningModules)
                            {
                                InvokeScanProgressChanged(new ScanProgressChangedEventArgs()
                                {
                                    TargetUid = _remoteHostInfo.TargetUid,
                                    HostName = _remoteHostInfo.HostName,
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    TotalFilesScanned = totalFiles - skipped,
                                    UnknownFilesFound = unknownFilesTotal,
                                    SafeFilesFound = safeFiles,
                                    MalwaresFound = malwares,
                                    SkippedFilesCount = skipped,
                                    FailedToAnalyzeFilesCount = errorFiles,
                                    NoVerdictFilesCount = failedAtNodeCount
                                });
                            }
                            else if (status == EnAptStage.EnApt_WritingReport)
                            {

                            }
                            else if (status == EnAptStage.EnApt_Complete)
                            {
                                _response.Status = DescentStatus.status_success; //(DescentStatus)status;

                                InvokeScanCompleted(new ScanCompletedEventArgs()
                                {
                                    TotalFilesScanned = totalFiles - skipped,
                                    UnknownFilesFound = unknownFilesTotal,
                                    NoVerdictFilesCount = failedAtNodeCount,
                                    FailedToAnalyzeFilesCount = errorFiles,
                                    SkippedFilesCount = skipped
                                });

                                string resultsFileLocal = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                    String.Format("{0}_{1:yyyyMMddHHmmss}.xml", _remoteHostInfo.HostName, DateTime.Now));

                                InvokeReceivingResults(EventArgs.Empty);

                                File.Copy(_resultsFile, resultsFileLocal, true);
                                //File.Delete(resultsFile);
                                InvokeScanResultsFileReceived(new ScanResultsFileReceivedEventArgs()
                                {
                                    TargetUid = _remoteHostInfo.TargetUid,
                                    HostName = _remoteHostInfo.HostName,
                                    FileName = resultsFileLocal,
                                    EnumeratedFilesCount = enumeratedCount,
                                    EnumeratedFilesSize = enumeratedSize,
                                    TotalFilesScanned = totalFiles - skipped,
                                    UnknownFilesFound = unknownFilesTotal,
                                    SafeFilesFound = safeFiles,
                                    MalwaresFound = malwares,
                                    EmptyFilesFound = emptyFiles,
                                    SkippedFilesCount = skipped,
                                    FailedToAnalyzeFilesCount = errorFiles,
                                    NoVerdictFilesCount = failedAtNodeCount
                                });
                                break;
                            }
                            else if (status == EnAptStage.EnApt_Failed)
                            {
                                throw new DescentException(DescentStatus.status_error,
                                    String.Format("Error returned from a remote computer: {0}", error), 0);
                            }
                            else
                            {
                                throw new ArgumentException(String.Format("Unknown EnAptStage value: {0}", (int) status));
                            }
                            Thread.Sleep(3000);
                        }
                    }
                    catch (FileNotFoundException ex)
                    {
                        // If file is not found, we can sleep and try again
                        if (findFileAttemptsCount == 0)
                        {
                            // unless attempts count is zero...
                            const DescentStatus status = DescentStatus.status_remote_invalid_response;
                            InvokeScanFailed(new ScanFailedEventArgs()
                            {
                                Status = status,
                                HostName = _remoteHostInfo.HostName,
                                TargetUid = _remoteHostInfo.TargetUid
                            });
                            throw new DescentException(status, ex.Message, 0);
                        }
                        else
                        {
                            // sleep and try again
                            Thread.Sleep(1000);
                        }
                    }
                    catch (ResponseFileHeaderInvalidException ex)
                    {
                        // If file is not found, we can sleep and try again
                        if (checkFileHeaderAttemptsCount == 0)
                        {
                            // unless attempts count is zero...
                            const DescentStatus status = DescentStatus.status_remote_invalid_response;
                            InvokeScanFailed(new ScanFailedEventArgs()
                            {
                                Status = status,
                                HostName = _remoteHostInfo.HostName,
                                TargetUid = _remoteHostInfo.TargetUid
                            });
                            throw new DescentException(status, ex.Message, 0);
                        }
                        else
                        {
                            // sleep and try again
                            Thread.Sleep(1000);
                        }
                    }
                    catch (StatusFileEmptyException ex)
                    {
                        // If file is not found, we can sleep and try again
                        if (readFileAttemptsCount == 0)
                        {
                            // unless attempts count is zero...
                            const DescentStatus status = DescentStatus.status_remote_invalid_response;
                            InvokeScanFailed(new ScanFailedEventArgs()
                            {
                                Status = status,
                                HostName = _remoteHostInfo.HostName,
                                TargetUid = _remoteHostInfo.TargetUid
                            });
                            throw new DescentException(status, ex.Message, 0);
                        }
                        else
                        {
                            // sleep and try again
                            Thread.Sleep(1000);
                        }
                    }
                    catch (OperationCanceledException ex)
                    {
                        throw new ScanCanceledException();
                    }
                    catch (ThreadAbortException ex)
                    {
                        if ((ex.ExceptionState is bool) == true)
                        {
                            throw new ScanCanceledException();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex is DescentException)
                            throw;

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
            
            //public void StopService()
            //{
            //    _serviceHelper.StopService();
            //}

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
