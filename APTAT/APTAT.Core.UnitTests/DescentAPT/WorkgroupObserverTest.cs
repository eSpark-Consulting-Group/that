using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Descent;
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
    public class WorkgroupObserverTest
    {
        public WorkgroupObserverTest()
        {
        }

        [Fact(DisplayName = "Constructor test.")]
        public void ConstructorTest()
        {
            INetworkBrowser networkBrowser = Mock.Create<INetworkBrowser>();
            WorkgroupObserver workgroupObserver = new WorkgroupObserver(networkBrowser);
            Mock.Assert(workgroupObserver);
        }


        //[Fact(DisplayName = "BeginSearch.")]
        //public void BeginSearch()
        //{
        //    bool iscalled = false;
        //    ManualResetEventSlim ev = new ManualResetEventSlim(false);
        //    Action<IComputer[]> action=(a)=>{
        //        iscalled = true;
        //    };
        //    INetworkBrowser networkBrowser = Mock.Create<INetworkBrowser>();
        //    WorkgroupObserver workgroupObserver = new WorkgroupObserver(networkBrowser);
        //    workgroupObserver.BeginSearch("WORKGROUP", action);
        //    ev.Wait(TimeSpan.FromSeconds(3));
        //}

    }
}
