using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using System;

namespace APTAT.Core.Descent.Copy
{
    public sealed class CopyComputerDescentResultFactory : IComputerDescentResultFactory
    {
        public IComputerDescentResultInternal CreateComputerDescentResult(IComputer computer
            , Guid operationUid, ITimeStampProvider timeStampProvider)
        {
            return new CopyResult(computer, operationUid, timeStampProvider);
        }
    }
}
