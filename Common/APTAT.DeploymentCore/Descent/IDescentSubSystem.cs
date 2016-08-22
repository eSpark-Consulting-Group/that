using System;
using APTAT.Common;

namespace APTAT.DeploymentCore.Descent
{
    public interface IDescentSubSystem : ISubSystem
	{

        //void StartAptScan(IEnumerable<DeploymentTarget> computers, string login, string password,
        //    DevicesSourceDescription devicesSourceDescription,out Guid operationUid, string scanScope,
        //    string tcpPort, string udpPort);
	    void StartScan(IScanner scanner, out Guid operationUid);
        //void StartWmiScan(IEnumerable<DeploymentTarget> computers, string login, string password,
        //    DevicesSourceDescription devicesSourceDescription, out Guid operationUid);
        IDescentQueryResult GetDescentResultsByOperationUid(Guid operationUid);
        //AptScanOperationContext GetAptScanResultsByOperationUid(Guid operationUid);
        //WmiScanOperationContext GetWmiScanResultsByOperationUid(Guid operationUid);
        //AptScanOperationContext GetFirstScanResults();

        void AbortAllThreads(int code = 0);

	    event EventHandler<DescentEventArgs> DescentCompleted;
    }
}
