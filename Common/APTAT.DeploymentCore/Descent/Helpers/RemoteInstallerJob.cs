using System;
using System.Diagnostics;
using System.Threading;
using APTAT.Common;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Messages;
using APTAT.DeploymentCore.Properties;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public class RemoteInstallerJob : IDescentCallback//, IDisposable
    {
        private readonly IDescentQueryResult _queryResult;
        private readonly IChangeTrackingSubSystem _changeTrackingSubSystem;
        private readonly IScanner _scanner;
        private readonly IComputer _computer;
        private Thread _descentThread;
        //private bool _disposed;

        #region RemoteInstallerJob class

        public RemoteInstallerJob(
            IDescentQueryResult queryResult,
            IChangeTrackingSubSystem changeTrackingSubSystem,
            IScanner scanner,
            IComputer computer)
        {
            _queryResult = queryResult;
            _changeTrackingSubSystem = changeTrackingSubSystem;
            _scanner = scanner;
            _computer = computer;
            IsCompleted = false;
            _queryResult.RegisterTimeoutCallback(_computer.Uid, OnTimeout);
        }
        public void SetDescentThread()
        {
            if (_descentThread != null)
            {
                LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Descent thread re-assignment ignored.");
                return;
            }
            _descentThread = Thread.CurrentThread;
        }
        public bool IsCompleted
        {
            get;
            private set;
        }

        public IDescentQueryResult QueryResult
        {
            get { return _queryResult; }
        }

        private void OnTimeout(TimeSpan timeout)
        {
            LogHelper.Trace(TraceEventType.Warning, TraceCategory.Descent, "Agent descent to computer {0} has exceeded timeout of: {1}", _computer.Name, timeout);

            if (!IsCompleted)
            {
                try
                {
                    if (_descentThread != null)
                        _descentThread.Abort(DescentMod.TimeoutAbortString);
                    var responseText = new FormattedText(ServerSideMessagesResources.DescentTimeoutSystemMessage, timeout.ToString());
                    OnDescentCallback(DescentStatus.status_agent_timeout, 0, responseText.ToString());
                }
                catch (ThreadStateException ex)
                {
                    LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent, "{0}: {1}", ex.GetType().Name, ex.Message);
                }
            }

            _queryResult.UnregisterTimeoutCallback(_computer.Uid);
        }

        public void OnDescentCallback(DescentStatus status, int win32Error, string response)
        {
            response = (response ?? String.Empty).Trim();
            if (IsCompleted)
            {
                LogHelper.Trace(TraceEventType.Verbose, TraceCategory.Descent,
                    "Descent installer job is thought to be already completed{0} computer: " +
                    "{1}{0} status: {2}{0} win32Error: {3}{0} response: {4}{0}",
                    Environment.NewLine, _computer, status, win32Error, response);
                return;
            }

            IsCompleted = true;
            if (status == DescentStatus.status_success)
            {
                var text = new FormattedText(ServerSideMessagesResources.DescentInstallAgentSuccess);
                if (QueryResult != null)
                {
                    QueryResult.OnDescendOperationEvent(true, _computer.Uid, text, text, status.ToString(), win32Error, response);
                }
            }
            else if (status == DescentStatus.status_success_reboot_required)
            {
                FormattedText text;
               _scanner.GetDescentStatusDescription(status, out text) ;
                if (QueryResult != null)
                {
                    QueryResult.OnDescendOperationEvent(true, _computer.Uid, text, text, status.ToString(), win32Error, response);
                }
            }
            else if (status == DescentStatus.status_operation_canceled)
            {
                if (QueryResult != null)
                {
                    FormattedText shortMessage;
                    FormattedText description = _scanner.GetDescentStatusDescription(status, out shortMessage);
                    _changeTrackingSubSystem.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new DescentProgressItem(_computer.Uid,
                        _computer.Name, QueryResult.OperationUid, shortMessage, 100, false, true)
                    { IsCanceled = true }), false);

                    QueryResult.OnDescendOperationEvent(true, _computer.Uid, description, shortMessage, status.ToString(), win32Error, response, true);
                }
            }
            else
            {
                // Error occurred
                ResourceText errorFormat = ServerSideMessagesResources.DescentInstallAgentFailedFormat;
                FormattedText messageText = null;
                FormattedText shortMessage = null;
                try
                {
                    messageText = _scanner.GetDescentStatusInformation(errorFormat, status, win32Error, response, out shortMessage);
                }
                catch { }

                if (messageText != null && QueryResult != null)
                {
                    FormattedText formattedText;
                    if (status == DescentStatus.status_error)
                        formattedText = new FormattedText(new ResourceText("null", "Failed: {0}"), response);
                    else
                        formattedText = new FormattedText(new ResourceText("null", "Failed: {0} {1}"), shortMessage, response);
                    _changeTrackingSubSystem.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new DescentProgressItem
                        (_computer.Uid, _computer.Name, QueryResult.OperationUid, formattedText, 100, false, true)), false);

                    QueryResult.OnDescendOperationEvent(false, _computer.Uid, messageText, shortMessage, status.ToString(), win32Error, response);
                }
            }
        }

        #endregion

    }
}
