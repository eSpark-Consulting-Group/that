using APTAT.Common;
using APTAT.DeploymentCore.Descent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using APTAT.Core.Properties;
using System.Threading;
using System.Collections.Concurrent;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Facade;
using APTAT.DeploymentCore.Directory;
using APTAT.Core.Descent.Apt;
using Telerik.JustMock;
using Xunit;

namespace APTAT.Core.UnitTests.DescentAPT
{
    public class DescentSubSystemTest
    {
        private ConcurrentDictionary<Guid, IScanner> _scanners = new ConcurrentDictionary<Guid, IScanner>();
        private Dictionary<Guid, IDescentQueryResult> _descentResults = new Dictionary<Guid, IDescentQueryResult>();
        private IEnumerable<IComputer> targets;
        private IDevicesSourceDescription devices;
        private IScanOptions scanOptions;
        private IComputerDescentResultFactory computerDescentResultFactory;
        private IDescentWatcherFactory descentWatcherFactory;
        public DescentSubSystemTest()
        {
            int length = 10;
            for (int i = 0; i < length; i++)
            {
                _scanners[Guid.NewGuid()] = Mock.Create<IScanner>();
                _descentResults[Guid.NewGuid()] = Mock.Create<IDescentQueryResult>();
            }

            var computers = new List<IComputer>();
            for (int i = 0; i <= 9; i++)
            {
                var computer = Mock.Create<IComputer>();
                Mock.Arrange(() => computer.Uid).Returns(Guid.NewGuid());
                Mock.Arrange(() => computer.IPAddress).Returns("127.0.0." + (i + 1));
                Mock.Arrange(() => computer.IsLocalComputer).Returns(true);
                Mock.Arrange(() => computer.IsOnline).Returns(true);
                Mock.Arrange(() => computer.Name).Returns("CompName_" + i);
                Mock.Arrange(() => computer.Source).Returns(DeploymentTargetSource.Ip);
                computers.Add(computer);
            }
            targets = computers.ToArray();

            devices = Mock.Create<IDevicesSourceDescription>(Behavior.Strict);
            scanOptions = Mock.Create<IScanOptions>(Behavior.Strict);
            computerDescentResultFactory = Mock.Create<IComputerDescentResultFactory>();
            descentWatcherFactory = Mock.Create<IDescentWatcherFactory>(Behavior.Strict);
        }


        [Fact(DisplayName = "DescentSubSystem constructor test.")]
        public void DescentSubSystemConstructorTest()
        {
            //Arrange
            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);
            var timer = Mock.Create<IScheduler>(Behavior.Strict);
            TimeSpan period;
            TimeSpan.TryParse(Settings.Default.DescentResultLifetime, out period);
            Mock.Arrange(() => timer.SetInterval(period)).OccursOnce();
            //Act
            DescentSubSystem descentSubSystem = new DescentSubSystem(tracker, timer);
            //Assert
            Mock.Assert(timer);
            Mock.Arrange(() => timer.Dispose());
            descentSubSystem.Dispose();
        }

        [Fact(DisplayName = "DescentSubSystem dispose test.")]
        public void DescentSubSystemDisposeTest()
        {
            //Arrange
            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);
            var timer = Mock.Create<IScheduler>();
            Mock.Arrange(() => timer.SetInterval(Arg.AnyTimeSpan));
            Mock.Arrange(() => timer.Dispose()).OccursOnce();
            //Act
            DescentSubSystem descentSubSystem = new DescentSubSystem(tracker, timer);
            descentSubSystem.Dispose();
            //Assert
            Mock.Assert(timer);
        }

        [Fact(DisplayName = "On Raise  result.DescentCompleted check that OnDescentCompleted called.")]
        public void OnStartScanCheckIfEventOnDescentCompletedRaised()
        {
            //Arrange
            bool raised = false;
            Guid operationGuid;
            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>();
            IScanner scanner = Mock.Create<IScanner>();
            var timer = Mock.Create<IScheduler>();
            IDescentQueryResult queryResult = Mock.Create<IDescentQueryResult>();
            Mock.Arrange(() => queryResult.TotalTargetsCount).Returns(() => 10);
            Mock.Arrange(() => scanner.ValidateAndGetResult()).Returns(() => queryResult);

            DescentSubSystem DescentSubSystem = new DescentSubSystem(tracker, timer);
            DescentSubSystem.DescentCompleted += (s, e) =>
            {
                raised = true;
            };
            DescentSubSystem.StartScan(scanner, out operationGuid);
            //Act
            Mock.Raise(() => queryResult.DescentCompleted += null, queryResult);
            //Assert
            Assert.True(raised);
        }

        [Fact(DisplayName = "On StartScan and twice raise event OnDescentCompleted should be raised once.")]
        public void OnStartScanEventShouldBeRaisedOnce()
        {
            //Arrange
            bool raised = false;
            Guid operationGuid;
            var timer = Mock.Create<IScheduler>();
            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>();
            IDescentQueryResult queryResult = Mock.Create<IDescentQueryResult>();
            Mock.Arrange(() => queryResult.TotalTargetsCount).Returns(() => 10);
            IScanner scanner = Mock.Create<IScanner>();
            Mock.Arrange(() => scanner.ValidateAndGetResult()).Returns(() => queryResult);

            DescentSubSystem DescentSubSystem = new DescentSubSystem(tracker, timer);
            DescentSubSystem.DescentCompleted += (s, e) =>
            {
                raised = true;
            };
            DescentSubSystem.StartScan(scanner, out operationGuid);
            //Act
            Mock.Raise(() => queryResult.DescentCompleted += null, queryResult);
            raised = false;
            Mock.Raise(() => queryResult.DescentCompleted += null, queryResult);
            //Assert
            Assert.False(raised);
        }

        [Fact(DisplayName = " Check that " +
        "_descentResults collection is not empty and method GetDescentResultsByOperationUid " +
        "return right value, on call DescentResultsCleanupTimerCallback _descentResults " +
        "should be empty test.")]
        public void DescentResultsCleanupTimerCallbackTest()
        {
            //Arrange
            Guid operationGuid = Guid.NewGuid();
            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);

            var timer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => timer.SetInterval(Arg.IsAny<TimeSpan>()));
            Mock.Arrange(() => timer.Stop());
            Mock.Arrange(() => timer.Start(false)).Returns(true);
            Mock.Arrange(() => timer.IsEnabled).Returns(true);

            DescentSubSystem descentSubSystem = new DescentSubSystem(tracker, timer);

            //Act
            var resultByOperationUidAfter = descentSubSystem.GetDescentResultsByOperationUid(operationGuid);

            //Assert
            Assert.Null(resultByOperationUidAfter);
        }

        [Fact(DisplayName = "On StartScan testing ValidateAndGetResult value test.")]
        public void ValidateAndGetResultTest()
        {
            //Arrange
            Guid operationGuid = Guid.NewGuid();
            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);
            IScheduler timer = Mock.Create<IScheduler>(Behavior.Strict);
            IScanner scanner = Mock.Create<IScanner>();
            IDescentQueryResult queryResult = Mock.Create<IDescentQueryResult>();
            Mock.Arrange(() => timer.SetInterval(Arg.IsAny<TimeSpan>()));
            Mock.Arrange(() => queryResult.TotalTargetsCount).Returns(() => 10);
            Mock.Arrange(() => queryResult.OperationUid).Returns(() => operationGuid);
            Mock.Arrange(() => scanner.ValidateAndGetResult()).Returns(() => queryResult);
            Mock.Arrange(() => tracker.RegisterListener(queryResult));
            DescentSubSystem descentSubSystem = new DescentSubSystem(tracker, timer);
            //Act
            descentSubSystem.StartScan(scanner,out operationGuid);
            IDescentQueryResult result= descentSubSystem.GetDescentResultsByOperationUid(operationGuid);
            //Assert
            Assert.Equal(queryResult.OperationUid, result.OperationUid);
        }


        [Fact(DisplayName = "StopOperationForTargetUidForFewScansTest.")]
        public void StopOperationForTargetUidForFewScansTest()
        {
            //Arrange
            Guid operationUid1 = Guid.NewGuid();
            Guid operationUid2 = Guid.NewGuid();
            Guid operationUid3 = Guid.NewGuid();
            Guid[] operationUids = new Guid[] { operationUid1, operationUid2, operationUid3 };

            ManualResetEventSlim ev = new ManualResetEventSlim(false);
            var timer = Mock.Create<IScheduler>();
            Guid[] targetUids = targets.Select(c => c.Uid).ToArray();
            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);

            IScanner scanner1 = Mock.Create<IScanner>(Behavior.Strict);
            IScanner scanner2 = Mock.Create<IScanner>(Behavior.Strict);
            IScanner scanner3 = Mock.Create<IScanner>(Behavior.Strict);
    
            IDescentQueryResult queryResult1 = Mock.Create<IDescentQueryResult>(Behavior.Strict);
            Mock.Arrange(() => queryResult1.TotalTargetsCount).Returns(() => 10);
            Mock.Arrange(() => queryResult1.OperationUid).Returns(() => operationUid1);
            Mock.Arrange(() => scanner1.StartScan(tracker, queryResult1));
            Mock.Arrange(() => scanner1.ValidateAndGetResult()).Returns(() => queryResult1);
            Mock.Arrange(() => tracker.RegisterListener(queryResult1));

            IDescentQueryResult queryResult2 = Mock.Create<IDescentQueryResult>(Behavior.Strict);
            Mock.Arrange(() => queryResult2.TotalTargetsCount).Returns(() => 12);
            Mock.Arrange(() => queryResult2.OperationUid).Returns(() => operationUid2);
            Mock.Arrange(() => scanner2.StartScan(tracker, queryResult2));
            Mock.Arrange(() => scanner2.ValidateAndGetResult()).Returns(() => queryResult2);
            Mock.Arrange(() => tracker.RegisterListener(queryResult2));

            IDescentQueryResult queryResult3 = Mock.Create<IDescentQueryResult>(Behavior.Strict);
            Mock.Arrange(() => queryResult3.TotalTargetsCount).Returns(() => 13);
            Mock.Arrange(() => queryResult3.OperationUid).Returns(() => operationUid3);
            Mock.Arrange(() => scanner3.StartScan(tracker, queryResult3));
            Mock.Arrange(() => scanner3.ValidateAndGetResult()).Returns(() => queryResult3);
            Mock.Arrange(() => tracker.RegisterListener(queryResult3));
 

            foreach (var target in targetUids)
            {
                Mock.Arrange(() => scanner1.StopScan(target)).OccursOnce();
                Mock.Arrange(() => scanner2.StopScan(target)).OccursOnce();
                Mock.Arrange(() => scanner3.StopScan(target)).OccursOnce();
            }
 
            //Act
            DescentSubSystem descentSubSystem = new DescentSubSystem(tracker, timer);

            descentSubSystem.StartScan(scanner1, out operationUid1);
            descentSubSystem.StartScan(scanner2, out operationUid2);
            descentSubSystem.StartScan(scanner3, out operationUid3);

            descentSubSystem.StopOperationForTargetUid(targetUids);
            //Assert
            Mock.Assert(() => scanner1.StopScan(Arg.IsAny<Guid>()), Occurs.Exactly(10));
            Mock.Assert(() => scanner2.StopScan(Arg.IsAny<Guid>()), Occurs.Exactly(10));
            Mock.Assert(() => scanner3.StopScan(Arg.IsAny<Guid>()), Occurs.Exactly(10));

        }

        [Fact(DisplayName = "Test call methods inside StartScan by order Test.")]
        public void CallingMethodsInsideStartScanByOrder()
        {
            //Arrange
            Guid operationGuid = Guid.NewGuid();
            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);
            IScheduler timer = Mock.Create<IScheduler>(Behavior.Strict);
            IScanner scanner = Mock.Create<IScanner>();
            IDescentQueryResult queryResult = Mock.Create<IDescentQueryResult>();
            
            Mock.Arrange(() => timer.SetInterval(Arg.IsAny<TimeSpan>()));
            Mock.Arrange(() => queryResult.TotalTargetsCount).Returns(() => 10);
            Mock.Arrange(() => queryResult.OperationUid).Returns(() => operationGuid);
                       
            Mock.Arrange(() => scanner.ValidateAndGetResult()).Returns(() => queryResult).InOrder();
            Mock.Arrange(() => tracker.RegisterListener(queryResult)).InOrder();
            Mock.Arrange(() => scanner.StartScan(tracker, queryResult)).InOrder();

            DescentSubSystem descentSubSystem = new DescentSubSystem(tracker, timer);
            //Act
            descentSubSystem.StartScan(scanner, out operationGuid);
            Mock.Assert(scanner);
            Mock.Assert(tracker);
        }


        [Fact(DisplayName = "StopOperationForTargetUid Test.")]
        public void StopOperationForTargetUidTest()
        {
            //Arrange
            Guid generalGuid = Guid.NewGuid();
            Guid operationGuid;
            ManualResetEventSlim ev = new ManualResetEventSlim(false);
            var timer = Mock.Create<IScheduler>();
            Guid[] targetUids = targets.Select(c => c.Uid).ToArray();
            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);
            IScanner scanner = Mock.Create<IScanner>(Behavior.Strict);

            IDescentQueryResult queryResult = Mock.Create<IDescentQueryResult>(Behavior.Strict);
            Mock.Arrange(() => queryResult.TotalTargetsCount).Returns(() => 777);
            Mock.Arrange(() => queryResult.OperationUid).Returns(() => generalGuid);
            Mock.Arrange(() => scanner.StartScan(tracker, queryResult));
            Mock.Arrange(() => tracker.RegisterListener(queryResult));
            Mock.Arrange(() => scanner.ValidateAndGetResult()).Returns(() => queryResult);

            foreach (var target in targetUids.Take(3))
            {
                Mock.Arrange(() => scanner.StopScan(target)).OccursOnce();
            }

            //Act
            DescentSubSystem descentSubSystem = new DescentSubSystem(tracker, timer);
            descentSubSystem.StartScan(scanner, out operationGuid);
            descentSubSystem.StopOperationForTargetUid(targetUids.Take(3).ToArray());

            //Assert
            foreach (var targetUid in targetUids.Take(3).ToArray())
            {
                Mock.Assert(() => scanner.StopScan(targetUid), Occurs.Once());
            }
        }


        [Fact(DisplayName = "StopAllScans calling once test.")]
        public void StopAllScansTest()
        {
            //Arrange
            Guid operationGuid;
            Guid generalGuid = Guid.NewGuid();
            ManualResetEventSlim ev = new ManualResetEventSlim(false);
            var timer = Mock.Create<IScheduler>();
            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);
            IScanner scanner = Mock.Create<IScanner>();
            IDescentQueryResult queryResult = Mock.Create<IDescentQueryResult>();
            Mock.Arrange(() => queryResult.TotalTargetsCount).Returns(() => 10);
            Mock.Arrange(() => queryResult.OperationUid).Returns(() => generalGuid);
            Mock.Arrange(() => scanner.ValidateAndGetResult()).Returns(() => queryResult);
            Mock.Arrange(() => tracker.RegisterListener(queryResult));
            Mock.Arrange(() => scanner.AbortAllThread(0)).OccursOnce();
            //Act
            DescentSubSystem descentSubSystem = new DescentSubSystem(tracker, timer);
            descentSubSystem.StartScan(scanner, out operationGuid);
            descentSubSystem.StopAllScans(0);
            //Assert
            Mock.Assert(() => scanner.AbortAllThread(0), Occurs.Once());
        }


        [Fact(DisplayName = "3 scaners created,then called StopAllScans."+
            "checked that for each scanner called AbortAllThread.")]
        public void CheckThatForEach3ScansAbortAllThreadMethodCalledTest()
        {
            Guid operationUid1 = Guid.NewGuid();
            Guid operationUid2 = Guid.NewGuid();
            Guid operationUid3 = Guid.NewGuid();

            var timer = Mock.Create<IScheduler>();
            Guid[] targetUids = targets.Select(c => c.Uid).ToArray();

            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);

            IScanner scanner1 = Mock.Create<IScanner>(Behavior.Strict);
            IScanner scanner2 = Mock.Create<IScanner>(Behavior.Strict);
            IScanner scanner3 = Mock.Create<IScanner>(Behavior.Strict);

            IDescentQueryResult queryResult1 = Mock.Create<IDescentQueryResult>(Behavior.Strict);
            IDescentQueryResult queryResult2 = Mock.Create<IDescentQueryResult>(Behavior.Strict);
            IDescentQueryResult queryResult3 = Mock.Create<IDescentQueryResult>(Behavior.Strict);

            Mock.Arrange(() => queryResult1.TotalTargetsCount).Returns(() => 10);
            Mock.Arrange(() => queryResult1.OperationUid).Returns(() => operationUid1);
            Mock.Arrange(() => scanner1.StartScan(tracker, queryResult1));
            Mock.Arrange(() => scanner1.ValidateAndGetResult()).Returns(() => queryResult1);
            Mock.Arrange(() => tracker.RegisterListener(queryResult1));

            Mock.Arrange(() => queryResult2.TotalTargetsCount).Returns(() => 12);
            Mock.Arrange(() => queryResult2.OperationUid).Returns(() => operationUid2);
            Mock.Arrange(() => scanner2.StartScan(tracker, queryResult2));
            Mock.Arrange(() => scanner2.ValidateAndGetResult()).Returns(() => queryResult2);
            Mock.Arrange(() => tracker.RegisterListener(queryResult2));

            Mock.Arrange(() => queryResult3.TotalTargetsCount).Returns(() => 13);
            Mock.Arrange(() => queryResult3.OperationUid).Returns(() => operationUid3);
            Mock.Arrange(() => scanner3.StartScan(tracker, queryResult3));
            Mock.Arrange(() => scanner3.ValidateAndGetResult()).Returns(() => queryResult3);
            Mock.Arrange(() => tracker.RegisterListener(queryResult3));

            Mock.Arrange(() => scanner1.AbortAllThread(0)).OccursOnce();
            Mock.Arrange(() => scanner2.AbortAllThread(0)).OccursOnce();
            Mock.Arrange(() => scanner3.AbortAllThread(0)).OccursOnce();

            //Act
            DescentSubSystem descentSubSystem = 
                new DescentSubSystem(tracker, timer);

            descentSubSystem.StartScan(scanner1, out operationUid1);
            descentSubSystem.StartScan(scanner2, out operationUid2);
            descentSubSystem.StartScan(scanner3, out operationUid3);

            descentSubSystem.StopAllScans(0);

            //Assert
            Mock.Assert(() => scanner1.AbortAllThread(0), Occurs.Once());
            Mock.Assert(() => scanner2.AbortAllThread(0), Occurs.Once());
            Mock.Assert(() => scanner3.AbortAllThread(0), Occurs.Once());
        }


        [Fact(DisplayName = "NI. StopOperationForTargetUid 5  StopAllScansForFewScansTest.")]
        public void StopAllScans5ForFewScansTest()
        {
            Guid operationUid1 = Guid.NewGuid();
            Guid operationUid2 = Guid.NewGuid();

            var timer = Mock.Create<IScheduler>();
            Guid[] targetUids = targets.Select(c => c.Uid).ToArray();
            Guid[] targetUids0_5 = targetUids.Take(5).ToArray();
            Guid[] targetUids6_10 = targetUids.Skip(5).Take(5).ToArray();
 

            IChangeTrackingSubSystem tracker = Mock.Create<IChangeTrackingSubSystem>(Behavior.Strict);

            IScanner scanner1 = Mock.Create<IScanner>(Behavior.Strict);
            IScanner scanner2 = Mock.Create<IScanner>(Behavior.Strict);


            IDescentQueryResult queryResult1 = Mock.Create<IDescentQueryResult>(Behavior.Strict);
            IDescentQueryResult queryResult2 = Mock.Create<IDescentQueryResult>(Behavior.Strict);


            Mock.Arrange(() => queryResult1.TotalTargetsCount).Returns(() => 10);
            Mock.Arrange(() => queryResult1.OperationUid).Returns(() => operationUid1);
            Mock.Arrange(() => scanner1.StartScan(tracker, queryResult1));
            Mock.Arrange(() => scanner1.ValidateAndGetResult()).Returns(() => queryResult1);
            Mock.Arrange(() => tracker.RegisterListener(queryResult1));

            Mock.Arrange(() => queryResult2.TotalTargetsCount).Returns(() => 12);
            Mock.Arrange(() => queryResult2.OperationUid).Returns(() => operationUid2);
            Mock.Arrange(() => scanner2.StartScan(tracker, queryResult2));
            Mock.Arrange(() => scanner2.ValidateAndGetResult()).Returns(() => queryResult2);
            Mock.Arrange(() => tracker.RegisterListener(queryResult2));

            foreach (var targetUid in targetUids0_5)
            {
                Mock.Arrange(() => scanner1.StopScan(targetUid)).OccursOnce();
            }
            foreach (var targetUid in targetUids6_10)
            {
                Mock.Arrange(() => scanner2.StopScan(targetUid)).OccursOnce();
            }


            //Act
            DescentSubSystem descentSubSystem = new DescentSubSystem(tracker, timer);

            descentSubSystem.StartScan(scanner1, out operationUid1);
            descentSubSystem.StopOperationForTargetUid(targetUids0_5);
            Mock.Assert(() => scanner1.StopScan(Arg.IsAny<Guid>()), Occurs.Exactly(5));

           
            //descentSubSystem.StartScan(scanner2, out operationUid2);
            //descentSubSystem.StopOperationForTargetUid(targetUids6_10);
            //Mock.Assert(() => scanner2.StopScan(Arg.IsAny<Guid>()), Occurs.Exactly(5));

        }



    }
}
