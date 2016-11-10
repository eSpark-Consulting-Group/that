using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Common;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Descent.Apt
{
    public class AptDescentScanner : Scanner
    {        
        private AptDescentMod _descentMod;
        private ScanOptions _scanOptions;

        public AptDescentScanner(IEnumerable<DeploymentTarget> targets, string login, string password,
            IDevicesSourceDescription devicesSourceDescription, ScanOptions scanOptions) 
            : base(targets, login, password,devicesSourceDescription)
        {
            _scanOptions = scanOptions;
        }

        public override IDescentQueryResult ValidateAndGetResult()
        {
            var result = new AptScanOperationContext(OnlineTargets, Targets.Length, DevicesSourceDescription);
            return result;
          }

        public override DescentEventArgs GetDescentCompletedEventArgs(IDescentQueryResult result)
        {
            var aptResult = (AptScanOperationContext)result;
            int failed, notPerformed, succeeded, total, canceled;
            CalculateOperationStatistics(aptResult.ScanResults, out failed, out notPerformed, out succeeded, out total, out canceled);
            aptResult.SucceededTargetsCount = succeeded;
            aptResult.FailedTargetsCount = failed;
            aptResult.NotPerformedTargetsCount = aptResult.TotalTargetsCount - total;
            aptResult.CanceledTargetsCount = canceled;
            return new AptScanCompletedEventArgs() { OperationUid = result.OperationUid, Context = aptResult };
        }

        public override void StopScan(Guid targetUid)
        {
            _descentMod.StopScan(targetUid);
        }

        public override void StartScan(IChangeTrackingSubSystem changeTrackingSubSystem, IDescentQueryResult result)
        {
            var scanResults = (AptScanOperationContext)result;
            if (_descentMod != null)
            {
                _descentMod.Release();
            }
            _descentMod = new AptDescentMod(TaskFactory, changeTrackingSubSystem);
            foreach (var computer in OnlineTargets)
            {
                var ri = new RemoteHostInfo(computer.Uid,
                    (computer.IPAddress != null) ? computer.IPAddress.ToString() : computer.DnsName,
                    Login, Password, computer.IsLocalComputer);
                var scanResult = (AptScanResult)scanResults.GetComputerDescentResultByTargetUid(computer.Uid);
                var job = new RemoteInstallerJob(result, changeTrackingSubSystem, this, computer);
                _descentMod.StartAptScan(ri, job, computer.Uid, scanResult, result.OperationUid, _scanOptions);
            }
        }
        private static void CalculateOperationStatistics(IEnumerable<IAptScanResult> results, out int failed, 
            out int notPerformed, out int succeeded, out int total, out int canceled)
        {
            notPerformed = 0;
            succeeded = 0;
            failed = 0;
            canceled = 0;

            var scanResults = results as AptScanResult[] ?? results.ToArray();
            total = scanResults.Count();
            var groupped = from result in scanResults group result by result.ResultType into temp select temp;
            foreach (var item in groupped)
            {
                if (item.Key == ScanResultType.NotPerformed)
                    notPerformed = item.Count();
                else if (item.Key == ScanResultType.Failed)
                    failed = item.Count();
                else if (item.Key == ScanResultType.Completed)
                    succeeded = item.Count();
                else if (item.Key == ScanResultType.Canceled)
                    canceled = item.Count();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_descentMod != null)
            {
                _descentMod.Release();
                _descentMod = null;
            }
        }
    }
}