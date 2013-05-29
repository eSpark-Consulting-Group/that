using APTAT.Core.Facade.Descent.DataContracts;

namespace APTAT.Core.Facade.Descent.MessageContracts
{
	public class GetDeploymentResultsResponse
	{
		public GetDeploymentResultsResponse()
		{
			DeploymentResults = new DeploymentResultsCollection();
		}

		public bool Completed { get; set; }

		public DeploymentResultsCollection DeploymentResults { get; set; }
	}
}
