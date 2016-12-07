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
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce("Fls Ports Updated");
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            ScanOptions scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            var dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = new DevicesSourceDescription { Description = "Test Device" };

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var dts2 = new DeploymentTargetsCollection(1);
            dts2.Add(dt);
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2, Arg.AnyString, Arg.AnyString, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory);
                        
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => ModuleCollectionFactory.GetMockModules(CloudVerdict.Unknown));
            target.GetAptScanner(dts2, "", "", description, scanOptions);            
            bool flsAnalyzeProgressChangedEventCalled = false;

            EventHandler<FlsRequestEventArgs> handler = (sender, args) =>
            {
                flsAnalyzeProgressChangedEventCalled = true;
            };

            target.FlsAnalyzeProgressChanged += handler;

            Guid guid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(guid, scanResult) });
            Assert.False(flsAnalyzeProgressChangedEventCalled, "Should not be raised event FlsAnalyzeProgressChanged");
            
            Mock.Raise(() => aptScanner.ScanCompleted += null, new ScanCompletedEventArgs() { OperationUid = guid }, null);
        }

        [Fact]
        public void ShouldEnqueueOnlyFailedToAnalyzeFiles()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).Occurs(1, "Fls Ports Updated");
            Mock.Arrange(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(x => x.Any()), Arg.AnyBool)).Returns(new Task(() => { })).MustBeCalled("Enqueue method must be called");
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            ScanOptions scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            DeploymentTarget dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = new DevicesSourceDescription { Description = "Test Device" };

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var dts2 = new DeploymentTargetsCollection(1);
            dts2.Add(dt);

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2, Arg.AnyString, Arg.AnyString, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory);
            

            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            IModule[] modules = ModuleCollectionFactory.GetMixedModules();
            Mock.Arrange(() => scanResult.Files).Returns(() => modules);

            target.GetAptScanner(dts2, "", "", description, scanOptions);

            bool flsAnalyzeProgressChangedEventCalled = false;

            EventHandler<FlsRequestEventArgs> handler = (sender, args) =>
            {
                flsAnalyzeProgressChangedEventCalled = true;
            };

            target.FlsAnalyzeProgressChanged += handler;
            
            Guid guid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(guid, scanResult) });
            Assert.False(flsAnalyzeProgressChangedEventCalled, "Should not be called FlsAnalyzeProgressChanged");
            string[] failedToAnalyzeHashes = modules.Where(m => m.Cloud == CloudVerdict.FailedToAnalyze).Select(m => m.Sha1).ToArray();
            Mock.Assert(() => fls.Enqueue(Arg.IsAny<IEnumerable<string>>(), Arg.Matches<bool>(flag => flag == true)), "Flag should be true");
            Mock.Assert(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(s => s.Except(failedToAnalyzeHashes).Any() == false), Arg.AnyBool), "Should contain failed hashes only");
            Mock.Assert(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(s => s.Count() == modules.Where(m => m.Cloud == CloudVerdict.FailedToAnalyze).Count()), Arg.AnyBool), "Should be equal to count of failed");
            Mock.Raise(() => aptScanner.ScanCompleted += null, new ScanCompletedEventArgs() { OperationUid = guid }, null);

        }

    

        [Fact]
        public void ShouldNotEnqueueNotFailedToAnalyzeFiles()
        {
            IModule[] modules = ModuleCollectionFactory.GetModulesWithoutFailedToAnalyze();

            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce("FLS Ports Updated");
            Mock.Arrange(() => fls.Enqueue(Arg.IsAny<IEnumerable<string>>(), Arg.AnyBool)).Returns(new Task(() => { })).OccursNever("Enqueue method must NOT be called");
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            ScanOptions scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            DeploymentTarget dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = new DevicesSourceDescription { Description = "Test Device" };
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var dts2 = new DeploymentTargetsCollection(1);
            dts2.Add(dt);

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2, Arg.AnyString, Arg.AnyString, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory);



            Guid guid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");

            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => modules);
            target.GetAptScanner(dts2, "", "", description, scanOptions);

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(guid, scanResult) });            
            Mock.Raise(() => aptScanner.ScanCompleted += null, new ScanCompletedEventArgs() { OperationUid = guid }, null);

            Mock.AssertAll(fls);            
        }

        

        [Fact]
        public void UpdateFlsPortsTest()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(32), Arg.Is(15)));
            
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            var scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            var dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = new DevicesSourceDescription { Description = "Test Device" };
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var dts2 = new DeploymentTargetsCollection(1);
            dts2.Add(dt);

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2, Arg.AnyString, Arg.AnyString, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory);
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => null);                                  
            target.UpdateFlsPorts(32, 15);           
        }


        [Fact]
        public void SetFlsPortsToDefaultsOnGetScannerTest()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(53), Arg.Is(80)));

            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            var scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            var dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = new DevicesSourceDescription { Description = "Test Device" };
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var dts2 = new DeploymentTargetsCollection(1);
            dts2.Add(dt);

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2, Arg.AnyString, Arg.AnyString, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory);
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => null);
            target.GetAptScanner(dts2, "", "", description, scanOptions);

        }

    }

   
}
