using System;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore
{
    public class DeploymentTargetResolvingEventArg:EventArgs
    {
        public DeploymentTargetResolvingEventArg(DeploymentTarget target)
        {
            this.Target = target;
        }
        public DeploymentTarget Target { get; }
    }
}