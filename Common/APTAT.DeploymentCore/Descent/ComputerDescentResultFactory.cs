using System;

namespace APTAT.DeploymentCore.Descent
{
    public sealed class ComputerDescentResultFactory : IComputerDescentResultFactory
    {
        public IComputerDescentResultInternal CreateComputerDescentResult(IComputer computer, Guid operationUid, ITimeStampProvider timeStampProvider)
        {
            return new ComputerDescentResult(computer, operationUid, timeStampProvider);
        }
    }
    public interface IComputerDescentResultFactory
    {
        IComputerDescentResultInternal CreateComputerDescentResult(IComputer computer, Guid operationUid, ITimeStampProvider timeStampProvider);
    }
}