using System.Collections.ObjectModel;
using DDT.Application.Messages;
using System.Linq;
using Telerik.Windows.Controls;
using DDT.Application.Models;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using DDT.Application.Transitions;

namespace DDT.Application.ViewModels
{
    public partial class MainViewModel
    {
        partial void InitializeActiveDirectory()
        {
            DiscoverActiveDirectoryCommand = new DelegateCommand(DiscoverActiveDirectory, CanDiscoverActiveDirectory);
            ExpandActiveDirectoryCommand = new DelegateCommand(ExpandActiveDirectoryTree, CanExpandActiveDirectoryTree);
            ClearActiveDirectoryCommand = new DelegateCommand(ClearActiveDirectory);

            ActiveDirectoryItems = new DirectoryItem[0];
        }

        public DelegateCommand DiscoverActiveDirectoryCommand { get; private set; }
        public DelegateCommand ExpandActiveDirectoryCommand { get; private set; }
        public DelegateCommand ClearActiveDirectoryCommand { get; private set; }

        private Collection<Action> _activeDirectoryMementoActions = new Collection<Action>();
        private DirectoryItem[] _activeDirectoryItems;
        public DirectoryItem[] ActiveDirectoryItems { get; private set; }

        public string ActiveDirectoryHeader
        {
            get
            {
                if (!HasActiveDirectory)
                    return "Discover computers by selecting them from Active Directory";

                return string.Join(", ", GetSelectedActiveDirectoryItems());
            }
        }

        private List<DeploymentItem> GetSelectedActiveDirectoryItems()
        {
            var result = new List<DeploymentItem>();
            foreach (var item in ActiveDirectoryItems)
                result.AddRange(item.GetSelectedItems());
            return result;
        }

        public bool HasActiveDirectory
        {
            get { return ActiveDirectoryItems.Any(_ => _.IsSelected.GetValueOrDefault(true)); }
        }

        private string _domainControllerMemento;
        private string _domainController;
        public string DomainController
        {
            get { return UseCurrentDomainForActiveDirectory ? null : _domainController; }
            set
            {
                _domainController = value;
                RaisePropertyChanged(nameof(DomainController));

                ClearActiveDirectory(null);
            }
        }

        public string DomainControllerMask
        {
            get { return "Enter Domain Controller"; }
        }

        private string _activeDirectoryLoginMemento;
        private string _activeDirectoryLogin;
        public string ActiveDirectoryLogin
        {
            get { return UseCurrentDomainForActiveDirectory ? null : _activeDirectoryLogin; }
            set
            {
                _activeDirectoryLogin = value;
                RaisePropertyChanged(nameof(ActiveDirectoryLogin));

                ClearActiveDirectory(null);
            }
        }

        private string _activeDirectoryPasswordMemento;
        private string _activeDirectoryPassword;
        public string ActiveDirectoryPassword
        {
            get { return UseCurrentDomainForActiveDirectory ? null : _activeDirectoryPassword; }
            set
            {
                _activeDirectoryPassword = value;
                RaisePropertyChanged(nameof(ActiveDirectoryPassword));

                ClearActiveDirectory(null);
            }
        }

        private bool _useCurrentDomainForActiveDirectoryMemento;
        private bool _useCurrentDomainForActiveDirectory;
        public bool UseCurrentDomainForActiveDirectory
        {
            get { return _useCurrentDomainForActiveDirectory; }
            set
            {
                _useCurrentDomainForActiveDirectory = value;
                RaisePropertyChanged(nameof(UseCurrentDomainForActiveDirectory));
                RaisePropertyChanged(nameof(DomainController));
                RaisePropertyChanged(nameof(ActiveDirectoryLogin));
                RaisePropertyChanged(nameof(ActiveDirectoryPassword));

                ClearActiveDirectory(null);
            }
        }

        private string _keywordActiveDirectory;
        public string KeywordActiveDirectory
        {
            get { return _keywordActiveDirectory; }
            set
            {
                _keywordActiveDirectory = value;
                RaisePropertyChanged(nameof(KeywordActiveDirectory));
            }
        }

        public bool SearchActiveDirectory(object o)
        {
            if (o is DirectoryItem)
            {
                var dir = (DirectoryItem)o;
                switch (dir.Type)
                {
                    case DirectoryItemType.OrgUnit:
                    case DirectoryItemType.Computer:
                        return dir.Name.ToLower().Contains(KeywordActiveDirectory.ToLower());
                    case DirectoryItemType.Root:
                        return true;
                }
            }
            return false;
        }

        private void DiscoverActiveDirectory(object o)
        {
            ButtonsAvailable = false;

            AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.ActiveDirectory, async _ =>
            {
                SetCurrentScreen(_);

                if (!HasActiveDirectory)
                    await TryAction(GetActiveDirectoryRoot());
            });
        }

        private bool CanDiscoverActiveDirectory(object o)
        {
            return UseCurrentDomainForActiveDirectory || (!string.IsNullOrEmpty(DomainController) && !string.IsNullOrEmpty(ActiveDirectoryLogin) && !string.IsNullOrEmpty(ActiveDirectoryPassword));
        }

        private async Task GetActiveDirectoryRoot()
        {
            KeywordActiveDirectory = null;
            ProgressMessage = "Discovering Active Directory...";
            Progress = 100;

            await Task.Delay(1500);

            ActiveDirectoryItems = new DirectoryItem[0];
            RaisePropertyChanged(nameof(ActiveDirectoryItems));

            var e = await _discoveryService.GetDirectoryRoot(DomainController, ActiveDirectoryLogin, ActiveDirectoryPassword, () =>
            {
                RaisePropertyChanged(nameof(HasActiveDirectory));

                ChangeScreenCommand.InvalidateCanExecute();
            });

            if (e.Exception == null)
            {
                ActiveDirectoryItems = e.Items;
                RaisePropertyChanged(nameof(ActiveDirectoryItems));
            }

            if (ActiveDirectoryItems.Any())
                Progress = 0;
            else
                AppMessages.ChangeScreenMessage.Send(ChangeScreenTypes.ActiveDirectoryCredentials, _ =>
                {
                    SetCurrentScreen(_, () =>
                    {
                        Progress = 0;
                        ErrorMessage = "Domain does not exist or could not be contacted under the specified credentials. Please try another domain or credentials.";
                    });
                }, Direction.RightToLeft);
        }

        private async void ExpandActiveDirectoryTree(object o)
        {
            await TryAction(ExpandActiveDirectoryTree((DirectoryItem)o));
        }

        private bool CanExpandActiveDirectoryTree(object o)
        {
            return string.IsNullOrEmpty(KeywordActiveDirectory) || KeywordActiveDirectory.Length < 3;
        }

        private async Task ExpandActiveDirectoryTree(DirectoryItem item)
        {
            if (item.Items == null || item.Items[0] != null)
                return;

            ProgressMessage = "Discovering Active Directory...";
            InternalProgress = 100;

            var result = await _discoveryService.GetDirectoryItems(DomainController, ActiveDirectoryLogin, ActiveDirectoryPassword, item, () =>
            {
                RaisePropertyChanged(nameof(HasActiveDirectory));

                ChangeScreenCommand.InvalidateCanExecute();
            });

            if (result.Exception != null)
            {
                InternalProgress = 0;
                return;
            }

            item.Items = result.Items;
            InternalProgress = 0;
        }

        private async Task<string> GetCurrentDomainName()
        {
            var result = await _discoveryService.GetCurrentDomainName();

            if (result.Exception != null)
            {
                return null;
            }

            return result.Name;
        }

        private void ClearActiveDirectory(object o)
        {
            DiscoverActiveDirectoryCommand.InvalidateCanExecute();

            if (HasActiveDirectory)
                RestoreActiveDirectoryMemento(true);
        }

        private void SaveActiveDirectoryMemento()
        {
            _domainControllerMemento = _domainController;
            _activeDirectoryLoginMemento = _activeDirectoryLogin;
            _activeDirectoryPasswordMemento = _activeDirectoryPassword;
            _useCurrentDomainForActiveDirectoryMemento = _useCurrentDomainForActiveDirectory;

            _activeDirectoryItems = ActiveDirectoryItems;

            if (HasActiveDirectory)
            {
                _activeDirectoryMementoActions = new Collection<Action>();
                _activeDirectoryItems[0].SetMementoActions(_activeDirectoryMementoActions);
            }
        }

        private void RestoreActiveDirectoryMemento(bool reset = false)
        {
            ActiveDirectoryItems = new DirectoryItem[0];
            RaisePropertyChanged(nameof(ActiveDirectoryItems));

            if (!reset)
            {
                _domainController = _domainControllerMemento;
                _activeDirectoryLogin = _activeDirectoryLoginMemento;
                _activeDirectoryPassword = _activeDirectoryPasswordMemento;
                _useCurrentDomainForActiveDirectory = _useCurrentDomainForActiveDirectoryMemento;

                RaisePropertyChanged(nameof(DomainController));
                RaisePropertyChanged(nameof(ActiveDirectoryLogin));
                RaisePropertyChanged(nameof(ActiveDirectoryPassword));
                RaisePropertyChanged(nameof(UseCurrentDomainForActiveDirectory));

                DiscoverActiveDirectoryCommand.InvalidateCanExecute();

                foreach (var action in _activeDirectoryMementoActions)
                    action.Invoke();
                _activeDirectoryMementoActions.Clear();

                ActiveDirectoryItems = _activeDirectoryItems;
                RaisePropertyChanged(nameof(ActiveDirectoryItems));

                ChangeScreenCommand.InvalidateCanExecute();
            }

            RaisePropertyChanged(nameof(ActiveDirectoryHeader));
            RaisePropertyChanged(nameof(HasActiveDirectory));
        }
    }
}