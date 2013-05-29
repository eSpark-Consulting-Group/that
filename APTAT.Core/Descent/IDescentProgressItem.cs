using System;

namespace APTAT.Core.Descent
{
	public interface IDescentProgressItem
	{
		Guid ComputerUid { get; }
		Guid OperationUid { get; }
		int ProgressPercentage { get; }
		FormattedText StatusMessage { get; }
	}
}
