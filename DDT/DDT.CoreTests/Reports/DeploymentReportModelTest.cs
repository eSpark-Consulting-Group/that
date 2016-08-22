using DDT.Core.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.JustMock;
using Xunit;

namespace DDT.CoreTests.Reports
{
    public class DeploymentReportModelTest
    {
        [Fact()]
        public void TestEndpointCountByOS()
        {
            var list = GetDetailsOfScan();
            var model = new DeploymentReportModel(list);
            var endpointsByOs = model.WindowsEndpoints + model.LinuxEndpoints + model.MacEndpoints + model.OthersDevices;

            Assert.Equal(endpointsByOs, model.FoundDevices);

            var endpointsByStatus = model.FailedDeployments + model.SkippedDeployments + model.StoppedDeployments + model.SkippedDeployments;

        }

        [Fact()]
        public void TestEndpointCountByStatus()
        {
            var list = GetDetailsOfScan();
            var model = new DeploymentReportModel(list);
            var endpointsByStatus = model.FailedDeployments + model.SkippedDeployments + model.StoppedDeployments + model.SuccessfulDeployments;

            Assert.Equal(endpointsByStatus, model.FoundDevices);            

        }

        [Fact()]
        public void TestCalculation()
        {
            var list = GetDetailsOfScan();
            var model = new DeploymentReportModel(list);

            Assert.Equal(0, model.StoppedDeployments);
            Assert.Equal(2, model.SuccessfulDeployments);
            Assert.Equal(1, model.SkippedDeployments);
            Assert.Equal(2, model.FailedDeployments);
            Assert.Equal(1, model.WindowsEndpoints);
            Assert.Equal(1, model.LinuxEndpoints);
            Assert.Equal(3, model.MacEndpoints);
            Assert.Equal(0, model.OthersDevices);                        
        }

        private static List<IDetailsOfScan> GetDetailsOfScan()
        {
            var details1 = Mock.Create<IDetailsOfScan>(Behavior.Strict);
            Mock.Arrange(() => details1.DeploymentStatus).Returns(DeploymentStatus.Failed);                        
            Mock.Arrange(() => details1.SystemFamilyKind).Returns(APTAT.DeploymentCore.SystemFamilyKind.Windows);

            var details2 = Mock.Create<IDetailsOfScan>(Behavior.Strict);
            Mock.Arrange(() => details2.DeploymentStatus).Returns(DeploymentStatus.Failed);
            Mock.Arrange(() => details2.SystemFamilyKind).Returns(APTAT.DeploymentCore.SystemFamilyKind.Linux);

            var details3 = Mock.Create<IDetailsOfScan>(Behavior.Strict);            
            Mock.Arrange(() => details3.SystemFamilyKind).Returns(APTAT.DeploymentCore.SystemFamilyKind.MacOS);
            Mock.Arrange(() => details3.DeploymentStatus).Returns(DeploymentStatus.Succeded);

            var details4 = Mock.Create<IDetailsOfScan>(Behavior.Strict);
            Mock.Arrange(() => details4.DeploymentStatus).Returns(DeploymentStatus.Skipped);
            Mock.Arrange(() => details4.SystemFamilyKind).Returns(APTAT.DeploymentCore.SystemFamilyKind.MacOS);


            var details5 = Mock.Create<IDetailsOfScan>(Behavior.Strict);
            Mock.Arrange(() => details5.DeploymentStatus).Returns(DeploymentStatus.Succeded);
            Mock.Arrange(() => details5.SystemFamilyKind).Returns(APTAT.DeploymentCore.SystemFamilyKind.MacOS);


            List<IDetailsOfScan> list = new List<IDetailsOfScan>();
            list.AddRange(new []{ details1,details2, details3, details4, details5});

            return list;
            
        }

    }
}
