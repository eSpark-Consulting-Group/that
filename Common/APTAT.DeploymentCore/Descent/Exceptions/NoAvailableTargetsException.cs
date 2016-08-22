using System;

namespace APTAT.DeploymentCore.Descent.Exceptions
{
    public class NoAvailableTargetsException : Exception
    {
        public override string Message
        {
            get
            {
                return "No available targets.";
            }
        }
    }
}
