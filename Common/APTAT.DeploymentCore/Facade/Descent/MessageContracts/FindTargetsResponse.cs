using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore.Facade.Descent.MessageContracts
{
	public class FindTargetsResponse
	{
		public FindTargetsResponse()
		{
			Targets = new DeploymentTargetsCollection();
		}

		public bool IsCompleted { get; set; }

		public DeploymentTargetsCollection Targets { get; set; }
	}
}
