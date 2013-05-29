using APTAT.Core.Facade.Descent.DataContracts;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Facade.Descent.MessageContracts
{
	public class DeployAgentRequest
	{
		public DeploymentTargetsCollection Targets  { get; set; }

		public string Login { get; set; }

		public string Password { get; set; }

		public string DomainAccessLogin { get; set; }

		public string DomainAccessPassword { get; set; }

		public InstallPackageInfo InstallPackage { get; set; }

		public bool UpdateEndpoints { get; set; }
	}
}
