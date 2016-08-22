namespace APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets
{
	public class EndpointTarget : DeploymentTarget
	{

        public override DeploymentTargetSource Source
        {
            get
            {
                return DeploymentTargetSource.Ip;
            }

            set
            {
                //base.Source = value;
            }
        }
    }
}
