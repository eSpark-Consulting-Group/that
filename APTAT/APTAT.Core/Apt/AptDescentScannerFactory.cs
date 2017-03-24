using System.Collections.Generic;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore;
using static APTAT.Core.Apt.AptScanner;

namespace APTAT.Core.Apt
{
    class AptDescentScannerFactory : IDescentScannerFactory
    {
        IContextFactory _contextFactory;
        public AptDescentScannerFactory(IContextFactory factory)
        {
            _contextFactory = factory;
        }
        public IScanner GetScanner(IEnumerable<IComputer> targets, string login, string password, IDevicesSourceDescription devicesSourceDescription, IScanOptions scanOptions)
        {
            return new AptDescentScanner(targets, login, password, devicesSourceDescription, scanOptions, _contextFactory);
        }
    }
}