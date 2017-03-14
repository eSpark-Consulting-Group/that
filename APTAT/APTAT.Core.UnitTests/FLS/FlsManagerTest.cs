using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Xunit;
using APTAT.Common;

namespace APTAT.Core.UnitTests.FLS
{
    public class FlsManagerTest
    {
        [Fact]
        public void UpdateFlsPortsTest()
        {
            //Arrange
            var apiProvider = Mock.Create<IFlsApiProvider>(Behavior.Strict);

            var timer = Mock.Create<ITimer>();
            Mock.Arrange(() => apiProvider.UpdateFlsPorts(Arg.Is(32), Arg.Is(98)))
                .DoNothing().MustBeCalled();

            //Act
            var fls = new FlsManager(apiProvider,timer);
            fls.UpdateFlsPorts(32, 98);
            
            //Assert
            Mock.Assert(apiProvider);
            fls.Dispose();
        }

        [Fact]
        public void EnqueueTest()
        {
            //Arrange
            var apiProvider = Mock.Create<IFlsApiProvider>(Behavior.Strict);
            var timer = Mock.Create<ITimer>();
            var flsResponce = Mock.Create<IFlsResponse>();

            var hashes = new String[] { "hash1", "hash2" };
            Mock.Arrange(() => apiProvider.RequestType0(Arg.Matches<String[]>(hashesInput 
                => Enumerable.SequenceEqual(hashesInput.OrderBy(h => h),hashes.OrderBy(h => h)))))
                .Returns(flsResponce)
                .OccursOnce();
            //Act
            var fls = new FlsManager(apiProvider,timer);
            
            fls.Enqueue(hashes);
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Mock.Assert(apiProvider);
            fls.Dispose();
        }


        [Fact]
        public void EnqueueShouldStartTimerTest()
        {
            //Arrange
            var apiProvider = Mock.Create<IFlsApiProvider>();
            var timer = Mock.Create<ITimer>(Behavior.Strict);
            var hashes = new String[] { "hash1", "hash2" };
            Mock.Arrange(() => timer.SetInterval(Arg.Matches<TimeSpan>(
                ts => ts.Equals(TimeSpan.FromSeconds(2))))).OccursOnce();
            Mock.Arrange(() => timer.Start(false))
                .Returns(true).OccursOnce();
            Mock.Arrange(() => timer.Dispose());
                //.OccursOnce();
            //Act
            var fls = new FlsManager(apiProvider, timer);
            fls.Enqueue(hashes);

            //Assert
            Mock.Assert(timer);
            fls.Dispose();
        }

        [Fact]
        public void DisposeTestShouldDisposeTimer()
        {
            //Arrange
            var apiProvider = Mock.Create<IFlsApiProvider>();
            var timer = Mock.Create<ITimer>(Behavior.Strict);
            Mock.Arrange(() => timer.Dispose())
                .OccursOnce();
            //Act
            var fls = new FlsManager(apiProvider, timer);
            fls.Dispose();

            //Assert
            Mock.Assert(timer);
        }








        

    }
}
