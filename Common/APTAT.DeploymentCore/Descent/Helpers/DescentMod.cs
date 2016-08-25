using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore.Descent.Exceptions;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Descent.Helpers.Transport;
using APTAT.DeploymentCore.Messages;
using APTAT.DeploymentCore.Properties;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public abstract class DescentMod
    {
        protected readonly TaskFactory TaskFactory;
        protected readonly IChangeTrackingSubSystem Tracker;
        protected ConcurrentDictionary<Guid, InstallAgentThreadParams> ParamsByUid 
            = new  ConcurrentDictionary<Guid, InstallAgentThreadParams>();

        public const string TimeoutAbortString = "OH SHI--";
        private const int E_FAIL = unchecked((int) 0x80004005);

        public DescentMod(TaskFactory taskFactory, IChangeTrackingSubSystem changeTrackingSubSystem)
        {
            Tracker = changeTrackingSubSystem;
            TaskFactory = taskFactory;
        }

        protected class InstallAgentThreadParams
        {
            public RemoteHostInfo RemoteHostInfo { get; set; }
            public IDescentCallback DescentCallback { get; set; }
            public Guid AgentIdentity { get; set; }
            public Guid OperationUid { get; set; }
        }

        protected void StartDeploymentThread(InstallAgentThreadParams tp)
        {
            var task = TaskFactory.StartNew(DeploymentThread, tp, TaskCreationOptions.LongRunning)
                .ContinueWith(t => ProcessErrorOrCancel(DescentStatus.status_operation_canceled, 0, String.Empty, tp),
                    TaskContinuationOptions.OnlyOnCanceled);

        }
        

        protected abstract DescentHelper CreateDescentHelper(InstallAgentThreadParams tp);

        protected virtual void ProcessResults(InstallAgentThreadParams tp, DescentHelper dh)
        {

        }

        protected virtual void ExecuteAction(DescentHelper dh, InstallAgentThreadParams tp)
        {
            
        }

        protected virtual void OnRemotelyConnected(object sender, DescentHelperEventArgs args)
        {
            var tp = ParamsByUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, 
                ServerSideMessagesResources.DescentProgressAgentInstallCopyingHelperServicePackage, 20);
        }

        protected virtual void NotifyProgressChanged(Guid operationUid, string hostName, Guid computerUid, ResourceText resourceMessage, int percentage,
            bool checkCancellation = true)
        {
            if (checkCancellation && TaskFactory.CancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();
            FormattedText formattedText = new FormattedText(resourceMessage);
            Tracker?.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed,
                new DescentProgressItem(computerUid, hostName, operationUid, formattedText, percentage)), false);
        }

        protected virtual void OnDescentHelperCopied(object sender, DescentHelperEventArgs args)
        {
            var tp = ParamsByUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallConfiguringHelperService, 25);
        }

        protected virtual void OnDescentHelperStarted(object sender, DescentHelperEventArgs args)
        {
            var tp = ParamsByUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallConfiguringHelperService, 25);
        }

        protected virtual void OnDescentHelperParametersCopied(object sender, DescentHelperEventArgs args)
        {
            var tp = ParamsByUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallStartingHelperService, 30);
        }



        protected virtual void DeploymentThread(object obj)
        {
            //RemoteInstallerJob remoteInstallerJob = tp._cb as RemoteInstallerJob;
            var tp = (InstallAgentThreadParams) obj;
            ParamsByUid[tp.AgentIdentity] = tp;
            tp.DescentCallback.SetDescentThread();

            string computer = tp.RemoteHostInfo.HostName;
            LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
                "Descent operation started{0} computer name: {1}{0} computer UID: {2}{0} thread ID: {3}{0}",
                Environment.NewLine, computer, tp.AgentIdentity, Thread.CurrentThread.ManagedThreadId);

            //using (tp.InformOperationStart(Thread.CurrentThread))
            {
                DescentHelper dh = CreateDescentHelper(tp);

                //==========================================================================
                // New events


                //==========================================================================


                // 20% Copying remote deployment service package
                dh.RemotelyConnected += OnRemotelyConnected;
                // 25% Configuring remote deployment service
                dh.DescentHelperCopied += OnDescentHelperCopied;
                // 30% Starting remote deployment service
                dh.DescentHelperParametersCopied += OnDescentHelperParametersCopied;
                // 40% Searching existing agent installation
                dh.DescentHelperStarted += OnDescentHelperStarted;
                try
                {
                    ImpersonationHelper.Logon32LogonType impersonationLogonType =
                        ImpersonationHelper.Logon32LogonType.NewCredentials;

                    dh.Init(impersonationLogonType);
                    ExecuteAction(dh, tp);
                    ProcessResults(tp, dh);

                    if (dh.Response.Status != DescentStatus.status_success &&
                        dh.Response.Status != DescentStatus.status_agent_is_already_installed)
                    {
                        throw new DescentException(dh.Response);
                    }

                    tp.DescentCallback.OnDescentCallback(DescentStatus.status_success, 0, String.Empty);
                }
                catch (DescentException ex)
                {
                    if (ex._status == DescentStatus.status_agent_was_crashed)
                    {
                        try
                        {
                            string strToLog;

                            using (TextReader sr = new StreamReader(dh.RemoteWorkingDirectory +
                                                                    @"\CESM_AGENT_LOGS\descentAgn.log",
                                System.Text.Encoding.Unicode))
                            {
                                strToLog = sr.ReadToEnd();
                            }
                            LogHelper.Trace(TraceEventType.Error, TraceCategory.Descent, strToLog);
                        }
                        catch (Exception)
                        {
                            LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, "{0} occurred in getting logs from remote endpoints: {1}",
                                ex.GetType().Name, ex.Message);
                        }
                    }
                    //if( tp.DescentCallback != null )
                    //	tp.DescentCallback.OnDescentCallback( ex._status, ex._win32error, ex._errorString );
                    LogHelper.Trace(TraceEventType.Error, TraceCategory.Descent, "{0} occurred in descent thread: {1}",
                        ex.GetType().Name, ex.Message);

                    ProcessErrorOrCancel(ex._status, ex._win32error, ex._errorString, tp);
                }
                catch (ScanCanceledException)
                { 
                    //ProcessScanCancelation(tp);
                    LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent,
                        "Scan was canceled for '{0}' computer.",
                        tp.RemoteHostInfo.HostName);
                    ProcessErrorOrCancel(DescentStatus.status_operation_canceled, 0, String.Empty, tp);
                }
                catch (OperationCanceledException)
                {
                    LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent,
                        "Scan was canceled for '{0}' computer.", tp.RemoteHostInfo.HostName);
                    ProcessErrorOrCancel(DescentStatus.status_operation_canceled, 0, String.Empty, tp);
                }
                catch (ThreadAbortException ex)
                {
                    if ((ex.ExceptionState as string) == TimeoutAbortString)
                    {
                        LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent,
                            "Descent operation timed out for computer {0}. Thread terminated with exception: ",
                            tp.RemoteHostInfo.HostName);
                        Thread.ResetAbort();
                    }

                    if (ex.ExceptionState is bool)
                    {
                        LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent,
                            "Scan was canceled for '{0}' computer.", tp.RemoteHostInfo.HostName);
                        ProcessErrorOrCancel(DescentStatus.status_operation_canceled, 0, String.Empty, tp);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Trace(TraceEventType.Error, TraceCategory.Descent, "{0} occurred in descent thread: {1}",
                        ex.GetType().Name, ex.Message);
                    //if (tp.DescentCallback != null)
                    //	tp.DescentCallback.OnDescentCallback(DescentStatus.status_error, GetHResult(x).GetValueOrDefault(E_FAIL), x.Message);
                    ProcessErrorOrCancel(DescentStatus.status_error, GetHResult(ex).GetValueOrDefault(E_FAIL),
                        ex.Message, tp);
                }
                finally
                {
                    DescentHelperBeforeRelease(dh);
                    dh.Release();
                }
            }
        }

        protected virtual void DescentHelperBeforeRelease(DescentHelper dh)
        {
            dh.RemotelyConnected -= OnRemotelyConnected;
            dh.DescentHelperCopied -= OnDescentHelperCopied;
            dh.DescentHelperParametersCopied -= OnDescentHelperParametersCopied;
        }

        protected virtual void ProcessErrorOrCancel(DescentStatus descentStatus, int win32ErrorCode, string errorMessage, 
            InstallAgentThreadParams tp)
        {
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