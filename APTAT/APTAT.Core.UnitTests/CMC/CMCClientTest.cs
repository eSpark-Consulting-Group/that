using System;
using System.Globalization;
using APTAT.Common;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace APTAT.Core.UnitTests.CMC
{
    public class CmcClientTest
    {
      //private readonly string[] _productIds = { "enp.c1.fat", "enp.c1.ufh" };
        private ICmcWebApiProvider _provider;
        private IMachineKeyProvider _keyProviderMock;
        private ISystemInfoProvider _systemInfoProvider;
        private ICultureInfoProvider _cultureInfoProvider;

        private ILaunchedRequest _launchedRequest;
        private IAutoDiscoveryFinishedRequest _autoDiscoveryFinishedRequest;
        private IScanStartedRequest _scanStartedRequest;
        private IScanFinishedRequest _scanFinishedRequest;
        private ScanUpdatedRequest _scanUpdatedRequest;
        private IReportGeneratedRequest _reportGeneratedRequest;
        private IReportPrintedOrSavedRequest _reportPrintedOrSavedRequest;
        private IApplicationClosedRequest _applicationClosedRequest;

        private CmcClient _client;

        private void MocksCreate()
        {
            _keyProviderMock = Mock.Create<IMachineKeyProvider>(Behavior.Strict);
            Mock.Arrange(() => _keyProviderMock.GlobalMachineKey).Returns("TBD");

            //ISystemInfo systemInfo = Mock.Create<ISystemInfo>(Behavior.Strict);
            //Mock.Arrange(() => systemInfo.FamilyName).Returns("Windows");
            //Mock.Arrange(() => systemInfo.Platform).Returns("x86");
            //Mock.Arrange(() => systemInfo.Version).Returns("6.4");
            //_systemInfoProvider = Mock.Create<ISystemInfoProvider>(Behavior.Strict);
            //Mock.Arrange(() => _systemInfoProvider.GetSystemInfo()).Returns(systemInfo);

            //string cultureName = CultureInfo.CurrentCulture.Name;
            //_cultureInfoProvider = Mock.Create<ICultureInfoProvider>(Behavior.Strict);
            //Mock.Arrange(() => _cultureInfoProvider.GetCulture()).Returns(cultureName);

            _launchedRequest = Mock.Create<ILaunchedRequest>(Behavior.Strict);
            Mock.Arrange(() => _launchedRequest.Acid).Returns(Guid.NewGuid().ToString());
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns("1.5.20801.85");
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns("enp.c1.fat");
        }

        private void MocksRequest()
        {
            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.Acid).Returns(Guid.NewGuid().ToString());
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns("1.5.20801.85");
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns("enp.c1.fat");

            _autoDiscoveryFinishedRequest = Mock.Create<IAutoDiscoveryFinishedRequest>();
            Mock.Arrange(() => _autoDiscoveryFinishedRequest.CountOfPotentiallyScanableDiscoveredComputers).Returns(27);
            Mock.Arrange(() => _autoDiscoveryFinishedRequest.DiscoveryStatus).Returns(DiscoveryStatus.Success);
            Mock.Arrange(() => _autoDiscoveryFinishedRequest.DiscoveryStatus).Returns(0);

            _scanStartedRequest = Mock.Create<IScanStartedRequest>();
            Mock.Arrange(() => _scanStartedRequest.CountOfScannedComputers).Returns(12);
            Mock.Arrange(() => _scanStartedRequest.ScanTarget).Returns(ScanTarget.DiscoveredByWorkGroup);
            Mock.Arrange(() => _scanStartedRequest.ScanTriggerType).Returns(ScanTriggerType.ScanNow);
            Mock.Arrange(() => _scanStartedRequest.ScanType).Returns(ScanType.LocalScan);

            _scanFinishedRequest = Mock.Create<IScanFinishedRequest>();
            Mock.Arrange(() => _scanFinishedRequest.CountOfFailedScans).Returns(4);
            Mock.Arrange(() => _scanFinishedRequest.CountOfMaliciousFiles).Returns(2);
            Mock.Arrange(() => _scanFinishedRequest.CountOfSuccessScans).Returns(23);
            Mock.Arrange(() => _scanFinishedRequest.CountOfUnknownFiles).Returns(24);
            Mock.Arrange(() => _scanFinishedRequest.ScanStatus).Returns(ScanStatus.Success);

            _scanUpdatedRequest = Mock.Create<ScanUpdatedRequest>();
            Mock.Arrange(() => _scanUpdatedRequest.CountOfNewMaliciousFilesWhichWasFoundAfterTheScanWasFinished).Returns(12);
            Mock.Arrange(() => _scanUpdatedRequest.CountOfNewUnknownFilesWhichWasFoundAfterTheScanWasFinished).Returns(21);

            _reportGeneratedRequest = Mock.Create<IReportGeneratedRequest>();
            Mock.Arrange(() => _reportGeneratedRequest.ReportKind).Returns(ReportKind.PerDevice);

            _reportPrintedOrSavedRequest = Mock.Create<IReportPrintedOrSavedRequest>();
            Mock.Arrange(() => _reportPrintedOrSavedRequest.ReportKind).Returns(ReportKind.PerDevice);
            Mock.Arrange(() => _reportPrintedOrSavedRequest.ReportAction).Returns(ReportAction.Saved);

            _applicationClosedRequest = Mock.Create<IApplicationClosedRequest>();
            Mock.Arrange(() => _applicationClosedRequest.Sid).Returns(Guid.NewGuid().ToString());
        }

        private void Mocks()
        {
            MocksCreate();
            MocksRequest();
            _provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);

            SystemInfo sinfo=new SystemInfo()
            {
                FamilyName = "Windows",
                Platform = "x86",
                Version = "6.4"
            };
            _systemInfoProvider =new SystemInfoProvider(sinfo);
            _cultureInfoProvider=new CultureInfoProvider();

            _client = new CmcClient(_provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
        }

        [Fact(DisplayName = "CmcClient constructor test.")]
        public void CmcClientConstructorTest()
        {
            //Arrange
            Mocks();
            //Act
            //Assert
           Mock.Assert(_client);
        }

        [Fact(DisplayName = "Launch method test.")]
        public void LaunchMethodTest()
        {
          //Arrange
            bool called = false;
            Mocks();
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { called = true; });
            //Act
            _client.Launched(_launchedRequest);
            //Assert
            Assert.True(called);
        }

        [Fact(DisplayName = "AutoDiscovery Finished method test.")]
        public void AutoDiscoveryFinishedMethodTest()
        {
            //Arrange
            bool called = false;
            Mocks();
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { called = true; });
            //Act
            _client.AutoDiscoveryFinished(_autoDiscoveryFinishedRequest);
            //Assert
            Assert.True(called);
        }

        [Fact(DisplayName = "ScanStarted method test.")]
        public void ScanStartedMethodTest()
        {
            //Arrange
            bool called = false;
            Mocks();
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { called = true; });
            //Act
            _client.ScanStarted(_scanStartedRequest);
            //Assert
            Assert.True(called);
        }

        [Fact(DisplayName = "ScanFinished method test.")]
        public void ScanFinishedMethodTest()
        {
            //Arrange
            bool called = false;
            Mocks();
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { called = true; });
            //Act
            _client.ScanFinished(_scanFinishedRequest);
            //Assert
            Assert.True(called);
        }

        [Fact(DisplayName = "ScanUpdated method test.")]
        public void ScanUpdatedMethodTest()
        {
            //Arrange
            bool called = false;
            Mocks();
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { called = true; });
            //Act
            _client.ScanUpdated(_scanUpdatedRequest);
            //Assert
            Assert.True(called);
        }

        [Fact(DisplayName = "ReportGenerated method test.")]
        public void ReportGeneratedMethodTest()
        {
            //Arrange
            bool called = false;
            Mocks();
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { called = true; });
            //Act
            _client.ReportGenerated(_reportGeneratedRequest);
            //Assert
            Assert.True(called);
        }

        [Fact(DisplayName = "ReportPrintedOrSaved method test.")]
        public void ReportPrintedOrSavedMethodTest()
        {
            //Arrange
            bool called = false;
            Mocks();
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { called = true; });
            //Act
            _client.ReportPrintedOrSaved(_reportPrintedOrSavedRequest);
            //Assert
            Assert.True(called);
        }

        [Fact(DisplayName = "AppicationClosed method test.")]
        public void AppicationClosedMethodTest()
        {
            //Arrange
            bool called = false;
            Mocks();
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { called = true; });
            //Act
            _client.AppicationClosed(_applicationClosedRequest);
            //Assert
            Assert.True(called);
        }

        [Fact(DisplayName = "Check full chain on Client calls test.")]
        public void TestCmcClientCalls()
        {
            //Arrange
            int cnt = 0;
            Mocks();
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { cnt++; });
            //Act
            _client.Launched(_launchedRequest);
            _client.AutoDiscoveryFinished(_autoDiscoveryFinishedRequest);
            _client.ScanStarted(_scanStartedRequest);
            _client.ScanFinished(_scanFinishedRequest);
            _client.ScanUpdated(_scanUpdatedRequest);
            _client.ReportGenerated(_reportGeneratedRequest);
            _client.ReportPrintedOrSaved(_reportPrintedOrSavedRequest);
            _client.AppicationClosed(_applicationClosedRequest);
            //Assert
            Assert.Equal(8,cnt);
        }



        [Fact(DisplayName = "..... test.")]
        public void TestCmcClient3Calls()
        {
            //Arrange
            int cnt = 0;
            Mocks();
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { cnt++; });
            //Act
            _client.Launched(_launchedRequest);
            _client.AutoDiscoveryFinished(_autoDiscoveryFinishedRequest);
            _client.ScanStarted(_scanStartedRequest);
            _client.ScanFinished(_scanFinishedRequest);
            _client.ScanUpdated(_scanUpdatedRequest);
            _client.ReportGenerated(_reportGeneratedRequest);
            _client.ReportPrintedOrSaved(_reportPrintedOrSavedRequest);
            _client.AppicationClosed(_applicationClosedRequest);
            //Assert
            Assert.Equal(8, cnt);
        }

    }
}
