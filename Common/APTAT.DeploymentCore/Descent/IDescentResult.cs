using System;
using System.Collections.Generic;

namespace APTAT.DeploymentCore.Descent
{
    public interface IDescentResult
    {
        Guid OperationUid { get; }
        int TotalTargetsCount { get; }
        int SucceededCount { get; }
        int FailedCount { get; }
        int InProgressCount { get; }
        bool Completed { get; }
        DateTime? CompletedTime { get; }

        event Action DescentCompleted;

        List<IComputerDescentResult> ComputerDescentResults { get; }
    }
}
