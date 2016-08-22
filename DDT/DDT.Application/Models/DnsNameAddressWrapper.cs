using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;

namespace DDT.Application.Models
{
    public class DnsNameAddressWrapper : EndpointAddressWrapper<DnsNameAddress>
    {
        public DnsNameAddressWrapper(DnsNameAddress dnsNameAddress)
        {
            _address = dnsNameAddress;
        }

        public string DnsName
        {
            get { return EndpointAddress.DnsName; }
            set
            {
                if (EndpointAddress.DnsName == value)
                    return;
                EndpointAddress.DnsName = value;
                RaisePropertyChanged(nameof(DnsName));
            }
        }
    }
}