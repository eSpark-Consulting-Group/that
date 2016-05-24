using System;
using System.Collections.Generic;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Descent.Wmi;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Descent
{
	public interface IDescentSubSystem : ISubSystem
	{
        
        void StartAptScan(IEnumerable<DeploymentTarget> computers, string login, string password, 
            out Guid operationUid, string scanScope, string tcpPort, string udpPort);

        void StartWmiScan(IEnumerable<DeploymentTarget> computers, string login, string password, 
            out Guid operationUid);

        AptScanOperationContext GetAptScanResultsByOperationUid(Guid operationUid);
        WmiScanOperationContext GetWmiScanResultsByOperationUid(Guid operationUid);
        AptScanOperationContext GetFirstScanResults();

        void AbortAllThreads();

	    event EventHandler<DescentEventArgs> DescentCompleted;
	}
}
