using APTAT.DeploymentCore.Directory.Discoverers;
using APTAT.DeploymentCore.Directory.Resolvers;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;

namespace APTAT.DeploymentCore.Directory
{
    public interface ITargetsManager
    {
        IDomainTargetsResolver GetDomainTargetsResolver(DeploymentTargetsCollection deploymentTargets, string admin, string password);
        IDomainDiscoverer GetDomainDiscoverer(string ldapPath, bool bfromRoot, string domainController, string admin, string password);
        IWorkgroupDiscoverer GetWorkgroupDiscoverer(string workgroupName);
        IIpAddressDiscoverer GetIpAddressDiscoverer(EndpointAddressCollection ipAddressCollection);
    }
}
