using System;
using System.Collections.Generic;
using System.Net;
using FAT.Application.Models;
using System.Threading.Tasks;

namespace FAT.Application.Services
{
    public interface ITargetsDiscoveryService
    {
        event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryItemsCompleted;
        event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryRootCompleted;
        event EventHandler<DirectoryItemsResolvedEventArgs> DirectoryItemsResolved;
        event EventHandler<GetDirectoryItemsEventArgs> GetWorkgroupCompleted;
        event EventHandler<GetByIpAddressEventArgs> GetByIpAddressesCompleted;
        void GetByIpAddresses(IEnumerable<IpAddressWrapper> ipAddresses, Action<bool> localAction, Object userState);

        void GetWorkgroup(String workgroupName, DirectoryItem parent, Object userState);

        Task<GetDirectoryItemsEventArgs> GetWorkgroups(Object userState);

        void GetDirectoryItems(String domainController, String admin, String password, DirectoryItem parent, Object userState);

        void GetDirectoryRoot(String domainController, String admin, String password, Object userState);

        void ResolveDirectoryItems(DeploymentItem[] directoryItems, String admin, String password, Object userState);

        Task<GetDomainNameResponse> GetCurrentDomainName();
        bool IsLocalAddress(string name);
        

        string GetLoggedInUser();
    }
}
