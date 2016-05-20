using System;
using System.Collections.Concurrent;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Descent.Wmi;

namespace APTAT.Core.Descent
{
    public interface IAptScanOperationContext : IDescentQueryResult
    {
        ConcurrentDictionary<Guid, AptScanResult> ScanResults { get; }
    }
    public interface IWmiScanOperationContext : IDescentQueryResult
    {
        ConcurrentDictionary<Guid, WmiScanResult> ScanResults { get; }
    }

    public interface IDescentQueryResult: IChangeTrackingListener
    {
		Guid OperationUid { get; }
        int TotalTargetsCount { get; }
        bool Completed { get; }
        DateTime? CompletedTime { get; }
        void OnDescendOperationEvent( bool ok, IComputer computer, FormattedText statusMessage, FormattedText shortStatusMessage, string errorCode, int systemCode, string systemMessage );
		
		void RegisterTimeoutCallback(Guid agentIdentity, Action<TimeSpan> callback);
		void UnregisterTimeoutCallback(Guid agentIdentity);
    
		event Action DescentCompleted;
	}
}
