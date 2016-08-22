using DDT.Core.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDT.Application.Models
{
    public static class MapHelper
    {
        public static DeploymentStatus CalculateDeploymentStatus(this DeploymentItem item)
        {
            if (!item.Succeded && !item.IsOnline) return DeploymentStatus.Skipped;
            if (!item.Succeded && !item.Stopping && item.IsOnline) return DeploymentStatus.Failed;
            if (item.Stopping) return DeploymentStatus.Stoped;
            if (item.Succeded) return DeploymentStatus.Succeded;

            throw new InvalidOperationException("Deployment status calculatiion error");

        }
    }
}
