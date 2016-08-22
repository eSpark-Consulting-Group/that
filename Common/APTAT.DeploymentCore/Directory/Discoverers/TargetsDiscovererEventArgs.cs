using System;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore.Directory.Discoverers
{
    public class TargetsDiscovererEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public DeploymentTarget[] Targets { get; set; }
        public object State { get; set; }
    }
}
