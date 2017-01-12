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
            : base(targets, login, password, devicesSourceDescription)
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
            aptResult.CalculateOperationStatistics();
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