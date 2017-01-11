using System;
using APTAT.Common;

namespace APTAT.DeploymentCore.Descent
{
    public interface IDescentSubSystem : ISubSystem
	{
        void StartScan(IScanner scanner, Action<Guid> guidAcquired);
        IDescentQueryResult GetDescentResultsByOperationUid(Guid operationUid);

        void StopAllScans(int code = 0);
	    void StopOperationForTargetUid(Guid[] targetUids);

        event EventHandler<DescentEventArgs> DescentCompleted;
    }
}
