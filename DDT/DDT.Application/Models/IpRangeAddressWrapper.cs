using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;

namespace DDT.Application.Models
{
    public class IpRangeAddressWrapper : EndpointAddressWrapper<IPRangeAddress>
    {
        public IpRangeAddressWrapper(IPRangeAddress ipRangeAddress)
        {
            _address = ipRangeAddress;
        }

        public string StartAddress
        {
            get { return EndpointAddress.StartAddress; }
            set
            {
                if (EndpointAddress.StartAddress == value)
                    return;
                EndpointAddress.StartAddress = value;
                RaisePropertyChanged(nameof(StartAddress));
            }
        }

        public string EndAddress
        {
            get { return EndpointAddress.EndAddress; }
            set
            {
                if (EndpointAddress.EndAddress == value)
                    return;
                EndpointAddress.EndAddress = value;
                RaisePropertyChanged(nameof(EndAddress));
            }
        }
    }
}