using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Apt;
using APTAT.DeploymentCore.Descent;
using APTAT.Valkyrie;
using Telerik.JustMock;
using Xunit;

namespace APTAT.Core.UnitTests
{
    public class AptAtManagerTests
    {
        public AptAtManagerTests()
        {
             
        }

        [Fact]
        public void ConstructorTest()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory);
            Assert.True(target.ValkyrieAnalysisResult.IsEmpty);
            Assert.Null(target.LastAptScanOperationContext);
        }
    }
}
