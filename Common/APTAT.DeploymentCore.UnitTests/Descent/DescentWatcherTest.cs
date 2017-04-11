using APTAT.Common;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.JustMock;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Descent
{
    public class DescentWatcherTest
    {
        private readonly List<IComputer> _targets=new List<IComputer>();
        private readonly List<Guid> _uids=new List<Guid>();
        public DescentWatcherTest()
        {
            for (int i = 0; i <= 9; i++)
            {
                var computer = Mock.Create<IComputer>();
                Mock.Arrange(() => computer.Uid).Returns(Guid.NewGuid());
                Mock.Arrange(() => computer.IPAddress).Returns("127.0.0." + (i + 1));
                Mock.Arrange(() => computer.IsLocalComputer).Returns(true);
                Mock.Arrange(() => computer.IsOnline).Returns(true);
                Mock.Arrange(() => computer.Name).Returns("ComputerName_" + i);
                Mock.Arrange(() => computer.Source).Returns(DeploymentTargetSource.Ip);
                _targets.Add(computer);
            }
            _uids = _targets.Select(c=>c.Uid).ToList();
        }

        [Fact(DisplayName = "DescentWatcher Constructor test.")]
        public void DescentWatcherConstructorTest()
        {
            //Arrange
            ManualResetEventSlim ev = new ManualResetEventSlim(false);
            TimeSpan timeout = new TimeSpan(0, 2, 0);
            IScheduler timer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => timer.SetInterval(Arg.IsAny<TimeSpan>()));
            Mock.Arrange(() => timer.Start(false)).DoInstead(() => ev.Set()).Returns(true);
            bool called = false;
            Action<Guid> action = (guid) => { called = true; };

            //Act
            DescentWatcher descentWatcher = new DescentWatcher(timeout, action, timer);

            //Assert
            Assert.Equal(descentWatcher.DescentTimeout, timeout);
        }


        [Fact(DisplayName = "Dispose test.")]
        public void DisposeTest()
        {
            //Arrange
            TimeSpan timeout = new TimeSpan(0, 0, -10);
            IScheduler timer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => timer.SetInterval(Arg.IsAny<TimeSpan>()));
            Mock.Arrange(() => timer.Start(false)).Returns(true);
            Mock.Arrange(() => timer.Stop());
            Mock.Arrange(() => timer.Dispose()).MustBeCalled();
            Action<Guid> action = (guid) => { };

            //Act
            DescentWatcher descentWatcher = new DescentWatcher(timeout, action, timer);
            descentWatcher.Start();
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Mock.AssertAll(timer);
        }


        [Fact(DisplayName = "Check TimerCallBack Calling test.")]
        public void CheckTimerCallBackCallingTest()
        {
            //Arrange
            bool called = false;
            Action<Guid> action = (guid) => { called = true; };
            ManualResetEventSlim ev = new ManualResetEventSlim(false);
            TimeSpan timeout = new TimeSpan(0, 0, -10);
            IScheduler timer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => timer.SetInterval(Arg.IsAny<TimeSpan>()));
            Mock.Arrange(() => timer.Start(false)).DoInstead(() => ev.Set()).Returns(true);
            Mock.Arrange(() => timer.Stop());

            //Act
            DescentWatcher descentWatcher = new DescentWatcher(timeout, action, timer);

            foreach (var uid in _uids) 
                descentWatcher.AddIdentity(uid);

            descentWatcher.Start();
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.True(called);
        }


        [Fact(DisplayName = "Check that on call Start method" +
            "all values from computerActivity are removed test.")]
        public void CheckThatAllValuesAreExcudedFromComputerActivityTest()
        {
            //Arrange
            bool called = false;
            Action<Guid> action = (guid) => { called = true; };
            TimeSpan timeout = new TimeSpan(0, 0, -10);
            IScheduler timer = Mock.Create<IScheduler>(Behavior.Strict);

            Mock.Arrange(() => timer.SetInterval(Arg.IsAny<TimeSpan>()));
            Mock.Arrange(() => timer.Start(false)).Returns(true);
            Mock.Arrange(() => timer.Stop());
            Mock.Arrange(() => timer.Dispose()).OccursOnce();
            DescentWatcher descentWatcher = new DescentWatcher(timeout, action, timer);

            foreach (var uid in _uids.Take(1))
                descentWatcher.AddIdentity(uid);

            //Act
            descentWatcher.Start();
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            descentWatcher.Start();
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.True(called);
            Mock.Assert(() => timer.Dispose(),Occurs.Once());
        }


        [Fact(DisplayName = "Check that timeoutCallback calls 10 times " +
            "in case when computer activity value" +
            "is less than dead line value test.")]
        public void CheckThatTimeoutCallbackCalls10TimesTest()
        {
            //Arrange
            int cntTimeoutCallbackCalls = 0;
            Action<Guid> action = (guid) => {cntTimeoutCallbackCalls++; };
            TimeSpan timeout = new TimeSpan(0, 0, -10);
            IScheduler timer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => timer.SetInterval(Arg.IsAny<TimeSpan>()));
            Mock.Arrange(() => timer.Start(false)).Returns(true);
            DescentWatcher descentWatcher = new DescentWatcher(timeout, action, timer);
            foreach (var uid in _uids)
                descentWatcher.AddIdentity(uid);

            //Act
            descentWatcher.Start();
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Mock.Equals(cntTimeoutCallbackCalls, 10);
        }
        
    }
}
