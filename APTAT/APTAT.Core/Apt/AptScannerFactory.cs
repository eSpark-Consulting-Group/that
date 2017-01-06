
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;


namespace APTAT.Core.Apt
{
    internal class AptScannerFactory : IAptScannerFactory
    {
        public IAptScanner CreateAptScanner(IDescentSubSystem descentSubSystem, 
            IDevicesSourceDescription devicesSourceDescription, ScanOptions options)
        {
            return new AptScanner(descentSubSystem, devicesSourceDescription, options);
        }
    }
}
