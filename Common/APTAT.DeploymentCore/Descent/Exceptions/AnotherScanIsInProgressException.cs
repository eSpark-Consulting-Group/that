using System;

namespace APTAT.DeploymentCore.Descent.Exceptions
{
    public class AnotherScanIsInProgressException : Exception
    {
        public override string Message
        {
            get
            {
                return "Another scan is in progress.";
            }
        }
    }
}
