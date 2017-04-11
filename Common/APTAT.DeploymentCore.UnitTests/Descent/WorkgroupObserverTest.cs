using System;
using System.Collections.Generic;
using System.Threading;
using APTAT.DeploymentCore.Descent;
using Telerik.JustMock;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Descent
{
    public class WorkgroupObserverTest
    {
        [Fact(DisplayName = "Constructor test.")]
        public void ConstructorTest()
        {
            INetworkBrowser networkBrowser = Mock.Create<INetworkBrowser>();
            WorkgroupObserver workgroupObserver = new WorkgroupObserver(networkBrowser);
            Mock.Assert(workgroupObserver);
        }

        [Fact(DisplayName = "BeginSearch test")]
        public void BeginSearch()
        {
            //Arrange
            bool raised = false;
            string DomainName = "WORKGROUP";
            ManualResetEventSlim ev = new ManualResetEventSlim(false);
            var iNetworkBrowser = Mock.Create<INetworkBrowser>(Behavior.Strict);
            Action<IComputer[]> action = (a) => { raised = true; };
            var computers = new List<string>();
            for (int i = 0; i < 5; i++) 
                computers.Add(Guid.NewGuid().ToString());

            Mock.Arrange(() => iNetworkBrowser
            .GetNetworkComputers(Arg.IsAny<ServerType>(), Arg.IsAny<string>()))
            .Returns(new NetworkComputersResult()
            {
                Computers = computers,
                Result = NetServerEnumResults.NERR_Success
            });

            //Act
            WorkgroupObserver workgroupObserver = new WorkgroupObserver(iNetworkBrowser);
            workgroupObserver.BeginSearch(DomainName, action);

            ev.Wait(TimeSpan.FromSeconds(10));
            Assert.True(raised);
        }
    }
}
