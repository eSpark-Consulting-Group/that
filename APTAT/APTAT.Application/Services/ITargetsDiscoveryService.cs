using System;
using System.Collections.Generic;
using APTAT.Application.Models;
using System.Threading.Tasks;

namespace APTAT.Application.Services
{
    public interface ITargetsDiscoveryService
    {
        event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryItemsCompleted;
        event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryRootCompleted;
        event EventHandler<DirectoryItemsResolvedEventArgs> DirectoryItemsResolved;
        event EventHandler<GetWorkgroupEventArgs> GetWorkgroupCompleted;
        event EventHandler<GetByIpAddressEventArgs> GetByIpAddressesCompleted;
        void GetByIpAddresses(IEnumerable<IpAddressWrapper> ipAddresses, Object userState);

        void GetWorkgroup(String workgroupName, Object userState);

        Task<GetWorkgroupEventArgs> GetWorkgroups(Action selectAction, Object userState);

        void GetDirectoryItems(String domainController, String admin, String password, DirectoryItem parent, Object userState);

        void GetDirectoryRoot(String domainController, String admin, String password, Object userState);

        void ResolveDirectoryItems(DeploymentItem[] directoryItems, String admin, String password, Object userState);

        Task<GetDomainNameResponse> GetCurrentDomainName();
        string GetLoggedInUser();
    }
}
