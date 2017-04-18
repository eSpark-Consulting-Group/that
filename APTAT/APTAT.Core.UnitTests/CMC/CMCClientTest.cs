using System;
using System.Globalization;
using APTAT.Common;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using Telerik.JustMock;
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

        private void MocksCreate()
        {
            _keyProviderMock = Mock.Create<IMachineKeyProvider>(Behavior.Strict);
            Mock.Arrange(() => _keyProviderMock.GlobalMachineKey).Returns("TBD");

            ISystemInfo systemInfo = Mock.Create<ISystemInfo>(Behavior.Strict);
            Mock.Arrange(() => systemInfo.FamilyName).Returns("Windows");
            Mock.Arrange(() => systemInfo.Platform).Returns("x86");
            Mock.Arrange(() => systemInfo.Version).Returns("6.4");

            _systemInfoProvider = Mock.Create<ISystemInfoProvider>(Behavior.Strict);
            Mock.Arrange(() => _systemInfoProvider.GetSystemInfo()).Returns(systemInfo);

            string cultureName = CultureInfo.CurrentCulture.Name;
            _cultureInfoProvider = Mock.Create<ICultureInfoProvider>(Behavior.Strict);
            Mock.Arrange(() => _cultureInfoProvider.GetCulture()).Returns(cultureName);

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


        [Fact(DisplayName = "CmcClient constructor test.")]
        public void CmcClientConstructorTest()
        {
          //Arrange
            MocksCreate();
            MocksRequest();
            _provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            CmcClient client = new CmcClient(_provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
            //Assert
           Mock.Assert(client);
        }


        [Fact(DisplayName = "Launch method test.")]
        public void LaunchMethodTest()
        {
          //Arrange
            MocksCreate();
            MocksRequest();
            bool called = false;
            _provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { called = true; });
            CmcClient client = new CmcClient(_provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);

            //Act
            client.Launched(_launchedRequest);

            //Assert
            Assert.True(called);
        }

        //[Fact(DisplayName = "AutoDiscovery Finished method test.")]


   

        [Fact(DisplayName = "Check full chain on Client calls test.")]
        public void TestCmcClientCalls()
        {
            //Arrange
            int cnt = 0;
            MocksCreate();
            MocksRequest();
            _provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => _provider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => _provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { cnt++; });
            CmcClient client =new CmcClient(_provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);

            //Act
            client.Launched(_launchedRequest);
            client.AutoDiscoveryFinished(_autoDiscoveryFinishedRequest);
            client.ScanStarted(_scanStartedRequest);
            client.ScanFinished(_scanFinishedRequest);
            client.ScanUpdated(_scanUpdatedRequest);
            client.ReportGenerated(_reportGeneratedRequest);
            client.ReportPrintedOrSaved(_reportPrintedOrSavedRequest);
            client.AppicationClosed(_applicationClosedRequest);

            //Assert
            Assert.Equal(8,cnt);


        }
    }
}
