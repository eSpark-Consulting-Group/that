using System;
using System.Collections.Generic;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Directory.Resolvers
{
    public class TargetsResolverEventArgs : EventArgs
    {
        public Exception Error { get; set; }
        public DeploymentTarget[] Targets { get; set; }
        public object State { get; set; }
    }
}
