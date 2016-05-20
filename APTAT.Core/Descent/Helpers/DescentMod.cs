using System;
using System.Collections.Concurrent;
using System.Security.Permissions;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Properties;
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
        void SetDescentThread();
        bool IsCompleted { get; }
    }

    public class DescentMod
    {
        private static TaskFactory _taskFactory;
		private readonly IChangeTrackingSubSystem _tracker;

		public const string TimeoutAbortString = "OH SHI--";
	    private const int E_FAIL = unchecked((int) 0x80004005);

        private readonly ConcurrentDictionary<Guid, Thread> _threads = new ConcurrentDictionary<Guid, Thread>();
		public DescentMod(ISubSystemCatalog catalog)
		{
			_taskFactory = new TaskFactory( 
				CancellationToken.None, 
				TaskCreationOptions.LongRunning, 
				TaskContinuationOptions.None, 
				new LimitedConcurrencyLevelTaskScheduler( Settings.Default.DescentMaximumConcurrencyLevel ) );
            
			_tracker = catalog.Find<IChangeTrackingSubSystem>();
		}

        private struct InstallAgentThreadParams
        {
            public RemoteHostInfo RemoteHostInfo;
            public IDescentCallback DescentCallback;
            public AptScanResult Result;
            public Guid AgentIdentity;
			public Guid OperationUid;
            public string args;
        }

        public void StartAptScan(RemoteHostInfo remoteHostInfo, IDescentCallback cb, Guid agentIdentity,
            AptScanResult result, Guid operationUid, string args = null)
        {
            InstallAgentThreadParams tp = new InstallAgentThreadParams();
            tp.RemoteHostInfo = remoteHostInfo;
            tp.DescentCallback = cb;
            tp.AgentIdentity = agentIdentity;
			tp.OperationUid = operationUid;
            tp.Result = result;
            tp.args = args;

            //Thread thread = new Thread(InstallAgentThread);
            //thread.Start(tp);
			_taskFactory.StartNew( StartAptScanThread, tp, TaskCreationOptions.LongRunning ).ContinueWith( task => task.Dispose() );
            //ThreadPool.QueueUserWorkItem(InstallAgentThread_, tp);
        }

        private void NotifyProgressChanged(Guid operationUid, string hostName, Guid computerUid, ResourceText resourceMessage, AptScanStage scanStage, int percentage)
        {
            NotifyProgressChanged(operationUid, hostName, computerUid, resourceMessage, scanStage, 0, 0, 0, 0, percentage, 0, 0);
        }

        private void NotifyProgressChanged(Guid operationUid, string hostName, Guid computerUid, ResourceText resourceMessage, AptScanStage scanStage, int totalFilesCount, int unknownFilesCount, int malwaresCount, int safeFilesCount, int percentage, int errorFilesCount, int failedAtNodeCount)
        {
            FormattedText formattedText = new FormattedText(resourceMessage);
            _tracker.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new DescentProgressItem(computerUid, hostName, operationUid, formattedText, scanStage, totalFilesCount, unknownFilesCount, malwaresCount, safeFilesCount, percentage, errorFilesCount, failedAtNodeCount)), false);
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void AbortThreadByTargetUid(Guid uid)
        {
            Thread thread;
            if(_threads.TryGetValue(uid, out thread))
            {
                if (thread != null)
                {
                    try
                    {
                        thread.Abort(true);
                    }
                    catch (ThreadStateException ex)
                    {
                        LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Thread abort exception occurred: {0}", ex);
                        throw new InvalidOperationException("Cannot stop operation.");
                    }
                    
                }
            }
        }

        public void AbortAllThreads()
        {
            foreach (var item in _threads)
            {
                if (item.Value != null)
                {
                    try
                    {
                        item.Value.Abort(true);
                    }
                    catch (ThreadStateException ex)
                    {
                        LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Thread abort exception occurred: {0}", ex);
                        throw new InvalidOperationException("Cannot stop operation.");
                    }

                }
            }
        }

		private void StartAptScanThread( object obj )
		{
            //RemoteInstallerJob remoteInstallerJob = tp._cb as RemoteInstallerJob;
            var tp = (InstallAgentThreadParams)obj;
            tp.DescentCallback.SetDescentThread();

            string computer = tp.RemoteHostInfo.HostName;
            LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
                "Descent operation started{0} computer name: {1}{0} computer UID: {2}{0} thread ID: {3}{0}",
                Environment.NewLine, computer, tp.AgentIdentity, Thread.CurrentThread.ManagedThreadId);
            _threads.TryAdd(tp.AgentIdentity, Thread.CurrentThread);

            //using (tp.InformOperationStart(Thread.CurrentThread))
			{
				DescentHelper dh = new DescentHelper( tp.RemoteHostInfo, tp.args );
                ScanResultsProcessor resultsProcessor = new ScanResultsProcessor();

				EventHandler onRemotelyConnected = null;
				onRemotelyConnected = (sender, args) =>
				{
					// ReSharper disable AccessToModifiedClosure
					dh.RemotelyConnected -= onRemotelyConnected;
					// ReSharper restore AccessToModifiedClosure
					NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallCopyingHelperServicePackage, AptScanStage.CopyingScanner, 0);
				};

				EventHandler onDescentHelperCopied = null;
				onDescentHelperCopied = (sender, args) =>
				{
					// ReSharper disable AccessToModifiedClosure
					dh.DescentHelperCopied -= onDescentHelperCopied;
					// ReSharper restore AccessToModifiedClosure
                    NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallConfiguringHelperService, AptScanStage.ConfiguringScanner, 0);
				};

				EventHandler onDescentHelperParametersCopied = null;
				onDescentHelperParametersCopied = (sender, args) =>
				{
					// ReSharper disable AccessToModifiedClosure
					dh.DescentHelperParametersCopied -= onDescentHelperParametersCopied;
					// ReSharper restore AccessToModifiedClosure
                    NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallStartingHelperService, AptScanStage.StartingScanner, 0);
				};
                

                //==========================================================================
                // New events
			    EventHandler<DescentHelper.ScanProgressChangedEventArgs> onScanProgressChanged = null;
			    onScanProgressChanged = (sender, args) =>
			    {

			        string errorCounterText = args.NetworkErrorFilesCount > 0 ? String.Format("Failed to analyze count: {0}", args.NetworkErrorFilesCount): String.Empty;

                    string serverErrorCounterText = args.FailedAtNodeCount > 0 ? String.Format("Server error count: {0}", args.FailedAtNodeCount): String.Empty;

                    ResourceText text = ResourceText.Empty;

                    if (args.FailedAtNodeCount > 0 || args.NetworkErrorFilesCount > 0)
			        {
                        text = new ResourceText("null", String.Format("{0} programs scanned ({3}%). Unknown programs found: {1}. Malwares found: {2}. Failed to analyse count: {4}, Failed with server error: {5}", args.TotalFilesScanned, args.UnknownFilesFound, args.MalwaresFound, args.Percentage, errorCounterText, serverErrorCounterText));
                    }
                    
                    NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, AptScanStage.ScanInProgress, args.TotalFilesScanned, args.UnknownFilesFound, args.MalwaresFound, args.SafeFilesFound, args.Percentage, args.NetworkErrorFilesCount, args.FailedAtNodeCount);
			    };

                EventHandler<DescentHelper.AptEnumeratingEventArgs> onEnumerationProgressChanged = null;
                onEnumerationProgressChanged = (sender, args) =>
                {
                    bool notify = true;
                    string fileTypes = String.Empty;
                    AptScanStage scanStage = AptScanStage.Pending;
                    switch(args.Stage)
                    {
                        case EnAptStage.EnApt_EnumeratingProcesses:
                            fileTypes = "processes";
                            scanStage = AptScanStage.EnumeratingProcesses;
                            break;
                        case EnAptStage.EnApt_EnumeratingDrivers:
                            fileTypes = "drivers";
                            scanStage = AptScanStage.EnumeratingDrivers;
                            break;
                        case EnAptStage.EnApt_EnumeratingAutoruns:
                            fileTypes = "autoruns";
                            scanStage = AptScanStage.EnumeratingAutoruns;
                            break;
                        case EnAptStage.EnApt_EnumeratingFolder:
                            fileTypes = "folders";
                            scanStage = AptScanStage.EnumeratingFolders;
                            break;
                        default:
                            notify = false;
                            break;
                    }

                    if (notify)
                    {
                        var text = new ResourceText("null", String.Format("Enumerating {0}: {1}", fileTypes, args.EnumeratedFilesCount));
                        NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, scanStage, 0);
                    }
                };

                EventHandler<DescentHelper.ScanCompletedEventArgs> onScanCompleted = null;
                onScanCompleted = (sender, args) =>
                {
                    // ReSharper disable AccessToModifiedClosure
                    dh.ScanCompleted -= onScanCompleted;
                    dh.ScanProgressChanged -= onScanProgressChanged;
                    dh.EnumerationProgressChanged -= onEnumerationProgressChanged;
                    // ReSharper restore AccessToModifiedClosure
                    var text = new ResourceText("null", String.Format("Scan completed. Total programs scanned: {0}. Unknown programs found: {1}. Malwares found: {2}. Errors count {3}. Server error {4}.", args.TotalFilesScanned, args.UnknownFilesFound, args.MalwaresFound, args.NetworkErrorFilesCount, args.FailedAtNodeCount));
                    NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, AptScanStage.ScanCompleted, args.TotalFilesScanned, args.UnknownFilesFound, args.MalwaresFound, args.SafeFilesFound, 100, args.NetworkErrorFilesCount, args.FailedAtNodeCount);
                };

                EventHandler<DescentHelper.ScanFailedEventArgs> onScanFailed = null;
                onScanFailed = (sender, args) =>
                {
                    // ReSharper disable AccessToModifiedClosure
                    dh.ScanFailed -= onScanFailed;
                    dh.ScanProgressChanged -= onScanProgressChanged;
                    dh.EnumerationProgressChanged -= onEnumerationProgressChanged;
                    // ReSharper restore AccessToModifiedClosure
                    var text = new ResourceText("null", String.Format("Scan failed: {0}.", args.Status));
                    NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, AptScanStage.ScanFailed, args.TotalFilesScanned, args.UnknownFilesFound, args.MalwaresFound, args.SafeFilesFound, 100, args.NetworkErrorFilesCount, args.FailedAtNodeCount);
                };

                EventHandler<DescentHelper.ScanResultsFileReceivedEventArgs> onScanResultsFileReceived = null;
                onScanResultsFileReceived = (sender, args) =>
                {
                    // ReSharper disable AccessToModifiedClosure
                    dh.ScanResultsFileReceived -= onScanResultsFileReceived;
                    // ReSharper restore AccessToModifiedClosure

                    AptScanResult result = tp.Result;

                    if (result != null)
                    {
                        result.FileName = args.FileName;
                        result.TotalFilesCount = args.TotalFilesScanned;
                        result.UnknownFilesCount = args.UnknownFilesFound;
                        result.SafeFilesCount = args.SafeFilesFound;
                        result.MalwareFilesCount = args.MalwaresFound;
                        result.EmptyFilesCount = args.EmptyFilesFound;
                        result.SkippedFilesCount = args.SkippedFilesCount;
                        result.ErrorFilesCount = args.NetworkErrorFilesCount;
                        result.ServerErrorFilesCount = args.FailedAtNodeCount;
                    }
                    //_scanResults.TryAdd(args.TargetUid, new ScanResult(){TargetUid = args.TargetUid, HostName = args.HostName, FileName = args.FileName, TotalFilesCount = args.TotalFilesScanned, UnknownFilesCount = args.UnknownFilesFound});
                };

                EventHandler<EventArgs> onResultsReceiving = null;
                onResultsReceiving = (sender, args) =>
                {
                    // ReSharper disable AccessToModifiedClosure
                    dh.ReceivingResults -= onResultsReceiving;
                    // ReSharper restore AccessToModifiedClosure

                    var text = new ResourceText("null", "Obtaining results... ");
                    NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, 
                        AptScanStage.ScanResultsObtaning, 100);
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
			    dh.ReceivingResults += onResultsReceiving;
			    dh.EnumerationProgressChanged += onEnumerationProgressChanged;

				try
				{
					// 5% Connecting to the target computer
                    NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallConnectingToTarget, AptScanStage.Connecting, 0);
                    //_scanResults.TryAdd(tp.AgentIdentity, new ScanResult() { TargetUid = tp.AgentIdentity, HostName = tp.RemoteHostInfo.HostName, ResultType = ScanResultType.NotPerformed});

					// NewCredentials will use the impersonated user for network connections only.
					ImpersonationHelper.Logon32LogonType impersonationLogonType = ImpersonationHelper.Logon32LogonType.NewCredentials;

					dh.Init( impersonationLogonType );
					dh.ExecuteAction(0 );

                    AptScanResult result = tp.Result;
                    if (result != null)
                    {
                        var text = new ResourceText("null", String.Format("Scan results received. Total programs scanned: {0}. " +
                               "Unknown programs found: {1}. Malwares found: {2}.", result.TotalFilesCount, result.UnknownFilesCount, 
                               result.MalwareFilesCount));
                        NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, AptScanStage.ScanResultsProcessing, result.TotalFilesCount, result.UnknownFilesCount, result.MalwareFilesCount, result.SafeFilesCount, 100, result.ErrorFilesCount, result.ServerErrorFilesCount);

                        if(!resultsProcessor.TryParse(ref result))
                            throw new DescentException(DescentStatus.status_error, "Cannot process scan results.", 0);

                        result.ResultType = ScanResultType.Completed;
                        NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity,
                            new ResourceText("null",
                                String.Format("Completed: Total programs scanned: {0}. Unknown programs found: {1}. Malwares found: {2}. Failed to analise: {3}, Server errors: {4}", result.TotalFilesCount, result.UnknownFilesCount, result.MalwareFilesCount, result.ErrorFilesCount, result.ServerErrorFilesCount)),
                                AptScanStage.OperationCompleted, result.TotalFilesCount, result.UnknownFilesCount, result.MalwareFilesCount, result.SafeFilesCount, 100, result.ErrorFilesCount, result.ServerErrorFilesCount);
                    }

                    if( dh.Response.Status != DescentStatus.status_success &&
						dh.Response.Status != DescentStatus.status_agent_is_already_installed)
					{
						throw new DescentException( dh.Response );
					}

                    tp.DescentCallback.OnDescentCallback(DescentStatus.status_success, 0, String.Empty);
				}
				catch( DescentException ex )
				{
					//if( tp.DescentCallback != null )
					//	tp.DescentCallback.OnDescentCallback( ex._status, ex._win32error, ex._errorString );
                    LogHelper.Trace(TraceEventType.Error, TraceCategory.Descent, "{0} occurred in descent thread: {1}", ex.GetType().Name, ex.Message);

                    ProcessErrorOrCancel(ex._status, ex._win32error, ex._errorString, tp);
				}
                catch( ScanCanceledException )
                {
                    dh.ScanProgressChanged -= onScanProgressChanged;
                    //ProcessScanCancelation(tp);
                    LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Scan was canceled for '{0}' computer.", tp.RemoteHostInfo.HostName);
                    ProcessErrorOrCancel(DescentStatus.status_operation_canceled, 0, String.Empty, tp);
                }
				catch( ThreadAbortException ex )
				{
					if( (ex.ExceptionState as string) == TimeoutAbortString )
					{
						LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Descent operation timed out for computer {0}. Thread terminated with exception: ", tp.RemoteHostInfo.HostName);
						Thread.ResetAbort();
					}

                    if ((ex.ExceptionState is bool) == true)
                    {
                        //ProcessScanCancelation(tp);
                        LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Scan was canceled for '{0}' computer.", tp.RemoteHostInfo.HostName);
                        ProcessErrorOrCancel(DescentStatus.status_operation_canceled, 0, String.Empty, tp);
                    }
				}
				catch( Exception ex )
				{
					LogHelper.Trace(TraceEventType.Error, TraceCategory.Descent, "{0} occurred in descent thread: {1}", ex.GetType().Name, ex.Message);
					//if (tp.DescentCallback != null)
					//	tp.DescentCallback.OnDescentCallback(DescentStatus.status_error, GetHResult(x).GetValueOrDefault(E_FAIL), x.Message);
                    ProcessErrorOrCancel(DescentStatus.status_error, GetHResult(ex).GetValueOrDefault(E_FAIL), ex.Message, tp);
				}
				finally
				{
					dh.Release();

				    Thread thread;
                    _threads.TryRemove(tp.AgentIdentity, out thread);
				}
			}
		}

        private void ProcessErrorOrCancel(DescentStatus descentStatus, int win32ErrorCode, string errorMessage, 
            InstallAgentThreadParams tp)
        {
            AptScanResult result = tp.Result;
            if (result != null)
            {
                string errorText = errorMessage;
                ScanResultType scanResultType = ScanResultType.Failed;
                if (descentStatus == DescentStatus.status_operation_canceled)
                    scanResultType = ScanResultType.Canceled;

                result.ResultErrorText = errorText;
                result.ResultType = scanResultType;
            }

            if (tp.DescentCallback != null)
                tp.DescentCallback.OnDescentCallback(descentStatus, win32ErrorCode, errorMessage);
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

        public void Release()
        {

        }
    }
}