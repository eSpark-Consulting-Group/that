using System;

namespace APTAT.Core.Descent
{
	public interface IDescentQueryResult
	{
		Guid OperationUid { get; }
		void OnDescendOperationEvent( bool ok, IComputer computer, FormattedText statusMessage, FormattedText shortStatusMessage, string errorCode, int systemCode, string systemMessage );
		
        // Check OriginalAgentIdentityOnConnect: it matters when descend subsystem was used
        Guid GetComputerOriginalUid(IComputer computer);

		void RegisterTimeoutCallback(Guid agentIdentity, Action<TimeSpan> callback);
		void UnregisterTimeoutCallback(Guid agentIdentity);

		event Action<IComputer, bool> AgentDescentCompleted;
		event Action DescentCompleted;
	}
}
