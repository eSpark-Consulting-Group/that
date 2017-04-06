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

namespace APTAT.Core.UnitTests.DescentAPT
{
    public class DescentWatcherTest
    {
        private  List<IComputer> Targets=new List<IComputer>();
        private  List<Guid> Uids=new List<Guid>();
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
                Targets.Add(computer);
            }
            Uids = Targets.Select(c=>c.Uid).ToList();
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

            foreach (var uid in Uids) 
                descentWatcher.AddIdentity(uid);

            descentWatcher.Start();
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.True(called);
        }


        [Fact(DisplayName = "ExcludeIdentity test.")]
        public void ExcludeIdentityTest()
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

            foreach (var uid in Uids) 
                descentWatcher.AddIdentity(uid);

            descentWatcher.Start();
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.True(called);
        }
    }
}
