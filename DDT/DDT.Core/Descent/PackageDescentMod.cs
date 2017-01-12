using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Descent.Helpers.Transport;
using APTAT.DeploymentCore.Messages;
using APTAT.DeploymentCore.Properties;

namespace DDT.Core.Descent
{
    public class PackageDescentMod : DescentMod
    {
        private const string InstallMsiActionName = "action_install_msi"; 
        private const string InstallExeActionName = "action_install_exe";
        private bool _isCancel;
        public PackageDescentMod(TaskFactory taskFactory, IChangeTrackingSubSystem changeTrackingSubSystem) 
            : base(taskFactory, changeTrackingSubSystem)
        {
        }

        protected override DescentHelper CreateDescentHelper(InstallAgentThreadParams tp)
        {
            var dtp = (PackageDeploymentThreadParams) tp;
            
            var dh =  new PackageDescentHelper(tp.RemoteHostInfo, dtp.PackageInfoX86, dtp.PackageInfoX64,
                TaskFactory.CancellationToken);
            dh.InstallProgressChanged += OnInstallProgressChanged;
            dh.SystemKindSpeciefied += OnSystemKindSpecified;
            return dh;
        }

        public void SetCancellationToken()
        {
            _isCancel = true;
        }

        protected override void DescentHelperBeforeRelease(DescentHelper dh)
        {
            base.DescentHelperBeforeRelease(dh);
            var pdh = (PackageDescentHelper)dh;
            pdh.InstallProgressChanged -= OnInstallProgressChanged;
            pdh.SystemKindSpeciefied -= OnSystemKindSpecified;

        }

        protected override void NotifyProgressChanged(Guid operationUid, string hostName, Guid computerUid, ResourceText resourceMessage,
            int percentage, bool checkCancellation = true)
        {
            if (_isCancel)
                checkCancellation = true;
            base.NotifyProgressChanged(operationUid, hostName, computerUid, resourceMessage, percentage, checkCancellation);
        }

        protected virtual void OnInstallProgressChanged(object sender, PackageDescentHelper.InstallProgressChangedEventArgs args)
        {
            ResourceText text;
            text = args.ActionName == InstallExeActionName || args.Percentage == 0? 
                new ResourceText("null", "Installation... ") 
                : new ResourceText("null", $"Installation ({args.Percentage}%). ");
            var tp = ParamsByUid[args.TargetUid];
            var percentage = ((double)args.Percentage/100)*40;
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, (int)(50 + percentage), false);
        }

        protected virtual void OnSystemKindSpecified(object sender, PackageDescentHelper.SystemKindSpecifyEventArgs args)
        {
            var tp = ParamsByUid[args.TargetUid];

            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, args.SystemKind);

        }

        private void NotifyProgressChanged(Guid operationUid, string hostName, Guid targetUid, SystemKind systemKind)
        {
            if (TaskFactory.CancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(); 
            Tracker.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, new SystemKindItem(systemKind, operationUid, targetUid)), false);
        }
        private void NotifyProgressChanged(Guid operationUid, Guid targetUid, SystemKind systemKind, bool is64, string packageName)
        {
            if (TaskFactory.CancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();
            Tracker.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed, 
                new PackageNameSpecified(systemKind, packageName, is64, operationUid, targetUid)), false);
        }

        protected override void DeploymentThread(object obj)
        {
            
            var tp = (InstallAgentThreadParams)obj;
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity,
                        ServerSideMessagesResources.DescentProgressAgentInstallConnectingToTarget, 0);
            base.DeploymentThread(obj);

        }

        public void StartDeployment(RemoteHostInfo ri, IDescentCallback cb, Guid targetUid, Guid operationUid, TimeSpan timeout, InstallPackageInfo packageX86, InstallPackageInfo packageX64)
        {
            var tp = new PackageDeploymentThreadParams()
            {
                RemoteHostInfo = ri,
                DescentCallback = cb,
                AgentIdentity = targetUid,
                OperationUid = operationUid,
                PackageInfoX86 = packageX86,
                PackageInfoX64 = packageX64,
                TimeOut = timeout
            };
            StartDeploymentThread(tp);
        }
        protected override void ExecuteAction(DescentHelper dh, InstallAgentThreadParams tp)
        {
            var parameters = (PackageDeploymentThreadParams) tp;
            PackageDescentHelper helper = (PackageDescentHelper)dh;
            bool is64 = helper.Is64Bit();

            var agentKind = SystemKind.Windows;
            InstallPackageInfo package;
            if (is64)
            {
                package = parameters.PackageInfoX64;
                agentKind |= SystemKind.IsIntelX64Edition;
            }
            else
            {
                package = parameters.PackageInfoX86;
            }
            // 50% Copying agent installation package
            NotifyProgressChanged(tp.OperationUid, tp.AgentIdentity, agentKind, is64, Path.GetFileName(package.Path));
            NotifyProgressChanged(tp.OperationUid,tp.RemoteHostInfo.HostName, tp.AgentIdentity, ServerSideMessagesResources.DescentProgressAgentInstallCopyingAgentPackage, 50);
            


            ////TODO: Make DescentHelper report full SystemKind instead of just platform
            //string[] split = dh.Response.Response.Split(',');
            //if (split.Length != 5)
            //{
            //    throw new DescentException(DescentStatus.status_remote_invalid_response, "Query agent information failed: invalid response", 0);
            //}

            //var agentKind = SystemKind.Windows;
            //bool is64bit = false;
            //string platform = split[0];

            //if (platform == "64")
            //{
            //    agentKind |= SystemKind.IsIntelX64Edition;
            //    is64bit = true;
            //}

            //// We need to reestablish connection to the remote computer now.
            //dh.Release();
            //ImpersonationHelper.Logon32LogonType impersonationLogonType =
            //            ImpersonationHelper.Logon32LogonType.NewCredentials;
            //dh.Init(impersonationLogonType);

            
            
            //dh.CopyFile(package.Path, true);
            var packageParams = package.Parameters;
            if (packageParams == null)
                packageParams = String.Empty;
            if (package.PackageType == InstallPackageType.Msi && !String.IsNullOrEmpty(package.MstPath))
            {
                var remoteMstPath = helper.GetRemotePath(package.MstPath);
                var index = packageParams.IndexOf("transforms", StringComparison.InvariantCultureIgnoreCase);
                if (index != -1)
                {
                    int startIndex = packageParams.IndexOf(" ", index, StringComparison.InvariantCultureIgnoreCase);
                    if (startIndex == -1)
                    {
                        packageParams += String.Format(@" transforms=""{0}""", remoteMstPath);
                    }
                    else
                    {
                        packageParams.Insert(startIndex - 1, String.Format(@";""{0}""", remoteMstPath));
                    }
                }
                else
                {
                    packageParams += String.Format(@" transforms=""{0}""", remoteMstPath);
                }
            }

            string remotePath = helper.GetRemotePath(package.Path);
            string agentInstallParameters = string.Format("{0}*{1}", remotePath, packageParams);
            var packageParameters = new List<string> {agentInstallParameters};
            if (package.PackageType == InstallPackageType.Exe && package.ImpersonateToCurrentLoggedUser)
            {
                packageParameters.Add("-impersonate");
                packageParameters.Add("LoggedUserExecutionContext");    
            }
            // Install msi 
            dh.ExecuteAction(
                package.PackageType == InstallPackageType.Msi ? InstallMsiActionName : InstallExeActionName,
                packageParameters.ToArray(),
                (int)parameters.TimeOut.TotalMilliseconds);

            // 90% Finalizing agent istallation
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, 
                ServerSideMessagesResources.DescentProgressAgentInstallFinalizingAgentInstallation, 90, false);
            
        }

        protected override void ProcessResults(InstallAgentThreadParams tp, DescentHelper dh)
        {
            base.ProcessResults(tp, dh);

        }

        protected class PackageDeploymentThreadParams : InstallAgentThreadParams 
        {
            public InstallPackageInfo PackageInfoX86 { get; set; }
            public InstallPackageInfo PackageInfoX64 { get; set; }
            public TimeSpan TimeOut { get; set; }
        }
    }
}
