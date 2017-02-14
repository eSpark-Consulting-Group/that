
using DDT.Application.Models;
using DDT.Core.Descent;
using System;


namespace DDT.Application.Services
{
    public interface IDeploymentService
    {
        GetModifiedComputerResultsForOperationEventArgs GetModifiedComputerResultsForOperation(Guid operationUid, TimeSpan? lastUtcTime);
        void StopDeployment(bool stopInProgressItems);
        DeploymentStartedEventArgs BeginDeployment(DeploymentItem[] items, string login, string password, TimeSpan deploymentTimeout,
            InstallPackageInfo packageX86, InstallPackageInfo packageX64);
    }
}