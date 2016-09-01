
using DDT.Application.Messages;
using System.Linq;
using Telerik.Windows.Controls;
using DDT.Application.Models;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using DDT.Application.Transitions;

namespace DDT.Application.ViewModels
{
    public partial class MainViewModel
    {
        partial void InitializeWorkgroup()
        {
            DiscoverWorkgroupCommand = new DelegateCommand(DiscoverWorkgroup, CanDiscoverWorkgroup);
            ExpandWorkgroupCommand = new DelegateCommand(ExpandWorkgroupTree, CanExpandWorkgroupTree);
            ClearWorkgroupCommand = new DelegateCommand(ClearWorkgroup);

            WorkgroupItems = new DirectoryItem[0];
        }

        public DelegateCommand DiscoverWorkgroupCommand { get; private set; }
        public DelegateCommand ExpandWorkgroupCommand { get; private set; }
        public DelegateCommand ClearWorkgroupCommand { get; private set; }

        private Collection<Action> _workgroupMementoActions = new Collection<Action>();
        private DirectoryItem[] _workgroupItems;
        public DirectoryItem[] WorkgroupItems { get; private set; }

        public string WorkgroupHeader
        {
            get
            {
                if (!HasWorkgroup)
                    return "Discover computers by selecting them from Workgroup";

                return string.Join(", ", GetSelectedWorkgroupItems());
            }
        }

        private List<DeploymentItem> GetSelectedWorkgroupItems()
        {
            var result = new List<DeploymentItem>();
            foreach (var item in WorkgroupItems)
                result.AddRange(item.GetSelectedItems());
            return result;
        }

        public bool HasWorkgroup
        {
            get { return WorkgroupItems.Any(_ => _.IsSelected.GetValueOrDefault(true)); }
        }

        private string _workgroupNameMemento;
        private string _workgroupName;
        public string WorkgroupName
        {
            get { return EnumerateWorkgroups ? null : _workgroupName; }
            set
            {
                _workgroupName = value;
                RaisePropertyChanged(nameof(WorkgroupName));

                ClearWorkgroup(null);
            }
        }

        private bool _enumerateWorkgroupsMemento;
        private bool _enumerateWorkgroups = true;
        public bool EnumerateWorkgroups
        {
            get { return _enumerateWorkgroups; }
            set
            {
                _enumerateWorkgroups = value;
                RaisePropertyChanged(nameof(EnumerateWorkgroups));
                RaisePropertyChanged(nameof(WorkgroupName));

                ClearWorkgroup(null);
            }
        }

        public string WorkgroupMask
        {
            get { return "Enter Workgroup Name"; }
        }

        private string _keywordWorkgroup;
        public string KeywordWorkgroup
        {
            get { return _keywordWorkgroup; }
            set
            {
                _keywordWorkgroup = value;
                RaisePropertyChanged(nameof(KeywordWorkgroup));
            }
        }

        public bool SearchWorkgroup(object o)
        {
            if (o is DirectoryItem)
            {
                var dir = (DirectoryItem)o;
                switch (dir.Type)
                {
                    case DirectoryItemType.Root:
                    case DirectoryItemType.Computer:
                        return dir.Name.ToLower().Contains(KeywordWorkgroup.ToLower());
                }
            }
            return false;
        }

        private void DiscoverWorkgroup(object o)
        {
            ButtonsAvailable = false;

            AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.Workgroup, async _ =>
            {
                SetCurrentScreen(_);

                if (!HasWorkgroup)
                {
                    if (EnumerateWorkgroups)
                        await TryAction(GetWorkgroups());
                    else
                        await TryAction(GetWorkgroup());
                }
            });
        }

        private bool CanDiscoverWorkgroup(object o)
        {
            return EnumerateWorkgroups || !string.IsNullOrEmpty(WorkgroupName);
        }

        private async Task GetWorkgroup()
        {
            KeywordWorkgroup = null;
            ProgressMessage = "Discovering Workgroup...";
            Progress = 100;

            WorkgroupItems = new DirectoryItem[0];
            RaisePropertyChanged(nameof(WorkgroupItems));

            var e = await _discoveryService.GetWorkgroup(WorkgroupName, null, () =>
            {
                RaisePropertyChanged(nameof(HasWorkgroup));

                ChangeScreenCommand.InvalidateCanExecute();
            });

            if (e.Exception == null)
            {
                WorkgroupItems = e.Items;
                RaisePropertyChanged(nameof(WorkgroupItems));
            }

            if (WorkgroupItems.Any())
                Progress = 0;
            else
                AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.WorkgroupCredentials, _ =>
                {
                    SetCurrentScreen(_, () =>
                    {
                        Progress = 0;
                        ErrorMessage = "No workgroups found. Please try another name.";
                    });
                }, Direction.RightToLeft);
        }

        private async Task GetWorkgroups()
        {
            KeywordWorkgroup = null;
            ProgressMessage = "Discovering Workgroup...";
            Progress = 100;

            WorkgroupItems = new DirectoryItem[0];
            RaisePropertyChanged(nameof(WorkgroupItems));

            var e = await _discoveryService.GetWorkgroups(() =>
            {
                RaisePropertyChanged(nameof(HasWorkgroup));

                ChangeScreenCommand.InvalidateCanExecute();
            });

            if (e.Exception == null)
            {
                WorkgroupItems = e.Items;
                RaisePropertyChanged(nameof(WorkgroupItems));
            }

            if (WorkgroupItems.Any())
                Progress = 0;
            else
                AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.WorkgroupCredentials, _ =>
                {
                    SetCurrentScreen(_, () =>
                    {
                        Progress = 0;
                        ErrorMessage = "No workgroups found. Please try another name.";
                    });
                }, Direction.RightToLeft);
        }

        private async void ExpandWorkgroupTree(object o)
        {
            await TryAction(ExpandWorkgroupTree((DirectoryItem)o));
        }

        private bool CanExpandWorkgroupTree(object o)
        {
            return string.IsNullOrEmpty(KeywordWorkgroup) || KeywordWorkgroup.Length < 3;
        }

        private async Task ExpandWorkgroupTree(DirectoryItem item)
        {
            if (item.Items == null || item.Items[0] != null)
                return;

            ProgressMessage = "Discovering Workgroup...";
            InternalProgress = 100;

            var result = await _discoveryService.GetWorkgroup(item.Name, item, () =>
            {
                RaisePropertyChanged(nameof(HasWorkgroup));

                ChangeScreenCommand.InvalidateCanExecute();
            });

            if (result.Exception != null)
            {
                InternalProgress = 0;
                return;
            }

            InternalProgress = 0;
        }

        private void ClearWorkgroup(object o)
        {
            DiscoverWorkgroupCommand.InvalidateCanExecute();
            ErrorMessage = null;

            if (HasWorkgroup)
                RestoreWorkgroupMemento(true);
        }

        private void SaveWorkgroupMemento()
        {
            _workgroupNameMemento = _workgroupName;
            _enumerateWorkgroupsMemento = _enumerateWorkgroups;

            _workgroupItems = WorkgroupItems;

            if (HasWorkgroup)
            {
                _workgroupMementoActions = new Collection<Action>();
                _workgroupItems[0].SetMementoActions(_workgroupMementoActions);
            }
        }

        private void RestoreWorkgroupMemento(bool reset = false)
        {
            WorkgroupItems = new DirectoryItem[0];
            RaisePropertyChanged(nameof(WorkgroupItems));

            if (!reset)
            {
                _workgroupName = _workgroupNameMemento;
                _enumerateWorkgroups = _enumerateWorkgroupsMemento;

                RaisePropertyChanged(nameof(WorkgroupName));
                RaisePropertyChanged(nameof(EnumerateWorkgroups));

                DiscoverWorkgroupCommand.InvalidateCanExecute();

                foreach (var action in _workgroupMementoActions)
                    action.Invoke();
                _workgroupMementoActions.Clear();

                WorkgroupItems = _workgroupItems;
                RaisePropertyChanged(nameof(WorkgroupItems));
            }

            RaisePropertyChanged(nameof(WorkgroupHeader));
            RaisePropertyChanged(nameof(HasWorkgroup));
            ChangeScreenCommand.InvalidateCanExecute();
        }
    }
}