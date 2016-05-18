using System;

namespace APTAT.Core.Descent
{
	public interface IDescentProgressItem : IBusinessObject
	{
		Guid ComputerUid { get; }
		Guid OperationUid { get; }
		int ProgressPercentage { get; }
		FormattedText StatusMessage { get; }
	    AptScanStage ScanStage { get; }
	    int TotalFilesCount { get; }
	    int UnknownFilesCount { get; }
        int FailedAtNodeCount { get; set; }
        int MalwaresCount { get; }
	    int SafeFilesCount { get; }
        int ErrorFilesCount { get; }
	}
}
