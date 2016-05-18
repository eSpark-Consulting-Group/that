using System;
using System.Collections.Generic;

namespace APTAT.Core.Descent
{
	public interface IDescentSubSystem : ISubSystem
	{

	    void AbortThreadByTargetUid(Guid uid);
        
        void StartAptScan(IEnumerable<TargetUidComputerPair> computers, string login, string password, 
            out Guid operationUid, string scanScope);

        void StartWmiScan(IEnumerable<TargetUidComputerPair> computers, string login, string password, 
            out Guid operationUid);

        AptScanOperationContext GetScanResultsByOperationUid(Guid operationUid);
	    AptScanOperationContext GetFirstScanResults();

        void AbortAllThreads();

	    event EventHandler<DescentCompletedEventArgs> DescentCompleted;
	}
}
