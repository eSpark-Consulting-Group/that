using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using System;

namespace APTAT.Core.Descent.Apt
{
    public sealed class AptComputerDescentResultFactory : IComputerDescentResultFactory
    {
        public IComputerDescentResultInternal CreateComputerDescentResult(IComputer computer
            , Guid operationUid, ITimeStampProvider timeStampProvider)
        {
            return new AptScanResult(computer, operationUid, timeStampProvider);
        }
    }
}
