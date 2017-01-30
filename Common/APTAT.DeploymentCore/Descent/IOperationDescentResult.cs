using APTAT.DeploymentCore.Messages;
using System;

namespace APTAT.DeploymentCore.Descent
{
    public interface IOperationDescentResult
    {
        string ErrorCode { get; }
        FormattedText ShortStatusMessage { get; }
        FormattedText StatusMessage { get; }
        bool Succeeded { get; }
        bool IsCanceled { get; }
        int SystemCode { get; }
        string SystemMessage { get; }
    }
}