using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Directory.Discoverers;
using APTAT.DeploymentCore.Directory.Resolvers;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using DDT.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDT.Application.Services
{
    public class TargetsDiscoveryService : ITargetsDiscoveryService
    {
        private readonly ITargetsManager _manager;

        public TargetsDiscoveryService(ITargetsManager manager)
        {
            _manager = manager;
        }
     
        public async Task<GetDirectoryItemsEventArgs> GetDirectoryRoot(String domainController, String admin, String password, Action selectAction)
        {
            IDomainDiscoverer discoverer = _manager.GetDomainDiscoverer(null, true, domainController, admin, password);
            TargetsDiscovererEventArgs e = await discoverer.DiscoverAwaitable();

            if (e.Error == null)
            {
                DomainTarget domainRoot = (DomainTarget) e.Targets[0];
                DirectoryItem root = new DirectoryItem
                {
                    DnsName = domainRoot.DnsName,
                    Name = domainRoot.Name.Replace("DC=", ""),
                    LdapPath = domainRoot.LdapPath,
                    TargetUid = domainRoot.Uid,
                    Type = FromDomainEntryType(domainRoot.EntryType),
                    IsSelected = false,
                    SelectionChangedAction = selectAction,
                    Items = new DirectoryItem[] { null }
                };
                var result = new DirectoryItem[] { root };

                return new GetDirectoryItemsEventArgs(result);
            }
            return new GetDirectoryItemsEventArgs(null, e.Error);
        }

        public async Task<GetDomainNameResponse> GetCurrentDomainName()
        {
            IDomainDiscoverer discoverer = _manager.GetDomainDiscoverer(null, true, null, null, null);
            TargetsDiscovererEventArgs e = await discoverer.DiscoverAwaitable();
            if (e.Error == null)
            {
                DomainTarget domainRoot = (DomainTarget)e.Targets[0];
                return new GetDomainNameResponse(domainRoot.DomainName, null);

            }
            else if (e.Error is DirectoryQueryException &&
                     ((DirectoryQueryException) e.Error).Error == DirectoryError.DefaultDomainNotFound)
            {
                DomainTarget domainRoot = (DomainTarget)e.Targets[0];
                return new GetDomainNameResponse(null, null);
            }
            return new GetDomainNameResponse(null, e.Error);
        }

        public async Task<GetDirectoryItemsEventArgs> GetDirectoryItems(String domainController, String admin, String password, DirectoryItem parent, Action selectAction)
        {
            IDomainDiscoverer discoverer = _manager.GetDomainDiscoverer(parent.LdapPath, false, domainController, admin, password);
            TargetsDiscovererEventArgs e = await discoverer.DiscoverAwaitable();
                                    
            if (e.Error == null)
            {
                var targets = e.Targets;
                var result = new DirectoryItem[targets.Length];
                for (var i = 0; i < targets.Length; ++i)
                {
                    var target = targets[i];
                    var type = FromDomainEntryType(((DomainTarget)target).EntryType);
                    result[i] = new DirectoryItem
                    {
                        DnsName = target.DnsName,
                        Name = target.Name,
                        LdapPath = target.LdapPath,
                        TargetUid = target.Uid,
                        SystemKind = target.SystemKind,
                        Type = type,
                        Source = target.Source.MapFromTargetSource(),
                        Parent = parent,
                        IsSelected = parent.IsSelected == true,
                        SelectionChangedAction = selectAction,
                        Items = type == DirectoryItemType.OrgUnit ? new DirectoryItem[] { null } : null
                    };
                }                
                return new GetDirectoryItemsEventArgs(result); 
            }
            else
            {
                return new GetDirectoryItemsEventArgs(null, e.Error);                
            }
        }

        public async Task<GetWorkgroupEventArgs> GetWorkgroups(Action selectAction)
        {
            IWorkgroupDiscoverer discoverer = _manager.GetWorkgroupDiscoverer(null);
            try
            {
                var targets = await discoverer.EnumerateWorkgroups();

                var result = new DirectoryItem[targets.Length];

                for (var i = 0; i < targets.Length; ++i)
                {
                    result[i] = new DirectoryItem
                    {
                        IsWorkgroup = true,
                        Name = targets[i],
                        Type = DirectoryItemType.Root,
                        IsSelected = false,
                        SelectionChangedAction = selectAction,
                        Items = new DirectoryItem[] { null }
                    };
                }

                return new GetWorkgroupEventArgs(result);
            }
            catch (Exception ex)
            {
                return new GetWorkgroupEventArgs(null, ex);
            }
        }

        public async Task<GetWorkgroupEventArgs> GetWorkgroup(String workgroupName, DirectoryItem parent, Action selectAction)
        {
            IWorkgroupDiscoverer discoverer = _manager.GetWorkgroupDiscoverer(workgroupName);
            TargetsDiscovererEventArgs e = await discoverer.DiscoverAwaitable();

            if (e.Error == null)
            {
                var targets = e.Targets;

                if (parent == null)
                    parent = new DirectoryItem
                    {
                        IsWorkgroup = true,
                        Name = workgroupName,
                        Type = DirectoryItemType.Root,
                        IsSelected = false,
                        SelectionChangedAction = selectAction
                    };

                var result = new[] { parent };

                var results = new DirectoryItem[targets.Length];
                for (var i = 0; i < targets.Length; ++i)
                {
                    var target = targets[i];
                    results[i] = new DirectoryItem
                    {
                        Name = target.Name,
                        Type = DirectoryItemType.Computer,
                        TargetUid = target.Uid,
                        IP = string.IsNullOrEmpty(target.IPAddress) ? "Unknown" : target.IPAddress,
                        Status = target.IsOnline ? "Ready" : "Offline",
                        IsOnline = target.IsOnline,
                        Source = DeploymentItemSource.Workgroup,
                        IsSelected = parent.IsSelected,
                        Parent = parent,
                        SelectionChangedAction = selectAction
                    };
                }

                parent.Items = results;

                return new GetWorkgroupEventArgs(result);  
            }
            else
            {
                return new GetWorkgroupEventArgs(null, e.Error);                
            }
        }        

        public async Task<GetByIpAddressEventArgs> GetByIpAddresses(IEnumerable<IpAddressWrapper> ipAddresses)
        {
            EndpointAddressCollection ipAddressCollection = new EndpointAddressCollection();
            foreach (IpAddressWrapper ipAddress in ipAddresses)
            {
                ipAddressCollection.Add(ipAddress.GetEndpointAddress().BaseEntity);
            }

            IIpAddressDiscoverer discoverer = _manager.GetIpAddressDiscoverer(ipAddressCollection);
            var e = await discoverer.DiscoverAwaitable();

            if (e.Error == null)
            {
                DeploymentTarget[] targets = e.Targets;
                DeploymentItem[] result = new DeploymentItem[targets.Length];
                for (var i = 0; i < targets.Length; ++i)
                {
                    DeploymentTarget target = targets[i];
                    result[i] = new DeploymentItem
                    {
                        Name = target.Name,
                        TargetUid = target.Uid,
                        IP = string.IsNullOrEmpty(target.IPAddress) ? "Unknown" : target.IPAddress,
                        Status = target.IsOnline ? "Ready" : "Offline",
                        IsOnline = target.IsOnline,
                        Source = DeploymentItemSource.Ip
                    };
                }
                return new GetByIpAddressEventArgs(result);                
            }
            else
            {
                return new GetByIpAddressEventArgs(null, e.Error);                
            }
        }

        public async Task<AggregatedDeploymentItemsResolvedEventArgs> ResolveAggregatedDeploymentItems(IList<DeploymentItem> directoryItemsByIP, IList<DeploymentItem> directoryItemsByWorkGroup,
            IList<DeploymentItem> directoryItemsByDomain, string admin, string password)
        {
            DeploymentTargetsCollection deploymentTargetsByIP = ToDeploymentTargetsCollection(directoryItemsByIP);
            DeploymentTargetsCollection deploymentTargetsByWorkGroup = ToDeploymentTargetsCollection(directoryItemsByWorkGroup);
            DeploymentTargetsCollection deploymentTargetsByDomain = ToDeploymentTargetsCollection(directoryItemsByDomain);

            DeploymentTarget[] resultByIP = new DeploymentTarget[] { };
            DeploymentTarget[] resultByWorkGroup = new DeploymentTarget[] { };
            DeploymentTarget[] resultByDomain = new DeploymentTarget[]{ };

            TargetsResolverEventArgs resolvedByIpArgs;
            TargetsResolverEventArgs resolvedByWorkGroup;
            TargetsResolverEventArgs resolvedByDomain;

            Exception error = null;

            if (deploymentTargetsByIP != null && deploymentTargetsByIP.Any())
            {
                IDomainTargetsResolver resolverByIP = _manager.GetDomainTargetsResolver(deploymentTargetsByIP, admin, password);
                resolvedByIpArgs = await resolverByIP.ResolveAsync(null);
                resultByIP = resolvedByIpArgs.Targets?? new DeploymentTarget[] { }; 
                if (resolvedByIpArgs.Error != null)
                {
                    error = resolvedByIpArgs.Error;
                }                
            }
            if (deploymentTargetsByWorkGroup != null && deploymentTargetsByWorkGroup.Any())
            {
                IDomainTargetsResolver resolverByWorkGroup = _manager.GetDomainTargetsResolver(deploymentTargetsByWorkGroup, admin, password);
                resolvedByWorkGroup = await resolverByWorkGroup.ResolveAsync(null);
                resultByWorkGroup = resolvedByWorkGroup.Targets ?? new DeploymentTarget[] { }; 
            }

            if (deploymentTargetsByDomain != null && deploymentTargetsByDomain.Any())
            {
                IDomainTargetsResolver resolverByDomain = _manager.GetDomainTargetsResolver(deploymentTargetsByDomain, admin, password);
                resolvedByDomain = await resolverByDomain.ResolveAsync(null);
                resultByDomain = resolvedByDomain.Targets?? new DeploymentTarget[] { };
            }            

            UpdateUID(resultByIP, resultByWorkGroup, resultByDomain);

            return new AggregatedDeploymentItemsResolvedEventArgs(
                resultByIP.ToArray().MapToDeploymentItems(),
                resultByDomain.ToArray().MapToDeploymentItems(),
                resultByWorkGroup.ToArray().MapToDeploymentItems(),
                error);
        }

        private DeploymentTargetsCollection ToDeploymentTargetsCollection(ICollection<DeploymentItem> deploymentItems)
        {
            DeploymentTargetsCollection collection = new DeploymentTargetsCollection(deploymentItems.Count);
            foreach (DeploymentItem item in deploymentItems)
            {
                if (item is DirectoryItem)
                {
                    DirectoryItem directoryItem = (DirectoryItem)item;
                    if (directoryItem.IsWorkgroup)
                    {
                        WorkgroupTarget deploymentTarget = new WorkgroupTarget();
                        deploymentTarget.IsGroup = FromDirectoryItemType(directoryItem.Type) == DomainEntryType.Root;
                        deploymentTarget.SmbName = item.Name;
                        deploymentTarget.Name = item.Name;
                        collection.Add(deploymentTarget);
                    }
                    else
                    {
                        DomainTarget deploymentTarget = new DomainTarget();
                        deploymentTarget.LdapPath = directoryItem.LdapPath;
                        deploymentTarget.EntryType = FromDirectoryItemType(directoryItem.Type);
                        deploymentTarget.DnsName = directoryItem.DnsName;
                        deploymentTarget.Name = item.Name;
                        deploymentTarget.SystemKind = item.SystemKind;
                        deploymentTarget.Uid = item.TargetUid;
                        deploymentTarget.Source = item.Source.ToBusiness();
                        collection.Add(deploymentTarget);
                    }
                }
                else
                {
                    DeploymentTarget deploymentTarget = new DeploymentTarget();
                    deploymentTarget.Name = item.Name;
                    deploymentTarget.SystemKind = item.SystemKind;
                    deploymentTarget.Source = item.Source.ToBusiness();
                    deploymentTarget.Uid = item.TargetUid;
                    collection.Add(deploymentTarget);
                }
            }

            return collection;
        }

        private DirectoryItemType FromDomainEntryType(DomainEntryType type)
        {
            if (type == DomainEntryType.Root)
            {
                return DirectoryItemType.Root;
            }

            if (type == DomainEntryType.OrganizationUnit)
            {
                return DirectoryItemType.OrgUnit;
            }

            if (type == DomainEntryType.Computer)
            {
                return DirectoryItemType.Computer;
            }

            throw new ArgumentException("type");
        }

        private DomainEntryType FromDirectoryItemType(DirectoryItemType type)
        {
            if (type == DirectoryItemType.Root)
            {
                return DomainEntryType.Root;
            }

            if (type == DirectoryItemType.OrgUnit)
            {
                return DomainEntryType.OrganizationUnit;
            }

            if (type == DirectoryItemType.Computer)
            {
                return DomainEntryType.Computer;
            }

            throw new ArgumentException("type");
        }

        private static void UpdateUID(IEnumerable<DeploymentTarget> resultByIP,
            IEnumerable<DeploymentTarget> resultByWorkGroup, IEnumerable<DeploymentTarget> resultByDomain)
        {
            Dictionary<string, Guid> guids = new Dictionary<string, Guid>();
            var collection = new List<DeploymentTarget>();
            collection.AddRange(resultByIP);
            collection.AddRange(resultByWorkGroup);
            collection.AddRange(resultByDomain);

            foreach (var r in collection)
            {
                if (String.IsNullOrEmpty(r.IPAddress)) continue;
                if (guids.ContainsKey(key: r.IPAddress))
                {
                    r.Uid = guids[r.IPAddress];
                }
                else
                {
                    guids.Add(r.IPAddress, r.Uid);
                }
            }
        }
    }
}