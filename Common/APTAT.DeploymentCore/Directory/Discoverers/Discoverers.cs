using System.Threading.Tasks;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;

namespace APTAT.DeploymentCore.Directory.Discoverers
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
        private readonly IServiceCommand<EnumerateWorkgroupsResponse> _commandForEnumerate;

        internal WorkgroupDiscoverer()
        {
        }

        internal WorkgroupDiscoverer(IServiceCommand<FindTargetsResponse> command, IServiceCommand<EnumerateWorkgroupsResponse> commandForEnumerate)
            : base(command)
        {
            _commandForEnumerate = commandForEnumerate;
        }

        public async Task<string[]> EnumerateWorkgroups()
        {
            await Task.Run(() => _commandForEnumerate.Execute());
            return _commandForEnumerate.Response.Workgroups;
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
