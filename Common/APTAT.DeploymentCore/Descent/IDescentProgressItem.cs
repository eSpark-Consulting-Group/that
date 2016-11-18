using System;
using APTAT.Common;
using APTAT.DeploymentCore.Messages;

namespace APTAT.DeploymentCore.Descent
{
	public interface IDescentProgressItem : IBusinessObject
	{
		Guid ComputerUid { get; }
		Guid OperationUid { get; }
		double ProgressPercentage { get; }
		FormattedText StatusMessage { get; }
        bool IsCanceled { get; }
    }
}
