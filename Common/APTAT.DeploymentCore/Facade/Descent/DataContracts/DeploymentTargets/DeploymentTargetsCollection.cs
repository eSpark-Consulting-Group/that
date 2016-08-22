using System.Collections.Generic;

namespace APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets
{
	public class DeploymentTargetsCollection : List<DeploymentTarget>
	{
	    public DeploymentTargetsCollection()
	    {
	        
	    }

	    public DeploymentTargetsCollection(int capacity)
            : base(capacity)
	    {
	        
	    }

        public DeploymentTargetsCollection(IEnumerable<DeploymentTarget> collection)
            : base(collection)
	    {
	        
	    }
	}
}
