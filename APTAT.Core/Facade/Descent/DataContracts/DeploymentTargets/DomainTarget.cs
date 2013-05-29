namespace APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets
{
	public class DomainTarget : DeploymentTarget
	{
		public string LdapPath { get; set; }

		public DomainEntryType EntryType { get; set; }
	}
}
