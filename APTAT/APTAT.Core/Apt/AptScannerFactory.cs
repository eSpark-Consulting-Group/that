
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using System.Collections.Generic;
using APTAT.DeploymentCore;

namespace APTAT.Core.Apt
{
    internal class AptScannerFactory : IAptScannerFactory
    {
        public IAptScanner CreateAptScanner(IDescentSubSystem descentSubSystem, 
            IEnumerable<IComputer> targets, 
            IDevicesSourceDescription devicesSourceDescription, IScanOptions options)
        {
            return new AptScanner(descentSubSystem, targets, devicesSourceDescription, options);
        }
    }
}
