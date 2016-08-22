using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore.Facade.Descent.MessageContracts
{
	public class GetTargetsSummaryRequest
	{
		public DeploymentTargetsCollection Targets { get; set; }

		public string DomainAccessLogin { get; set; }

		public string DomainAccessPassword { get; set; }
	}

	public class GetTargetsSummaryResponse
	{
		public GetTargetsSummaryResponse()
		{
			Targets = new DeploymentTargetsCollection();
		}

		public bool IsCompleted { get; set; }

		public DeploymentTargetsCollection Targets { get; set; }
	}
}
