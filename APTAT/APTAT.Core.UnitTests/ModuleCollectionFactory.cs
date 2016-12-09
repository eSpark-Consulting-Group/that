using APTAT.Core.Descent.Apt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.JustMock;

namespace APTAT.Core.UnitTests
{
    public static class ModuleCollectionFactory
    {
        public static IModule[] GetMixedModules()
        {
            var module1 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module1.Sha1).Returns("7457a2146e4ffaa326fc32b1eb4473c3bfecbbab");
            Mock.Arrange(() => module1.Cloud).Returns(CloudVerdict.Unknown);

            var module2 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module2.Sha1).Returns("623780deebc4732a67142bbf948fbe4672b7af81");
            Mock.Arrange(() => module2.Cloud).Returns(CloudVerdict.FailedToAnalyze);

            var module3 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module3.Sha1).Returns("659b6279cacd43f0311bc2e7bfda91bf3ddcaf3b");
            Mock.Arrange(() => module3.Cloud).Returns(CloudVerdict.FirstSeen);

            var module4 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module4.Sha1).Returns("113799e6123a2377bb330ed20bda108f60e95f93");
            Mock.Arrange(() => module4.Cloud).Returns(CloudVerdict.FailedToAnalyze);

            var module5 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module5.Sha1).Returns("0e3d32035931f027173227ee57da531fd5354cf9");
            Mock.Arrange(() => module5.Cloud).Returns(CloudVerdict.FailedToAnalyze);

            var modules = new[] { module1, module2, module3, module4, module5 };
            return modules;
        }

        public static IModule[] GetModulesWithoutFailedToAnalyze()
        {
            var module1 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module1.Sha1).Returns("7457a2146e4ffaa326fc32b1eb4473c3bfecbbab");
            Mock.Arrange(() => module1.Cloud).Returns(CloudVerdict.Unknown);

            var module2 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module2.Sha1).Returns("623780deebc4732a67142bbf948fbe4672b7af81");
            Mock.Arrange(() => module2.Cloud).Returns(CloudVerdict.Malware);

            var module3 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module3.Sha1).Returns("659b6279cacd43f0311bc2e7bfda91bf3ddcaf3b");
            Mock.Arrange(() => module3.Cloud).Returns(CloudVerdict.Unknown);

            var module4 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module4.Sha1).Returns("113799e6123a2377bb330ed20bda108f60e95f93");
            Mock.Arrange(() => module4.Cloud).Returns(CloudVerdict.FirstSeen);

            var module5 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module5.Sha1).Returns("0e3d32035931f027173227ee57da531fd5354cf9");
            Mock.Arrange(() => module5.Cloud).Returns(CloudVerdict.FirstSeen);

            var modules = new[] { module1, module2, module3, module4, module5 };
            return modules;
        }

        public static IModule[] GetMockModules(CloudVerdict verdict)
        {
            var module1 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module1.Sha1).Returns("7457a2146e4ffaa326fc32b1eb4473c3bfecbbab");
            Mock.Arrange(() => module1.Cloud).Returns(verdict);

            var module2 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module2.Sha1).Returns("623780deebc4732a67142bbf948fbe4672b7af81");
            Mock.Arrange(() => module2.Cloud).Returns(verdict);

            var module3 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module3.Sha1).Returns("659b6279cacd43f0311bc2e7bfda91bf3ddcaf3b");
            Mock.Arrange(() => module3.Cloud).Returns(verdict);

            var module4 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module4.Sha1).Returns("113799e6123a2377bb330ed20bda108f60e95f93");
            Mock.Arrange(() => module4.Cloud).Returns(verdict);

            var module5 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module5.Sha1).Returns("0e3d32035931f027173227ee57da531fd5354cf9");
            Mock.Arrange(() => module5.Cloud).Returns(verdict);

            var modules = new[] { module1, module2, module3 };
            return modules;
        }

    }
}
