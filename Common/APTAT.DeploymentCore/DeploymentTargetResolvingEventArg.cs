using System;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore
{
    public class DeploymentTargetResolvingEventArg:EventArgs
    {
        public DeploymentTargetResolvingEventArg(DeploymentTarget target, double  progress, Exception exception = null)
        {
            this.Target = target;
            this.Progress = progress;
            this.Error = exception;

        }

        public Exception Error { get; set; }
        public double Progress { get; set; }


        public DeploymentTarget Target { get; }
    }
}