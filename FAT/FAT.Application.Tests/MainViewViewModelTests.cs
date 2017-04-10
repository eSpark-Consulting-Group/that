using FAT.Application.Models;
using FAT.Application.Services;
using System;
using Telerik.JustMock;
using Xunit;

namespace FAT.Application.Tests
{
    public class MainViewViewModelTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var service1 = Mock.Create<ITargetsDiscoveryService>(Behavior.Strict);
            var service2 = Mock.Create<IAptService>(Behavior.Strict);
            var service3 = Mock.Create<IStorageService>(Behavior.Strict);
            var service4 = Mock.Create<IScanCounterManager>(Behavior.Strict);
            var service5 = Mock.Create<IScanOperationsManager>(Behavior.Strict);
            var col = Mock.Create<IScanCounterCollection>(Behavior.Strict);
            var col2 = Mock.Create<IScanOperationCollection>(Behavior.Strict);

            Mock.Arrange(() => service4.CounterCollection).Returns(col);
            Mock.ArrangeSet(() => service4.SelectedOrdersChanged = Arg.IsAny<Action<int[]>>()).MustBeCalled();
            Mock.Arrange(() => service5.OperationCollection).Returns(col2);

            //act
            MainViewViewModel target = new MainViewViewModel(service1, service2, service3, service4, service5);

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
    }
}
