using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;

namespace DDT.Application.Models
{
    public class SubnetAddressWrapper : EndpointAddressWrapper<SubnetAddress>
    {
        public SubnetAddressWrapper(SubnetAddress subnetAddress)
        {
            _address = subnetAddress;
        }

        public string IPAddress
        {
            get { return EndpointAddress.IPAddress; }
            set
            {
                if (EndpointAddress.IPAddress == value)
                    return;
                EndpointAddress.IPAddress = value;
                RaisePropertyChanged(nameof(IPAddress));
            }
        }

        public string SubnetMask
        {
            get { return EndpointAddress.SubnetMask; }
            set
            {
                if (EndpointAddress.SubnetMask == value)
                    return;
                EndpointAddress.SubnetMask = value;
                RaisePropertyChanged(nameof(SubnetMask));
            }
        }
    }
}