
using System.Linq;

namespace DDT.Application.Models
{
    public enum DiscoveryTargetType
    {
        ActiveDirectory,
        Workgroup,
        NetworkAddress
    }

    public class DiscoveryTargetWrapper : SelectableItemWrapper<string>
    {
        public DiscoveryTargetType Type { get; set; }
        private bool _inProgress;
        public bool InProgress
        {
            get { return Stopping ? false : _inProgress; }
            set
            {
                _inProgress = value;
                RaisePropertyChanged(nameof(InProgress));
                RaisePropertyChanged(nameof(Failed));
            }
        }

        private bool _stopping;
        public bool Stopping
        {
            get { return _stopping; }
            set
            {
                _stopping = value;
                RaisePropertyChanged(nameof(Failed));
                RaisePropertyChanged(nameof(InProgress));
            }
        }

        public bool Succeded { get; set; }

        public bool Failed
        {
            get { return !Stopping && Items != null && Items.Any(_ => _.Failed); }
        }

        public string PackageName
        {
            get { return string.Empty; }
        }

        public bool HasPackageName
        {
            get { return false; }
        }

        public DeploymentItem[] Items { get; set; }

        public string Name
        {
            get
            {
                switch (Type)
                {
                    case DiscoveryTargetType.ActiveDirectory:
                        return "Active Directory";
                    case DiscoveryTargetType.Workgroup:
                        return "Workgroup";
                    default:
                    case DiscoveryTargetType.NetworkAddress:
                        return "Network Address";
                }
            }
        }

        public double Height { get { return 45; } }
    }
}
