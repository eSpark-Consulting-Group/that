using System.Collections.Generic;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Apt
{
    public interface IAptScannerFactory
    {
        IAptScanner CreateAptScanner(IDescentSubSystem descentSubSystem, 
            IDevicesSourceDescription devicesSourceDescription, ScanOptions options);
    }
}