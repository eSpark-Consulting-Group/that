using System;
using System.Collections.Generic;

namespace APTAT.DeploymentCore.Descent
{
    public interface IDescentResult
    {
        Guid OperationUid { get; }
        int TotalTargetsCount { get; }
        bool Completed { get; }
        DateTime? CompletedTime { get; }

        event Action DescentCompleted;

        List<IComputerDescentResult> ComputerDescentResults { get; }
    }
}
