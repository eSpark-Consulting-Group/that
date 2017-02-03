using System;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore
{
    public class DeploymentTargetResolvingEventArg:EventArgs
    {
        public DeploymentTargetResolvingEventArg(DeploymentTarget target, int endpointCount, int endpointProcessed)
        {
            this.Target = target;
            this.TotalEndpointsFound = endpointCount;
            this.EndpointProcessed = endpointProcessed;
        }

        public int EndpointProcessed { get; set; }

        public int TotalEndpointsFound { get; set; }

        public DeploymentTarget Target { get; }
    }
}