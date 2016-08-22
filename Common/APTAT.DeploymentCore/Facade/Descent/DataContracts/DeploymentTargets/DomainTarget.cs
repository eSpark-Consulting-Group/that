namespace APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets
{
	public class DomainTarget : DeploymentTarget
	{
		public DomainEntryType EntryType { get; set; }
        public string DomainName { get; set; }

        public override DeploymentTargetSource Source
        {
            get
            {
                return DeploymentTargetSource.Domain;
            }

            set
            {
                
            }
        }
    }
}
