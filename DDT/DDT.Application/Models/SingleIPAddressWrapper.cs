using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;

namespace DDT.Application.Models
{
    public class SingleIPAddressWrapper : EndpointAddressWrapper<SingleIPAddress>
    {
        public SingleIPAddressWrapper(SingleIPAddress singleIpAddress)
        {
            _address = singleIpAddress;
        }

        public SingleIPAddressWrapper()
        {
            _address = new SingleIPAddress();
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
    }
}