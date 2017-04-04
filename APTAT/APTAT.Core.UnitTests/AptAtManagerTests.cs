using System;
using System.Collections.Generic;
using System.Linq;
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
using APTAT.Core.Descent;
using APTAT.Core.FLS;
using System.Threading;
using APTAT.Core.Storage;
using APTAT.Core.Reports;
using APTAT.Core.Valkyrie;

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
            var valkyrie = Mock.Create<IValkyrieClientEx>();
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var distinctFileHelper = Mock.Create<IDistinctFileHelper>(Behavior.Strict);
            var storage = Mock.Create<IStorageSubSystem>(Behavior.Strict);                        
            var reports = Mock.Create<IReportBuilder>();
            
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, distinctFileHelper, storage, reports);
            Assert.NotNull(target.ValkyrieAnalysisResult);
            Assert.Null(target.AggregatedScanResults);
            Mock.Assert(valkyrie);
        }

        [Fact]
        public void ConstructorShouldSetUriToValkyrieProduction()
        {

            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.Matches<Uri>(uri => uri.ToString() == @"https://valkyrie.comodo.com/")))
                .OccursOnce();
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var distinctFileHelper = Mock.Create<IDistinctFileHelper>(Behavior.Strict);
            var storage = Mock.Create<IStorageSubSystem>(Behavior.Strict);
            var reports = Mock.Create<IReportBuilder>();

            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, distinctFileHelper, storage, reports);
            
            Mock.Assert(valkyrie);
        }
                

        [Theory]
        [InlineData(CloudVerdict.NoVerdict)]
        [InlineData(CloudVerdict.FailedToAnalyze)]
        public void FlsEnqueueShouldBeCalledIfComputersContainModulesWithError(CloudVerdict cloudVerdict)
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var flsManager = Mock.Create<IFlsManager>(Behavior.Strict);
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(cloudVerdict, 3);
            var sha1WithNextItterationClean = failedToAnalyzeModules.Select(s => s.Sha1);
            var failedToAnalyzeHashes = failedToAnalyzeModules.Select(m => m.Sha1).ToArray();

            Mock.Arrange(() => flsManager.Enqueue(Arg.Matches<IEnumerable<string>>(
                s => s.Compare(failedToAnalyzeHashes))));
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var scanOptions = CreateScanOptions(80, 53);
            Mock.Arrange(() => flsManager.UpdateFlsPorts(scanOptions.UdpPort,
                scanOptions.TcpPort)).OccursOnce();
            var deploymentTargets = GetDeploymentTargets();

            var description = Mock.Create<IDevicesSourceDescription>();

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(Arg.IsAny<IDescentSubSystem>(), deploymentTargets, 
                description, scanOptions)).Returns(() => aptScanner).OccursOnce();

            var aptAtManagerUnderTest = new AptAtManager(Mock.Create<IDescentSubSystem>(Behavior.Strict), Mock.Create<IValkyrieClientEx>(), flsManager, 
                aptScannerFactory, Mock.Create<IDistinctFileHelper>(), 
                Mock.Create<IStorageSubSystem>(), Mock.Create<IReportBuilder>());

            aptAtManagerUnderTest.GetAptScanner(deploymentTargets, description, scanOptions);
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);

            Mock.Arrange(() => scanResult.Files).Returns(() => failedToAnalyzeModules.ToArray()).MustBeCalled();

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[]
            {
                new ComputerScanCompletedEventArgs(Guid.NewGuid(),scanResult)}
            );
            
            
            Mock.Assert(() => flsManager.Enqueue(Arg.Matches<IEnumerable<string>>(
                s => s.Compare(failedToAnalyzeHashes))), Occurs.Once());

            Mock.AssertAll(flsManager);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
        }

        [Theory]
        [InlineData(CloudVerdict.Malware)]
        [InlineData(CloudVerdict.Safe)]
        [InlineData(CloudVerdict.FirstSeen)]
        [InlineData(CloudVerdict.Unknown)]
        public void FlsEnqueueShouldBeCalledOnlyWithErrorsIfComputersContainModulesWithError(CloudVerdict cloudVerdict)
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var flsManager = Mock.Create<IFlsManager>(Behavior.Strict);
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.NoVerdict, 3);
            var anotherModules = ModuleCollectionFactory.GetMockModules(cloudVerdict, 2);
            var sha1WithNextItterationClean = failedToAnalyzeModules.Select(s => s.Sha1);
            var failedToAnalyzeHashes = failedToAnalyzeModules.Select(m => m.Sha1).ToArray();

            Mock.Arrange(() => flsManager.Enqueue(Arg.Matches<IEnumerable<string>>(
                s => s.Compare(failedToAnalyzeHashes))));
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            Mock.Arrange(() => flsManager.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt))
                .OccursOnce();
            var deploymentTargets = GetDeploymentTargets();

            var description = Mock.Create<IDevicesSourceDescription>();

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(Arg.IsAny<IDescentSubSystem>(), deploymentTargets,
                description, Arg.IsAny<IScanOptions>())).Returns(() => aptScanner).OccursOnce();

            var aptAtManagerUnderTest = new AptAtManager(Mock.Create<IDescentSubSystem>(Behavior.Strict), Mock.Create<IValkyrieClientEx>(), flsManager,
                aptScannerFactory, Mock.Create<IDistinctFileHelper>(),
                Mock.Create<IStorageSubSystem>(), Mock.Create<IReportBuilder>());

            aptAtManagerUnderTest.GetAptScanner(deploymentTargets, description, Mock.Create<IScanOptions>());
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);

            Mock.Arrange(() => scanResult.Files).Returns(() => failedToAnalyzeModules.Concat(anotherModules).ToArray()).MustBeCalled();

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[]
            {
                new ComputerScanCompletedEventArgs(Guid.NewGuid(),scanResult)}
            );


            Mock.Assert(() => flsManager.Enqueue(Arg.Matches<IEnumerable<string>>(
                s => s.Compare(failedToAnalyzeHashes))), Occurs.Once());

            Mock.AssertAll(flsManager);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
        }

        [Theory]
        [InlineData(CloudVerdict.Malware)]
        [InlineData(CloudVerdict.Safe)]
        [InlineData(CloudVerdict.FirstSeen)]
        [InlineData(CloudVerdict.Unknown)]
        public void FlsEnqueueShouldNotBeCalledOnlyWithErrorsIfComputersDoesntContainModulesWithError(CloudVerdict cloudVerdict)
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var flsManager = Mock.Create<IFlsManager>(Behavior.Strict);
            var anotherModules = ModuleCollectionFactory.GetMockModules(cloudVerdict, 2);

            Mock.Arrange(() => flsManager.Enqueue(Arg.IsAny<IEnumerable<string>>()));
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var scanOptions = CreateScanOptions(80, 53);
            Mock.Arrange(() => flsManager.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt));
            var deploymentTargets = GetDeploymentTargets();

            var description = Mock.Create<IDevicesSourceDescription>();

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(Arg.IsAny<IDescentSubSystem>(), deploymentTargets,
                Arg.IsAny<IDevicesSourceDescription>(), Arg.IsAny<IScanOptions>())).Returns(() => aptScanner).OccursOnce();

            var aptAtManagerUnderTest = new AptAtManager(Mock.Create<IDescentSubSystem>(Behavior.Strict), Mock.Create<IValkyrieClientEx>(), flsManager,
                aptScannerFactory, Mock.Create<IDistinctFileHelper>(),
                Mock.Create<IStorageSubSystem>(), Mock.Create<IReportBuilder>());

            aptAtManagerUnderTest.GetAptScanner(deploymentTargets, description, Mock.Create<IScanOptions>());
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);

            Mock.Arrange(() => scanResult.Files).Returns(() => anotherModules.ToArray()).MustBeCalled();

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[]
            {
                new ComputerScanCompletedEventArgs(Guid.NewGuid(),scanResult)}
            );
            
            Mock.Assert(() => flsManager.Enqueue(Arg.IsAny<IEnumerable<string>>()), Occurs.Never());

            Mock.Assert(descent);
            Mock.Assert(aptScanner);
            Mock.Assert(aptScannerFactory);
        }


        /// <summary>
        /// Remote Scanner returns Failed to analyze 
        /// Than Fls Manager returns 3 safe result for previously failed files
        /// Espected: 
        /// 1. Must be called Enque with all Failed To Analyze files
        /// 2. Must be raized event FlsAnalyzeProgressChanged with all Safe files from flsManager
        /// </summary>
        [Fact]
        public void FlsEventTest()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var flsManager = Mock.Create<IFlsManager>(Behavior.Strict);
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.FailedToAnalyze, 3);
            var sha1WithNextItterationClean = failedToAnalyzeModules.Select(s=>s.Sha1);
            var failedToAnalyzeHashes = failedToAnalyzeModules.Select(m => m.Sha1).ToArray();
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri)).MustBeCalled(Arg.AnyString);
            Mock.Arrange(() => flsManager.Enqueue(Arg.Matches<IEnumerable<string>>(
                s=>s.Compare(failedToAnalyzeHashes)))).MustBeCalled();            
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            Mock.Arrange(() => flsManager.UpdateFlsPorts(Arg.AnyInt, 
                Arg.AnyInt)).OccursOnce();
            var deploymentTargets = GetDeploymentTargets();
            var description = Mock.Create<IDevicesSourceDescription>();
            var distinctFileHelper = Mock.Create<IDistinctFileHelper>(Behavior.Strict);
            Mock.Arrange(() => distinctFileHelper.ShouldSendToValkyrie(null)).IgnoreArguments().Returns(false);
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, deploymentTargets, description, Arg.IsAny<IScanOptions>()))
                .Returns(() => aptScanner).OccursOnce();
            var storage = Mock.Create<IStorageSubSystem>();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, flsManager, aptScannerFactory, distinctFileHelper, storage, 
                Mock.Create<IReportBuilder>());

            aptAtManagerUnderTest.GetAptScanner(deploymentTargets, description, Mock.Create<IScanOptions>());
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            
            Mock.Arrange(() => scanResult.Files).Returns(() => failedToAnalyzeModules.ToArray()).MustBeCalled();
            bool isRaised = false;
            ManualResetEventSlim ev = new ManualResetEventSlim(); 
            aptAtManagerUnderTest.FlsAnalyzeProgressChanged += (s, e) =>            
            {
                isRaised = true;
                var sha1Array = e.Results.Select(r => r.Sha1).ToArray();
                Assert.True(sha1WithNextItterationClean.Compare(sha1Array));
                ev.Set();        
            };
            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[]
            {
                new ComputerScanCompletedEventArgs(Guid.NewGuid(),scanResult)}
            );

            List<IFlsResult> flsResults = new List<IFlsResult>();
            foreach (var ss in sha1WithNextItterationClean)
            {
                var flsResult = Mock.Create<IFlsResult>();
                Mock.Arrange(() => flsResult.Cloud).Returns(CloudVerdict.Safe);
                Mock.Arrange(() => flsResult.Sha1).Returns(ss);
                flsResults.Add(flsResult);
            }
            
            Mock.Raise(() => flsManager.FlsAnalyzed += null, args: new object[] {
                new FlsRequestEventArgs<IFlsResult>(flsResults.ToArray(), null) });
            ev.Wait(TimeSpan.FromMilliseconds(500));

            Assert.True(isRaised);
            Mock.AssertAll(flsManager);
            Mock.AssertAll(valkyrie);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
        }

        [Fact]
        public void DisposeTest()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.Dispose()).OccursOnce();
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => valkyrie.Dispose()).OccursOnce();
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var reports = Mock.Create<IReportBuilder>();
            var distinctFileHelper = Mock.Create<IDistinctFileHelper>(Behavior.Strict);
            var storage = Mock.Create<IStorageSubSystem>(Behavior.Strict);

            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory,distinctFileHelper,
                storage, reports);

            aptAtManagerUnderTest.Dispose();

            Mock.AssertAll(fls);
            Mock.AssertAll(valkyrie);
        }

        [Fact]
        public void GetValkyrieDetailsLinkTest()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var description = Mock.Create<IDevicesSourceDescription>();
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);

            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => valkyrie.GetValkyrieDetailsLink()).Returns("linkurl").OccursOnce();

            var storage = Mock.Create<IStorageSubSystem>();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>(), storage, Mock.Create<IReportBuilder>());
           
            var link = aptAtManagerUnderTest.GetValkyrieDetailsLink();

            Assert.Equal("linkurl", link);
            Mock.AssertAll(valkyrie);
        }

        [Fact]
        public void GetValkyrieDetailsLinkWithParametersTest()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var description = Mock.Create<IDevicesSourceDescription>();
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);

            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => valkyrie.GetValkyrieDetailsLink("apiKey", "ScanId")).Returns("linkurl2").OccursOnce();

            var storage = Mock.Create<IStorageSubSystem>();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>(), storage, Mock.Create<IReportBuilder>());

            var link = aptAtManagerUnderTest.GetValkyrieDetailsLink("apiKey", "ScanId");

            Assert.Equal("linkurl2", link);
            Mock.AssertAll(valkyrie);
        }


        //[Fact]
        //public void GetScanResultByTargetUid()
        //{
        //    Guid operationGuid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
        //    var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);

        //    var fls = Mock.Create<IFlsManager>(Behavior.Strict);
        //    Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce();
        //    var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
        //    var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

        //    var scanOptions = new ScanOptions("80", "53", ScanKind.Quick, "", "");
        //    var dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
        //    var description = Mock.Create<IDevicesSourceDescription>();

        //    var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
        //    var deploymentTargets = new DeploymentTarget[1];
        //    deploymentTargets[0] = dt;
        //    Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, deploymentTargets, description, scanOptions)).Returns(() => aptScanner);
        //    var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>());
        //    var safeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.Safe);

        //    var operationContext = Mock.Create<IAptScanOperationContext>();
        //    var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
        //    Mock.Arrange(() => scanResult.Files).Returns(() => safeModules.ToArray());

        //    Mock.Arrange(() => operationContext.ScanResults).Returns(() => new[] { scanResult });

        //    Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid)).Returns(() => operationContext).OccursOnce();
        //    aptAtManagerUnderTest.GetAptScanner(deploymentTargets, description, scanOptions);

        //    Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResult) });
        //    Mock.Raise(() => aptScanner.ScanCompleted += null, aptScanner, new ScanCompletedEventArgs() { OperationUid = operationGuid });

        //    var result = aptAtManagerUnderTest.GetScanResultByTargetUid(operationGuid);
        //    Assert.NotNull(result);
        //    Mock.AssertAll(fls);
        //    Mock.AssertAll(valkyrie);
        //    Mock.AssertAll(descent);
        //    Mock.AssertAll(aptScanner);
        //    Mock.AssertAll(aptScannerFactory);

        //}


        //[Fact]
        //public void GetDistinctFileResultsTest()
        //{
        //    Guid operationGuid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
        //    var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);

        //    var fls = Mock.Create<IFlsManager>(Behavior.Strict);
        //    Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce();
        //    var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
        //    var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
        //    Mock.Arrange(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(x => x.Any()), Arg.AnyBool)).MustBeCalled();
        //    ScanOptions scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
        //    var dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
        //    var description = Mock.Create<IDevicesSourceDescription>();

        //    var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
        //    var deploymentTargets = new DeploymentTargetsCollection(1);
        //    deploymentTargets.Add(dt);
        //    Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, deploymentTargets, Arg.AnyString, Arg.AnyString, description, scanOptions)).Returns(() => aptScanner);
        //    var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>());
        //    var safeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.Safe);
        //    var nonDistinctModules = new List<IModule>();
        //    nonDistinctModules.AddRange(safeModules);
        //    nonDistinctModules.AddRange(safeModules.Take(3));

        //    var operationContext = Mock.Create<IAptScanOperationContext>();
        //    var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
        //    Mock.Arrange(() => scanResult.Files).Returns(() => nonDistinctModules.ToArray());
        //    Mock.Arrange(() => scanResult.Computer).Returns(() => dt);
        //    Mock.Arrange(() => scanResult.BadFiles).Returns(() => nonDistinctModules.ToArray());
        //    Mock.Arrange(() => operationContext.ScanResults).Returns(() => new[] { scanResult });

        //    Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid)).Returns(() => operationContext).OccursOnce();
        //    aptAtManagerUnderTest.GetAptScanner(deploymentTargets, "", "", description, scanOptions);

        //    Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResult) });
        //    Mock.Raise(() => aptScanner.ScanCompleted += null, aptScanner, new ScanCompletedEventArgs() { OperationUid = operationGuid });

        //    var hs = new HashSet<string>();
        //    foreach (var file in scanResult.Files)
        //    {
        //        hs.Add(file.Sha1);
        //    }

        //    Assert.Equal(8, scanResult.Files.Count()); //10 not distinct files 2 same collection                       
        //    Assert.Equal(hs.Count(), aptAtManagerUnderTest.GetDistinctFileCount()); // distinct             

        //}

        /// <summary>
        /// AptAtManager Stop should stop related subsistems FLS, Valkyrie and DescentSubStystem by calling corresponding  methods
        /// </summary>
        [Fact]
        public void StopScanTest()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            Mock.Arrange(() => descent.StopAllScans(Arg.AnyInt)).OccursOnce();
            //FLS Manager Arange
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.Cancel()).OccursOnce();
            //Valkyrie Client Arange 
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => valkyrie.AnalyzeAsyncCancel(true)).OccursOnce();
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var storage = Mock.Create<IStorageSubSystem>();

            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>(), storage, Mock.Create<IReportBuilder>());
            aptAtManagerUnderTest.StopScan(new Guid[0]);
            
            Mock.AssertAll(fls);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScannerFactory);
        }


        [Fact]
        public void ValkyrieAnalyzeTest()
        {
            Guid operationGuid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.FailedToAnalyze);

            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var scanContext = Mock.Create<IAptScanOperationContext>();
            //Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid)).Returns(() => new AptScanOperationContext(new List<IComputer>(), 1, new DevicesSourceDescription()));
            //Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid))
            //    .Returns(() => scanContext);

            //FLS Manager Arange
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(53), Arg.Is(80))).OccursOnce();
            Mock.Arrange(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(x => x.Count() == failedToAnalyzeModules.Count()))).OccursOnce();
            Mock.Arrange(() => fls.Cancel()).OccursOnce();

            //Valkyrie Client Arange 
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri)).MustBeCalled(Arg.AnyString);
            Mock.Arrange(() => valkyrie.StartAnalyze(new CamRegistration("", "", ""), "", "", 1, null,"", Arg.AnyGuid)).IgnoreArguments();

            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            Mock.Arrange(() => aptScanner.ScanOperationContext).Returns(() => scanContext);
            var scanOptions = CreateScanOptions(80, 53);
            var deploymentTargets = GetDeploymentTargets();

            var description = Mock.Create<IDevicesSourceDescription>();

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);

            var storage = Mock.Create<IStorageSubSystem>();
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, deploymentTargets, description, scanOptions)).Returns(() => aptScanner).OccursOnce();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>(),
                storage, Mock.Create<IReportBuilder>());

            //Arange ScanResult  (FailedToAnalyze)
            var scanResultFailedToAnalyze = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResultFailedToAnalyze.Files).Returns(() => failedToAnalyzeModules);
            Mock.Arrange(() => scanResultFailedToAnalyze.RecalculateStatistics()).OccursNever();

            //Arange ScanResult  (Safe)
            var scanResultSafe = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResultSafe.Files).Returns(() => ModuleCollectionFactory.GetMockModules(CloudVerdict.Safe)).Occurs(2);

            aptAtManagerUnderTest.GetAptScanner(deploymentTargets, description, scanOptions);

            Mock.Raise(() => aptScanner.ScanStarted += null, args: new object[] { new ScanStartedEventArgs() {Error = null, OperationUid = operationGuid} });
            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResultFailedToAnalyze) });
            Mock.Raise(() => aptScanner.ScanCompleted += null, aptScanner, new ScanCompletedEventArgs() { OperationUid = operationGuid});
            Mock.AssertAll(fls);

            Mock.AssertAll(scanResultFailedToAnalyze);
            
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
            

            Assert.Empty(aptAtManagerUnderTest.ValkyrieAnalysisResult);
            Assert.Equal(Guid.Empty, aptAtManagerUnderTest.ValkyrieOperationUid);

            bool valkyrieAnalyzeEventHanderRaised = false;
            bool valkyrieAnalyzeCompletedEventHanderRaised = false;
            bool valkyrieAnalyzeProgressChanged = false;

            EventHandler onValkyrieAnalyzeStarted = (s, e) =>
            {
                valkyrieAnalyzeEventHanderRaised = true;
            };
            aptAtManagerUnderTest.ValkyrieAnalyzeStarted += onValkyrieAnalyzeStarted;
            EventHandler<AnalyzeCompletedEventArgs> onCompleted = (s, e) =>
            {
                valkyrieAnalyzeCompletedEventHanderRaised = true;
            };
            aptAtManagerUnderTest.ValkyrieAnalyzeCompleted += onCompleted;

            EventHandler<ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> onValkyrieAnalyzeProgressChanged = (s, e) =>
            {
                Assert.Equal(20, e.ProgressInfo.TotalFileCount);
                Assert.Equal(10, e.ProgressInfo.ProcessedFileCount);
                Assert.Equal(50, e.ProgressInfo.Percentage);
                valkyrieAnalyzeProgressChanged = true;
            };

            aptAtManagerUnderTest.ValkyrieAnalyzeProgressChanged += onValkyrieAnalyzeProgressChanged;



            aptAtManagerUnderTest.StartValkyrieAnalysis("user", "test", "licenceNumber", "user", "test", null, "");
            Mock.AssertAll(descent);
            Mock.Raise(() => valkyrie.AnalyzeStarted += null, new ValkyrieAnalyzeStartedEventArgs(String.Empty, String.Empty));
            Mock.Raise(() => valkyrie.AnalyzeProgressChanged += null, new ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult>(10, new ProgressInfo<ValkyrieAnalysisResult>(20, 10, new ValkyrieAnalysisResult[] { })));
            Mock.Raise(() => valkyrie.AnalyzeCompleted += null, new AnalyzeCompletedEventArgs(null, false, null));

            Assert.True(valkyrieAnalyzeEventHanderRaised);
            Assert.True(valkyrieAnalyzeProgressChanged);
            Assert.True(valkyrieAnalyzeCompletedEventHanderRaised);
            Mock.AssertAll(fls);
            Mock.AssertAll(valkyrie);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
        }

        /// <summary>
        /// If first frist computer scan returns Failed To Analyze for all files, and second computer scan returns Safe 
        /// 1. UpdateFlsPort should be called once with tcp 80 udp 53 values for ports
        /// 2. Enqueue should be called once with items count equal ot failed to analyze or NoVerdict
        /// 3. CreateAptScanner should be called once
        /// 4. FlsAnalyzeProgressChanged should be rised 
        /// </summary>
        [Fact]
        public void TestFirstComputerHasFailedToAnalyzeResultsAndSecondComputerReturnsSafeForSameHashes()
        {
            Guid operationGuid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.FailedToAnalyze, 0 , 3);

            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
        //    Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid)).Returns(() => new AptScanOperationContext(new List<IComputer>(), 1, new DevicesSourceDescription()));

            //FLS Manager Arange
            var fls = Mock.Create<IFlsManager>();

            //Valkyrie Client Arange 
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri));
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var scanOptions = CreateScanOptions(80, 53);
            var deploymentTargets = GetDeploymentTargets();

            var description = Mock.Create<IDevicesSourceDescription>(Behavior.Strict);
            Mock.Arrange(() =>  description.Description).Returns("Test Device");

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);

            var storage = Mock.Create<IStorageSubSystem>();

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, deploymentTargets, description, scanOptions))
                .Returns(() => aptScanner).OccursOnce();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, 
                Mock.Create<IDistinctFileHelper>(), storage, Mock.Create<IReportBuilder>());

            //Arange ScanResult  (FailedToAnalyze)
            var scanResultFailedToAnalyze = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResultFailedToAnalyze.Files).Returns(() => failedToAnalyzeModules);
            Mock.Arrange(() => scanResultFailedToAnalyze.RecalculateStatistics()).OccursNever();

            //Arange ScanResult  (Safe)
            var scanResultSafe = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResultSafe.Files).Returns(() => ModuleCollectionFactory.GetMockModules(CloudVerdict.Safe, 0, 5)).Occurs(2);
            Mock.Arrange(() => scanResultSafe.RecalculateStatistics()).OccursNever();

            aptAtManagerUnderTest.GetAptScanner(deploymentTargets, description, scanOptions);
            bool flsAnalyzeProgressChangedEventCalled = false;
            Assert.Equal(Guid.Empty, aptAtManagerUnderTest.FlsLookupOperationUid);
            ManualResetEventSlim ev = new ManualResetEventSlim();

            aptAtManagerUnderTest.FlsAnalyzeProgressChanged += (sender, args) =>
            {
                Assert.Null(args.Exception);
                Assert.True(args.Results.Count() == failedToAnalyzeModules.Count());
                Assert.All(args.Results, (r) => Assert.Equal(CloudVerdict.Safe, r.Cloud));
                flsAnalyzeProgressChangedEventCalled = true;
                ev.Set();
            };                        

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResultFailedToAnalyze) });
            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResultSafe) });            
            ev.Wait(TimeSpan.FromMilliseconds(500));
            Assert.True(flsAnalyzeProgressChangedEventCalled, "Should  be raised event FlsAnalyzeProgressChanged");

            Mock.AssertAll(scanResultFailedToAnalyze);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);


            Assert.Empty(aptAtManagerUnderTest.ValkyrieAnalysisResult);
            Assert.Equal(Guid.Empty, aptAtManagerUnderTest.ValkyrieOperationUid);
        }

        private IComputer[] GetDeploymentTargets()
        {
            var uid1 = Guid.NewGuid();
            var uid2 = Guid.NewGuid();
            var uid3 = Guid.NewGuid();
            var deploymentTarget1 = Mock.Create<IComputer>(Behavior.Strict);
            Mock.Arrange(() => deploymentTarget1.Uid).Returns(uid1);
            Mock.Arrange(() => deploymentTarget1.IPAddress).Returns("10.100.65.1");
            Mock.Arrange(() => deploymentTarget1.IsLocalComputer).Returns(false);
            Mock.Arrange(() => deploymentTarget1.Name).Returns("testComp1");
            Mock.Arrange(() => deploymentTarget1.IsOnline).Returns(true);
            Mock.Arrange(() => deploymentTarget1.IsLocalComputer).Returns(false);
            Mock.Arrange(() => deploymentTarget1.SystemKind).Returns(SystemKind.Windows8);
            var deploymentTarget2 = Mock.Create<IComputer>(Behavior.Strict);
            Mock.Arrange(() => deploymentTarget2.Uid).Returns(uid2);
            Mock.Arrange(() => deploymentTarget2.IPAddress).Returns("10.100.65.2");
            Mock.Arrange(() => deploymentTarget1.IsLocalComputer).Returns(false);
            Mock.Arrange(() => deploymentTarget1.Name).Returns("testComp2");
            Mock.Arrange(() => deploymentTarget1.IsOnline).Returns(true);
            Mock.Arrange(() => deploymentTarget1.IsLocalComputer).Returns(false);
            Mock.Arrange(() => deploymentTarget1.SystemKind).Returns(SystemKind.Windows7);
            var deploymentTargets = new IComputer[] { deploymentTarget1, deploymentTarget2};
            return deploymentTargets;
        }
                
        [Fact]
        public void OnFirstComputerHasResultAndSecondHasFailed_ShouldBeCalledRecalculateStatisticsAndUpdatedVerdict()
        {
            var storage = Mock.Create<IStorageSubSystem>();
            Guid operationGuid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            //Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid)).Returns(() => new AptScanOperationContext(new List<IComputer>(), 1, new DevicesSourceDescription())).OccursOnce();
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce();
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri)).MustBeCalled(Arg.AnyString);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            Mock.Arrange(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(x => x.Any()))).MustBeCalled();
            var scanOptions = CreateScanOptions(80, 53);
            var dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = Mock.Create<IDevicesSourceDescription>();

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var dts2 = new DeploymentTarget[1];
            dts2[0] = dt;
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2, description, scanOptions)).Returns(() => aptScanner);
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>(), storage, Mock.Create<IReportBuilder>());

            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.FailedToAnalyze);
            foreach (var m in failedToAnalyzeModules)
            {
                Mock.ArrangeSet(() => m.Cloud = Arg.Matches<CloudVerdict>(x => x == CloudVerdict.Safe)).OccursOnce();
            }
            Mock.Arrange(() => scanResult.Files).Returns(() => failedToAnalyzeModules);
            Mock.Arrange(() => scanResult.RecalculateStatistics()).OccursOnce();

            var scanResult2 = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult2.Files).Returns(() => ModuleCollectionFactory.GetMockModules(CloudVerdict.Safe));

            aptAtManagerUnderTest.GetAptScanner(dts2, description, scanOptions);
            bool flsAnalyzeProgressChangedEventCalled = false;

            EventHandler<FlsRequestEventArgs<IAptVerdictContext>> handler = (sender, args) =>
            {
                flsAnalyzeProgressChangedEventCalled = true;
            };

            aptAtManagerUnderTest.FlsAnalyzeProgressChanged += handler;
            Assert.Equal(Guid.Empty, aptAtManagerUnderTest.FlsLookupOperationUid);

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResult2) });

            Assert.False(flsAnalyzeProgressChangedEventCalled, "Should not be raised event FlsAnalyzeProgressChanged");
            Mock.Raise(() => aptScanner.ScanCompleted += null, aptScanner, new ScanCompletedEventArgs() { OperationUid = operationGuid });
            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResult) });

        }

        [Fact]
        public void ShouldNotRaiseFlsAnalyzeProgressChangedIfAllUnkonwn()
        {
            var storage = Mock.Create<IStorageSubSystem>();
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce("Fls Ports Updated");
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri)).MustBeCalled(Arg.AnyString);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            var scanOptions = CreateScanOptions(80, 53);
            var description = Mock.Create<IDevicesSourceDescription>();
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var deploymentTargets = GetDeploymentTargets();
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, deploymentTargets, description, scanOptions)).Returns(() => aptScanner);
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>(), storage, Mock.Create<IReportBuilder>());

            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => ModuleCollectionFactory.GetMockModules(CloudVerdict.Unknown));
            aptAtManagerUnderTest.GetAptScanner(deploymentTargets, description, scanOptions);
            bool flsAnalyzeProgressChangedEventCalled = false;

            aptAtManagerUnderTest.FlsAnalyzeProgressChanged += (sender, args) =>
           {
               flsAnalyzeProgressChangedEventCalled = true;
           };

            Guid guid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(guid, scanResult) });
            Assert.False(flsAnalyzeProgressChangedEventCalled, "Should not be raised event FlsAnalyzeProgressChanged");
            Mock.Raise(() => aptScanner.ScanCompleted += null, args: new object[] { new ScanCompletedEventArgs() { OperationUid = guid } });
        }

        [Fact]
        public void ShouldEnqueueOnlyFailedToAnalyzeFiles()
        {
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).Occurs(1, "Fls Ports Updated");
            Mock.Arrange(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(x => x.Any()))).MustBeCalled("Enqueue method must be called");
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri)).MustBeCalled(Arg.AnyString);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            var storage = Mock.Create<IStorageSubSystem>();

            var scanOptions = CreateScanOptions(80, 53);
            DeploymentTarget dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = Mock.Create<IDevicesSourceDescription>();

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var dts2 = new DeploymentTarget[1];
            dts2[0] = dt;

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>(), storage, Mock.Create<IReportBuilder>());

            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            IModule[] modules = ModuleCollectionFactory.GetMixedModules();
            Mock.Arrange(() => scanResult.Files).Returns(() => modules);

            target.GetAptScanner(dts2, description, scanOptions);

            bool flsAnalyzeProgressChangedEventCalled = false;

            EventHandler<FlsRequestEventArgs<IAptVerdictContext>> handler = (sender, args) =>
            {
                flsAnalyzeProgressChangedEventCalled = true;
            };

            target.FlsAnalyzeProgressChanged += handler;

            Guid guid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(guid, scanResult) });
            Assert.False(flsAnalyzeProgressChangedEventCalled, "Should not be called FlsAnalyzeProgressChanged");
            string[] failedToAnalyzeHashes = modules.Where(m => m.Cloud == CloudVerdict.FailedToAnalyze).Select(m => m.Sha1).ToArray();
            Mock.Assert(() => fls.Enqueue(Arg.IsAny<IEnumerable<string>>()), "Flag should be true");
            Mock.Assert(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(s => s.Except(failedToAnalyzeHashes).Any() == false && s.Count() == modules.Where(m => m.Cloud == CloudVerdict.FailedToAnalyze).Count())), "Should contain failed hashes only");

            Mock.Raise(() => aptScanner.ScanCompleted += null, new object[] { new ScanCompletedEventArgs() { OperationUid = guid }});
        }


        [Fact]
        public void ShouldNotEnqueueNotFailedToAnalyzeFiles()
        {
            var storage = Mock.Create<IStorageSubSystem>();
            IModule[] modules = ModuleCollectionFactory.GetModulesWithoutFailedToAnalyze();

            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt));
            Mock.Arrange(() => fls.Enqueue(Arg.IsAny<IEnumerable<string>>())).OccursNever("Enqueue method must NOT be called");
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri)).MustBeCalled(Arg.AnyString);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            var scanOptions = CreateScanOptions(80, 53);
            IComputer dt = Mock.Create<IComputer>(Behavior.Strict);
            Mock.Arrange(() => dt.Uid).Returns(Guid.NewGuid());
            Mock.Arrange(() => dt.IPAddress).Returns("10.100.65.102");
            //new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = Mock.Create<IDevicesSourceDescription>();
            Mock.Arrange(()=>description.Description).Returns("Test Device");
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var dts2 = new[] { dt };

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>(),
                storage, Mock.Create<IReportBuilder>());
            
            Guid guid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");

            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => modules);
            target.GetAptScanner(dts2, description, scanOptions);

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(guid, scanResult) });
            Mock.Raise(() => aptScanner.ScanCompleted += null, args: new object[] { new ScanCompletedEventArgs() { OperationUid = guid } });

            Mock.AssertAll(fls);
        }



        [Fact]
        public void UpdateFlsPortsTest()
        {
            var storage = Mock.Create<IStorageSubSystem>();
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(32), Arg.Is(15)));

            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri)).MustBeCalled(Arg.AnyString);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            var scanOptions = CreateScanOptions(80, 53);
            var dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = Mock.Create<IDevicesSourceDescription>();
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var dts2 = new DeploymentTargetsCollection(1);
            dts2.Add(dt);

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2,  description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>(), storage, Mock.Create<IReportBuilder>());
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => null);
            target.UpdateFlsPorts(32, 15);
        }


        [Fact]
        public void SetFlsPortsToDefaultsOnGetScannerTest()
        {
            var storage = Mock.Create<IStorageSubSystem>();
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(53), Arg.Is(80)));
            var valkyrie = Mock.Create<IValkyrieClientEx>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.SetBaseUri(Arg.AnyUri)).MustBeCalled(Arg.AnyString);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var scanOptions = CreateScanOptions(80, 53);
            var dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = Mock.Create<IDevicesSourceDescription>();
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var dts2 = new DeploymentTarget[1];
            dts2[0] = dt;
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, dts2, description, scanOptions)).Returns(() => aptScanner);

            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, Mock.Create<IDistinctFileHelper>(), storage, Mock.Create<IReportBuilder>());

            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => null);

            target.GetAptScanner(dts2, description, scanOptions);
        }

        private static IScanOptions CreateScanOptions(int tcpPort, int udpPort)
        {
            var scanOptions = Mock.Create<IScanOptions>(Behavior.Strict);
            Mock.Arrange(() => scanOptions.TcpPort).Returns(() => tcpPort);
            Mock.Arrange(() => scanOptions.UdpPort).Returns(() => udpPort);
            return scanOptions;
        }


    }
}
