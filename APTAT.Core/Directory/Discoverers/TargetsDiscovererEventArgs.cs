using System;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Directory.Discoverers
{
    public class TargetsDiscovererEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public DeploymentTarget[] Targets { get; set; }
        public object State { get; set; }
    }
}
