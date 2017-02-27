using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore;
using System.Collections.Generic;
using System.Linq;
using Telerik.JustMock;
using Xunit;

namespace APTAT.Core.UnitTests
{
    class DistinctFileHelperTest
    {
        public DistinctFileHelperTest()
        {

        }

        [Fact]
        public void TestDistictFiltering()
        {
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            List<IModule> duplicatedSha1Collection = new List<IModule>();

            duplicatedSha1Collection.AddRange(ModuleCollectionFactory.GetMockModules(CloudVerdict.Unknown));
            duplicatedSha1Collection.AddRange(ModuleCollectionFactory.GetMockModules(CloudVerdict.Unknown));
            duplicatedSha1Collection.AddRange(ModuleCollectionFactory.GetMockModules(CloudVerdict.FirstSeen));
            duplicatedSha1Collection.AddRange(ModuleCollectionFactory.GetMockModules(CloudVerdict.FirstSeen));


            //Mock.Arrange(() => scanResult.Files).Returns(() => modules.ToArray());
            Mock.Arrange(() => scanResult.UnknownFiles).Returns(() => duplicatedSha1Collection.ToArray());

            var computer1 = Mock.Create<IComputer>();
            Mock.Arrange(() => computer1.IPAddress).Returns(() => "10.100.65.102");
            Mock.Arrange(() => scanResult.Computer).Returns(() => computer1);
            Mock.Arrange(() => scanResult.Files).Returns(() => new IModule[] { });

            var scanResult2 = Mock.Create<IAptScanResult>(Behavior.Strict);
            var modules2 = ModuleCollectionFactory.GetMockModules(CloudVerdict.FirstSeen);
            Mock.Arrange(() => scanResult2.UnknownFiles).Returns(() => modules2.ToArray());
            Mock.Arrange(() => scanResult2.Files).Returns(() => modules2.ToArray());

            var computer2 = Mock.Create<IComputer>();
            Mock.Arrange(() => computer2.IPAddress).Returns(() => "10.100.65.105");
            Mock.Arrange(() => scanResult2.Computer).Returns(() => computer2);

            DistinctFileHelper helper = new DistinctFileHelper();
            var result = helper.GetDistinctFileInfo(new[] { scanResult, scanResult2 });
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(5, result.Count);
        }
    }
}
