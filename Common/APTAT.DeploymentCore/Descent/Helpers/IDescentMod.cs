using System;

namespace APTAT.DeploymentCore.Descent.Helpers
{
    public interface IDescentMod
    {
        void Release();
        void StopScan(Guid targetGuid);
    }
}