using System;
using System.Collections.Generic;
using APTAT.Application.Models;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Directory.Discoverers;
using APTAT.DeploymentCore.Directory.Resolvers;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using Composition = APTAT.Common.CompositionHelper;
using System.Threading.Tasks;

namespace APTAT.Application.Services
{
    [Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class TargetsDiscoveryService : ITargetsDiscoveryService
    {
        private readonly ITargetsManager _manager;

        public TargetsDiscoveryService(ITargetsManager manager)
        {
            _manager = manager;
        }

        public event EventHandler<DirectoryItemsResolvedEventArgs> DirectoryItemsResolved;
        public event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryItemsCompleted;
        public event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryRootCompleted;
        public event EventHandler<GetWorkgroupEventArgs> GetWorkgroupCompleted;
        public event EventHandler<GetByIpAddressEventArgs> GetByIpAddressesCompleted;
        public void ResolveDirectoryItems(DeploymentItem[] directoryItems, String admin, String password, Object userState)
        {
            DeploymentTargetsCollection deploymentTargets = ToDeploymentTargetsCollection(directoryItems);
            IDomainTargetsResolver resolver = _manager.GetDomainTargetsResolver(deploymentTargets, admin, password);

            resolver.TargetsResolved += OnTargetsResolved;
            resolver.BeginResolve(userState);
        }

        private void OnTargetsResolved(Object sender, TargetsResolverEventArgs e)
        {

            //File.AppendAllText("OnTargetsResolved.txt", String.Format("Error {0}; Targets {1}", e.Error, e.Targets != null ? e.Targets.Length.ToString() : "e.Targets == null"));

            if (e.Error == null)
            {
                DirectoryItemsResolvedEventArgs args = new DirectoryItemsResolvedEventArgs(e.Targets, e.State);
                Action action = () =>
                {
                    if (DirectoryItemsResolved != null)
                    {
                        DirectoryItemsResolved(this, args);
                    }
                };
                App.Current.Dispatcher.BeginInvoke(action);
            }
            else
            {
                DirectoryItemsResolvedEventArgs args = new DirectoryItemsResolvedEventArgs(null, e.State, e.Error);
                Action action = () =>
                {
                    DirectoryItemsResolved?.Invoke(this, args);
                };
                App.Current.Dispatcher.BeginInvoke(action);
            }
        }

        public void GetDirectoryRoot(string domainController, string admin, string password, object userState)
        {
            IDomainDiscoverer d = _manager.GetDomainDiscoverer(null, true, domainController, admin, password);
            d.Discovered += OnRootDiscovered;
            d.BeginDiscover(userState);
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
                     ((DirectoryQueryException)e.Error).Error == DirectoryError.DefaultDomainNotFound)
            {
                DomainTarget domainRoot = (DomainTarget)e.Targets[0];
                return new GetDomainNameResponse(null, null);
            }
            return new GetDomainNameResponse(null, e.Error);
        }

        private void OnRootDiscovered(object sender, TargetsDiscovererEventArgs e)
        {
            if (e.Error == null)
            {
                DomainTarget domainRoot = (DomainTarget)e.Targets[0];
                DirectoryItem root = new DirectoryItem
                {
                    DnsName = domainRoot.DnsName,
                    Name = domainRoot.Name,
                    LdapPath = domainRoot.LdapPath,
                    TargetUid = domainRoot.Uid,
                    Type = FromDomainEntryType(domainRoot.EntryType)
                };
                DirectoryItem[] result = new[] { root };
                GetDirectoryItemsEventArgs args = new GetDirectoryItemsEventArgs(null, result, e.State);
                Action action = () =>
                {
                    if (GetDirectoryRootCompleted != null)
                    {
                        GetDirectoryRootCompleted(this, args);
                    }
                };
                App.Current.Dispatcher.BeginInvoke(action);
            }
            else
            {
                GetDirectoryItemsEventArgs args = new GetDirectoryItemsEventArgs(null, null, e.State, e.Error);
                Action action = () =>
                {
                    if (GetDirectoryRootCompleted != null)
                    {
                        GetDirectoryRootCompleted(this, args);
                    }
                };
                App.Current.Dispatcher.BeginInvoke(action);
            }
        }

        public void GetDirectoryItems(String domainController, String admin, String password, DirectoryItem parent, Object userState)
        {
            GetDirectoryItemsContext context = new GetDirectoryItemsContext
            {
                Parent = parent,
                UserState = userState
            };
            IDomainDiscoverer d = _manager.GetDomainDiscoverer(parent.LdapPath, false, domainController, admin, password);
            d.Discovered += OnDiscovered;
            d.BeginDiscover(context);
        }

        private void OnDiscovered(Object sender, TargetsDiscovererEventArgs e)
        {
            GetDirectoryItemsContext context = (GetDirectoryItemsContext)e.State;
            if (e.Error == null)
            {
                DeploymentTarget[] targets = e.Targets;
                DirectoryItem[] result = new DirectoryItem[targets.Length];
                for (Int32 i = 0; i < targets.Length; ++i)
                {
                    DomainTarget target = (DomainTarget)targets[i];

                    result[i] = new DirectoryItem
                    {
                        DnsName = target.DnsName,
                        Name = target.Name,
                        LdapPath = target.LdapPath,
                        TargetUid = target.Uid,
                        Type = FromDomainEntryType(target.EntryType)
                    };
                }

                GetDirectoryItemsEventArgs args = new GetDirectoryItemsEventArgs(context.Parent, result, context.UserState);
                Action action = () =>
                {
                    if (GetDirectoryItemsCompleted != null)
                    {
                        GetDirectoryItemsCompleted(this, args);
                    }
                };
                App.Current.Dispatcher.BeginInvoke(action);

            }
            else
            {
                GetDirectoryItemsEventArgs args = new GetDirectoryItemsEventArgs(context.Parent, null, context.UserState, e.Error);
                Action action = () =>
                {
                    if (GetDirectoryItemsCompleted != null)
                    {
                        GetDirectoryItemsCompleted(this, args);
                    }
                };
                App.Current.Dispatcher.BeginInvoke(action);
            }
        }
        public void GetWorkgroup(String workgroupName, Object userState)
        {
            IWorkgroupDiscoverer discoverer = _manager.GetWorkgroupDiscoverer(workgroupName);
            discoverer.Discovered += OnWorkgroupDiscovered;
            discoverer.BeginDiscover(userState);
        }

        private void OnWorkgroupDiscovered(Object sender, TargetsDiscovererEventArgs e)
        {
            if (e.Error == null)
            {
                DeploymentTarget[] targets = e.Targets;
                DeploymentItem[] result = new DeploymentItem[targets.Length];
                for (Int32 i = 0; i < targets.Length; ++i)
                {
                    DeploymentTarget target = targets[i];
                    result[i] = new DeploymentItem
                    {
                        Name = target.Name,
                        TargetUid = target.Uid,
                        IPAddress = target.IPAddress ?? "Unknown",
                        Status = target.IsOnline ? "Ready" : "Offline",
                        IsOnline = target.IsOnline
                    };
                }

                GetWorkgroupEventArgs args = new GetWorkgroupEventArgs(result, e.State);
                Action action = () =>
                {
                    if (GetWorkgroupCompleted != null)
                    {
                        GetWorkgroupCompleted(this, args);
                    }
                };
                App.Current.Dispatcher.BeginInvoke(action);

            }
            else
            {
                GetWorkgroupEventArgs args = new GetWorkgroupEventArgs(null, e.State, e.Error);
                Action action = () =>
                {
                    if (GetWorkgroupCompleted != null)
                    {
                        GetWorkgroupCompleted(this, args);
                    }
                };
                App.Current.Dispatcher.BeginInvoke(action);
            }
        }

        public void GetByIpAddresses(IEnumerable<IpAddressWrapper> ipAddresses, Object userState)
        {
            EndpointAddressCollection ipAddressCollection = new EndpointAddressCollection();
            foreach (IpAddressWrapper ipAddress in ipAddresses)
            {
                ipAddressCollection.Add(ipAddress.GetEndpointAddress().BaseEntity);
            }

            IIpAddressDiscoverer discoverer = _manager.GetIpAddressDiscoverer(ipAddressCollection);
            discoverer.Discovered += OnComputersByIpAddressesDiscovered;
            discoverer.BeginDiscover(userState);
        }

        private void OnComputersByIpAddressesDiscovered(Object sender, TargetsDiscovererEventArgs e)
        {
            if (e.Error == null)
            {
                DeploymentTarget[] targets = e.Targets;
                DeploymentItem[] result = new DeploymentItem[targets.Length];
                for (Int32 i = 0; i < targets.Length; ++i)
                {
                    DeploymentTarget target = targets[i];
                    result[i] = new DeploymentItem
                    {
                        Name = target.Name,
                        TargetUid = target.Uid,
                        IPAddress = target.IPAddress == null ? "Unknown" : target.IPAddress,
                        Status = target.IsOnline ? "Ready" : "Offline",
                        IsOnline = target.IsOnline
                    };
                }

                GetByIpAddressEventArgs args = new GetByIpAddressEventArgs(result, e.State);
                Action action = () =>
                {
                    if (GetByIpAddressesCompleted != null)
                    {
                        GetByIpAddressesCompleted(this, args);
                    }
                };
                App.Current.Dispatcher.BeginInvoke(action);
            }
            else
            {
                GetByIpAddressEventArgs args = new GetByIpAddressEventArgs(null, e.State, e.Error);
                Action action = () =>
                {
                    if (GetByIpAddressesCompleted != null)
                    {
                        GetByIpAddressesCompleted(this, args);
                    }
                };
                App.Current.Dispatcher.BeginInvoke(action);
            }
        }
        private DeploymentTargetsCollection ToDeploymentTargetsCollection(DeploymentItem[] deploymentItems)
        {
            DeploymentTargetsCollection collection = new DeploymentTargetsCollection(deploymentItems.Length);
            foreach (DeploymentItem item in deploymentItems)
            {
                if (item is DirectoryItem)
                {
                    DirectoryItem directoryItem = (DirectoryItem)item;
                    DomainTarget deploymentTarget = new DomainTarget();
                    deploymentTarget.LdapPath = directoryItem.LdapPath;
                    deploymentTarget.EntryType = FromDirectoryItemType(directoryItem.Type);
                    deploymentTarget.DnsName = directoryItem.DnsName;
                    deploymentTarget.Name = item.Name;
                    deploymentTarget.Uid = item.TargetUid;
                    collection.Add(deploymentTarget);
                }
                else
                {
                    DeploymentTarget deploymentTarget = new DeploymentTarget();
                    deploymentTarget.Name = item.Name;
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
    }

    public class GetDirectoryItemsContext
    {
        public DirectoryItem Parent { get; set; }

        public Object UserState { get; set; }
    }
}
