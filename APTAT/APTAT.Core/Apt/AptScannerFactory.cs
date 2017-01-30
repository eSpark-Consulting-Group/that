
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using System.Collections.Generic;

namespace APTAT.Core.Apt
{
    internal class AptScannerFactory : IAptScannerFactory
    {
        public IAptScanner CreateAptScanner(IDescentSubSystem descentSubSystem, 
            IEnumerable<DeploymentTarget> targets, 
            IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {
            return new AptScanner(descentSubSystem, targets, devicesSourceDescription, options);
        }
    }
}
