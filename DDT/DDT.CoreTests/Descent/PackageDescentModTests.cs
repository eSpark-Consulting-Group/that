using Xunit;
using DDT.Core.Descent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.DeploymentCore.Descent.Helpers;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Descent;
using Telerik.JustMock;
using APTAT.Common;

namespace DDT.Core.Descent.Tests
{
    public class PackageDescentModTests
    {
        public void BuildDescentModTest()
        {

            TaskFactory tf = new TaskFactory();
            var changeTracking = new ChangeTrackingSubSystem();
            PackageDescentMod descentMod = new PackageDescentMod(tf, changeTracking);
            Assert.NotNull(descentMod);
        }

        //[Fact()]
        //public void StartDeploymentTest()
        //{
        //    TaskFactory tf = new TaskFactory();
        //    var changeTracking = new ChangeTrackingSubSystem();
        //    PackageDescentMod descentMod = new PackageDescentMod(tf, changeTracking);
        //    var id = Guid.NewGuid();
        //    var operationId = Guid.NewGuid();
        //    var hostInfo = new RemoteHostInfo(id, "10.100.65.102", "user","test", false );
            
        //    var package1 = InstallPackageInfo.CreateMsiPackageInfo("7zip.msi", "", "", TargetSystemType.x86, false);
        //    var package2 = InstallPackageInfo.CreateMsiPackageInfo("7zip64.msi", "", "", TargetSystemType.x64, false);
        //    APTAT.DeploymentCore.IComputer computer = new DeploymentTarget() { Uid = Guid.NewGuid() };
        //    var result = Mock.Create<IDescentQueryResult>();
        //    Action<TimeSpan> action = (ts) => { };
        //    Mock.Arrange(() =>  result.RegisterTimeoutCallback(Guid.Empty, action)).IgnoreArguments();
         
        //    IScanner scanner = new DescentScanner(new List<DeploymentTarget>(),"user","test", null, TimeSpan.FromMinutes(3), package1, package2);
        //    var job = new RemoteInstallerJob(result, changeTracking, scanner, computer);
            
        //        descentMod.StartDeployment(
        //        hostInfo, job,
        //        id,
        //        operationId,
        //        TimeSpan.FromMinutes(3),
        //        package1,
        //        package2);
        //}
    }
}