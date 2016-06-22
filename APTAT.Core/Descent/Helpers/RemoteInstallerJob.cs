using System;
using System.Diagnostics;
using System.Threading;
using APTAT.Core.Descent.Transport;
using APTAT.Core.Properties;

namespace APTAT.Core.Descent.Helpers
{
    internal class RemoteInstallerJob : IDescentCallback//, IDisposable
    {
        private readonly IDescentQueryResult _queryResult;
        private readonly IChangeTrackingSubSystem _changeTrackingSubSystem;
        private readonly IComputer _computer;
        private Thread _descentThread;
        //private bool _disposed;

        #region RemoteInstallerJob class

        public RemoteInstallerJob(
            IDescentQueryResult queryResult,
            IChangeTrackingSubSystem changeTrackingSubSystem,
            IComputer computer)
        {
            _queryResult = queryResult;
            _changeTrackingSubSystem = changeTrackingSubSystem;
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

        public static FormattedText GetDescentStatusInformation(ResourceText errorFormat, DescentStatus status, int win32Error, string errorString, out FormattedText shortMessage)
        {
            try
            {
                FormattedText strMarshal = new FormattedText(ServerSideMessagesResources.ErrorCodeMessageFormat,
                    win32Error);

                // Format description
                FormattedText description = GetDescentStatusDescription(status, out shortMessage);

                return new FormattedText(errorFormat,
                    description,
                    status.ToString(),
                    strMarshal,
                    errorString
                    );
            }
            catch { }

            shortMessage = FormattedText.Empty;
            return FormattedText.Empty;
        }


        public static FormattedText GetDescentStatusDescription(DescentStatus status, out FormattedText shortMessage)
        {
            ResourceText suggestion;
            ResourceText description;

            GetDescriptionAndSuggestionByStatus(status, out description, out suggestion);

            shortMessage = new FormattedText(description);

            if (suggestion.Equals(ResourceText.Empty))
            {
                return new FormattedText(ServerSideMessagesResources.DescentFormatDescriptionWithoutSuggestion, description);
            }
            ResourceText format = ServerSideMessagesResources.DescentStatusDescriptionWithSuggestionFormat;
            return new FormattedText(format, description, suggestion);
        }

        private static void GetDescriptionAndSuggestionByStatus(DescentStatus status,
            out ResourceText description, out ResourceText suggestion)
        {
            suggestion = ResourceText.Empty;
            switch (status)
            {
                case DescentStatus.status_error:
                    description = ServerSideMessagesResources.UnknownErrorMessage;
                    break;
                case DescentStatus.status_cannot_resolve_remote_hostname:
                    description = ServerSideMessagesResources.CannotResolveHostMessage;
                    suggestion = ServerSideMessagesResources.CannotResolveHostSuggestionMessage;
                    break;
                case DescentStatus.status_login_problem:
                    description = ServerSideMessagesResources.LoginProblemMessage;
                    suggestion = ServerSideMessagesResources.LoginProblemSuggestionMessage;
                    break;
                case DescentStatus.status_login_invalid_username_or_bad_password:
                    description = ServerSideMessagesResources.InvalidUserNameOrPasswordMessage;
                    suggestion = ServerSideMessagesResources.LoginProblemSuggestionMessage;
                    break;
                case DescentStatus.status_login_invalid_username:
                    description = ServerSideMessagesResources.InvalidUserNameMessage;
                    break;
                case DescentStatus.status_login_trusted_relationship_failure:
                    description = ServerSideMessagesResources.TrustedRelationshipFailureMessage;
                    break;
                case DescentStatus.status_agent_is_already_installed:
                    description = ServerSideMessagesResources.AgentIsAlreadyInstalledMessage;
                    break;
                case DescentStatus.status_agent_is_not_installed:
                    description = ServerSideMessagesResources.AgentIsNotInstalledMessage;
                    break;
                case DescentStatus.status_installer_unknown_product:
                    description = ServerSideMessagesResources.UnknownProductMessage;
                    break;
                case DescentStatus.status_installer_fatal_error:
                    description = ServerSideMessagesResources.FatalInstallationErrorMessage;
                    break;
                case DescentStatus.status_installer_install_already_running:
                    description = ServerSideMessagesResources.AnotherInstallationInProgressMessage;
                    suggestion = ServerSideMessagesResources.AnotherInstallationInProgressSuggestionMessage;
                    break;
                case DescentStatus.status_installer_product_already_installed:
                    description = ServerSideMessagesResources.ProductAlreadyInstalledMessage;
                    break;
                case DescentStatus.status_remote_cannot_copy_file:
                    description = ServerSideMessagesResources.CannotCopyFileMessage;
                    suggestion = ServerSideMessagesResources.CannotCopyFileSuggestionMessage;
                    break;
                case DescentStatus.status_remote_cannot_setup_descent_service:
                    description = ServerSideMessagesResources.CannotSetupDescentServiceMessage;
                    suggestion = ServerSideMessagesResources.CannotSetupDescentServiceSuggestionMessage;
                    break;
                case DescentStatus.status_remote_cannot_cleanup_descent_service:
                    description = ServerSideMessagesResources.CannotRemoveRemoteServiceMessage;
                    break;
                case DescentStatus.status_remote_cannot_run_descent_service:
                    description = ServerSideMessagesResources.CannotStartRemoteServiceMessage;
                    break;
                case DescentStatus.status_remote_invalid_response:
                    description = ServerSideMessagesResources.InvalidResponseMessage;
                    break;
                case DescentStatus.status_bad_admin_netpath:
                    description = ServerSideMessagesResources.TargetComputerNotRespondingMessage;
                    suggestion = ServerSideMessagesResources.TargetComputerNotRespondingSuggestionMessage;
                    break;
                case DescentStatus.status_agent_timeout:
                    description = ServerSideMessagesResources.AgentDescentTimeoutMessage;
                    suggestion = ServerSideMessagesResources.AgentDescentTimeoutSuggestionMessage;
                    break;
                case DescentStatus.status_operation_canceled:
                    description = new ResourceText("null", "Scan operation was canceled.");
                    break;
                default:
                    description = ServerSideMessagesResources.UnsupportedErrorMessage;
                    break;
            }
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
                    QueryResult.OnDescendOperationEvent(true, _computer, text, text, status.ToString(), win32Error, response);
                }
            }
            else if (status == DescentStatus.status_operation_canceled)
            {
                if (QueryResult != null)
                {
                    FormattedText shortMessage;
                    FormattedText description = GetDescentStatusDescription(status, out shortMessage);
                    _changeTrackingSubSystem.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new DescentProgressItem(_computer.Uid, _computer.Name, QueryResult.OperationUid, shortMessage, AptScanStage.OperationCanceled, 100)), false);

                    QueryResult.OnDescendOperationEvent(true, _computer, description, shortMessage, status.ToString(), win32Error, response);
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
                    messageText = GetDescentStatusInformation(errorFormat, status, win32Error, response, out shortMessage);
                }
                catch { }

                if (messageText != null && QueryResult != null)
                {
                    FormattedText formattedText;
                    if (status == DescentStatus.status_error)
                        formattedText = new FormattedText(new ResourceText("null", "Failed: {0}"), response);
                    else
                        formattedText = new FormattedText(new ResourceText("null", "Failed: {0}. {1}"), shortMessage, response);
                    _changeTrackingSubSystem.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new DescentProgressItem
                        (_computer.Uid, _computer.Name, QueryResult.OperationUid, formattedText, AptScanStage.OperationFailed, 100)), false);

                    QueryResult.OnDescendOperationEvent(false, _computer, messageText, shortMessage, status.ToString(), win32Error, response);
                }
            }
        }

        #endregion

    }
}
