using System;
using System.Collections.Generic;
using System.Globalization;

namespace APTAT.Core.Descent
{
	public interface IDescentSubSystem : ISubSystem
	{

	    void AbortThreadByTargetUid(Guid uid);
        
        void DoDescentOperationOnNotExistingComputers(IEnumerable<TargetUidComputerPair> computers, string login, string password, out Guid operationUid, string scanScope, string domain, DevicesSourceDescription devicesSrcDescr);

		string GetDescentErrorMessage( string errorCode, int systemCode, string systemMessage, string[] extensionTexts, bool isUninstall,CultureInfo cultureInfo );
        

		ushort GetSocketPortForAgent();
		DescentResult GetDescentResult( Guid operationUid );

	    AptScanOperationContext GetScanResultsByOperationUid(Guid operationUid);
	    AptScanOperationContext GetFirstScanResults();

        void AbortAllThreads();

	    event EventHandler<DescentCompletedEventArgs> DescentCompleted;
	}
}
