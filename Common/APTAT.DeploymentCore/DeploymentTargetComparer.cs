using System.Collections.Generic;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.DeploymentCore.Facade.Descent.Commands
{
    public class DeploymentTargetComparer : IEqualityComparer<DeploymentTarget>
    {
        public bool Equals(DeploymentTarget x, DeploymentTarget y)
        {
            return x.IPAddress == y.IPAddress || (string.IsNullOrEmpty(x.IPAddress) && string.IsNullOrEmpty(y.IPAddress) && x.Name == y.Name);
        }

        public int GetHashCode(DeploymentTarget obj)
        {
            return !string.IsNullOrEmpty(obj.IPAddress) ? obj.IPAddress.GetHashCode() : obj.Name.GetHashCode();
        }
    }
}