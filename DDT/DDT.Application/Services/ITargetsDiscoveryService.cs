using System;
using System.Collections.Generic;
using DDT.Application.Models;
using System.Threading.Tasks;

namespace DDT.Application.Services
{
    public interface ITargetsDiscoveryService
    {
        Task<GetByIpAddressEventArgs> GetByIpAddresses(IEnumerable<IpAddressWrapper> ipAddresses);
        Task<GetWorkgroupEventArgs> GetWorkgroup(String workgroupName, DirectoryItem parent, Action selectAction);
        Task<GetWorkgroupEventArgs> GetWorkgroups(Action selectAction);
        Task<GetDirectoryItemsEventArgs> GetDirectoryItems(String domainController, String admin, String password, DirectoryItem parent, Action selectAction);
        Task<GetDirectoryItemsEventArgs> GetDirectoryRoot(String domainController, String admin, String password, Action selectAction);
        Task<GetDomainNameResponse> GetCurrentDomainName();
        Task<AggregatedDeploymentItemsResolvedEventArgs> ResolveAggregatedDeploymentItems(
            IList<DeploymentItem> directoryItemsByIP, 
            IList<DeploymentItem> directoryItemsByWorkGroup, 
            IList<DeploymentItem> directoryItemsByDomain, 
            String admin, String password);
    }
}