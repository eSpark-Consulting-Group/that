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
using APTAT.Core.Descent;
using Telerik.JustMock.Helpers;

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
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());
            Assert.True(target.ValkyrieAnalysisResult.IsEmpty);
            Assert.Null(target.LastAptScanOperationContext);
        }

        [Fact]
        public void FlsEventTest()
        {
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.FailedToAnalyze);
            var scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(Int32.Parse(scanOptions.UdpPort)), Arg.Is(Int32.Parse(scanOptions.TcpPort)))).OccursOnce();

            var description = new DevicesSourceDescription();
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner).OccursOnce();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());
            bool rescan;
            aptAtManagerUnderTest.GetAptScanner(description, scanOptions, out rescan);
            Mock.Raise(() => fls.FlsAnalyzed+= null, args: new object[] 
            {
                new FlsRequestEventArgs(new IFlsResult[] {new FlsResult {Sha1="", Cloud=CloudVerdict.Unknown } }, null)}
            );

            

            var uid = aptAtManagerUnderTest.FlsLookupOperationUid;

            Mock.AssertAll(fls);
            Mock.AssertAll(valkyrie);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
        }

        [Fact]
        public void GetAptReportBuilderTest()
        {
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.FailedToAnalyze);
            var scanOptions = new ScanOptions("111", "444", ScanKind.Quick);
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);            
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(Int32.Parse(scanOptions.UdpPort)), Arg.Is(Int32.Parse(scanOptions.TcpPort)))).OccursOnce();

            var description = new DevicesSourceDescription();
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner).OccursOnce();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());
            bool rescan;
            aptAtManagerUnderTest.GetAptScanner(description, scanOptions, out rescan);
            var reportBuilder = aptAtManagerUnderTest.GetAptReportBuilder();
            
            Assert.NotNull(reportBuilder);
            Mock.AssertAll(fls);
            Mock.AssertAll(valkyrie);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
        }

        


        [Fact]
        public void DisposeTest()
        {

            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.FailedToAnalyze);
            var scanOptions = new ScanOptions("111", "444", ScanKind.Quick);
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);


            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(Int32.Parse(scanOptions.UdpPort)), Arg.Is(Int32.Parse(scanOptions.TcpPort)))).OccursOnce();            
            Mock.Arrange(() => valkyrie.Dispose()).OccursOnce();


            var description = new DevicesSourceDescription();
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner).OccursOnce();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());
            bool rescan;
            aptAtManagerUnderTest.GetAptScanner(description, scanOptions, out rescan);
            aptAtManagerUnderTest.Dispose();

            Mock.AssertAll(fls);
            Mock.AssertAll(valkyrie);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
        }                

        [Fact]
        public void GetValkyrieDetailsLinkTest()
        {
           
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.FailedToAnalyze);
            var scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);                  
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            var description = new DevicesSourceDescription();
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);

            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(Int32.Parse(scanOptions.UdpPort)), Arg.Is(Int32.Parse(scanOptions.TcpPort)))).OccursOnce();            
            Mock.Arrange(() => valkyrie.GetValkyrieDetailsLink()).Returns("linkurl").OccursOnce();
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner).OccursOnce();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory,  new DistinctFileHelper());
            bool rescan;
            aptAtManagerUnderTest.GetAptScanner(description, scanOptions, out rescan);

            var link = aptAtManagerUnderTest.GetValkyrieDetailsLink();
            Assert.Equal("linkurl", link);
            Mock.AssertAll(fls);
            Mock.AssertAll(valkyrie);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
        }


        /*[Fact]
        public void GetScanResultByTargetUid()
        {
            Guid operationGuid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);

            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce();
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            
            var scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            var dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = new DevicesSourceDescription { Description = "Test Device" };

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var deploymentTargets = new DeploymentTargetsCollection(1);
            deploymentTargets.Add(dt);
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, deploymentTargets, Arg.AnyString, Arg.AnyString, description, scanOptions)).Returns(() => aptScanner);
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());
            var safeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.Safe);
                        
            var operationContext = Mock.Create<IAptScanOperationContext>();
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => safeModules.ToArray());
            
            Mock.Arrange(() => operationContext.ScanResults).Returns(() => new[] { scanResult });

            Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid)).Returns(() => operationContext).OccursOnce();
            aptAtManagerUnderTest.GetAptScanner(deploymentTargets, "", "", description, scanOptions);

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResult) });
            Mock.Raise(() => aptScanner.ScanCompleted += null, aptScanner, new ScanCompletedEventArgs() { OperationUid = operationGuid });

            var result = aptAtManagerUnderTest.GetScanResultByTargetUid(operationGuid);
            Assert.NotNull(result);
            Mock.AssertAll(fls);
            Mock.AssertAll(valkyrie);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);

        }*/


        [Fact]
        public void GetDistinctFileResultsTest()
        {
            Guid operationGuid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce();
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            Mock.Arrange(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(x => x.Any()), Arg.AnyBool)).MustBeCalled();
            ScanOptions scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            var dt = new DeploymentTarget { Uid = Guid.NewGuid(), IPAddress = "10.100.65.102", IsLocalComputer = false, Name = "testComp102", IsOnline = true, SystemKind = SystemKind.Windows8 };
            var description = new DevicesSourceDescription { Description = "Test Device" };

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            var deploymentTargets = new DeploymentTargetsCollection(1);
            deploymentTargets.Add(dt);
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner);
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());
            var safeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.Safe);
            var nonDistinctModules = new List<IModule>();
            nonDistinctModules.AddRange(safeModules);
            nonDistinctModules.AddRange(safeModules.Take(3));

            var operationContext = Mock.Create<IAptScanOperationContext>();                        
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => nonDistinctModules.ToArray());
            Mock.Arrange(() => scanResult.Computer).Returns(() => dt);
            Mock.Arrange(() => scanResult.BadFiles).Returns(() => nonDistinctModules.ToArray());
            Mock.Arrange(() => operationContext.ScanResults).Returns(() => new[] { scanResult });            
            
            Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid)).Returns(() => operationContext).OccursOnce();
            bool rescan;
            aptAtManagerUnderTest.GetAptScanner(description, scanOptions, out rescan);

            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResult) });            
            Mock.Raise(() => aptScanner.ScanCompleted += null, aptScanner, new ScanCompletedEventArgs() { OperationUid = operationGuid });

            var hs = new HashSet<string>();
            foreach (var file in scanResult.Files)
            {
                hs.Add(file.Sha1);
            }

            Assert.Equal(8, scanResult.Files.Count()); //10 not distinct files 2 same collection                       
            Assert.Equal(hs.Count(), aptAtManagerUnderTest.GetDistinctFileCount()); // distinct             
            
        }

        /// <summary>
        /// AptAtManager Stop should stop related subsistems FLS, Valkyrie and DescentSubStystem by calling corresponding  methods
        /// </summary>
        [Fact]
        public void StopScanTest()
        {
            Guid operationGuid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.FailedToAnalyze);
            var scanOptions = new ScanOptions("111", "444", ScanKind.Quick);
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            Mock.Arrange(() => descent.StopAllScans(Arg.AnyInt)).OccursOnce();
            //FLS Manager Arange
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(Int32.Parse(scanOptions.UdpPort)), Arg.Is(Int32.Parse(scanOptions.TcpPort)))).OccursOnce();
            Mock.Arrange(() => fls.Cancel()).OccursOnce();

            //Valkyrie Client Arange 
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.AnalyzeAsyncCancel()).OccursOnce();
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);


            var description = new DevicesSourceDescription();

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);


            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner).OccursOnce();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());

            bool rescan;
            aptAtManagerUnderTest.GetAptScanner(description, scanOptions, out rescan);

            aptAtManagerUnderTest.StopScan();
            Mock.AssertAll(fls);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
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
            Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid)).Returns(() => scanContext);
            //FLS Manager Arange
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(53), Arg.Is(80))).OccursOnce();
            Mock.Arrange(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(x => x.Count() == failedToAnalyzeModules.Count()), Arg.AnyBool)).OccursOnce();

            //Valkyrie Client Arange 
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            Mock.Arrange(() => valkyrie.StartAnalyze(new CamRegistration("","",""), new List<ValkyrieFileInfo>(), "", "", 1, null)).IgnoreArguments();

            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            var scanOptions = new ScanOptions("80", "53", ScanKind.Quick);


            var description = new DevicesSourceDescription { Description = "Test Device" };

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);

            
            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner).OccursOnce();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());

            //Arange ScanResult  (FailedToAnalyze)
            var scanResultFailedToAnalyze = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResultFailedToAnalyze.Files).Returns(() => failedToAnalyzeModules);
            Mock.Arrange(() => scanResultFailedToAnalyze.RecalculateStatistics()).OccursNever();

            //Arange ScanResult  (Safe)
            var scanResultSafe = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResultSafe.Files).Returns(() => ModuleCollectionFactory.GetMockModules(CloudVerdict.Safe)).Occurs(2);

            bool rescan;
            aptAtManagerUnderTest.GetAptScanner(description, scanOptions, out rescan);


            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResultFailedToAnalyze) });
            Mock.Raise(() => aptScanner.ScanCompleted += null, aptScanner, new ScanCompletedEventArgs() { OperationUid = operationGuid });
            Mock.AssertAll(fls);

            Mock.AssertAll(scanResultFailedToAnalyze);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
            foreach (var item in failedToAnalyzeModules)
            {
                Mock.AssertAll(item);
            }
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
            AnalyzeCompletedEventHandler<FileResult> onCompleted = (s,e)=> 
            {
                valkyrieAnalyzeCompletedEventHanderRaised = true;
            };
            aptAtManagerUnderTest.ValkyrieAnalyzeCompleted += onCompleted;

            EventHandler<APTAT.Valkyrie.ValkyrieAnalyzeProgressChangedEventArgs<FileResult>> onValkyrieAnalyzeProgressChanged = (s, e) =>
            {
                Assert.Equal(20, e.ProgressInfo.TotalFileCount);
                Assert.Equal(10, e.ProgressInfo.ProcessedFileCount);
                Assert.Equal(50, e.ProgressInfo.Percentage);
                valkyrieAnalyzeProgressChanged = true;
            };

            aptAtManagerUnderTest.ValkyrieAnalyzeProgressChanged += onValkyrieAnalyzeProgressChanged;



            aptAtManagerUnderTest.StartValkyrieAnalysis("user", "test", "licenceNumber", "user", "test", null);
            Mock.Raise(() => valkyrie.AnalyzeStarted += null, EventArgs.Empty );            
            Mock.Raise(() => valkyrie.AnalyzeProgressChanged+= null, new ValkyrieAnalyzeProgressChangedEventArgs<ValkyrieAnalysisResult>(10,new ProgressInfo<ValkyrieAnalysisResult>(20,10,new ValkyrieAnalysisResult[] { }) ));
            Mock.Raise(() => valkyrie.AnalyzeCompleted += null, new AnalyzeCompletedEventArgs<ValkyrieAnalysisResult>(new ValkyrieAnalysisResult[] { }, null, false, null));

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
            var failedToAnalyzeModules = ModuleCollectionFactory.GetMockModules(CloudVerdict.FailedToAnalyze);

            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid)).Returns(() => new AptScanOperationContext(new List<IComputer>(), 1, new DevicesSourceDescription()));
            
            //FLS Manager Arange
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.Is(53), Arg.Is(80))).OccursOnce();
            Mock.Arrange(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(x => x.Count() == failedToAnalyzeModules.Count()), Arg.AnyBool)).OccursOnce();
            
            //Valkyrie Client Arange 
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);

            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);            
            var scanOptions = new ScanOptions("80", "53", ScanKind.Quick);


            var description = new DevicesSourceDescription { Description = "Test Device" };

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);
            

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner).OccursOnce();
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());

            //Arange ScanResult  (FailedToAnalyze)
            var scanResultFailedToAnalyze = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResultFailedToAnalyze.Files).Returns(() => failedToAnalyzeModules);
            Mock.Arrange(() => scanResultFailedToAnalyze.RecalculateStatistics()).OccursNever();

            //Arange ScanResult  (Safe)
            var scanResultSafe = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResultSafe.Files).Returns(() => ModuleCollectionFactory.GetMockModules(CloudVerdict.Safe)).Occurs(2);

            bool rescan;
            aptAtManagerUnderTest.GetAptScanner(description, scanOptions, out rescan);
            bool flsAnalyzeProgressChangedEventCalled = false;
            Assert.Equal(Guid.Empty, aptAtManagerUnderTest.FlsLookupOperationUid);
            EventHandler<FlsRequestEventArgs> handler = (sender, args) =>
            {
                Assert.Null(args.Exception);
                Assert.True(args.Results.Count() == scanResultSafe.Files.Count());
                Assert.All(args.Results, (r) => Assert.Equal(CloudVerdict.Safe, r.Cloud));            
                flsAnalyzeProgressChangedEventCalled = true;
            };

            aptAtManagerUnderTest.FlsAnalyzeProgressChanged += handler;


            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResultFailedToAnalyze) });            
            Mock.Raise(() => aptScanner.ScanCompleted += null, aptScanner, new ScanCompletedEventArgs() { OperationUid = operationGuid });
            Mock.Raise(() => aptScanner.ComputerScanCompleted += null, args: new object[] { new ComputerScanCompletedEventArgs(operationGuid, scanResultSafe) });
            Assert.True(flsAnalyzeProgressChangedEventCalled, "Should  be raised event FlsAnalyzeProgressChanged");
            Mock.AssertAll(fls);

            Mock.AssertAll(scanResultFailedToAnalyze);
            Mock.AssertAll(descent);
            Mock.AssertAll(aptScanner);
            Mock.AssertAll(aptScannerFactory);
            foreach (var item in failedToAnalyzeModules)
            {
                Mock.AssertAll(item);
            }
            Assert.Empty(aptAtManagerUnderTest.ValkyrieAnalysisResult);
            Assert.Equal(Guid.Empty, aptAtManagerUnderTest.ValkyrieOperationUid);            
            aptAtManagerUnderTest.FlsAnalyzeProgressChanged -= handler;
            
    
        }



        [Fact]
        public void OnFirstComputerHasResultAndSecondHasFailed_ShouldBeCalledRecalculateStatisticsAndUpdatedVerdict()
        {
            Guid operationGuid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            Mock.Arrange(() => descent.GetDescentResultsByOperationUid(operationGuid)).Returns(() => new AptScanOperationContext(new List<IComputer>(), 1, new DevicesSourceDescription())).OccursOnce();
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce();
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);
            Mock.Arrange(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(x => x.Any()), Arg.AnyBool)).MustBeCalled();
            ScanOptions scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            var description = new DevicesSourceDescription { Description = "Test Device" };

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner);
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());

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

            bool rescan;
            aptAtManagerUnderTest.GetAptScanner(description, scanOptions, out rescan);
            bool flsAnalyzeProgressChangedEventCalled = false;

            EventHandler<FlsRequestEventArgs> handler = (sender, args) =>
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
            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce("Fls Ports Updated");
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            ScanOptions scanOptions = new ScanOptions("80", "53", ScanKind.Quick);            
            var description = new DevicesSourceDescription { Description = "Test Device" };
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);

            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner);
            var aptAtManagerUnderTest = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());
                        
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => ModuleCollectionFactory.GetMockModules(CloudVerdict.Unknown));
            bool rescan;
            aptAtManagerUnderTest.GetAptScanner(description, scanOptions, out rescan);
            bool flsAnalyzeProgressChangedEventCalled = false;

            EventHandler<FlsRequestEventArgs> handler = (sender, args) =>
            {
                flsAnalyzeProgressChangedEventCalled = true;
            };

            aptAtManagerUnderTest.FlsAnalyzeProgressChanged += handler;

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
            Mock.Arrange(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(x => x.Any()), Arg.AnyBool)).MustBeCalled("Enqueue method must be called");
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            ScanOptions scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            var description = new DevicesSourceDescription { Description = "Test Device" };

            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);


            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());
            

            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            IModule[] modules = ModuleCollectionFactory.GetMixedModules();
            Mock.Arrange(() => scanResult.Files).Returns(() => modules);

            bool rescan;
            target.GetAptScanner(description, scanOptions, out rescan);

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
            Mock.Assert(() => fls.Enqueue(Arg.Matches<IEnumerable<string>>(s => s.Except(failedToAnalyzeHashes).Any() == false && s.Count() == modules.Where(m => m.Cloud == CloudVerdict.FailedToAnalyze).Count()), Arg.AnyBool), "Should contain failed hashes only");
            
            Mock.Raise(() => aptScanner.ScanCompleted += null, new ScanCompletedEventArgs() { OperationUid = guid }, null);
        }
    

        [Fact]
        public void ShouldNotEnqueueNotFailedToAnalyzeFiles()
        {
            IModule[] modules = ModuleCollectionFactory.GetModulesWithoutFailedToAnalyze();

            var descent = Mock.Create<IDescentSubSystem>(Behavior.Strict);
            var fls = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => fls.UpdateFlsPorts(Arg.AnyInt, Arg.AnyInt)).OccursOnce("FLS Ports Updated");
            Mock.Arrange(() => fls.Enqueue(Arg.IsAny<IEnumerable<string>>(), Arg.AnyBool)).OccursNever("Enqueue method must NOT be called");
            var valkyrie = Mock.Create<IValkyrieClient>(Behavior.Strict);
            var aptScanner = Mock.Create<IAptScanner>(Behavior.Strict);

            ScanOptions scanOptions = new ScanOptions("80", "53", ScanKind.Quick);
            var description = new DevicesSourceDescription { Description = "Test Device" };
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);


            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());



            Guid guid = new Guid("C2630D90-9CCE-4C94-A452-4EAE4DE78863");

            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => modules);
            bool rescan;
            target.GetAptScanner(description, scanOptions, out rescan);

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

            var description = new DevicesSourceDescription { Description = "Test Device" };
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);


            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent, description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());
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
            var description = new DevicesSourceDescription { Description = "Test Device" };
            var aptScannerFactory = Mock.Create<IAptScannerFactory>(Behavior.Strict);


            Mock.Arrange(() => aptScannerFactory.CreateAptScanner(descent,description, scanOptions)).Returns(() => aptScanner);
            var target = new AptAtManager(descent, valkyrie, fls, aptScannerFactory, new DistinctFileHelper());
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            Mock.Arrange(() => scanResult.Files).Returns(() => null);
            bool rescan;
            target.GetAptScanner(description, scanOptions, out rescan);

        }

        [Fact]
        public void TestDistictFileHelper()
        {
            
            var operationContext = Mock.Create<IAptScanOperationContext>();
            var scanResult = Mock.Create<IAptScanResult>(Behavior.Strict);
            List<IModule> duplicatedSha1Collection = new List<IModule>();
            
            duplicatedSha1Collection.AddRange(ModuleCollectionFactory.GetMockModules(CloudVerdict.Unknown));
            duplicatedSha1Collection.AddRange(ModuleCollectionFactory.GetMockModules(CloudVerdict.Unknown));
            duplicatedSha1Collection.AddRange(ModuleCollectionFactory.GetMockModules(CloudVerdict.FirstSeen));
            duplicatedSha1Collection.AddRange(ModuleCollectionFactory.GetMockModules(CloudVerdict.FirstSeen));

            //Mock.Arrange(() => scanResult.Files).Returns(() => modules.ToArray());
            Mock.Arrange(() => scanResult.GetUnknownFiles()).Returns(() => duplicatedSha1Collection.ToArray());
            
            var computer1 = Mock.Create<IComputer>();
            Mock.Arrange(() => computer1.IPAddress).Returns(() => "10.100.65.102");
            Mock.Arrange(() => scanResult.Computer).Returns(() => computer1);
            
            var scanResult2 = Mock.Create<IAptScanResult>(Behavior.Strict);
            var modules2 = ModuleCollectionFactory.GetMockModules(CloudVerdict.FirstSeen);            
            Mock.Arrange(() => scanResult2.GetUnknownFiles()).Returns(() => modules2.ToArray());

            var computer2 = Mock.Create<IComputer>();
            Mock.Arrange(() => computer2.IPAddress).Returns(() => "10.100.65.105");
            Mock.Arrange(() => scanResult2.Computer).Returns(() => computer2);


            DistinctFileHelper helper = new DistinctFileHelper();
            var result = helper.GetDistinctUknownFileInfo(new[] { scanResult, scanResult2 });
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.Equal(5, result.Count);

        }

    }

   
}
