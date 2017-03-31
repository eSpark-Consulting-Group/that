using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Xunit;
using APTAT.Common;
using System.Diagnostics;
using static APTAT.DeploymentCore.Descent.DescentResult;
using APTAT.DeploymentCore;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade;
using APTAT.Core.Descent;
using System.Threading;
using System.Threading.Tasks;
using APTAT.DeploymentCore.Descent.Helpers;
using Telerik.JustMock.Helpers;

namespace APTAT.Core.UnitTests.DescentAPT
{
    public class AptDescentScannerTest
    {
        string login = "user";
        string password = "test";
        private static void CreateMocks(out IEnumerable<IComputer> targets, out IDevicesSourceDescription devices,
        out IScanOptions scanOptions, out IAptFactory aptFactory,out IRemoteFactory remoteFactory)
        {
            var computers = new List<IComputer>();
            for (int i = 0; i <= 9; i++)
            {
                var computer = Mock.Create<IComputer>();
                Mock.Arrange(() => computer.Uid).Returns(Guid.NewGuid());
                Mock.Arrange(() => computer.IPAddress).Returns("127.0.0." + (i+1) );
                Mock.Arrange(() => computer.IsLocalComputer).Returns(true);
                Mock.Arrange(() => computer.IsOnline).Returns(true);
                Mock.Arrange(() => computer.Name).Returns("CompName_" + i);
                Mock.Arrange(() => computer.Source).Returns(DeploymentTargetSource.Ip);
                computers.Add(computer);
            }
            targets = computers.ToArray();

            devices = Mock.Create<IDevicesSourceDescription>(Behavior.Strict);
            scanOptions = Mock.Create<IScanOptions>(Behavior.Strict);
            aptFactory = Mock.Create<IAptFactory>(Behavior.Strict);
            remoteFactory = Mock.Create<IRemoteFactory>(Behavior.Strict);
        }


        [Fact(DisplayName = "AptDescentScanner constructor test.")]
        public void AptDescentScannerConstructorTest()
        {
            //Arrange
            IEnumerable<IComputer> targets;
            IScanOptions scanOptions;
            IDevicesSourceDescription devices;
            IAptFactory aptFactory;
            IRemoteFactory remoteFactory;
            CreateMocks(out targets, out devices, out scanOptions, out aptFactory,out remoteFactory);
            //Act
            AptDescentScanner aptDescentScanner =
            new AptDescentScanner(targets, login, password, devices, scanOptions, aptFactory, remoteFactory);
            //Assert
            Mock.Assert(aptDescentScanner);
        }

        [Fact(DisplayName = "Check that CreateContext calls Once on ValidateAndGetResultTest.")]
        public void CheckValidateAndGetResultTest()
        {
            //Arrange
            IEnumerable<IComputer> targets;
            IScanOptions scanOptions;
            IDevicesSourceDescription devices;
            IAptFactory aptFactory;
            IRemoteFactory remoteFactory;
            CreateMocks(out targets, out devices, out scanOptions, out aptFactory,out remoteFactory);

            Mock.Arrange(() => aptFactory.CreateContext(targets.ToArray(), devices))
           .Returns(Mock.Create<IAptScanOperationContextInternal>()).OccursOnce();

            Mock.Arrange(()=>scanOptions.ScanKind).Returns(ScanKind.Quick);

            //Act
            var aptDescentScannerAct =
                new AptDescentScanner(targets, login, password, devices, scanOptions, 
                aptFactory, remoteFactory);
            var result = aptDescentScannerAct.ValidateAndGetResult();

            //Assert
            Mock.Assert(result);
        }

        [Fact(DisplayName = "Check if ValidateAndGetResultTest. return expeted result.")]
        public void CheckIfValidateAndGetResultReturnExpectedResultTest()
        {
            //Arrange
            IEnumerable<IComputer> targets;
            IScanOptions scanOptions;
            IDevicesSourceDescription devices;
            IAptFactory aptFactory;
            IRemoteFactory remoteFactory;
            CreateMocks(out targets, out devices, out scanOptions, out aptFactory,out remoteFactory);

            Mock.Arrange(() => aptFactory.CreateContext(targets.ToArray(), devices))
           .Returns(Mock.Create<IAptScanOperationContextInternal>()).OccursOnce();

            Mock.Arrange(() => scanOptions.ScanKind).Returns(ScanKind.Quick);

            IDescentQueryResult resultExpected = Mock.Create<IDescentQueryResult>(Behavior.Strict);

            //Act
            var aptDescentScannerAct =
                new AptDescentScanner(targets, login, password, devices, 
                scanOptions, aptFactory,remoteFactory);
            var result = aptDescentScannerAct.ValidateAndGetResult();

            //Assert
            Mock.Assert(result);
            Mock.Equals(result, resultExpected);
        }

        [Fact(DisplayName = "Calculate operation statistics test.")]
        public void CalculateOperationStatisticsTest()
        {
            //Arrange
            Guid operationUid = Guid.NewGuid();
            IEnumerable<IComputer> targets;
            IScanOptions scanOptions;
            IDevicesSourceDescription devices;
            IAptFactory aptFactory;
            IRemoteFactory remoteFactory;
            CreateMocks(out targets, out devices, out scanOptions, out aptFactory,out remoteFactory);

            IAptScanOperationContextInternal result = 
                Mock.Create<IAptScanOperationContextInternal>(Behavior.Strict);

            IEnumerable<IAptScanResult> aptScanResult =Mock.Create<IEnumerable<IAptScanResult>>();

            Mock.Arrange(() => result.ScanResults).Returns(aptScanResult);
            Mock.Arrange(() => result.SetCounters(0,0,50,0));
            Mock.Arrange(() => result.OperationUid).Returns(operationUid);
            Mock.Arrange(() => result.TotalTargetsCount).Returns(50);

            //Act

            var aptDescentScannerAct = 
                new AptDescentScanner(targets.ToArray(), login, password, devices,
                scanOptions, aptFactory,remoteFactory);

            var getDescentCompletedEventsArgs = aptDescentScannerAct.GetDescentCompletedEventArgs(result);
            var completedEventsArgsContext = ((AptScanCompletedEventArgs)getDescentCompletedEventsArgs).Context;
            //Assert

             Mock.Equals(result.OperationUid, getDescentCompletedEventsArgs.OperationUid);
             Mock.Equals(result, completedEventsArgsContext);
             Mock.Assert(getDescentCompletedEventsArgs);
        }

        [Fact(DisplayName = "Methods RemoteInstallerJob,RemoteHostInfo called once for one target test.")]
        public void RemoteInstallerJobAndRemoteHostInfoCalledOnceForTargetTest()
        {
            //Arrange
            IEnumerable<IComputer> targets;
            IScanOptions scanOptions;
            IDevicesSourceDescription devices;
            IAptFactory aptFactory;
            IRemoteFactory remoteFactory;
            ManualResetEventSlim ev = new ManualResetEventSlim(false);

            CreateMocks(out targets, out devices, out scanOptions, out aptFactory,out remoteFactory);
            var computer = targets.FirstOrDefault();

            var changeTrackingSubSystem = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);
            var descentMod = Mock.Create<IAptDescentMod>();

            Mock.Arrange(() => aptFactory.CreateDescentMode(Arg.IsAny<TaskFactory>(),
                changeTrackingSubSystem)).Returns(() => descentMod);

            var descentResult = Mock.Create<IAptScanOperationContextInternal>();
            IAptScanResult aptScanResult = Mock.Create<IAptScanResult>();
            Mock.Arrange(() => descentResult.GetComputerDescentResultByTargetUid(Arg.IsAny<Guid>()))
            .Returns(() => aptScanResult);

            var remoteHostingInfo = Mock.Create<IRemoteHostInfo>();
            Mock.Arrange(() => remoteFactory.CreateRemoteHostInfo(computer.Uid,
                computer.IPAddress ?? computer.DnsName,
                login, password, computer.IsLocalComputer)).DoInstead(()=>ev.Set()).Returns(remoteHostingInfo);

            var descentCallback = Mock.Create<IDescentCallback>(Behavior.Strict);
            var sc = new AptDescentScanner(targets.Take(1), login, password, devices, scanOptions, aptFactory, remoteFactory);
            Mock.Arrange(() => remoteFactory.CreateRemoteInstallerJob(
                descentResult, changeTrackingSubSystem, sc, computer)).Returns(descentCallback);

            //Act
            sc.StartScan(changeTrackingSubSystem, descentResult);
            ev.Wait(TimeSpan.FromMilliseconds(700));

            //Assert
            Mock.Assert(()=>remoteFactory.CreateRemoteHostInfo(
                computer.Uid, computer.IPAddress ?? computer.DnsName,
                login, password, computer.IsLocalComputer),Occurs.Once());
            Mock.Assert(()=>remoteFactory.CreateRemoteInstallerJob(
                descentResult, changeTrackingSubSystem, sc, computer),Occurs.Once());
        }

        [Fact(DisplayName = "Compare ScanResults for all targets test.")]
        public void CompareScanResults()
        {
            //Arrange
            IEnumerable<IComputer> targets;
            IScanOptions scanOptions;
            IDevicesSourceDescription devices;
            IAptFactory aptFactory;
            IRemoteFactory remoteFactory;
            ManualResetEventSlim ev = new ManualResetEventSlim(false);

            CreateMocks(out targets, out devices, out scanOptions, out aptFactory, out remoteFactory);
            var sc = new AptDescentScanner(targets, login, password, devices, scanOptions, aptFactory, remoteFactory);

            var changeTrackingSubSystem = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);
            var descentMod = Mock.Create<IAptDescentMod>();
            Mock.Arrange(() => aptFactory.CreateDescentMode(Arg.IsAny<TaskFactory>(),
                changeTrackingSubSystem)).Returns(() => descentMod);

            Guid operationUid = Guid.NewGuid();
            var descentResult = Mock.Create<IAptScanOperationContextInternal>(Behavior.Strict);
            Mock.Arrange(() => descentResult.OperationUid).Returns(() => operationUid);

            Dictionary<Guid,IAptScanResult> scanResults = new Dictionary<Guid, IAptScanResult>();

            foreach (var target in targets)
            {
                var remoteHostingInfo = Mock.Create<IRemoteHostInfo>();
                Mock.Arrange(() => remoteFactory.CreateRemoteHostInfo(target.Uid,
                target.IPAddress ?? target.DnsName, login, password, target.IsLocalComputer)).DoInstead(()=>ev.Set())
                .Returns(remoteHostingInfo);

                var descentCallback = Mock.Create<IDescentCallback>(Behavior.Strict);
                Mock.Arrange(() => remoteFactory.CreateRemoteInstallerJob(
                descentResult, changeTrackingSubSystem, sc, target)).Returns(descentCallback);

                IAptScanResult scanResult = Mock.Create<IAptScanResult>();
                               scanResults.Add(target.Uid,scanResult);

                Mock.Arrange(() => descentResult.GetComputerDescentResultByTargetUid(target.Uid))
                .Returns(() => scanResult);

                Mock.Arrange(() => descentMod.StartAptScan(
                remoteHostingInfo, descentCallback, target.Uid, scanResult,
                operationUid, scanOptions));
            }
            //ev.Set();
            //Act
            sc.StartScan(changeTrackingSubSystem, descentResult);
            ev.Wait(TimeSpan.FromMilliseconds(700));

            //Assert
            foreach (var computer in targets)
            {
                var scanResult = scanResults[computer.Uid];
                Mock.ReferenceEquals(scanResult, descentResult.GetComputerDescentResultByTargetUid(computer.Uid));
            }
        }

        [Fact(DisplayName = @"Call of methods CreateRemoteHostInfo,CreateRemoteInstallerJob occurs once 
        for each target.")]
        public void CallMethodsCreateRemoteHostInfoAndForCreateRemoteInstallerJobOnceForEachTarget()
        {
            //Arrange
            IEnumerable<IComputer> targets;
            IScanOptions scanOptions;
            IDevicesSourceDescription devices;
            IAptFactory aptFactory;
            IRemoteFactory remoteFactory;
            ManualResetEventSlim ev = new ManualResetEventSlim(false);

            CreateMocks(out targets, out devices, out scanOptions, out aptFactory, out remoteFactory);
            var sc = new AptDescentScanner(targets, login, password, devices, scanOptions, aptFactory, remoteFactory);
            var changeTrackingSubSystem = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);
            var descentResult = Mock.Create<IAptScanOperationContextInternal>();
            var descentMod = Mock.Create<IAptDescentMod>();
            Mock.Arrange(() => aptFactory.CreateDescentMode(Arg.IsAny<TaskFactory>(), changeTrackingSubSystem))
            .Returns(() => descentMod);

            IAptScanResult aptScanResult = Mock.Create<IAptScanResult>();
            Mock.Arrange(() => descentResult.GetComputerDescentResultByTargetUid(Arg.IsAny<Guid>()))
            .Returns(() => aptScanResult);

            foreach (var computer in targets)
            {
                var remoteHostingInfo = Mock.Create<IRemoteHostInfo>();
                Mock.Arrange(() => remoteFactory.CreateRemoteHostInfo(computer.Uid,
                computer.IPAddress ?? computer.DnsName, login, password, computer.IsLocalComputer)).DoInstead(()=>ev.Set())
                .Returns(remoteHostingInfo);

                var descentCallback = Mock.Create<IDescentCallback>(Behavior.Strict);
                Mock.Arrange(() => remoteFactory.CreateRemoteInstallerJob(
                descentResult, changeTrackingSubSystem, sc, computer)).Returns(descentCallback);
            }

            //Act
            sc.StartScan(changeTrackingSubSystem, descentResult);
           var istrue= ev.Wait(TimeSpan.FromSeconds(20));
            //Assert.True(istrue);



            //Assert
            foreach (var computer in targets)
            {
                Mock.Assert(() => remoteFactory.CreateRemoteHostInfo(
                    computer.Uid, computer.IPAddress ?? computer.DnsName,
                    login, password, computer.IsLocalComputer), Occurs.Once());

                Mock.Assert(() => remoteFactory.CreateRemoteInstallerJob(
                    descentResult, changeTrackingSubSystem, sc, computer), Occurs.Once());
            }
        }

        [Fact(DisplayName = "Check computers which is offline.")]
        public void CheckComputersWhichIsOffline()
        {
            //Check if set property only for isofline
            //Check if called only few

            //Arrange
            IEnumerable<IComputer> targets;
            IScanOptions scanOptions;
            IDevicesSourceDescription devices;
            IAptFactory aptFactory;
            IRemoteFactory remoteFactory;
            ManualResetEventSlim ev = new ManualResetEventSlim(false);
            CreateMocks(out targets, out devices, out scanOptions, out aptFactory, out remoteFactory);

            var computers = new List<IComputer>();
            for (int i = 0; i <= 9; i++)
            {
                var computer = Mock.Create<IComputer>();
                Mock.ArrangeSet(() => computer.Uid = Guid.NewGuid());
                Mock.ArrangeSet(() => computer.IPAddress="127.0.0." + (i + 1));
                Mock.ArrangeSet(() => computer.IsOnline=i % 2 == 1 ? false : true);
                Mock.ArrangeSet(() => computer.Name = "CompName_" + i);
                computers.Add(computer);
            }
            targets =new List<IComputer>();
            targets = computers.ToArray();

            int onlineTargesCount = targets.Where(c=>c.IsOnline).Count();
            int oflineTargesCount = targets.Where(c => !c.IsOnline).Count();
            int resultTypeCounter = 0;
            int startAptScanCounter = 0;

            var sc = new AptDescentScanner(targets,login, password, devices, scanOptions, 
                aptFactory, remoteFactory);

            var changeTrackingSubSystem = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);
            Guid operationUid = Guid.NewGuid();
            var descentResult = Mock.Create<IAptScanOperationContextInternal>(Behavior.Strict);
            Mock.Arrange(() => descentResult.OperationUid).Returns(() => operationUid);

            var descentMod = Mock.Create<IAptDescentMod>(Behavior.Strict);
            Mock.Arrange(() => aptFactory.CreateDescentMode(Arg.IsAny<TaskFactory>(), changeTrackingSubSystem))
            .Returns(() => descentMod);
                
            foreach (var computer in targets)
            {
                IAptScanResult aptScanResult = Mock.Create<IAptScanResult>(Behavior.Strict);

                Mock.Arrange(() => descentResult.GetComputerDescentResultByTargetUid(computer.Uid))
                .Returns(() => aptScanResult);

                if (!computer.IsOnline)
                {
                    Mock.ArrangeSet(() => aptScanResult.ResultType = ScanResultType.NotPerformed)
                        .DoInstead(() => resultTypeCounter++);

                    Mock.Arrange(() => descentResult.OnDescendOperationEvent(
                    false, computer.Uid,
                    Arg.IsAny<FormattedText>(), Arg.IsAny<FormattedText>(), 
                    string.Empty, 0, string.Empty, false)).DoInstead(() => ev.Set());
                }
                else
                {
                    var remoteHostingInfo = Mock.Create<IRemoteHostInfo>();
                    Mock.Arrange(() => remoteFactory.CreateRemoteHostInfo(computer.Uid,
                    computer.IPAddress ?? computer.DnsName, login, password, computer.IsLocalComputer))
                    .Returns(remoteHostingInfo);

                    var descentCallback = Mock.Create<IDescentCallback>(Behavior.Strict);
                    Mock.Arrange(() => remoteFactory.CreateRemoteInstallerJob(
                    descentResult, changeTrackingSubSystem, sc, computer)).Returns(descentCallback);

                    Mock.Arrange(() => descentMod.StartAptScan(
                    remoteHostingInfo, descentCallback, computer.Uid, aptScanResult,
                    operationUid, scanOptions)).DoInstead(()=> startAptScanCounter++);
                }
            }

            //Act
            sc.StartScan(changeTrackingSubSystem, descentResult);
            ev.Wait(TimeSpan.FromSeconds(700));

            //Assert
            foreach (var computer in targets)
            {
                if (!computer.IsOnline)
                {
                    Mock.Assert(() => descentResult.OnDescendOperationEvent(
                    false, computer.Uid,
                    Arg.IsAny<FormattedText>(), Arg.IsAny<FormattedText>(),
                    String.Empty, 0, String.Empty, false));
                }
            }

            Assert.Equal(oflineTargesCount, resultTypeCounter);
            Assert.Equal(onlineTargesCount, startAptScanCounter);
        }


        [Fact(DisplayName = "Call Dispose Test.")]
        public void DisposeTest()
        {
            //Arrange
            IEnumerable<IComputer> targets;
            IScanOptions scanOptions;
            IDevicesSourceDescription devices;
            IAptFactory aptFactory;
            IRemoteFactory remoteFactory;
            CreateMocks(out targets, out devices, out scanOptions, out aptFactory,out remoteFactory);
            var scanner = Mock.Create<IScanner>(Behavior.Strict);
            Mock.Arrange(() => scanner.Dispose()).OccursOnce();
            //Act
            var aptDescentScannerAct =
            new AptDescentScanner(targets, login, password, devices, scanOptions, aptFactory, remoteFactory);
            aptDescentScannerAct.Dispose();
            //Assert
            Mock.Assert(aptDescentScannerAct);
        }
    }
}
