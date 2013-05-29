using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;

namespace APTAT.Core.Facade.Descent.DataContracts.SearchCriteria
{
	public class EndpointSearchCriterion : TargetsSearchCriterion
	{
		public EndpointAddressCollection EndpointAddresses { get; set; }
	}
}
