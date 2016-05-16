using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APTAT.Core.Directory.Discoverers;
using APTAT.Core.Directory.Resolvers;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;

namespace APTAT.Core.Directory
{
    public interface ITargetsManager
    {
        IDomainTargetsResolver GetDomainTargetsResolver(DeploymentTargetsCollection deploymentTargets, string admin, string password);
        IDomainDiscoverer GetDomainDiscoverer(string ldapPath, bool bfromRoot, string domainController, string admin, string password);
        IWorkgroupDiscoverer GetWorkgroupDiscoverer(string workgroupName);
        IIpAddressDiscoverer GetIpAddressDiscoverer(EndpointAddressCollection ipAddressCollection);
    }
}
