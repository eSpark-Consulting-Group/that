using System.Collections.Generic;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;

namespace APTAT.Core.Descent.Apt
{
    public interface IContextFactory
    {
        IDescentQueryResult CreateContext(
        IEnumerable<IComputer> computers, IDevicesSourceDescription deviceSourceDescription);
    }
}