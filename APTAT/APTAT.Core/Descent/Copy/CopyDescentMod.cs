using System;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Descent.Helpers.Transport;
using APTAT.DeploymentCore.Messages;
using APTAT.DeploymentCore.Properties;

namespace APTAT.Core.Descent.Copy
{
    public class CopyDescentMod : DescentMod
    {
        private bool _isCanceled;
        public CopyDescentMod(TaskFactory taskFactory, IChangeTrackingSubSystem changeTrackingSubSystem)
            : base(taskFactory, changeTrackingSubSystem)
        {
        }

        protected override DescentHelper CreateDescentHelper(InstallAgentThreadParams tp)
        {
            var dtp = (PackageDeploymentThreadParams)tp;

            var dh = new CopyDescentHelper(tp.RemoteHostInfo, dtp.PackagePathes,
                TaskFactory.CancellationToken);
            dh.CopyProgressChanged += OnCopyProgressChanged;
            dh.CopyCompleted += OnCopyCompleted;
            return dh;
        }

        public void SetCancellationToken()
        {
            _isCanceled = true;
        }

        protected override void DescentHelperBeforeRelease(DescentHelper dh)
        {
            base.DescentHelperBeforeRelease(dh);
            var pdh = (CopyDescentHelper)dh;
            pdh.CopyProgressChanged -= OnCopyProgressChanged;
            pdh.CopyCompleted -= OnCopyCompleted;

        }

        protected override void NotifyProgressChanged(Guid operationUid, string hostName, Guid computerUid, ResourceText resourceMessage,
            int percentage, bool checkCancellation = true)
        {
            if (_isCanceled)
                checkCancellation = true;
            base.NotifyProgressChanged(operationUid, hostName, computerUid, resourceMessage, percentage, checkCancellation);
        }

        protected virtual void OnCopyProgressChanged(object sender, CopyDescentHelper.CopyProgressChangedEventArgs args)
        {
            var text = new ResourceText("null", "copying... ");
            var tp = ParamsByUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, args.Percentage, args.FilePathes);
        }

        protected virtual void OnCopyCompleted(object sender, CopyDescentHelper.CopyCompletedEventArgs args)
        {
            var text = new ResourceText("null", "copying... ");
            var tp = ParamsByUid[args.TargetUid];
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, text, 100, args.FilePathes);
        }

        private void NotifyProgressChanged(Guid operationUid, string hostName, Guid computerUid, ResourceText resourceMessage,
            int percentage, string[] filePathes)
        {
            if (TaskFactory.CancellationToken.IsCancellationRequested)
                throw new OperationCanceledException();
            
            FormattedText formattedText = new FormattedText(resourceMessage);
            Tracker?.NotifyChanged(new ChangesTrackingItem(ChangeType.Changed,
                new CopyDescentProgressItem(computerUid, hostName, operationUid, formattedText, percentage, filePathes)), false);
        }

        protected override void DeploymentThread(object obj)
        {
            var tp = (InstallAgentThreadParams)obj;
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity,
                ServerSideMessagesResources.DescentProgressAgentInstallConnectingToTarget, 0);
            base.DeploymentThread(obj);
        }

        public void StartDeployment(RemoteHostInfo ri, IDescentCallback cb, Guid targetUid, Guid operationUid,
            string[] packagePathes)
        {
            var tp = new PackageDeploymentThreadParams()
            {
                RemoteHostInfo = ri,    
                DescentCallback = cb,
                AgentIdentity = targetUid,
                OperationUid = operationUid,
                PackagePathes = packagePathes
            };
            StartDeploymentThread(tp);
        }

        protected override void ExecuteAction(DescentHelper dh, InstallAgentThreadParams tp)
        {
            var parameters = (PackageDeploymentThreadParams)tp;

            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity, 
                ServerSideMessagesResources.DescentProgressAgentInstallCopyingAgentPackage, 50);
                                   
            // Install msi 
            dh.ExecuteAction(String.Empty, new string[0], 0);

            // 90% Finalizing agent istallation
            NotifyProgressChanged(tp.OperationUid, tp.RemoteHostInfo.HostName, tp.AgentIdentity,
                ServerSideMessagesResources.DescentProgressAgentInstallFinalizingAgentInstallation, 90, false);
        }


        protected class PackageDeploymentThreadParams : InstallAgentThreadParams
        {
            public string[] PackagePathes { get; set; }
        }
    }
}

