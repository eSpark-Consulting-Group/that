using System;

namespace APTAT.DeploymentCore.Descent
{
    public class OperationProgressChangedEventArgs : EventArgs
    {
        public double Percentage { get; set; }
        public Guid OperationUid { get; set; }
    }
}
