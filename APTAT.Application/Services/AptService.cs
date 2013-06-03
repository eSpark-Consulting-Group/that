using APTAT.Application.Models;
using APTAT.Core;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using System;
using System.Collections.Generic;
using System.Linq;

namespace APTAT.Application.Services
{
    public class GetDirectoryItemsContext
    {
        public DirectoryItem Parent { get; set; }

        public Object UserState { get; set; }
    }

    public class GetDirectoryRootRequest
    {
        public String Name { get; set; }

        public Object UserState { get; set; }
    }

    public class GetDomainTargetsContext
    {
        public Action Action { get; set; }

        public Object UserState { get; set; }

        public Boolean FromRoot { get; set; }

        public DirectoryItem Parent { get; set; }
    }

    public class AptService : IAptService, IChangeTrackingListener
    {
        public event EventHandler<DirectoryItemsResolvedEventArgs> DirectoryItemsResolved;
        
        public event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryItemsCompleted;

        public event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryRootCompleted;

        public event EventHandler<BeginScanCompletedEventArgs> BeginScanCompleted;

        private readonly AptAtManager _manager;

        private readonly Guid _sessionGuid = new Guid("C78DA9FA-4DC3-4F26-B7C3-304D24220B15");

        public Guid SessionId
        {
            get { return _sessionGuid; }
        }
  
        public AptService()
        {
           _manager = new AptAtManager(this);
        }

        public void BeginScan(ThreatInfoItem[] items, String login, String password, Object userState)
        {
            DomainTarget[] targets = items.Select(item => item.Target).OfType<DomainTarget>().ToArray();
            IAptScanner scanner = _manager.GetAptScanner(new DeploymentTargetsCollection(targets), login, password);
            scanner.ScanStarted += OnScanStarted;
            scanner.BeginScan(userState);
        }

        private void OnScanStarted(Object sender, ScanStartedEventArgs e)
        {
            BeginScanCompletedEventArgs args = new BeginScanCompletedEventArgs(e.State, e.Error);
            Action action = () =>
            {
                if (BeginScanCompleted != null)
                {
                    BeginScanCompleted(this, args);
                }
            };
            DispatcherHelper.Current.BeginInvoke(action);
        }

        public void ResolveDirectoryItems(DirectoryItem[] directoryItems, Object userState)
        {
            DeploymentTargetsCollection deploymentTargets = ToDeploymentTargetsCollection(directoryItems);
            IDomainTargetsResolver resolver = _manager.GetDomainTargetsResolver(deploymentTargets);
            resolver.TargetsResolved += OnTargetsResolved;  
            resolver.BeginResolve(userState);
        }

        private void OnTargetsResolved(Object sender, DomainTargetsResolverEventArgs e)
        {
            if(e.Error == null)
            {
                List<ThreatInfoItem> result = new List<ThreatInfoItem>(e.Targets.Length);
                foreach (DeploymentTarget deploymentTarget in e.Targets)
                {
                    ThreatInfoItem infoItem = new ThreatInfoItem
                    {
                        Target = deploymentTarget,
                        Name = deploymentTarget.Name,
                        IsRootItem = true,
                        IsOnline =  deploymentTarget.Online
                    };

                    result.Add(infoItem);
                }

                DirectoryItemsResolvedEventArgs args = new DirectoryItemsResolvedEventArgs(result.ToArray(), e.State);
                Action action = () =>
                {
                    if (DirectoryItemsResolved != null)
                    {
                        DirectoryItemsResolved(this, args);
                    }
                };
                DispatcherHelper.Current.BeginInvoke(action);
            }
            else
            {
                DirectoryItemsResolvedEventArgs args = new DirectoryItemsResolvedEventArgs(null, e.State, e.Error);
                Action action = () =>
                {
                    if (DirectoryItemsResolved != null)
                    {
                        DirectoryItemsResolved(this, args);
                    }
                };
                DispatcherHelper.Current.BeginInvoke(action);
            }
        }

        public void GetDirectoryRoot(String domainController, String admin, String password, Object userState)
        {
            IDomainDiscoverer d = _manager.GetDomainDiscoverer(null, true, domainController, admin, password);
            d.Discovered += OnRootDiscovered;
            d.BeginDiscover(userState);
        }

        private void OnRootDiscovered(Object sender, DomainDiscovererEventArgs e)
        {
            if(e.Error == null)
            {
                DomainTarget domainRoot = (DomainTarget) e.Targets[0];
                DirectoryItem root = new DirectoryItem
                {
                    DnsName = domainRoot.DnsName,
                    Name = domainRoot.Name,
                    LdapPath = domainRoot.LdapPath,
                    TargetUid = domainRoot.TargetUid,
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
                DispatcherHelper.Current.BeginInvoke(action);
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
                DispatcherHelper.Current.BeginInvoke(action);
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

        private void OnDiscovered(Object sender, DomainDiscovererEventArgs e)
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
                        TargetUid = target.TargetUid,
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
                DispatcherHelper.Current.BeginInvoke(action);
                
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
                DispatcherHelper.Current.BeginInvoke(action);   
            }
        }

     
        public void Changed(params ChangesTrackingItem[] items)
        {
           // throw new NotImplementedException();
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

        private DeploymentTargetsCollection ToDeploymentTargetsCollection(DirectoryItem[] directoryItems)
        {
            DeploymentTargetsCollection collection = new DeploymentTargetsCollection(directoryItems.Length);
            foreach (DirectoryItem item in directoryItems)
            {
                DomainTarget deploymentTarget = new DomainTarget();
                deploymentTarget.LdapPath = item.LdapPath;
                deploymentTarget.EntryType = FromDirectoryItemType(item.Type);
                deploymentTarget.DnsName = item.DnsName;
                deploymentTarget.Name = item.Name;
                deploymentTarget.TargetUid = item.TargetUid;
                collection.Add(deploymentTarget);
            }

            return collection;
        }
    }
}
