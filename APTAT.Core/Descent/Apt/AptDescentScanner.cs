using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.Core.Descent.Helpers;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Descent.Apt
{
    public class AptDescentScanner : Scanner, IDisposable
    {
        private readonly string _scanScope;
        private readonly DescentMod _descentMod;
        public AptDescentScanner(ISubSystemCatalog catalog, IEnumerable<DeploymentTarget> targets, string login, string password,
            string scanScope) 
            : base(catalog, targets, login, password)
        {
            _descentMod = new DescentMod(TaskFactory, Tracker);
            _scanScope = scanScope;
        }

        public override IDescentQueryResult ValidateAndGetResult()
        {
            if (Login == null)
                throw new ArgumentException("Login name not specified.", "login");
            if (Password == null)
                throw new ArgumentException("Password not specified.", "password");
            
            var result = new AptScanOperationContext(OnlineTargets, Targets.Length);
            return result;
            //         //DescentResult result = new DescentResult(onlineTargetUidComputerPairs);
            //         var result = new AptScanOperationContext(onlineTargetUidComputerPairs, computers.Length);
        }

        public override DescentEventArgs GetDescentCompletedEventArgs(IDescentQueryResult result)
        {
            var aptResult = (AptScanOperationContext)result;
            int failed, notPerformed, succeeded, total, canceled;
            CalculateOperationStatistics(aptResult.ScanResults.Values, out failed, out notPerformed, out succeeded, out total, out canceled);
            aptResult.TotalTargetsCount = total;
            aptResult.SucceededTargetsCount = succeeded;
            aptResult.FailedTargetsCount = failed;
            aptResult.NotPerformedTargetsCount = notPerformed;
            aptResult.CanceledTargetsCount = canceled;
            return new AptScanCompletedEventArgs() { OperationUid = result.OperationUid, Context = aptResult };
        }

        public override void StartScan(IDescentQueryResult result)
        {
            var scanResults = (AptScanOperationContext)result;
            
            foreach (TargetUidComputerPair computerInternal in OnlineTargets)
            {
                var computer = computerInternal.Computer;
                RemoteHostInfo ri = new RemoteHostInfo
                {
                    HostName = (computer.IPAddress != null) ? computer.IPAddress.ToString() : computer.DnsName,
                    Login = Login,
                    Password = Password,
                    TargetUid = computer.Uid
                };
                AptScanResult scanResult;
                scanResults.ScanResults.TryGetValue(computer.Uid, out scanResult);

                var job = new RemoteInstallerJob(result, Tracker, computer);
                _descentMod.StartAptScan(ri, job, computer.Uid, scanResult, result.OperationUid, _scanScope);
            }
        }
        private static void CalculateOperationStatistics(IEnumerable<AptScanResult> results, out int failed, out int notPerformed, out int succeeded, out int total, out int canceled)
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

        public override void AbortAllThread()
        {
            _descentMod.AbortAllThreads();
        }

        public void Dispose()
        {
            if (_descentMod != null)
            {
                _descentMod.Release();
            }
        }
    }
}
