using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Apt
{
    internal class AptScannerFactory : IAptScannerFactory
    {
        public IAptScanner CreateAptScanner(IDescentSubSystem descentSubSystem, IEnumerable<DeploymentTarget> targets,
            string login, string password, IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {
            return new AptScanner(descentSubSystem, targets, login, password, devicesSourceDescription, options);
        }
    }
}
