using System;
using APTAT.DeploymentCore.Descent.Helpers;

namespace APTAT.Core.Descent.Apt
{
    public interface IAptDescentMod: IDescentMod
    {
        void StartAptScan(IRemoteHostInfo remoteHostInfo, IDescentCallback cb, 
            Guid agentIdentity, IAptScanResult result, Guid operationUid, IScanOptions scanOptions);

    }
}