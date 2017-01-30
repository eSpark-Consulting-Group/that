using System;
using APTAT.Common;

namespace APTAT.DeploymentCore.Descent
{
    public interface IDescentSubSystem : ISubSystem
	{
        void StartScan(IScanner scanner, out Guid operationUid);
        IDescentQueryResult GetDescentResultsByOperationUid(Guid operationUid);
        void StopAllScans(int code = 0);
	    void StopOperationForTargetUid(Guid[] targetUids);

        event EventHandler<DescentEventArgs> DescentCompleted;
    }
}
