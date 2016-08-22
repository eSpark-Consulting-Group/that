using System;

namespace DDT.Application.Services
{
    public class DeploymentStartedEventArgs : DomainEventArgs
    {
        public DeploymentStartedEventArgs(Guid operationUid)
        {
            OperationUid = operationUid;
        }
        public DeploymentStartedEventArgs()
        {
            
        }

        public Guid OperationUid { get; private set; }
    }
}
