using System.Collections.Generic;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore;

namespace APTAT.Core.Apt
{
    public interface IDescentScannerFactory
    {
        IScanner GetScanner(IEnumerable<IComputer> targets, string login, string password,
        IDevicesSourceDescription devicesSourceDescription, IScanOptions scanOptions);
    }
}