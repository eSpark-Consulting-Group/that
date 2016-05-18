using System;
using System.Collections.Concurrent;

namespace APTAT.Core.Descent
{
    public interface IAptScanOperationContext : IDescentQueryResult
    {
        ConcurrentDictionary<Guid, ScanResult> ScanResults { get; }
    }

	public interface IDescentQueryResult
	{
		Guid OperationUid { get; }
		void OnDescendOperationEvent( bool ok, IComputer computer, FormattedText statusMessage, FormattedText shortStatusMessage, string errorCode, int systemCode, string systemMessage );
		
		void RegisterTimeoutCallback(Guid agentIdentity, Action<TimeSpan> callback);
		void UnregisterTimeoutCallback(Guid agentIdentity);
    
		event Action DescentCompleted;
	}
}
