using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using CRM.SrvCore.Descent;

namespace APTAT.Core.Descent
{
	public interface IDescentSubSystem : ISubSystem
	{
		byte[] GetServerDsaPublicKey();
		//string[] GetAvailableAgentPackages(SystemKind systemKind);
		//byte[] GetAgentPackageBinary( string packageName );
		//string[] GetAvailablePathAgentPackages(SystemKind systemKind);
		IAgentPackageHelper[] GetAgentPackageHelpers();
		IAgentPackageHelper GetAgentPackageHelper(SystemKind systemKind);
		IAgentPackageHelper GetAgentPackageHelper(Guid pluginUid);

	    void AbortThreadByTargetUid(Guid uid);

		///// <summary>
		///// Descent agent to all computers in Persistent group
		///// </summary>
		///// <param name="group">Target group</param>
		///// <param name="login">Login at target</param>
		///// <param name="password">Password at target</param>
		//void DescentOnGroup( IPersistentGroup group, string login, string password );
		//void UnDescentOnGroup( IPersistentGroup group, string login, string password );
        void DoDescentOperationOnNotExistingComputers(IEnumerable<TargetUidComputerPair> computers, string login, string password, bool withDelayedComputers, InstallPackageInfo package, out Guid operationUid, string scanScope);
	//	void AddDescentOperationOnNotExistingComputers( Guid operationUid, IEnumerable<IComputer> computers, string login, string password );
	//	void FinishDelayedComputers( Guid operationUid );
		string GetDescentErrorMessage( string errorCode, int systemCode, string systemMessage, string[] extensionTexts, bool isUninstall,CultureInfo cultureInfo );

		//void QueryAgentInformation( IDescendQueryResult queryCallback, IDirectoryItem[] items, string login, string password );
		//void DoDescendOperationOnFolders( IDescentQueryResult queryCallback, bool descend, IDirectoryItem[] items, string login, string password, bool skipNotificationsForThisSession );
		//void UpdateAgentByTask(IComputer computer);

		ushort GetSocketPortForAgent();
		bool CreateAgentInstallers();
		DescentResult GetDescentResult( Guid operationUid );
	    ConcurrentDictionary<Guid, ScanResult> ScanResults { get; }

        void AbortAllThreads();
    }
}
