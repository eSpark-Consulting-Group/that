using System;
using System.Collections.Concurrent;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using APTAT.Core.Properties;
using CRM.SrvCore;
using CRM.SrvCore.Descent;
using APTAT.Core.Descent.Transport;

namespace APTAT.Core.Descent
{
    public struct RemoteHostInfo
    {
        public string HostName;
        public string Login;
        public string Password;
        public Guid TargetUid;
    };

    public interface IDescentCallback
    {
        void OnDescentCallback(DescentStatus status, int win32Error, string response);
		bool IsCompleted { get; }
    }

    public struct AgentInformation
    {
        public bool Installed { get; set; }
		public uint VersionMajor { get; set; }
		public uint VersionMinor { get; set; }
		public string ServerHostName { get; set; }
		public uint ServerPort { get; set; }
		public bool CurrentServerAccessible { get; set; } // current server is accessible
    }

    public class DescentMod
    {
        private static TaskFactory _taskFactory;
		private readonly IChangeTrackingSubSystem _tracker;
        private readonly ConcurrentDictionary<Guid, ScanResult> _scanResults;

		public const string TimeoutAbortString = "OH SHI--";
	    private const int E_FAIL = unchecked((int) 0x80004005);

		public DescentMod( IDescentWatcherFactory watcherFactory, ISubSystemCatalog catalog, ConcurrentDictionary<Guid, ScanResult> scanResults )
		{
			_taskFactory = new TaskFactory( 
				CancellationToken.None, 
				TaskCreationOptions.LongRunning, 
				TaskContinuationOptions.None, 
				new LimitedConcurrencyLevelTaskScheduler( Settings.Default.DescentMaximumConcurrencyLevel ) );

			_tracker = catalog.Find<IChangeTrackingSubSystem>();

			DescentWatcherFactory = watcherFactory;

		    _scanResults = scanResults;
		}

		public IDescentWatcherFactory DescentWatcherFactory
		{
			get;
			set;
		}

        // TODO: Commented
        //private struct MsiInfo
        //{
        //    public string Path { get; set; }
        //    public string Platform { get; set; }
        //    public int Bits { get; set; }
        //    public Version Version { get; set; }
        //}

        // TODO: Commented
        //private MsiInfo[] QueryMsiPackages(string msiFolder)
        //{
        //    List<MsiInfo> result = new List<MsiInfo>();

        //    string[] agents = System.IO.Directory.GetFiles(msiFolder, "*.msi", SearchOption.TopDirectoryOnly);
        //    foreach (string agentPath in agents)
        //    {
        //        string fullPath = Path.GetFullPath(agentPath);

        //        try
        //        {
        //            string[] values;
        //            Guid productCode, upgradeCode;
        //            string productVersion;

        //            MsiTools.ExtractMsiProperties(fullPath,
        //                new string[] { "PLATFORM" },
        //                out values,
        //                out productCode,
        //                out upgradeCode,
        //                out productVersion);

        //            MsiInfo info = new MsiInfo();
        //            info.Path = fullPath;
        //            info.Platform = values[0];
        //            info.Bits = (info.Platform == "x86" ? 32 : 64);
        //            info.Version = new Version(productVersion);
        //            result.Add(info);
        //        }
        //        catch (Exception ex)
        //        {
        //            var text = String.Format("Querying Agent '{0}' properties failed.", fullPath);
        //            LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, text);
        //            Debug.Fail(text, ex.ToString());
        //        }
        //    }

        //    return result.ToArray();
        //}


        private struct UninstallAgentThreadParams
        {
            public RemoteHostInfo RemoteHostInfo;
            public IDescentCallback DescentCallback;
        }


        public void UninstallAgent(RemoteHostInfo remoteHostInfo, IDescentCallback cb)
        {
            UninstallAgentThreadParams tp = new UninstallAgentThreadParams();
            tp.RemoteHostInfo = remoteHostInfo;
            tp.DescentCallback = cb;

            ThreadPool.QueueUserWorkItem(UninstallAgent_, tp);
        }

        private void UninstallAgent_(object obj)
        {
            UninstallAgentThreadParams tp = (UninstallAgentThreadParams)obj;
            DescentHelper dh = new DescentHelper(tp.RemoteHostInfo);
            
            try
            {
			//	try
			//	{
					// NewCredentials will use the impersonated user for network connections only.
					dh.Init( ImpersonationHelper.Logon32LogonType.NewCredentials );
			//	}
			//	catch( Exception )
			//	{
			//		dh.Init( ImpersonationHelper.Logon32LogonType.Service );
			//	}

                // Install msi package
                dh.ExecuteAction(
                    "action_uninstall_agent",
                    new string[] {},
                    0);

                if (dh.Response.Status != DescentStatus.status_success)
                {
                    throw new DescentException(dh.Response);
                }

                tp.DescentCallback.OnDescentCallback(DescentStatus.status_success, 0, "");
            }
            catch (DescentException ex)
            {
                if (tp.DescentCallback != null)
                {
                    tp.DescentCallback.OnDescentCallback(ex._status, ex._win32error, ex._errorString);
                }
            }
            finally
            {
                dh.Release();
            }
        }

        private struct InstallAgentThreadParams
        {
            public RemoteHostInfo RemoteHostInfo;
            public IDescentCallback DescentCallback;
            public Guid AgentIdentity;
            public uint HostPort;
			public IDescentWatcherFactory DescentWatcherFactory;
			public Guid OperationUid;

			public IDisposable InformOperationStart( Thread thread )
			{
				string computer = RemoteHostInfo.HostName;
				LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
					"Queued thread informed descent operation start{0} computer name: {1}{0} computer UID: {2}{0} thread ID: {3}{0} timeout: {4}{0}",
					Environment.NewLine, computer, AgentIdentity, thread.ManagedThreadId, Settings.Default.AgentDescentingTimeout);

				IDescentWatcher<AgentDescentWatcherParams> watcher =
                    DescentWatcherFactory.CreateWatcher(new AgentDescentWatcherParams { Computer = RemoteHostInfo.HostName, DescentCallback = DescentCallback, DescentThread = thread });
				watcher.Watch();

				return watcher;
			}
		}

        public void InstallAgent(RemoteHostInfo remoteHostInfo, IDescentCallback cb, Guid agentIdentity, uint hostPort, Guid operationUid)
        {
            InstallAgentThreadParams tp = new InstallAgentThreadParams();
            tp.RemoteHostInfo = remoteHostInfo;
            tp.DescentCallback = cb;
            tp.AgentIdentity = agentIdentity;
            tp.HostPort = hostPort;
            tp.DescentWatcherFactory = DescentWatcherFactory;
			tp.OperationUid = operationUid;

			_taskFactory.StartNew( InstallAgentThread, tp ).ContinueWith( task => task.Dispose() );
            //ThreadPool.QueueUserWorkItem(InstallAgentThread_, tp);
        }

		private void NotifyProgressChanged( Guid operationUid, Guid computerUid, ResourceText resourceMessage, AptScanStage scanStage )
		{
		    NotifyProgressChanged(operationUid, computerUid, resourceMessage, scanStage, 0, 0, 0, 0);
		}

        private void NotifyProgressChanged(Guid operationUid, Guid computerUid, ResourceText resourceMessage, AptScanStage scanStage, int totalFilesCount, int unknownFilesCount)
        {
            NotifyProgressChanged(operationUid, computerUid, resourceMessage, scanStage, totalFilesCount, unknownFilesCount, 0, 0);
        }

        private void NotifyProgressChanged(Guid operationUid, Guid computerUid, ResourceText resourceMessage, AptScanStage scanStage, int totalFilesCount, int unknownFilesCount, int malwaresCount, int safeFilesCount)
        {
            FormattedText formattedText = new FormattedText(resourceMessage);
            _tracker.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new DescentProgressItem(computerUid, operationUid, formattedText, scanStage, totalFilesCount, unknownFilesCount, malwaresCount, safeFilesCount)), false);
        }

		private void InstallAgentThread( object obj )
		{
			InstallAgentThreadParams tp = ( InstallAgentThreadParams )obj;
            //RemoteInstallerJob remoteInstallerJob = tp._cb as RemoteInstallerJob;

			using( tp.InformOperationStart( Thread.CurrentThread ) )
			{
				DescentHelper dh = new DescentHelper( tp.RemoteHostInfo );
                ScanResultsProcessor resultsProcessor = new ScanResultsProcessor();

				EventHandler onRemotelyConnected = null;
				onRemotelyConnected = (sender, args) =>
				{
					// ReSharper disable AccessToModifiedClosure
					dh.RemotelyConnected -= onRemotelyConnected;
					// ReSharper restore AccessToModifiedClosure
					NotifyProgressChanged(tp.OperationUid, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallCopyingHelperServicePackage, AptScanStage.CopyingScanner);
				};

				EventHandler onDescentHelperCopied = null;
				onDescentHelperCopied = (sender, args) =>
				{
					// ReSharper disable AccessToModifiedClosure
					dh.DescentHelperCopied -= onDescentHelperCopied;
					// ReSharper restore AccessToModifiedClosure
                    NotifyProgressChanged(tp.OperationUid, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallConfiguringHelperService, AptScanStage.ConfiguringScanner);
				};

				EventHandler onDescentHelperParametersCopied = null;
				onDescentHelperParametersCopied = (sender, args) =>
				{
					// ReSharper disable AccessToModifiedClosure
					dh.DescentHelperParametersCopied -= onDescentHelperParametersCopied;
					// ReSharper restore AccessToModifiedClosure
                    NotifyProgressChanged(tp.OperationUid, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallStartingHelperService, AptScanStage.StartingScanner);
				};

                //EventHandler onDescentHelperStarted = null;
                //onDescentHelperStarted = ( sender, args ) =>
                //{
                //    // ReSharper disable AccessToModifiedClosure
                //    dh.DescentHelperStarted -= onDescentHelperStarted;
                //    // ReSharper restore AccessToModifiedClosure
                //    NotifyProgressChanged(tp._operationUid, tp._agentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallSearchingExistingInstallations, 40);
                //};

                //==========================================================================
                // New events
			    EventHandler<DescentHelper.ScanProgressChangedEventArgs> onScanProgressChanged = null;
			    onScanProgressChanged = (sender, args) =>
			    {
			        var text = new ResourceText("null", String.Format("Scanning... Total files scanned: {0}. Unknown files detected: {1}", args.TotalFilesScanned, args.UnknownFilesFound));
                    NotifyProgressChanged(tp.OperationUid, tp.AgentIdentity, text, AptScanStage.ScanInProgress, args.TotalFilesScanned, args.UnknownFilesFound);
			    };

                EventHandler<DescentHelper.ScanCompletedEventArgs> onScanCompleted = null;
                onScanCompleted = (sender, args) =>
                {
                    // ReSharper disable AccessToModifiedClosure
                    dh.ScanCompleted -= onScanCompleted;
                    dh.ScanProgressChanged -= onScanProgressChanged;
                    // ReSharper restore AccessToModifiedClosure
                    var text = new ResourceText("null", String.Format("Scan completed. Total files scanned: {0}. Unknown files detected: {1}", args.TotalFilesScanned, args.UnknownFilesFound));
                    NotifyProgressChanged(tp.OperationUid, tp.AgentIdentity, text, AptScanStage.ScanCompleted, args.TotalFilesScanned, args.UnknownFilesFound);
                };

                EventHandler<DescentHelper.ScanFailedEventArgs> onScanFailed = null;
                onScanFailed = (sender, args) =>
                {
                    // ReSharper disable AccessToModifiedClosure
                    dh.ScanFailed -= onScanFailed;
                    dh.ScanProgressChanged -= onScanProgressChanged;
                    // ReSharper restore AccessToModifiedClosure
                    var text = new ResourceText("null", String.Format("Scan failed: {0}.", args.Status));
                    NotifyProgressChanged(tp.OperationUid, tp.AgentIdentity, text, AptScanStage.ScanFailed, args.TotalFilesScanned, args.UnknownFilesFound);
                };

                EventHandler<DescentHelper.ScanResultsFileReceivedEventArgs> onScanResultsFileReceived = null;
                onScanResultsFileReceived = (sender, args) =>
                {
                    // ReSharper disable AccessToModifiedClosure
                    dh.ScanResultsFileReceived -= onScanResultsFileReceived;
                    dh.ScanProgressChanged -= onScanProgressChanged;
                    // ReSharper restore AccessToModifiedClosure

                    _scanResults.TryAdd(args.TargetUid, new ScanResult(){TargetUid = args.TargetUid, HostName = args.HostName, FileName = args.FileName, TotalFilesCount = args.TotalFilesScanned, UnknownFilesCount = args.UnknownFilesFound});
                };
                //==========================================================================


				// 20% Copying remote deployment service package
				dh.RemotelyConnected += onRemotelyConnected;
				// 25% Configuring remote deployment service
				dh.DescentHelperCopied += onDescentHelperCopied;
				// 30% Starting remote deployment service
				dh.DescentHelperParametersCopied += onDescentHelperParametersCopied;
				// 40% Searching existing agent installation
				//dh.DescentHelperStarted += onDescentHelperStarted;

			    dh.ScanProgressChanged += onScanProgressChanged;
                dh.ScanCompleted += onScanCompleted;
                dh.ScanFailed += onScanFailed;
			    dh.ScanResultsFileReceived += onScanResultsFileReceived;

				try
				{
					// 5% Connecting to the target computer
                    NotifyProgressChanged(tp.OperationUid, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallConnectingToTarget, AptScanStage.Connecting);

					// NewCredentials will use the impersonated user for network connections only.
					ImpersonationHelper.Logon32LogonType impersonationLogonType = ImpersonationHelper.Logon32LogonType.NewCredentials;

					dh.Init( impersonationLogonType );
					dh.ExecuteAction("action_install_agent_check", new string[] { tp.HostPort.ToString() }, 0 );

                    ScanResult result;
                    _scanResults.TryGetValue(tp.AgentIdentity, out result);
                    if (result != null)
                    {
                        var text = new ResourceText("null", String.Format("Scan results received. Total files scanned: {0}. Unknown files detected: {1}", result.TotalFilesCount, result.UnknownFilesCount));
                        NotifyProgressChanged(tp.OperationUid, tp.AgentIdentity, text, AptScanStage.ScanResultsProcessing, result.TotalFilesCount, result.UnknownFilesCount);

                        if(!resultsProcessor.TryParse(ref result))
                            throw new DescentException(DescentStatus.status_error, "Cannot process scan results.", 0);

                        NotifyProgressChanged(tp.OperationUid, tp.AgentIdentity, new ResourceText("null", String.Format("Completed. Total files scanned: {0}. Unknown files detected: {1}", result.TotalFilesCount, result.UnknownFilesCount)), AptScanStage.OperationCompleted, result.TotalFilesCount, result.UnknownFilesCount);
                    }

				    if( dh.Response.Status != DescentStatus.status_success &&
						dh.Response.Status != DescentStatus.status_agent_is_already_installed &&
						dh.Response.Status != DescentStatus.status_agent_is_already_installed_and_server_differs
						)
					{
						throw new DescentException( dh.Response );
					}

					// 50% Copying agent installation package
					//NotifyProgressChanged( tp._operationUid, tp._agentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallCopyingAgentPackage, 50 );

                    // TODO: Commented
                    ////TODO: Make DescentHelper report full SystemKind instead of just platform
                    //string[] split = dh.Response.Response.Split( ',' );
                    //if( split.Length != 5 )
                    //{
                    //    throw new DescentException( DescentStatus.status_remote_invalid_response, "Query agent information failed: invalid response", 0 );
                    //}

                    //AgentInformation info = new AgentInformation();
                    //string platform = split[0];
                    //info.VersionMajor = ( uint )Int32.Parse( split[1] );
                    //info.VersionMinor = ( uint )Int32.Parse( split[2] );
                    //info.ServerPort = ( uint )Int32.Parse( split[3] );
                    //info.ServerHostName = split[4];

                    //var agentKind = SystemKind.Windows;
                    //if (platform == "64")
                    //    agentKind |= SystemKind.IsIntelX64Edition;
                    //var productManagement = SubSystemFactory.Current.GetSubSystem<IProductManagementSubSystem>();
                    //var agentPackage = productManagement.GetAgentPackage(agentKind);
                    //if (agentPackage == null)
                    //    throw new DescentException(DescentStatus.status_no_compatible_msi_found, String.Format("No Agent package found for the target system (SystemKind = {0})", agentKind), 0);
                    //if (!ProductSubSystem.VerifyHash(agentPackage.Hash, agentPackage.StoredPath))
                    //    throw new DescentException(DescentStatus.status_package_file_hash_is_invalid, "Package file hash is invalid.", 0);

                    //// We need to reestablish connection to the remote computer now.
                    //dh.Release();
                    //dh.Init( impersonationLogonType );
                    //dh.CopyFile( agentPackage.StoredPath, true );

                    //// 60% Installing agent package
                    //NotifyProgressChanged( tp._operationUid, tp._agentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallInstallingAgentPackage, 60 );

                    //string remotePath = Path.Combine( dh.RemoteInstallerTempFolderName, Path.GetFileName( agentPackage.StoredPath ) );

                    //string[] serverHosts = NetHelp.GetSupportedExternalAddresses(false);
                    //string parameters = string.Format( "ALTSERVERHOSTS=\"{0}\" SERVERPORT={1} AGENTIDENTITY={2} EXECUTEACTION=INSTALL ",
                    //    String.Join(" ", serverHosts.Skip(1)),
                    //    tp._hostPort,
                    //    tp._agentIdentity.ToString()
                    //    );

                    //string agentInstallParameters = remotePath + "*" + parameters;

                    //// Install msi package
                    //dh.ExecuteAction(
                    //    "action_install_agent",
                    //    new string[] { agentInstallParameters },
                    //    0 );

                    //// 90% Finalizing agent istallation
                    //NotifyProgressChanged( tp._operationUid, tp._agentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallFinalizingAgentInstallation, 90 );

                    //// DescentStatus.status_success_agent_updated means success too
                    //if( dh.Response.Status != DescentStatus.status_success &&
                    //    dh.Response.Status != DescentStatus.status_success_agent_updated )
                    //{
                    //    throw new DescentException( dh.Response );
                    //}

					tp.DescentCallback.OnDescentCallback( DescentStatus.status_success, 0, "" );
				}
				catch( DescentException ex )
				{
					if( tp.DescentCallback != null )
					{
						tp.DescentCallback.OnDescentCallback( ex._status, ex._win32error, ex._errorString );
					}
				}
				catch( ThreadAbortException ex )
				{
					if( (ex.ExceptionState as string) == TimeoutAbortString )
					{
						LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent,
							"Descent operation timed out for computer {0}. Thread terminated with exception: ", tp.RemoteHostInfo.HostName);
						//LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, ex.ToString());
						Thread.ResetAbort();
					}
				}
				catch( Exception x )
				{
					LogHelper.Trace(TraceEventType.Error, TraceCategory.Descent, "{0} occurred in descent thread: {1}", x.GetType().Name, x.Message);
					if (tp.DescentCallback != null)
						tp.DescentCallback.OnDescentCallback(DescentStatus.status_error, GetHResult(x).GetValueOrDefault(E_FAIL), x.Message);
				}
				finally
				{
					dh.Release();
				}
			}
		}

		private int? GetHResult(Exception x)
		{
			var propHResult = x.GetType().GetProperty("HResult");
			if (propHResult == null)
				return null;
			object value = propHResult.GetValue(x, null);
			if (value == null)
				return null;
			return (int) value;
		}

        // TODO: Commented
        //// Sync operations
        //public AgentInformation QueryAgentInformation(RemoteHostInfo remoteHostInfo, int LocalPort)
        //{
        //    DescentHelper dh = new DescentHelper(remoteHostInfo);
        //    AgentInformation info = new AgentInformation();

        //    try
        //    {
        //    //	try
        //    //	{
        //            // NewCredentials will use the impersonated user for network connections only.
        //            dh.Init( ImpersonationHelper.Logon32LogonType.NewCredentials );
        //    //	}
        //    //	catch( Exception )
        //    //	{
        //    //		dh.Init( ImpersonationHelper.Logon32LogonType.Service );
        //    //	}

        //        info = new AgentInformation();
        //        dh.ExecuteAction(
        //            "action_query_agent_information",
        //            new string[] { LocalPort.ToString() },
        //            0);

        //        dh.Release();
        //        if ( dh.Response.Status != DescentStatus.status_success )
        //        {
        //            throw new DescentException(dh.Response);
        //        }

        //        string[] split = dh.Response.Response.Split(',');
        //        if (split.Length != 6)
        //        {
        //            throw new DescentException(DescentStatus.status_remote_invalid_response,
        //                "Query agent information failed: invalid response", 0);
        //        }

        //        info.Installed = (Int32.Parse(split[0]) == 1);
        //        info.VersionMajor = (uint)Int32.Parse(split[1]);
        //        info.VersionMinor = (uint)Int32.Parse(split[2]);
        //        info.ServerPort = (uint)Int32.Parse(split[3]);
        //        info.ServerHostName = split[4];
        //        info.CurrentServerAccessible = (Int32.Parse(split[5]) == 1);

        //        return info;
        //    }
        ////  catch (DescentException ex)
        ////  {
        ////      throw ex;
        ////  }
        //    finally
        //    {
        //        dh.Release();
        //    }
        //}

        public void Release()
        {

        }
    }
}