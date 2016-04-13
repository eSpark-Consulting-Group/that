using System;
using CRM.SrvCore.Descent;
using APTAT.Core.Tasks;

namespace APTAT.Core.Descent
{
	public interface IDescentQueryResult
	{
		Guid OperationUid { get; }
		void OnQueryAgentEvent( bool ok, RemoteAgentInformation info, FormattedText errorString );
		void OnDescendOperationEvent( bool ok, IComputer computer, FormattedText statusMessage, FormattedText shortStatusMessage, string errorCode, int systemCode, string systemMessage );
		void OnPackageInstallationEvent( bool ok, IComputer computer, FormattedText statusMessage, FormattedText shortStatusMessage, string errorCode, string systemMessage, int systemCode );
		void OnIncompatibleUninstallEvent( bool ok, IComputer computer, FormattedText statusMessage, FormattedText shortStatusMessage, string errorCode, string systemMessage, int systemCode, string[] unsupportedIncompatibleProducts, string[] supportedIncompatibleProducts );
		void OnOutdatedCisUninstallEvent( bool ok, IComputer computer, FormattedText statusMessage, FormattedText shortStatusMessage, string errorCode, string systemMessage, int systemCode, string outdatedCis );

        // Check OriginalAgentIdentityOnConnect: it matters when descend subsystem was used
        Guid GetComputerOriginalUid(IComputer computer);
        bool ContainsComputer( Guid uid, out ComputerDescentResult result );
        ComputerDescentResult UpdateComputerDescentResult(Guid uid, Func<ComputerDescentResult, ComputerDescentResult> updateFactory);
		void AddPackageTask(ComputerDescentResult result, ITask task);

		void RegisterTimeoutCallback(Guid agentIdentity, Action<TimeSpan> callback);
		void UnregisterTimeoutCallback(Guid agentIdentity);

		event Action<IComputer, bool> AgentDescentCompleted;
		event Action DescentCompleted;
	}
}
