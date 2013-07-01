using System.Diagnostics;
using System.IO;
using System.Windows.Threading;
using APTAT.Application.Models;
using APTAT.Core;
using APTAT.Core.Descent;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Reports;

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

        public event EventHandler<ScanProgressChangedEventArgs> ScanProgressChanged;

        public event EventHandler<ScanStatusChangedEventArgs> ScanStatusChanged;

        public event EventHandler<GetScanResultCompletedEventArgs> GetScanResultCompleted;

        public event EventHandler<ScanCompletedEventArgs> ScanCompleted;

        public event EventHandler<BuildReportCompletedEventArgs> BuildReportCompleted;



        private readonly AptAtManager _manager;

        private readonly Guid _sessionGuid = new Guid("C78DA9FA-4DC3-4F26-B7C3-304D24220B15");

        public Guid SessionId
        {
            get { return _sessionGuid; }
        }

        private static IAptService _instance;

        public static IAptService Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new AptService();
                }

                return _instance;
            }
        }

        private AptService()
        {
           _manager = new AptAtManager(this);
        }

        public void BuildReport(Guid operationGuid, ReportKind reportKind, Object userState)
        {
            IAptReportBuilder reportBuilder =  _manager.GetAptReportBuilder();
            reportBuilder.ReportCompleted += OnReportCompleted;
            reportBuilder.BeginBuildReport(operationGuid, ToReportType(reportKind), userState);
        }

        private void OnReportCompleted(Object sender, ReportCompletedEventArgs e)
        {
            BuildReportCompletedEventArgs args = new BuildReportCompletedEventArgs(e.FileName, e.Error, e.State);

            Action action = () =>
            {
                if (BuildReportCompleted != null)
                {
                    BuildReportCompleted(this, args);
                }
            };
            DispatcherHelper.Current.BeginInvoke(action);

           
        }

        public void BeginScan(ThreatInfoItem[] items, String domain, String login, String password, Object userState)
        {
            Debug.Assert(password != null);
            
            DeploymentTarget[] targets = items.Select(item => item.Target).OfType<DeploymentTarget>().ToArray();
            IAptScanner scanner = _manager.GetAptScanner(new DeploymentTargetsCollection(targets), login, password, domain);
            scanner.ScanStarted += OnScanStarted;
            scanner.ScanCompleted += OnScanCompleted;
            scanner.ScanInProgress += OnScanInProgress;
            scanner.BeginScan(userState);
        }

        private void OnScanInProgress(Object sender, ScanInProgressEventArgs e)
        {
            ScanProgressChangedEventArgs args = new ScanProgressChangedEventArgs(e.Percentage, e.OperationUid);
            Action action = () =>
            {
                if (ScanProgressChanged != null)
                {
                    ScanProgressChanged(this, args);
                }
            };
            DispatcherHelper.Current.BeginInvoke(action);   
        }

        private void OnScanStarted(Object sender, ScanStartedEventArgs e)
        {
            BeginScanCompletedEventArgs args = new BeginScanCompletedEventArgs(e.OperationUid, e.State, e.Error);

            if (BeginScanCompleted != null)
            {
                BeginScanCompleted(this, args);

            }
     
        }

        private void OnScanCompleted(Object sender, Core.ScanCompletedEventArgs e)
        {
            ScanCompletedEventArgs args = new ScanCompletedEventArgs();
            args.SuccededCount = e.SucceededCount;
            args.CanceledCount = e.CalceledCount;
            args.FailedCount = e.FailedCount;
            args.NotPerformedCount = e.NotPerformedCount;

            Action action = () =>
            {
                if (ScanCompleted != null)
                {
                    ScanCompleted(this, args);
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
                        IpAddress = deploymentTarget.IPAddress,
                        IsRootItem = true,
                        IsOnline =  deploymentTarget.Online,
                        ComputerGuid = deploymentTarget.TargetUid
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


        public void GetScanResult(Guid targetUid, Object userState)
        {
            ScanResult scanResult = _manager.GetScanResultByTargetUid(targetUid);
            
            ScanResultItem resultItem = new ScanResultItem
            {
                TargetUid = scanResult.TargetUid,
                TotalFilesCount = scanResult.TotalFilesCount,
                UnknownFilesCount = scanResult.UnknownFilesCount
            };


            BadFileItem[] badFileItems = null;
            if (scanResult.AptReport != null)
            {
                if (scanResult.AptReport.Modules != null)
                {
                    badFileItems = scanResult.BadFiles
                        .Select(m => new BadFileItem
                                         {
                                             Name = m.Path,
                                             Size = m.Size,
                                             Accessed = m.Accessed,
                                             Created = m.Created,
                                             Modified = m.Modified,
                                             Product = m.Product,
                                             Copyright = m.Copyright,
                                             Version = m.Version,
                                             Description = m.Description,
                                             Signer = m.Signer,
                                             Attributes = m.Attributes,
                                             Sha1 = m.Sha1,
                                             IsMalware = m.Cloud == CloudVerdict.Malware,
                                             IsUnknown = m.Cloud == CloudVerdict.Unknown
                                         }
                        ).ToArray();
                        
                    
                }
            }

            resultItem.BadFiles = badFileItems;
            

            if(GetScanResultCompleted != null)
            {
                GetScanResultCompleted(this, new GetScanResultCompletedEventArgs(resultItem, userState));
            }
        }

        public void Changed(params ChangesTrackingItem[] arg)
        {
            Action<ChangesTrackingItem[]> action = items =>
            {
                ScanProgressItem[] scanProgressItems = new ScanProgressItem[items.Length];
                for (Int32 i = 0; i < items.Length; ++i)
                {
                    ChangesTrackingItem changesTrackingItem = items[i];
                    DescentProgressItem descentProgressItem = changesTrackingItem.ChangedItem as DescentProgressItem;
                    if (descentProgressItem != null)
                    {
                        ScanProgressItem progressItem = new ScanProgressItem();
                        progressItem.ComputerGuid = descentProgressItem.ComputerUid;
                        progressItem.StatusMessage = descentProgressItem.StatusMessage.ToString();
                        progressItem.TotalFilesCount = descentProgressItem.TotalFilesCount;
                        progressItem.UnknownFilesCount = descentProgressItem.UnknownFilesCount;
                        progressItem.MalwareFilesCount = descentProgressItem.MalwaresCount;
                        progressItem.OperationUid = descentProgressItem.OperationUid;
                        progressItem.ScanStage = FromAptScanStage(descentProgressItem.ScanStage);
                        scanProgressItems[i] = progressItem;
                    }
                }

                if (ScanStatusChanged != null)
                {
                    ScanStatusChanged(this, new ScanStatusChangedEventArgs(scanProgressItems, null));
                }
            };

            DispatcherHelper.Current.BeginInvoke(DispatcherPriority.Send, action, arg);
        }

        public void StopScan()
        {
            _manager.StopScan();
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

        private ScanStage FromAptScanStage(AptScanStage stage)
        {
            switch (stage)
            {
                case AptScanStage.Pending:
                    return ScanStage.Pending;

                case AptScanStage.Connecting:
                    return ScanStage.Connecting;

                case AptScanStage.CopyingScanner:
                    return ScanStage.CopyingScanner;

                case AptScanStage.ConfiguringScanner:
                    return ScanStage.ConfiguringScanner;

                case AptScanStage.StartingScanner:
                    return ScanStage.StartingScanner;

                case AptScanStage.ScanInProgress:
                    return ScanStage.ScanInProgress;

                case AptScanStage.ScanCompleted:
                    return ScanStage.ScanCompleted;

                case AptScanStage.ScanFailed:
                    return ScanStage.ScanFailed;

                case AptScanStage.ScanResultsObtaning:
                    return ScanStage.ScanResultsObtaning;

                case AptScanStage.ScanResultsProcessing:
                    return ScanStage.ScanResultsProcessing;

                case AptScanStage.OperationCompleted:
                    return ScanStage.OperationCompleted;

                case AptScanStage.OperationFailed:
                    return ScanStage.OperationFailed;

                case AptScanStage.OperationCanceled:
                    return ScanStage.OperationCanceled;

                default:
                    return ScanStage.Unknown;
                    //  throw new ArgumentOutOfRangeException("stage");
            }
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

        private ReportType ToReportType(ReportKind kind)
        {
            if(kind == ReportKind.Executive)
            {
                return ReportType.Executive;
            }

            if(kind == ReportKind.DetailedPerProgram)
            {
                return ReportType.DetailedPerProgram;
            }

            if (kind == ReportKind.DetailedPerDevice)
            {
                return ReportType.DetailedPerDevice;
            }

            throw new ArgumentException("kind");
        }
    }
}
