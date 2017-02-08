using System;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore
{
    public class DeploymentTargetResolvingEventArg:EventArgs
    {
        public DeploymentTargetResolvingEventArg(DeploymentTarget target, double  progress)
        {
            this.Target = target;
            this.Progress = progress;
            
        }

        public double Progress { get; set; }


        public DeploymentTarget Target { get; }
    }
}