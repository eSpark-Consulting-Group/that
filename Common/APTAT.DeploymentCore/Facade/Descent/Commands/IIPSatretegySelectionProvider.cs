using APTAT.Common;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using APTAT.DeploymentCore.Transport;

namespace APTAT.DeploymentCore.Facade.Descent.Commands
{
	public interface IIPSatretegySelectionProvider : IService
	{
		IIPSelectionStrategy GetStrategy( BaseEndpointAddress address );
	}
}
