using System;
using System.ComponentModel;
using System.Linq;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using DDT.Core.Descent;
using DDT.Core;
using DDT.Application.Models;
using DDT.Core.Helper;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore;

namespace DDT.Application.Services
{
    public class DeploymentService : IDeploymentService
    {
        private readonly IDescentSubSystem _descent;
        public DeploymentService(IDescentSubSystem descent)
        {
            _descent = descent;
        }

        public async Task<DeploymentStartedEventArgs> BeginDeployment(DeploymentItem[] items, String login, 
            String password, TimeSpan deploymentTimeOut,InstallPackageInfo packageX86, InstallPackageInfo packageX64)
        {
            DeploymentTarget[] targets = items.ToBusiness();
            var scanner = new DescentScanner(targets, login, password, 
                null, deploymentTimeOut, packageX86, packageX64);
            Guid operationUid = Guid.Empty;

            await Task.Run(() => { _descent.StartScan(scanner, out operationUid); });
            return new DeploymentStartedEventArgs(operationUid);            
        }

        public GetModifiedComputerResultsForOperationEventArgs GetModifiedComputerResultsForOperation(Guid operationUid, DateTime? lastUtcTime)
        {
            IDescentQueryResult result = _descent.GetDescentResultsByOperationUid(operationUid);
            bool completed = result.Completed;
            var mappedResult = result.ComputerDescentResults.Where(c => lastUtcTime == null || c.Modified > lastUtcTime)
                .Select(ResultMapper.MapFromIComputerDescentResult).ToArray();
            return new GetModifiedComputerResultsForOperationEventArgs(mappedResult, completed);
        }

        public void StopDeployment(bool stopInProgressItems)
        {
            _descent.AbortAllThreads(stopInProgressItems ? DescentScanner.CancelAllITems : DescentScanner.CancelItemsInQueue);
        }

        [Obsolete]
        public event EventHandler<GetResultForOperationEventArgs> GetResultForOperationCompleted;

        [Obsolete]
        public event EventHandler<GetResultForTargetEventArgs> GetResultForTargetCompleted;

        [Obsolete]
        public void GetResultForTarget(Guid operationUid, Guid targetUid)
        {
            IDescentQueryResult operationResult = _descent.GetDescentResultsByOperationUid(operationUid);
            var computerResult = operationResult.GetComputerDescentResultByTargetUid(targetUid);
            ComputerDescentResultModel mappedResult = ResultMapper.MapFromIComputerDescentResult(computerResult);
            GetResultForTargetCompleted?.Invoke(this, new GetResultForTargetEventArgs(mappedResult));
        }

        [Obsolete]
        public void GetResultForOperation(Guid operationUid)
        {
            IDescentQueryResult result = _descent.GetDescentResultsByOperationUid(operationUid);
            DescentResultModel mappedResult = ResultMapper.MapFromDescentResult(result);
            GetResultForOperationCompleted?.Invoke(this, new GetResultForOperationEventArgs(mappedResult));
        }

        [Obsolete]
        public bool IsNmapInstalled(out string errorMesage)
        {
            CheckNmapInstallation nmapHelper = new CheckNmapInstallation();            
            var isInstalled = nmapHelper.Check(out errorMesage);
            return isInstalled;
        }
    }
}