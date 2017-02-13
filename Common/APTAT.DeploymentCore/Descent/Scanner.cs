using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;
using APTAT.DeploymentCore.Properties;

namespace APTAT.DeploymentCore.Descent
{
    public abstract class Scanner : IScanner
    {
        protected readonly TaskFactory TaskFactory;
        protected readonly string Login;
        protected readonly string Password;
        protected readonly IComputer[] Targets;
        protected readonly IComputer[] OnlineTargets;
        protected readonly CancellationToken CancellationToken;
        protected readonly IDevicesSourceDescription DevicesSourceDescription;
        protected CancellationTokenSource CancellationTokenSource { get; private set; }

        protected Scanner(IEnumerable<IComputer> targets, string login, string password
            , IDevicesSourceDescription devicesSourceDescription)
        {
            DevicesSourceDescription = devicesSourceDescription;
            CancellationTokenSource = new CancellationTokenSource();
            CancellationToken = CancellationTokenSource.Token;
            TaskFactory = new TaskFactory(
                CancellationToken,
                TaskCreationOptions.LongRunning,
                TaskContinuationOptions.None,
                new LimitedConcurrencyLevelTaskScheduler(Settings.Default.DescentMaximumConcurrencyLevel));
            var deploymentTargets   = targets as IComputer[] ?? targets.ToArray();
            Targets = deploymentTargets.ToArray();
            OnlineTargets = GetOnlineComputers(deploymentTargets); ;
            Login = login;
            Password = password;
        }

        protected static IComputer[] GetOnlineComputers(IEnumerable<IComputer> targets)
        {
            List<IComputer> onlineTargetsInternal = new List<IComputer>();
            var deploymentTargets = targets as IComputer[] ?? targets.ToArray();
            for (int index = 0; index < deploymentTargets.Length; index++)
            {
                var target = deploymentTargets[index];
                if (target.IsOnline)
                    onlineTargetsInternal.Add(target);
            }
            return onlineTargetsInternal.ToArray();
        }

        public virtual DescentEventArgs GetDescentCompletedEventArgs(IDescentQueryResult result)
        {
            return new DescentEventArgs() {OperationUid = result.OperationUid};
        }

        public const string TimeoutAbortString = "OH SHI--";

        public abstract IDescentQueryResult ValidateAndGetResult();
        public abstract void StartScan(IChangeTrackingSubSystem changeTrackingSubSystem,IDescentQueryResult result);

        public virtual void AbortAllThread(int code = 0)
        {
            CancellationTokenSource.Cancel();
        }
        public virtual void Dispose()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Dispose();
                CancellationTokenSource = null;
            }
        }


        public virtual FormattedText GetDescentStatusInformation(ResourceText errorFormat, DescentStatus status, int win32Error, string errorString, out FormattedText shortMessage)
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


        public virtual FormattedText GetDescentStatusDescription(DescentStatus status, out FormattedText shortMessage)
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

        public abstract void StopScan(Guid targetUid);

        protected virtual void GetDescriptionAndSuggestionByStatus(DescentStatus status,
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
                case DescentStatus.status_installing_timeout:
                    description = ServerSideMessagesResources.InstallationTimeoutMessage;
                    suggestion = ServerSideMessagesResources.InstallationTimeoutMessage;
                    break;
                case DescentStatus.status_operation_canceled:
                    description = new ResourceText("null", "Scan operation was canceled.");
                    break;
                case DescentStatus.status_rpc_server_is_too_busy:
                    description = ServerSideMessagesResources.RPCServerIsTooBusyToCompleteThisOperation;
                    break;
                default:
                    description = ServerSideMessagesResources.UnsupportedErrorMessage;
                    break;
            }
        }
    }
}
