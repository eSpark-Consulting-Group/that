using System;
using System.Collections.Generic;

using APTAT.Common;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Messages;
using APTAT.DeploymentCore.Properties;

namespace DDT.Core.Descent
{
    public class DescentScanner : Scanner
    {
        public const int CancelItemsInQueue = 0;
        public const int CancelAllITems = 1;
        private PackageDescentMod _descentMod;
        private readonly TimeSpan _timeout;
        private readonly InstallPackageInfo _packageX86;
        private readonly InstallPackageInfo _packageX64;

        public DescentScanner(IEnumerable<DeploymentTarget> targets, 
            string login, string password, IDevicesSourceDescription devicesSourceDescription, TimeSpan timeout,
            InstallPackageInfo packageX86, InstallPackageInfo packageX64)
            : base(targets, login, password, devicesSourceDescription)
        {
            _timeout = timeout;
            _packageX86 = packageX86;
            _packageX64 = packageX64;
        }


        public override IDescentQueryResult ValidateAndGetResult()
        {
            //if (Login == null)
            //    throw new ArgumentException("Login name not specified.", "login");
            //if (Password == null)
            //    throw new ArgumentException("Password not specified.", "password");

            var result = new PackageDescentResult(OnlineTargets, DevicesSourceDescription);
            return result;
        }

        public override void StartScan(IChangeTrackingSubSystem changeTrackingSubSystem,IDescentQueryResult result)
        {
            if (_descentMod != null)
            {
                _descentMod.Release();
            }
            _descentMod = new PackageDescentMod(TaskFactory, changeTrackingSubSystem);
            foreach (var computer in OnlineTargets)
            { 
                RemoteHostInfo ri = new RemoteHostInfo
                {
                    HostName = computer.IPAddress != null ? computer.IPAddress.ToString() : computer.DnsName,
                    Login = Login,
                    Password = Password,
                    TargetUid = computer.Uid
                };

                var job = new RemoteInstallerJob(result, changeTrackingSubSystem, this, computer);
                _descentMod.StartDeployment(ri, job, computer.Uid, result.OperationUid, _timeout,
                    _packageX86, _packageX64);
            }
        }

        public override void AbortAllThread(int code = 0)
        {
            if (code == CancelAllITems)
                _descentMod.SetCancellationToken();
            base.AbortAllThread(code);
        }

        protected override void GetDescriptionAndSuggestionByStatus(DescentStatus status, out ResourceText description, out ResourceText suggestion)
        {
            suggestion = ResourceText.Empty;
            description = ResourceText.Empty;
            var askBase = false;
            switch (status)
            {
                case DescentStatus.status_operation_canceled:
                    description = new ResourceText("", "null", "Deployment operation was stopped.");
                    break;
                case DescentStatus.status_invalid_command_line:
                    description = new ResourceText("", "null", "Invalid command line argument.");
                    break;
                case DescentStatus.status_error_product_version:
                    description = new ResourceText("", "null", "Another version of this product is already installed. " +
                                                                 "Installation of this version cannot continue.");
                    break;
                case DescentStatus.status_success_reboot_required:
                    description = new ResourceText("", "null", "Installation was successful, but changes will not be " +
                                                                 "effective until the system is rebooted.");
                    break;
                case DescentStatus.status_agent_was_crashed:
                    description = new ResourceText("", "null", "Agent was crashed.");
                    break;
                default:
                    askBase = true;
                    break;
            }
            if (askBase)
                base.GetDescriptionAndSuggestionByStatus(status, out description, out suggestion);
        }

        public override FormattedText GetDescentStatusInformation(ResourceText errorFormat, DescentStatus status, int win32Error,
            string errorString, out FormattedText shortMessage)
        {
            switch (status)
            {
                case DescentStatus.status_remote_invalid_response:
                {
                    if (errorString == "The network path was not found.")
                    {
                        var text = new FormattedText(new ResourceText("", "null", errorString));
                        shortMessage = text;
                        return text;
                    }
                    break;
                }
                case DescentStatus.status_msi_error:
                {
                    var text = new FormattedText(new ResourceText("", "null", $"Error \"{win32Error}\" occurred during installation"));
                    shortMessage = text;
                    return text;
                }
                case DescentStatus.status_exe_error:
                {
                    var text = new FormattedText(new ResourceText("", "null", $"Installation finished with error code \"{win32Error}\""));
                    shortMessage = text;
                    return text;
                }
                case DescentStatus.status_installing_timeout:
                {
                    var resource = ServerSideMessagesResources.InstallationTimeoutMessage;
                    var text = new FormattedText(resource, _timeout.Minutes);
                    shortMessage = text;
                    return text;
                }
            }
            return base.GetDescentStatusInformation(errorFormat, status, win32Error, errorString, out shortMessage);
        }
        public override void Dispose()
        {
            base.Dispose();
            if (_descentMod != null)
            {
                _descentMod.Release();
                _descentMod = null;
            }
        }
    }
}
