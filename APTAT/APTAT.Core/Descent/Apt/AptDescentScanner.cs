using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APTAT.Common;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;

namespace APTAT.Core.Descent.Apt
{

    public partial class AptDescentScanner : Scanner
    {        
        private AptDescentMod _descentMod;
        private readonly IScanOptions _scanOptions;
        private IContextFactory _contextFactory;

        public AptDescentScanner(IEnumerable<IComputer> targets, string login, string password,
            IDevicesSourceDescription devicesSourceDescription, IScanOptions scanOptions,
            IContextFactory contextFactory) 
            : base(targets, login, password, devicesSourceDescription)
        {
            _scanOptions = scanOptions;
            _contextFactory = contextFactory;
        }

        public override IDescentQueryResult ValidateAndGetResult()
        {
            var result = _contextFactory.CreateContext(Targets,  DevicesSourceDescription);
            ((AptScanOperationContext)result).ScanMethod = _scanOptions.ScanKind;
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
            TaskFactory.StartNew(StartScanThread, 
                new Tuple<IDescentQueryResult, IChangeTrackingSubSystem>(result, changeTrackingSubSystem), TaskCreationOptions.LongRunning);
            
        }

        private void StartScanThread(object parameters)
        {
            var tuple = (Tuple<IDescentQueryResult, IChangeTrackingSubSystem>) parameters;
            var changeTrackingSubSystem = tuple.Item2;
            var scanResults = (AptScanOperationContext)tuple.Item1;
            _descentMod?.Release();
            _descentMod = new AptDescentMod(TaskFactory, changeTrackingSubSystem);
            FormattedText offlineText = new FormattedText(new ResourceText("null", "This computer is not accessible."));
            foreach (var computer in Targets)
            {
                var scanResult = (AptScanResult)scanResults.GetComputerDescentResultByTargetUid(computer.Uid);
                if (!computer.IsOnline)
                {
                    scanResult.ResultType = ScanResultType.NotPerformed;
                    scanResults.OnDescendOperationEvent(false, computer.Uid, offlineText, offlineText, String.Empty, 0,
                        String.Empty);
                    continue;
                }
                var ri = new RemoteHostInfo(computer.Uid, computer.IPAddress ?? computer.DnsName,
                    Login, Password, computer.IsLocalComputer);
                var job = new RemoteInstallerJob(scanResults, changeTrackingSubSystem, this, computer);
                _descentMod.StartAptScan(ri, job, computer.Uid, scanResult, scanResults.OperationUid, _scanOptions);
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