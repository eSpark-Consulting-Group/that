using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Apt;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Valkyrie;
using Telerik.JustMock;
using Xunit;
using CloudVerdict = APTAT.Core.Descent.Apt.CloudVerdict;

namespace APTAT.Core.UnitTests
{
    public interface IExecutor<T>
    {
        event EventHandler Done;
    }
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

        [Fact]
        public void ShouldNotRaiseFlsAnalyzeProgressChangedIfAllUnkonwn()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).Occurs(1, "Fls Ports Updated");
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
                        
            ScanOptions scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            DeploymentTarget dt = new DeploymentTarget {Uid = Guid.NewGuid(),IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102",IsOnline = true,SystemKind = SystemKind.Windows8};
            var description = new DevicesSourceDescription {Description = "Test Device"};
           
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            DeploymentTargetsCollection dts2 = new DeploymentTargetsCollection(1);
            dts2.Add(dt);
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2, Arg.AnyString, Arg.AnyString, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory);
            bool flsAnalyzeProgressChangedEventCalled = false;

            EventHandler<FlsRequestEventArgs> handler = (sender, args) =>
            {
                flsAnalyzeProgressChangedEventCalled = true;
            };

            target.FlsAnalyzeProgressChanged += handler;
            
            Guid guid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
            IModule[] modules = GetMockModules();
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => modules);

            target.GetAptScanner(dts2, "", "", description,scanOptions);


            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(guid, scanResult) });
            Assert.False(flsAnalyzeProgressChangedEventCalled, "Should not be called FlsResult");


            Mock.Raise(() => aptScanner.ScanCompleted += null, new ScanCompletedEventArgs() { OperationUid = guid }, null);
        }

        private static IModule[] GetMockModules()
        {
            var module1 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module1.Sha1).Returns("7457a2146e4ffaa326fc32b1eb4473c3bfecbbab");
            Mock.Arrange(() => module1.Cloud).Returns(CloudVerdict.Unknown);

            var module2 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module2.Sha1).Returns("623780deebc4732a67142bbf948fbe4672b7af81");
            Mock.Arrange(() => module2.Cloud).Returns(CloudVerdict.Unknown);

            var module3 = Mock.Create<IModule>(Behavior.Strict);
            Mock.Arrange(() => module3.Sha1).Returns("659b6279cacd43f0311bc2e7bfda91bf3ddcaf3b");
            Mock.Arrange(() => module3.Cloud).Returns(CloudVerdict.Unknown);

            var modules = new[] { module1, module2, module3 };
            return modules;
        }

       
    }
}
