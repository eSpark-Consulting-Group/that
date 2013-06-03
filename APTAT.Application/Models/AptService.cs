using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using APTAT.Core.Facade.Descent.Commands;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.Core.Facade.Descent.MessageContracts;

namespace APTAT.Application.Models
{
    public class GetDirectoryItemRequest
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

        public IServiceCommand<FindTargetsResponse> Command { get; set; }
    }

    public class AptService : IAptService
    {
        private readonly CommandFactory _commandFactory;
        private static DirectoryItem _rootItem;
        private static Int64 _identity;
        private static Random rand = new Random(DateTime.Now.Millisecond);

        public event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryItemsCompleted;

        public event EventHandler<GetDirectoryItemsEventArgs> GetDirectoryRootCompleted;

        private DirectoryItemType FromDomainEntryType(DomainEntryType type)
        {
            if(type == DomainEntryType.Root)
            {
                return DirectoryItemType.Root;
            }
            
            if(type == DomainEntryType.OrganizationUnit)
            {
                return DirectoryItemType.OrgUnit;
            }

            if(type == DomainEntryType.Computer)
            {
                return DirectoryItemType.Computer;
            }

            throw new ArgumentException("type");
        }

        static void BuildTreeRecursive(DirectoryItem parent, Int32 level)
        {
            if(level == 2)
            {
                return;
            }

            if (parent.Items == null)
            {
                parent.Items = new ObservableCollection<DirectoryItem>();
            }

            Int32 random = rand.Next();
            if(random % 2 == 0 && parent != _rootItem)
            {
                return;
            }

            random = 100 + rand.Next() % 100;
            for (Int32 i = 0; i < random; ++i)
            {
                DirectoryItem directoryItem = new DirectoryItem
                {
                    Id = ++_identity,
                   // TargetId = ++_identity
                };

                if(rand.Next() % 2 == 0)
                {
                    directoryItem.Type = DirectoryItemType.Computer;
                   // directoryItem.Name = "Computer " + directoryItem.TargetId;
                    parent.Items.Add(directoryItem);
                }
                else
                {
                    directoryItem.Type = DirectoryItemType.OrgUnit;
                   // directoryItem.Name = "OrgUnit " + directoryItem.TargetId;
                    parent.Items.Add(directoryItem);
                    BuildTreeRecursive(directoryItem, level + 1);
                }

              
            }
        }

        static DirectoryItem[] Children(DirectoryItem directoryItem, Int64 directoryItemId)
        {
            if(directoryItem.Id == directoryItemId)
            {
                if(directoryItem.Items == null)
                {
                    return new DirectoryItem[0];
                }
                return directoryItem.Items.ToArray();
            }

            if(directoryItem.Items != null)
            {
                foreach (DirectoryItem item in directoryItem.Items)
                {
                    DirectoryItem[] children = Children(item, directoryItemId);
                    if (children != null)
                    {
                        return children;
                    }
                }
            }
         
            return null;
        }

        static AptService()
        {
            _rootItem = new DirectoryItem();
            _rootItem.Name = "Root";
            _rootItem.Type = DirectoryItemType.Root;
            _rootItem.Id = ++_identity;


            BuildTreeRecursive(_rootItem, 0);
        }
        
  
        public AptService()
        {
           _commandFactory  = new CommandFactory();
        }

        private void FindDomainTargets(String domainController, String login, String password, String path, Boolean fromRoot, DirectoryItem parent, Object userState)
        {
            FindTargetsRequest findTargetsRequest = new FindTargetsRequest();
            
            var searchCriterion = new DomainSearchCriterion();
            if (fromRoot)
            {
                searchCriterion.FromRoot = true;
            }
            else
            {
                if (String.IsNullOrEmpty(path))
                    throw new ArgumentException("Path is not specified.");
            }

            searchCriterion.Login = login;
            searchCriterion.Domain = domainController;
            searchCriterion.Password = password;
            searchCriterion.ParentAddress = path;

            findTargetsRequest.SearchCriterion = searchCriterion;
            IServiceCommand<FindTargetsResponse> command = _commandFactory.CreateCommand<FindTargetsResponse>(findTargetsRequest);

            Action action = command.Execute;
            GetDomainTargetsContext context = new GetDomainTargetsContext { Action = action, Command = command, FromRoot = fromRoot, Parent = parent, UserState = userState };
            action.BeginInvoke(GetTargetsCallback, context);
            
        }

        private void GetTargetsCallback(IAsyncResult ar)
        {

            GetDomainTargetsContext context = ar.AsyncState as GetDomainTargetsContext;
            if (context != null)
            {
                try
                {
                    context.Action.EndInvoke(ar);
                    FindTargetsResponse response = context.Command.Response;

                    if(context.FromRoot)
                    {
                        DomainTarget domainRoot = (DomainTarget) response.Targets[0];
                        DirectoryItem root = new DirectoryItem
                        {
                            Name = domainRoot.Name,
                            LdapPath = domainRoot.LdapPath,
                            TargetUid = domainRoot.TargetUid,
                            Type = FromDomainEntryType(domainRoot.EntryType)
                        };
                        DirectoryItem[] result = new[] { root };
                        if (GetDirectoryRootCompleted != null)
                        {
                            Action action = () => GetDirectoryRootCompleted(this, new GetDirectoryItemsEventArgs(null, result, context.UserState));
                            DispatcherHelper.Current.BeginInvoke(DispatcherPriority.Normal, action);
                        }  
                    }
                    else
                    {
                        DeploymentTargetsCollection targets = response.Targets;

                        DirectoryItem[] result = new DirectoryItem[targets.Count];
                        for (Int32 i = 0; i < targets.Count; ++i)
                        {
                            DomainTarget target = (DomainTarget) targets[i];

                            result[i] = new DirectoryItem
                            {
                                Name = target.Name,
                                LdapPath = target.LdapPath,
                                TargetUid = target.TargetUid,
                                Type = FromDomainEntryType(target.EntryType)
                            };
                        }

                        GetDirectoryItemsEventArgs args = new GetDirectoryItemsEventArgs(context.Parent, result, context.UserState);
                        if (GetDirectoryItemsCompleted != null)
                        {
                            Action action = () => GetDirectoryItemsCompleted(this, args);
                            DispatcherHelper.Current.BeginInvoke(DispatcherPriority.Normal, action);
                        }

                    }

                }
                catch (Exception ex)
                {

                    if (context.FromRoot)
                    {
                        if (GetDirectoryRootCompleted != null)
                        {
                            Action action = () => GetDirectoryRootCompleted(this, new GetDirectoryItemsEventArgs(null, null, context.UserState, ex.Message));
                            DispatcherHelper.Current.BeginInvoke(DispatcherPriority.Normal, action);
                        }
                    }
                    else
                    {
                      
                    }
                }
            }

        }



        public void GetDirectoryRoot(String domainController, String admin, String password, Object userState)
        {

            FindDomainTargets(domainController, admin, password, null, true, null, userState);
            //BackgroundWorker worker = new BackgroundWorker();
            // worker.DoWork += OnDoWork;
            //worker.RunWorkerCompleted += OnGetDirectoryRootCompleted;
            //worker.RunWorkerAsync(new GetDirectoryRootRequest { Name = domainController, UserState = userState });


            // FindDomainTargets()
        }

        //private void OnGetDirectoryRootCompleted(Object sender, RunWorkerCompletedEventArgs e)
        //{
        //    GetDirectoryRootRequest request = (GetDirectoryRootRequest)e.Result;
        //    DirectoryItem[] result = null;

        //    if(request.Name == _rootItem.Name)
        //    {
        //        DirectoryItem root = new DirectoryItem
        //        {
        //            Id = _rootItem.Id,
        //            Name = _rootItem.Name,
        //            TargetId = _rootItem.TargetId,
        //            Type = _rootItem.Type
        //        };
        //        result = new[] { root };

        //        if (GetDirectoryRootCompleted != null)
        //        {
        //            GetDirectoryRootCompleted(this, new GetDirectoryItemsEventArgs(null, result, request.UserState));
        //        }
        //    }
        //    else
        //    {
        //        if (GetDirectoryRootCompleted != null)
        //        {
        //            GetDirectoryRootCompleted(this, new GetDirectoryItemsEventArgs(null, null, request.UserState, "Domain Not Found"));
        //        }
        //    }           
        //}

        public void GetDirectoryItems(String domainController, String admin, String password, DirectoryItem parent, Object userState)
        {
            FindDomainTargets(domainController, admin, password, parent.LdapPath, false, parent, userState);
        //    BackgroundWorker worker = new BackgroundWorker();
        //    worker.DoWork += OnDoWork;
        //    worker.RunWorkerCompleted += OnGetDirectoryItemsCompleted;
        //    worker.RunWorkerAsync(new GetDirectoryItemRequest { Parent = parent, UserState = userState});
        }

        //private void OnGetDirectoryItemsCompleted(Object sender, RunWorkerCompletedEventArgs e)
        //{
        //    GetDirectoryItemRequest request = (GetDirectoryItemRequest)e.Result;

        //    DirectoryItem[] result = Children(_rootItem, request.Parent.Id);
        //    DirectoryItem[] realResult = new DirectoryItem[result.Length];
        //    for(Int32 i = 0; i < result.Length; ++i)
        //    {
        //        realResult[i] = new DirectoryItem
        //                            {
        //                                Id = result[i].Id,
        //                                Name = result[i].Name,
        //                                TargetId = result[i].TargetId,
        //                                Type = result[i].Type
        //                            };
        //    }


        //    GetDirectoryItemsEventArgs args = new GetDirectoryItemsEventArgs(request.Parent, realResult, request.UserState);

        //    if(GetDirectoryItemsCompleted != null)
        //    {
        //        GetDirectoryItemsCompleted(this, args);
        //    }

        //}

        private void OnDoWork(Object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
            e.Result = e.Argument;
        }


        
    }
}
