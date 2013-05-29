using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Facade.Descent.MessageContracts
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
