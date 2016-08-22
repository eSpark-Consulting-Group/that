namespace APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets
{
	public class WorkgroupTarget : DeploymentTarget
	{
        public bool IsGroup { get; set; }
		public string SmbName { get; set; }
        public override DeploymentTargetSource Source
        {
            get
            {
                return DeploymentTargetSource.Workgroup;
            }

            set
            {
                
            }
        }
    }
}
