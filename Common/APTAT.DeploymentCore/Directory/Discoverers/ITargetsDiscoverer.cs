using System;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using System.Threading.Tasks;

namespace APTAT.DeploymentCore.Directory.Discoverers
{
    public interface ITargetsDiscoverer
    {
        DeploymentTargetsCollection Discover();
        Task<TargetsDiscovererEventArgs> DiscoverAwaitable();
        void BeginDiscover(object state = null);
        event EventHandler<TargetsDiscovererEventArgs> Discovered;
    }

    public interface IDomainDiscoverer : ITargetsDiscoverer
    {
    }

    public interface IWorkgroupDiscoverer : ITargetsDiscoverer
    {
        Task<string[]> EnumerateWorkgroups();
    }

    public interface IIpAddressDiscoverer : ITargetsDiscoverer
    {
    }
}
