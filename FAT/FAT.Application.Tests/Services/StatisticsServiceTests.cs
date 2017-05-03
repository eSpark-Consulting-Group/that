
using APTAT.Common;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using FAT.Application.Models;
using FAT.Application.Services;
using System;
using System.Net;
using System.Threading.Tasks;
using Telerik.JustMock;
using Xunit;
using static FAT.Application.Messages.AppMessages;

namespace FAT.Application.Tests.Services
{
    public class StatisticsServiceTests
    {
        [Fact]
        public async void ApplicationLaunched()
        {
            var service1 = Mock.Create<IProxySettingsViewModel>(Behavior.Strict);
            var service2 = Mock.Create<ICmcClient>(Behavior.Strict);
            var service3 = Mock.Create<IScheduler>(Behavior.Strict);

            Mock.Arrange(() => service3.SetInterval(Arg.IsAny<TimeSpan>())).DoNothing();
            Mock.Arrange(() => service3.Start(Arg.IsAny<bool>())).DoNothing();
            Mock.Arrange(() => service1.GetProxyCredentials()).DoNothing();
            Mock.Arrange(() => service2.ApplicationLaunched(Arg.IsAny<IApplicationLaunchedRequest>(), Arg.IsAny<NetworkCredential>())).DoNothing().MustBeCalled();

            var target = new StatisticsService(service1, service2, service3);
            target.IsEnabledSendData = true;

            //act
            CMCStatisticsMessages<IApplicationLaunchedRequest>.Send();

            await Task.Delay(100);

            Mock.Assert(service2);
        }
    }
}
