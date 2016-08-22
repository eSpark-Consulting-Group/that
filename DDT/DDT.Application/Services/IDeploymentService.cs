
using DDT.Application.Models;
using DDT.Core.Descent;
using System;
using System.Threading.Tasks;

namespace DDT.Application.Services
{
    public interface IDeploymentService
    {
        GetModifiedComputerResultsForOperationEventArgs GetModifiedComputerResultsForOperation(Guid operationUid, DateTime? lastUtcTime);
        void StopDeployment(bool stopInProgressItems);
        Task<DeploymentStartedEventArgs> BeginDeployment(DeploymentItem[] items, string login, string password, TimeSpan deploymentTimeout,
            InstallPackageInfo packageX86, InstallPackageInfo packageX64);
    }
}