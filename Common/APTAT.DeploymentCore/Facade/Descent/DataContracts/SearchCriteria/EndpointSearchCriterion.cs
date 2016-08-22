using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;

namespace APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria
{
	public class EndpointSearchCriterion : TargetsSearchCriterion
	{
		public EndpointAddressCollection EndpointAddresses { get; set; }
	}
}
