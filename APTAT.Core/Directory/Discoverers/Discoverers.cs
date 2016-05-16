using APTAT.Core.Facade.Descent.Commands;
using APTAT.Core.Facade.Descent.MessageContracts;

namespace APTAT.Core.Directory.Discoverers
{
    internal class DomainDiscoverer : TargetsDiscoverer, IDomainDiscoverer
    {
        internal DomainDiscoverer()
        {
        }

        internal DomainDiscoverer(IServiceCommand<FindTargetsResponse> command)
            : base(command)
        {
        }
    }



    internal class WorkgroupDiscoverer : TargetsDiscoverer, IWorkgroupDiscoverer
    {
        internal WorkgroupDiscoverer()
        {
        }

        internal WorkgroupDiscoverer(IServiceCommand<FindTargetsResponse> command)
            : base(command)
        {
        }
    }

    internal class IpAddressDiscoverer : TargetsDiscoverer, IIpAddressDiscoverer
    {
        internal IpAddressDiscoverer()
        {
        }

        internal IpAddressDiscoverer(IServiceCommand<FindTargetsResponse> command)
            : base(command)
        {
        }
    }
}
