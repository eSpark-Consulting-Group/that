using System.Threading.Tasks;
using APTAT.Common;
using System.Collections.Generic;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;

namespace APTAT.Core.Descent.Apt
{
    public interface IAptFactory
    {
        IAptDescentMod CreateDescentMode(TaskFactory taskFactory, IChangeTrackingSubSystem changeTrackingSubSystem);
        IDescentQueryResult CreateContext(IEnumerable<IComputer> computers, IDevicesSourceDescription deviceSourceDescription);
    }

}