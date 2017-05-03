
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using APTAT.DeploymentCore.Directory;
using FAT.Application.Models;
using FAT.Application.Services;
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
        public async void ApplicationLaunchedTest()
        {
            var service1 = Mock.Create<IProxySettingsViewModel>(Behavior.Strict);
            var service2 = Mock.Create<ICmcClient>(Behavior.Strict);

            Mock.Arrange(() => service1.GetProxyCredentials()).DoNothing();
            Mock.Arrange(() => service2.ApplicationLaunched(Arg.IsAny<IApplicationLaunchedRequest>(), Arg.IsAny<NetworkCredential>())).DoNothing().MustBeCalled();

            var target = new StatisticsService(service1, service2, null);
            target.IsEnabledSendData = true;

            //act
            CMCStatisticsMessages<IApplicationLaunchedRequest>.Send();

            await Task.Delay(100);

            Mock.Assert(service2);
        }

        [Theory]
        [InlineData(DiscoveryStatus.Failed, 0)]
        [InlineData(DiscoveryStatus.Skipped, 1)]
        public async void AutoDiscoveryFinishedTest(DiscoveryStatus status, int count)
        {
            var service1 = Mock.Create<IProxySettingsViewModel>(Behavior.Strict);
            var service2 = Mock.Create<ICmcClient>(Behavior.Strict);

            Mock.Arrange(() => service1.GetProxyCredentials()).DoNothing();
            Mock.Arrange(() => service2.AutoDiscoveryFinished(Arg.Matches<IAutoDiscoveryFinishedRequest>(_ => 
                _.CountOfPotentiallyScanableDiscoveredComputers == count && 
                _.DiscoveryStatus == status), 
            Arg.IsAny<NetworkCredential>())).DoNothing().MustBeCalled();

            var target = new StatisticsService(service1, service2, null);
            target.IsEnabledSendData = true;

            //act
            CMCStatisticsMessages<IAutoDiscoveryFinishedRequest>.Send(request =>
            {
                request.CountOfPotentiallyScanableDiscoveredComputers = count;
                request.DiscoveryStatus = status;
            });

            await Task.Delay(100);

            Mock.Assert(service2);
        }

        [Theory]
        [InlineData(ScanTriggerType.CustomScan, ETargetsDiscovererType.ActiveDirectory, ScanType.Custom, 0, 0)]
        [InlineData(ScanTriggerType.ScanNow, ETargetsDiscovererType.AutoDiscovery, ScanType.Full, 1, 1)]
        public async void ScanStartedTest(ScanTriggerType triggerType, ETargetsDiscovererType discovererTarget, ScanType type, int count, int totalCount)
        {
            var service1 = Mock.Create<IProxySettingsViewModel>(Behavior.Strict);
            var service2 = Mock.Create<ICmcClient>(Behavior.Strict);

            Mock.Arrange(() => service1.GetProxyCredentials()).DoNothing();
            Mock.Arrange(() => service2.ScanStarted(Arg.Matches<IScanStartedRequest>(_ =>
                _.CountOfScannedComputers == count &&
                _.TotalCountOfDiscoveredComputers == totalCount &&
                _.ScanTarget == discovererTarget &&
                _.ScanTriggerType == triggerType &&
                _.ScanType == type),
            Arg.IsAny<NetworkCredential>())).DoNothing().MustBeCalled();

            var target = new StatisticsService(service1, service2, null);
            target.IsEnabledSendData = true;

            //act
            CMCStatisticsMessages<IScanStartedRequest>.Send(request =>
            {
                request.CountOfScannedComputers = count;
                request.TotalCountOfDiscoveredComputers = totalCount;
                request.ScanTarget = discovererTarget;
                request.ScanTriggerType = triggerType;
                request.ScanType = type;
            });

            await Task.Delay(100);

            Mock.Assert(service2);
        }
    }
}
