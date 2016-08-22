using System;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore.Directory.Resolvers
{
    public class TargetsResolverEventArgs : EventArgs
    {
        public TargetsResolverEventArgs()
        {
            Targets = new DeploymentTarget[]{ };
        }
        public Exception Error { get; set; }
        public DeploymentTarget[] Targets { get; set; }
        public object State { get; set; }
    }
}
