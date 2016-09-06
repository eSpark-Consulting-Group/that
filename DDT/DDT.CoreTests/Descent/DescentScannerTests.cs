using Xunit;
using DDT.Core.Descent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using Telerik.JustMock;

namespace DDT.Core.Descent.Tests
{
    public class DescentScannerTests
    {
        [Fact()]
        public void GetDescentStatusInformationTest()
        {
            var packageX64 = InstallPackageInfo.CreateMsiPackageInfo("path.msi", null, null, TargetSystemType.x64, false);
            InstallPackageInfo packageX86 = null;
            TimeSpan timeout = default(TimeSpan);
            APTAT.DeploymentCore.Directory.IDevicesSourceDescription description = null;

            DescentScanner scanner = new DescentScanner(new List<DeploymentTarget>(),"login", "pwd", description, timeout, packageX86, packageX64);

            Assert.NotNull(scanner);
          
        }
    }
}