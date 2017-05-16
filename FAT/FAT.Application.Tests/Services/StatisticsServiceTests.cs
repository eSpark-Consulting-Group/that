
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using APTAT.DeploymentCore.Directory;
using FAT.Application.Models;
using FAT.Application.Services;
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
            Mock.Arrange(() => service2.ApplicationLaunched(Arg.IsAny<IApplicationLaunchedRequest>())).DoNothing().MustBeCalled();

            var target = new StatisticsService(service1, service2, null);
            target.IsEnabledSendData = true;

            //act
            CMCStatisticsMessages<IApplicationLaunchedRequest>.Send();

            await Task.Delay(1000);

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
                _.DiscoveryStatus == status) 
            )).DoNothing().MustBeCalled();

            var target = new StatisticsService(service1, service2, null);
            target.IsEnabledSendData = true;

            //act
            CMCStatisticsMessages<IAutoDiscoveryFinishedRequest>.Send(request =>
            {
                request.CountOfPotentiallyScanableDiscoveredComputers = count;
                request.DiscoveryStatus = status;
            });

            await Task.Delay(500);

            Mock.Assert(service2);
        }

        [Theory]
        [InlineData(ScanTriggerType.CustomScan, ETargetsDiscovererType.ActiveDirectory, ScanType.Custom, 0, 0)]
        [InlineData(ScanTriggerType.ScanNow, ETargetsDiscovererType.AutoDiscovery, ScanType.Full, 1, 2)]
        public async void ScanStartedTest(ScanTriggerType triggerType, ETargetsDiscovererTypeRequest discovererTarget, ScanType type, int count, int totalCount)
        {
            var service1 = Mock.Create<IProxySettingsViewModel>(Behavior.Strict);
            var service2 = Mock.Create<ICmcClient>(Behavior.Strict);

            Mock.Arrange(() => service1.GetProxyCredentials()).DoNothing();
            Mock.Arrange(() => service2.ScanStarted(Arg.Matches<IScanStartedRequest>(_ =>
                _.CountOfScannedComputers == count &&
                _.TotalCountOfDiscoveredComputers == totalCount &&
                _.ScanTarget == discovererTarget &&
                _.ScanTriggerType == triggerType &&
                _.ScanType == type)
            )).DoNothing().MustBeCalled();

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

            await Task.Delay(500);

            Mock.Assert(service2);
        }

        [Theory]
        [InlineData(APTAT.Core.CMC.ScanStatus.Failed, 0, 0, 0, 0)]
        [InlineData(APTAT.Core.CMC.ScanStatus.Success, 1, 2, 3, 4)]
        public async void ScanFinishedTest(APTAT.Core.CMC.ScanStatus status, int count, int count2, int count3, int count4)
        {
            var service1 = Mock.Create<IProxySettingsViewModel>(Behavior.Strict);
            var service2 = Mock.Create<ICmcClient>(Behavior.Strict);

            Mock.Arrange(() => service1.GetProxyCredentials()).DoNothing();
            Mock.Arrange(() => service2.ScanFinished(Arg.Matches<IScanFinishedRequest>(_ =>
                _.CountOfFailedScans == count &&
                _.CountOfMaliciousFiles == count2 &&
                _.CountOfSuccessScans == count3 &&
                _.CountOfUnknownFiles == count4 &&
                _.ScanStatus == status)
            )).DoNothing().MustBeCalled();

            var target = new StatisticsService(service1, service2, null);
            target.IsEnabledSendData = true;

            //act
            CMCStatisticsMessages<IScanFinishedRequest>.Send(request =>
            {
                request.CountOfFailedScans = count;
                request.CountOfMaliciousFiles = count2;
                request.CountOfSuccessScans = count3;
                request.CountOfUnknownFiles = count4;
                request.ScanStatus = status;
            });

            await Task.Delay(500);

            Mock.Assert(service2);
        }
    }
}
