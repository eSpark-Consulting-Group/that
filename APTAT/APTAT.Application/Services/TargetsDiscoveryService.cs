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
using System.Security.Principal;
using System.Linq;
using System.Net;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore;

namespace APTAT.Application.Services
{
    [Composition.CreationPolicy(Composition.CreationPolicy.Singleton)]
    public class TargetsDiscoveryService : ITargetsDiscoveryService
    {
        private readonly ITargetsManager _manager;
        private readonly INetworkObserver _networkHelper;

        public TargetsDiscoveryService(ITargetsManager manager, INetworkObserver networkHelper)
        {
            _manager = manager;
            _networkHelper = networkHelper;            
        }

        public event EventHandler<DirectoryItemsResolvedEventArgs> DirectoryItemsResolved;
        public event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryItemsCompleted;
        public event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryRootCompleted;
        public event EventHandler<GetDirectoryItemsEventArgs> GetWorkgroupCompleted;
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
                SetLocalHostItem(e.Targets);
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

        public string GetLoggedInUser()
        {
            if (!String.IsNullOrEmpty(Environment.UserDomainName))
            {
                return Environment.UserDomainName + "\\" + Environment.UserName;
            }
            return Environment.UserName;
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

        private void SetLocalHostItem(DeploymentTarget[] targets )
        {
            IPHostEntry hostInfo = _networkHelper.GetHostEntry();
                            
            var target = targets.FirstOrDefault(t => hostInfo.AddressList.Contains(t.GetIPAddress()));
            if (target != null)
            {
                target.IsLocalComputer = true;
            }
            else
            {
                var targetByName = targets.FirstOrDefault(t => t.DnsName == hostInfo.HostName);
                if (targetByName != null)
                {
                    targetByName.IsLocalComputer = true;
                }
            }
        }

        private void OnDiscovered(Object sender, TargetsDiscovererEventArgs e)
        {
            GetDirectoryItemsContext context = (GetDirectoryItemsContext)e.State;
            if (e.Error == null)
            {
                DeploymentTarget[] targets = e.Targets;
                var filtered = targets.Where(t => ((DomainTarget)t).EntryType == DomainEntryType.Computer).ToArray();
                if (filtered.Any())
                {
                    SetLocalHostItem(filtered);
                }

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
        public void GetWorkgroup(String workgroupName, object userState)
        {
            IWorkgroupDiscoverer discoverer = _manager.GetWorkgroupDiscoverer(workgroupName);
            discoverer.Discovered += OnWorkgroupDiscovered;
            discoverer.BeginDiscover(userState);
        }

        public async Task<GetDirectoryItemsEventArgs> GetWorkgroups(object userState)
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
                        Items = new System.Collections.ObjectModel.ObservableCollection<DirectoryItem>()
                    };
                }

                var t = result.ToArray();
                return new GetDirectoryItemsEventArgs(null, t, userState);
            }
            catch (Exception ex)
            {
                return new GetDirectoryItemsEventArgs(null, null, ex);
            }
        }
        private void OnWorkgroupDiscovered(Object sender, TargetsDiscovererEventArgs e)
        {
            if (e.Error == null)
            {
                DeploymentTarget[] targets = e.Targets;
                DirectoryItem[] result = new DirectoryItem[targets.Length];
                for (Int32 i = 0; i < targets.Length; ++i)
                {
                    DeploymentTarget target = targets[i];
                    result[i] = new DirectoryItem
                    {
                        Name = target.Name,
                        TargetUid = target.Uid,
                        IPAddress = target.IPAddress ?? "Unknown",
                        Status = target.IsOnline ? "Ready" : "Offline",
                        IsOnline = target.IsOnline
                    };
                }

                GetDirectoryItemsEventArgs args = new GetDirectoryItemsEventArgs(null, result, e.State);
                Action action = () =>
                {
                    GetWorkgroupCompleted?.Invoke(this, args);
                };
                App.Current.Dispatcher.BeginInvoke(action);

            }
            else
            {
                GetDirectoryItemsEventArgs args = new GetDirectoryItemsEventArgs(null, null, e.Error);
                Action action = () =>
                {
                    GetWorkgroupCompleted?.Invoke(this, args);
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
                SetLocalHostItem(targets);

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

        public bool IsLocalAddress(IpAddressWrapper ip)
        {   
            if (ip.IsRange || ip.IsMask) return false;

            IPHostEntry hostInfo = _networkHelper.GetHostEntry();
            if (ip.IsDnsName)
            {                
                var host = Dns.GetHostEntry(ip.FormattedText);
                return host.HostName == hostInfo.HostName;      
            }
            else
            {
                IPAddress address = IPAddress.Parse(ip.FormattedText);
                return hostInfo.AddressList.Contains(address);    
            }
        }
    }

    public class GetDirectoryItemsContext
    {
        public DirectoryItem Parent { get; set; }

        public Object UserState { get; set; }
    }
}
