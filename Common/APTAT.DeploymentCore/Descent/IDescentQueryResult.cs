using System;
using APTAT.Common;
using APTAT.DeploymentCore.Messages;
using System.Collections.Generic;
using APTAT.DeploymentCore.Directory;

namespace APTAT.DeploymentCore.Descent
{


    public interface IDescentQueryResult: IChangeTrackingListener, IDescentResult
    {
        IComputerDescentResult GetComputerDescentResultByTargetUid(Guid targetUid);


        void OnDescendOperationEvent( bool ok, Guid computerUid, FormattedText statusMessage, FormattedText shortStatusMessage, 
            string errorCode, int systemCode, string systemMessage, bool isCanceled = false);
		
		void RegisterTimeoutCallback(Guid agentIdentity, Action<TimeSpan> callback);

        void UnregisterTimeoutCallback(Guid agentIdentity);
	}
}
