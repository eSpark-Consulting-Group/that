using System;
using APTAT.DeploymentCore.Descent.Exceptions.Transport;
using APTAT.DeploymentCore.Messages;

namespace APTAT.DeploymentCore.Descent
{
    public interface IComputerDescentResultInternal : IComputerDescentResult
    {
        void Invalidate(DescentStatus errorCode, string shortMessage, string detailedMessage);
        void SetSystemKind(SystemKind systemKind);
        void UpdateStatus(IOperationDescentResult result);
        void UpdateStatus(IDescentProgressItem progress);
    }

    public interface IComputerDescentResult
    {
        Guid TargetUid { get; }
        Guid OperationUid { get; }
        IComputer Computer { get; }
        bool Completed { get; }
        bool Succeeded { get; }
        FormattedText StatusMessage { get;}
        FormattedText ShortStatusMessage { get; }
        string ErrorCode { get; }
        string SystemMessage { get; }
        int SystemCode { get; }
        double Percentage { get;}
        bool IsCanceled { get; }
        DateTime Modified { get; }
    }
}