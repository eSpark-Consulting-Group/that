using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Directory.Discoverers
{
    public interface ITargetsDiscoverer
    {
        DeploymentTargetsCollection Discover();
        void BeginDiscover(object state = null);
        event EventHandler<TargetsDiscovererEventArgs> Discovered;
    }

    public interface IDomainDiscoverer : ITargetsDiscoverer
    {
        
    }

    public interface IWorkgroupDiscoverer : ITargetsDiscoverer
    {
    }
    
    public interface IIpAddressDiscoverer : ITargetsDiscoverer
    {
    }
}
