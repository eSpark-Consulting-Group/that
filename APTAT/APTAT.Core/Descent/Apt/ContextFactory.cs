using System.Collections.Generic;
using System.Linq;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;

namespace APTAT.Core.Descent.Apt
{
    public sealed class ContextFactory : IContextFactory
    {
        public IDescentQueryResult CreateContext(IEnumerable<IComputer> computers,
                IDevicesSourceDescription deviceSourceDesctiption)
        {
            var result = new AptScanOperationContext(computers, computers.Count(), deviceSourceDesctiption);
            return result;
        }
    }

}