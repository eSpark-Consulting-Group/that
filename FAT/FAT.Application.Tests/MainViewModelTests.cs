using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Directory;
using FAT.Application.Models;
using FAT.Application.Services;
using System;
using System.Net;
using System.Threading.Tasks;
using Telerik.JustMock;
using Xunit;

namespace FAT.Application.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var service1 = Mock.Create<ITargetsDiscoveryService>(Behavior.Strict);
            var service2 = Mock.Create<IAptService>(Behavior.Strict);
            var service3 = Mock.Create<IStorageService>(Behavior.Strict);
            var service4 = Mock.Create<IScanCounterManager>(Behavior.Strict);
            var service5 = Mock.Create<IScanOperationsManager>(Behavior.Strict);
            var service6 = Mock.Create<IScanTimer>(Behavior.Strict);
            var statisticsService = Mock.Create<IStatisticsService>(Behavior.Strict);
            var col = Mock.Create<IScanCounterCollection>(Behavior.Strict);
            var col2 = Mock.Create<IScanOperationCollection>(Behavior.Strict);

            Mock.Arrange(() => service4.CounterCollection).Returns(col);
            Mock.Arrange(() => service5.OperationCollection).Returns(col2);

            //act
            MainViewModel target = new MainViewModel(service1, service2, service3, service4, service5, service6, null);

            Assert.NotNull(target.OnLoadedCommand);
            Assert.NotNull(target.StartScanCommand);
            Assert.NotNull(target.StopScanCommand);
            Assert.NotNull(target.SelectCommand);
            Assert.NotNull(target.SubmitEmailCommand);
            Assert.NotNull(target.EditEmailCommand);
            Assert.NotNull(target.StopAutoDiscoveryCommand);
            Assert.NotNull(target.StartScanWithDiscoveryResultsCommand);
            Assert.NotNull(target.StartAutoDiscoveryCommand);
            Assert.NotNull(target.OpenPreviousScansCommand);
            Assert.NotNull(target.OpenCurrentScanCommand);
            Assert.NotNull(target.OpenPreviousScanDetailsCommand);
            Assert.NotNull(target.SearchPreviousScansCommand);
            Assert.NotNull(target.ClearScanKeywordCommand);
            Assert.Equal("/Views/StartingPage.xaml", target.Uri);
            Assert.Equal("80", target.TcpPort);
            Assert.Equal("53", target.UdpPort);
            Assert.Null(target.PreviousScansViewModel);
            Assert.Same(target, target.DataContextSwitcher);
            Assert.False(target.Loaded);
            Assert.Empty(target.ComputersHeader);
            Assert.Null(target.Computers);
            Assert.Null(target.Files);
            Assert.Null(target.DiscoveryResults);
            Assert.Same(col, target.ScanCounters);
            Assert.Same(col2, target.ScanOperations);
            Mock.Assert(service4);
        }

        [Theory]
        [InlineData("testname", "DiscoveryResults")]
        [InlineData(null, "HasCurrentDomainName")]
        public async void OnLoadedCommandTest(string name, string propertyChanged)
        {
            var service1 = Mock.Create<ITargetsDiscoveryService>(Behavior.Strict);
            var service2 = Mock.Create<IAptService>(Behavior.Strict);
            var service3 = Mock.Create<IStorageService>(Behavior.Strict);
            var service4 = Mock.Create<IScanCounterManager>(Behavior.Strict);
            var service5 = Mock.Create<IScanOperationsManager>(Behavior.Strict);
            var service6 = Mock.Create<IScanTimer>(Behavior.Strict);
            var service7 = Mock.Create<IProxySettingsViewModel>(Behavior.Strict);
            var service8 = Mock.Create<ICmcClient>(Behavior.Strict);
           

            Func<GetDomainNameResponse> func = () => new GetDomainNameResponse(name, null);

            Mock.Arrange(() => service4.Clear()).MustBeCalled();
            Mock.Arrange(() => service1.GetCurrentDomainName()).Returns(Task.Run(func)).MustBeCalled();
            Mock.Arrange(() => service1.StartTargetsAutoDiscovery()).MustBeCalled();
            Mock.Arrange(() => service1.GetLoggedInUser()).Returns(name).MustBeCalled();
            Mock.Arrange(() => service5.HasPreviousScans()).Returns(Arg.AnyBool).OnAllThreads().MustBeCalled();
            Mock.Arrange(() => service5.GetEmail()).Returns(Arg.AnyString).OnAllThreads().MustBeCalled();
            Mock.Arrange(() => service6.ScanActive).Returns(false);
            Mock.Arrange(() => service7.GetProxyCredentials()).DoNothing();
            Mock.Arrange(() => service8.ApplicationLaunched(Arg.IsAny<IApplicationLaunchedRequest>(), Arg.IsAny<NetworkCredential>())).DoNothing().MustBeCalled();

            var statisticsService = new StatisticsService(service7, service8, Mock.Create<APTAT.Common.IScheduler>());
            statisticsService.OverrideEnabledSendData(true);
            MainViewModel target = new MainViewModel(service1, service2, service3, service4, service5, service6, statisticsService);

            //act
            Action a = () =>
            {
                target.OnLoadedCommand.Execute(null);
                Mock.Raise(() => service1.DeploymentTargetsAutoResolving += null, args: new[] { new DeploymentTargetResolvingEventArgs(null, 0.5) });
            };

            Assert.PropertyChanged(target, propertyChanged, a);
            await Task.Delay(100);
            Assert.True(target.Loaded);
            Assert.True(target.IsAutoDiscoveryInProgress);
            Assert.NotNull(target.DiscoveryResults);
            Assert.Equal("/Views/DiscoveryResults.xaml", target.Uri);
            Assert.Equal(name, target.CurrentDomainName);
            Assert.Equal(name, target.CurrentUser);
            Assert.Equal(50, target.Percentage);
            Mock.Assert(service1);
            Mock.Assert(service4);
            Mock.Assert(service5);
            Mock.Assert(service8);
        }

        [Fact]
        public void ViewLoadedCommandTest()
        {
            var service1 = Mock.Create<ITargetsDiscoveryService>(Behavior.Strict);
            var service2 = Mock.Create<IAptService>(Behavior.Strict);
            var service3 = Mock.Create<IStorageService>(Behavior.Strict);
            var service4 = Mock.Create<IScanCounterManager>(Behavior.Strict);
            var service5 = Mock.Create<IScanOperationsManager>(Behavior.Strict);
            var service6 = Mock.Create<IScanTimer>(Behavior.Strict);

            Mock.ArrangeSet(() => service4.SelectedOrdersChanged = Arg.IsAny<Action<int[]>>()).MustBeCalled();
            Mock.Arrange(() => service4.Clear()).MustBeCalled();
            Mock.ArrangeSet(() => service6.ScanIterationHandler = Arg.IsAny<Action>()).MustBeCalled();
            Mock.Arrange(() => service6.Start()).DoNothing().MustBeCalled();
            Mock.Arrange(() => service6.ScanActive).Returns(true);
            Mock.Arrange(() => service3.GetValue<DevicesSourceDescription>(Arg.AnyString, Arg.AnyBool)).Returns(new DevicesSourceDescription());
            Mock.Arrange(() => service3.ContainsKey(Arg.AnyString)).Returns(false);

            MainViewModel target = new MainViewModel(service1, service2, service3, service4, service5, service6, null);

            //act
            target.ViewLoadedCommand.Execute(null);
            Mock.Raise(() => service1.DirectoryItemsResolved += null, args: new[] { new DirectoryItemsResolvedEventArgs(null, target) });
            Mock.Raise(() => service2.FatScanStarted += null, args: new[] { new APTAT.Core.ScanStartedEventArgs() });
            Mock.Raise(() => service2.FatScanCompleted += null, args: new[] { new APTAT.Application.Services.EventArgs.ErrorEventArgs(null) });
            Mock.Raise(() => service2.CancelScanStarted += null, args: new[] { new EventArgs() });

            Assert.True(target.CancelInProgress);
            Assert.True(target.IsScanCancelled);
            Assert.True(target.IsScanInProgress);
            Mock.Assert(service4);
            Mock.Assert(service6);
        }
    }
}