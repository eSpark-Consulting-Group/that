using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using APTAT.Common;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using Telerik.JustMock;
using Telerik.JustMock.Expectations.Abstraction;
using Telerik.JustMock.Helpers;
using Xunit;

namespace APTAT.Core.UnitTests.CMC
{

    public class CmcClientTest
    {
      //private readonly string[] _productIds = { "enp.c1.fat", "enp.c1.ufh" };

        private readonly IMachineKeyProvider _keyProviderMock;
        private readonly ISystemInfoProvider _systemInfoProvider;
        private readonly ICultureInfoProvider _cultureInfoProvider;

        private ILaunchedRequest _launchedRequest;
        private IAutoDiscoveryFinishedRequest _autoDiscoveryFinishedRequest;
        private IScanStartedRequest _scanStartedRequest;
        private IScanFinishedRequest _scanFinishedRequest;
        private ScanUpdatedRequest _scanUpdatedRequest;
        private IReportGeneratedRequest _reportGeneratedRequest;
        private IReportPrintedOrSavedRequest _reportPrintedOrSavedRequest;
        private IApplicationClosedRequest _applicationClosedRequest;

        public CmcClientTest()
        {
            _keyProviderMock = Mock.Create<IMachineKeyProvider>(Behavior.Strict);
            Mock.Arrange(() => _keyProviderMock.GlobalMachineKey).Returns("TBD");

            _systemInfoProvider = Mock.Create<ISystemInfoProvider>(Behavior.Strict);
            SystemInfo systemInfo = (new SystemInfoProvider()).GetSystemInfo();
            Mock.Arrange(() => _systemInfoProvider.GetSystemInfo()).Returns(systemInfo);

            string cultureName = CultureInfo.CurrentCulture.Name;
            _cultureInfoProvider = Mock.Create<ICultureInfoProvider>(Behavior.Strict);
            Mock.Arrange(() => _cultureInfoProvider.GetCulture()).Returns(cultureName);
        }

        private void MocksCreate()
        {
            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.AcId).Returns(Guid.NewGuid().ToString());
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
            Mock.Arrange(() => _applicationClosedRequest.SId).Returns(Guid.NewGuid().ToString());
        }

        [Fact(DisplayName = "CmcClient constructor test.")]
        public void CmcClientConstructorTest()
        {
            //Arrange
            MocksCreate();
            var provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            var client = new CmcClient(provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
            //Assert
            Mock.Assert(client);
        }

        [Fact(DisplayName = "Launch method test.")]
        public void LaunchMethodTest()
        {
          //Arrange
            MocksCreate();
         
            var provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => provider.SendData(Arg.IsAny<IGeneralInfo>())).OccursOnce();
            var client = new CmcClient(provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
            client.Launched(_launchedRequest);
            //Assert
         
            Mock.Assert(() => provider.SendData(Arg.IsAny<IGeneralInfo>()), Occurs.Once());
        }

        [Fact(DisplayName = "AutoDiscovery Finished method test.")]
        public void AutoDiscoveryFinishedMethodTest()
        {
            //Arrange
            MocksCreate();
            var provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => provider.SendData(Arg.IsAny<IGeneralInfo>())).OccursOnce();
            var client = new CmcClient(provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
            client.AutoDiscoveryFinished(_autoDiscoveryFinishedRequest);
            //Assert
            Mock.Assert(() => provider.SendData(Arg.IsAny<IGeneralInfo>()), Occurs.Once());
        }

        [Fact(DisplayName = "ScanStarted method test.")]
        public void ScanStartedMethodTest()
        {
            //Arrange
            MocksCreate();
            var provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => provider.SendData(Arg.IsAny<IGeneralInfo>())).OccursOnce();
            var client = new CmcClient(provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
            client.ScanStarted(_scanStartedRequest);
            //Assert
            Mock.Assert(() => provider.SendData(Arg.IsAny<IGeneralInfo>()), Occurs.Once());
        }

        [Fact(DisplayName = "ScanFinished method test.")]
        public void ScanFinishedMethodTest()
        {
            //Arrange
            MocksCreate();
            var provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => provider.SendData(Arg.IsAny<IGeneralInfo>())).OccursOnce();
            var client = new CmcClient(provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
            client.ScanFinished(_scanFinishedRequest);
            //Assert
            Mock.Assert(() => provider.SendData(Arg.IsAny<IGeneralInfo>()), Occurs.Once());
        }

        [Fact(DisplayName = "ScanUpdated method test.")]
        public void ScanUpdatedMethodTest()
        {
            //Arrange
            MocksCreate();
            var provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => provider.SendData(Arg.IsAny<IGeneralInfo>())).OccursOnce();
            var client = new CmcClient(provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
            client.ScanUpdated(_scanUpdatedRequest);
            //Assert
            Mock.Assert(() => provider.SendData(Arg.IsAny<IGeneralInfo>()), Occurs.Once());
        }

        [Fact(DisplayName = "ReportGenerated method test.")]
        public void ReportGeneratedMethodTest()
        {
            //Arrange
            MocksCreate();
            var provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => provider.SendData(Arg.IsAny<IGeneralInfo>())).OccursOnce();
            var client = new CmcClient(provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
            client.ReportGenerated(_reportGeneratedRequest);
            //Assert
            Mock.Assert(() => provider.SendData(Arg.IsAny<IGeneralInfo>()), Occurs.Once());
        }

        [Fact(DisplayName = "ReportPrintedOrSaved method test.")]
        public void ReportPrintedOrSavedMethodTest()
        {
            //Arrange
            MocksCreate();
            var provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => provider.SendData(Arg.IsAny<IGeneralInfo>())).OccursOnce();
            var client = new CmcClient(provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
            client.ReportPrintedOrSaved(_reportPrintedOrSavedRequest);
            //Assert
            Mock.Assert(() => provider.SendData(Arg.IsAny<IGeneralInfo>()), Occurs.Once());
        }

        [Fact(DisplayName = "AppicationClosed method test.")]
        public void AppicationClosedMethodTest()
        {
            //Arrange
            MocksCreate();
            var provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => provider.SendData(Arg.IsAny<IGeneralInfo>())).OccursOnce();
            var client = new CmcClient(provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
            client.AppicationClosed(_applicationClosedRequest);
            //Assert
            Mock.Assert(() => provider.SendData(Arg.IsAny<IGeneralInfo>()), Occurs.Once());
        }

        [Fact(DisplayName = "Check full chain on Client calls test.")]
        public void CmcClientCallsTest()
        {
            //Arrange
            int cnt = 0;
            MocksCreate();

            var provider = Mock.Create<ICmcWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => provider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead(() => { cnt++; });
            var client = new CmcClient(provider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
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

        [Fact(DisplayName = "Launched SendData Request test.")]
        public void LaunchedSendDataRequestTest()
        {
            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.AcId).Returns(Guid.NewGuid().ToString());
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns("1.5.20801.85");
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns("enp.c1.fat");

            IGeneralInfo generalInfo = null;

            var webApiProvider = Mock.Create<ICmcWebApiProvider>();
            Mock.Arrange(() => webApiProvider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => webApiProvider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead((IGeneralInfo gi) =>
                {
                    generalInfo = gi;
                }
            );
            IGeneralInfo actualGerGeneralInfo = getGeneralInfo();
            var client = new CmcClient(webApiProvider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            //Act
           

            client.Launched(_launchedRequest);

            Assert.Equal(generalInfo, actualGerGeneralInfo);
        }

        private IGeneralInfo getGeneralInfo()
        {
            var sessionId = Guid.NewGuid().ToString();
            var accountId = Guid.Parse("507c8028-4d21-4a1f-b763-8ac8ef41baf3").ToString();
            string eventParamValue = "eh_pu";
            string eventId = "ei_da";
            string eventGroupId = "eg_da";

            GeneralInfo generalInfoReal = new GeneralInfo(
                applicationIdentifier: "enp", machineCode: "TBD",
                globalMachineCode: "TBD", productIdentifier: "enp.c1.fat");
            generalInfoReal.EventsInfo = new List<EventInfo>{
                new EventInfo(){
                    EventGroupIdentifier = eventGroupId,
                    EventIdentifier = eventId,
                    EventValues = new List<Parameter>(){
                        new Parameter() {IsApplication = false, Name = "ev_for_hashing", Value = eventParamValue},
                        new Parameter() {IsApplication = false, Name = "acid", Value = accountId},
                        new Parameter() {IsApplication = false, Name = "app_pr_ve", Value = "1.5.20801.85"},
                        new Parameter() {IsApplication = false, Name = "sid", Value = sessionId},
                        new Parameter() {IsApplication = false, Name = "iu_tz", Value = "04/18/2017 16:43:42"},
                        new Parameter() {IsApplication = false, Name = "iu_ts", Value = "1492533822"}
                    }
                }
            };
            generalInfoReal.Values = new List<Parameter>{
                new Parameter() {IsApplication = true, Name = "op_sy", Value = "Windows"},
                new Parameter() {IsApplication = true, Name = "op_ve", Value = "6.4"},
                new Parameter() {IsApplication = true, Name = "op_pl", Value = "x86"},
                new Parameter() {IsApplication = true, Name = "cul", Value = "uk-UA"},
                new Parameter() {IsApplication = true, Name = "pr_ve", Value = "1.5.20801.85"}
            };
            return generalInfoReal;
        }

    }
}
