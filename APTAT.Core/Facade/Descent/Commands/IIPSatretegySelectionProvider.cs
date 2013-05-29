using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using APTAT.Core.Transport;

namespace APTAT.Core.Facade.Descent.Commands
{
	public interface IIPSatretegySelectionProvider
	{
		IIPSelectionStrategy GetStrategy( BaseEndpointAddress address );
	}
}
