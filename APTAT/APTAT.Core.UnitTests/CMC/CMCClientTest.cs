using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using APTAT.Common;
using APTAT.Core.CMC;
using APTAT.Core.CMC.Request;
using Telerik.JustMock;
using Xunit;

namespace APTAT.Core.UnitTests.CMC
{
    public class CmcClientTest
    {
        private readonly IMachineKeyProvider _keyProviderMock;
        private readonly ISystemInfoProvider _systemInfoProvider;
        private readonly ICultureInfoProvider _cultureInfoProvider;

        private ILaunchedRequest _launchedRequest;
        private IAutoDiscoveryFinishedRequest _autoDiscoveryFinishedRequest;
        private IScanStartedRequest _scanStartedRequest;
        private IScanFinishedRequest _scanFinishedRequest;
        private IScanUpdatedRequest _scanUpdatedRequest;
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

            _scanStartedRequest = Mock.Create<IScanStartedRequest>();
            Mock.Arrange(() => _scanStartedRequest.CountOfScannedComputers).Returns(12);
            Mock.Arrange(() => _scanStartedRequest.ScanTarget).Returns(ScanTarget.DiscoveredByWorkGroup);
            Mock.Arrange(() => _scanStartedRequest.ScanTriggerType).Returns(ScanTriggerType.ScanNow);
            Mock.Arrange(() => _scanStartedRequest.ScanType).Returns(ScanType.LocalScan);

            _scanUpdatedRequest = Mock.Create<IScanUpdatedRequest>();
            Mock.Arrange(() => _scanUpdatedRequest.CountOfNewMaliciousFilesWhichWasFoundAfterTheScanWasFinished).Returns(12);
            Mock.Arrange(() => _scanUpdatedRequest.CountOfNewUnknownFilesWhichWasFoundAfterTheScanWasFinished).Returns(21);

            _scanFinishedRequest = Mock.Create<IScanFinishedRequest>();
            Mock.Arrange(() => _scanFinishedRequest.CountOfFailedScans).Returns(4);
            Mock.Arrange(() => _scanFinishedRequest.CountOfMaliciousFiles).Returns(2);
            Mock.Arrange(() => _scanFinishedRequest.CountOfSuccessScans).Returns(23);
            Mock.Arrange(() => _scanFinishedRequest.CountOfUnknownFiles).Returns(24);
            Mock.Arrange(() => _scanFinishedRequest.ScanStatus).Returns(ScanStatus.Success);

            _reportGeneratedRequest = Mock.Create<IReportGeneratedRequest>();
            Mock.Arrange(() => _reportGeneratedRequest.ReportKind).Returns(ReportKind.PerDevice);

            _reportPrintedOrSavedRequest = Mock.Create<IReportPrintedOrSavedRequest>();
            Mock.Arrange(() => _reportPrintedOrSavedRequest.ReportKind).Returns(ReportKind.PerDevice);
            Mock.Arrange(() => _reportPrintedOrSavedRequest.ReportAction).Returns(ReportAction.Saved);

            _applicationClosedRequest = Mock.Create<IApplicationClosedRequest>();
            Mock.Arrange(() => _applicationClosedRequest.SId).Returns(Guid.NewGuid().ToString());
        }

        private string GetLinuxFormatTime(DateTime localTime)
        {
            DateTime unixEpoch = new DateTime(1970, 1, 1);
            TimeSpan timeSpan = localTime - unixEpoch;
            return ((int)timeSpan.TotalSeconds).ToString(CultureInfo.InvariantCulture);
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
            Assert.Equal(8, cnt);
        }

        [Fact(DisplayName = "Launched SendData Request test.")]
        public void LaunchedSendDataRequestTest()
        {
            //Assert
            var acId = Guid.NewGuid().ToString();
            var productVersion = "1.5.20801.85";
            var productId = "enp.c1.fat";
            var sessionId = Guid.NewGuid().ToString();
            string eventParamValue = "eh_pu";
            string eventId = "ei_da";
            string eventGroupId = "eg_da";

            var systemInfo = _systemInfoProvider.GetSystemInfo();
            var culture = _cultureInfoProvider.GetCulture();
            var localTimeZone = TimeZone.CurrentTimeZone.ToLocalTime(DateTime.Now);
            IGeneralInfo generalInfo = null;

            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.AcId).Returns(acId);
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns(productVersion);
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns(productId);

            var webApiProvider = Mock.Create<ICmcWebApiProvider>();
            Mock.Arrange(() => webApiProvider.SetBaseUri(Arg.AnyUri));

            Mock.Arrange(() => webApiProvider.SendData(Arg.IsAny<IGeneralInfo>()))
                .DoInstead((IGeneralInfo gi) => { generalInfo = gi; });

            GeneralInfo actualGeneralInfo = new GeneralInfo("enp", "TBD", "TBD", productId)
            {
                EventsInfo = new List<IEventInfo>{
                    new EventInfo{
                        EventGroupIdentifier = eventGroupId,
                        EventIdentifier = eventId,
                        EventValues = new List<IParameter>{
                            new Parameter { Name = "ev_for_hashing",  Value = eventParamValue,IsApplication = false},
                            new Parameter { Name = "acid",Value = acId,IsApplication = false},
                            new Parameter { Name = "pr_ve", Value = productVersion ,IsApplication = false},
                            new Parameter { Name = "sid", Value = sessionId,IsApplication = false},
                            new Parameter { Name = "iu_tz",Value = localTimeZone.ToString(CultureInfo.InvariantCulture),IsApplication = false},
                            new Parameter {Name = "iu_ts",Value = GetLinuxFormatTime(localTimeZone),IsApplication = false}
                        }
                    }
                },
                Values = new List<IParameter>{
                    new Parameter {IsApplication = true, Name = "op_sy", Value = systemInfo.FamilyName},
                    new Parameter {IsApplication = true, Name = "op_ve", Value = systemInfo.Version},
                    new Parameter {IsApplication = true, Name = "op_pl", Value = systemInfo.Platform},
                    new Parameter {IsApplication = true, Name = "cul", Value = culture},
                    new Parameter {IsApplication = true, Name = "pr_ve", Value = productVersion}
                }
            };
            var client = new CmcClient(webApiProvider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);

            //Act
            client.Launched(_launchedRequest);

            //Assert
            Assert.Equal(generalInfo.ApplicationIdentifier, actualGeneralInfo.ApplicationIdentifier);
            Assert.Equal(generalInfo.GlobalMachineCode, actualGeneralInfo.GlobalMachineCode);
            Assert.Equal(generalInfo.MachineCode, actualGeneralInfo.MachineCode);
            Assert.Equal(generalInfo.ProductIdentifier, actualGeneralInfo.ProductIdentifier);
            Assert.Equal(generalInfo.Values.Count, actualGeneralInfo.Values.Count);

            for (int i = 0; i < generalInfo.Values.Count; i++)
            {
                Assert.Equal(generalInfo.Values[i].Name, actualGeneralInfo.Values[i].Name);
                Assert.Equal(generalInfo.Values[i].Value, actualGeneralInfo.Values[i].Value);
            }
            for (int j = 0; j < generalInfo.EventsInfo.Count; j++)
            {
                for (int i = 0; i < generalInfo.EventsInfo[j].EventValues.Count; i++)
                {
                    Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Name,
                        actualGeneralInfo.EventsInfo[j].EventValues[i].Name);
                    if (generalInfo.EventsInfo[j].EventValues[i].Name != "sid")
                        Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Value,
                            actualGeneralInfo.EventsInfo[j].EventValues[i].Value);
                }
            }
        }

        [Fact(DisplayName = "AutoDiscovery SendData Request test.")]
        public void AutoDiscoverySendDataRequestTest()
        {
            //Assert
            var sId = Guid.NewGuid().ToString();
            var acId = Guid.NewGuid().ToString();
            var productVersion = "1.5.20801.85";
            var productId = "enp.c1.fat";

            string eventParamValue = "eh_ad_adf";
            string eventId = "ei_rp_ad";
            string eventGroupId = "eg_da";

            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.AcId).Returns(acId);
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns(productVersion);
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns(productId);

            _autoDiscoveryFinishedRequest = Mock.Create<IAutoDiscoveryFinishedRequest>();
            Mock.Arrange(() => _autoDiscoveryFinishedRequest.CountOfPotentiallyScanableDiscoveredComputers).Returns(27);
            Mock.Arrange(() => _autoDiscoveryFinishedRequest.DiscoveryStatus).Returns(DiscoveryStatus.Success);

            IGeneralInfo generalInfo = null;

            IGeneralInfo actualGeneralInfo = new GeneralInfo("enp", "TBD", "TBD", productId)
            {
                EventsInfo = new List<IEventInfo>{
                    new EventInfo{
                        EventGroupIdentifier = eventGroupId,
                        EventIdentifier = eventId,
                        EventValues = new List<IParameter>{
                            new Parameter() {Name = "ev_for_hashing",  Value = eventParamValue,IsApplication = false},
                            new Parameter() { Name = "ds", Value = "0" ,IsApplication = false},
                            new Parameter() { Name = "pc_cc", Value = "27" ,IsApplication = false},
                            new Parameter() {Name = "acid",Value = acId,IsApplication = false},
                            new Parameter() { Name = "sid", Value = sId,IsApplication = false},
                        }
                    }
                },
                Values = new List<IParameter>()
            };

            var webApiProvider = Mock.Create<ICmcWebApiProvider>();
            Mock.Arrange(() => webApiProvider.SetBaseUri(Arg.AnyUri));

            Mock.Arrange(() => webApiProvider.SendData(Arg.IsAny<IGeneralInfo>()))
                .DoInstead((IGeneralInfo gi) => { generalInfo = gi; });

            var client = new CmcClient(webApiProvider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            client.Launched(_launchedRequest);

            //Act
            client.AutoDiscoveryFinished(_autoDiscoveryFinishedRequest);

            for (int j = 0; j < generalInfo.EventsInfo.Count; j++)
            {
                for (int i = 0; i < generalInfo.EventsInfo[j].EventValues.Count; i++)
                {
                    Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Name,
                        actualGeneralInfo.EventsInfo[j].EventValues[i].Name);

                    if (generalInfo.EventsInfo[j].EventValues[i].Name != "sid" &&
                        generalInfo.EventsInfo[j].EventValues[i].Name != "acid")

                        Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Value,
                            actualGeneralInfo.EventsInfo[j].EventValues[i].Value);
                }
            }
        }

        [Fact(DisplayName = "ScanStarted SendData Request test.")]
        public void ScanStartedSendDataRequestTest()
        {
            //Assert
            var sId = Guid.NewGuid().ToString();
            var acId = Guid.NewGuid().ToString();
            var productVersion = "1.5.20801.85";
            var productId = "enp.c1.fat";

            string eventParamValue = "eh_sc_ss";
            string eventId = "ei_rp_sc";
            string eventGroupId = "eg_da";

            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.AcId).Returns(acId);
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns(productVersion);
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns(productId);

            _scanStartedRequest = Mock.Create<IScanStartedRequest>();
            Mock.Arrange(() => _scanStartedRequest.ScanTriggerType).Returns(ScanTriggerType.ScanNow);
            Mock.Arrange(() => _scanStartedRequest.ScanTarget).Returns(ScanTarget.DiscoveredByWorkGroup);
            Mock.Arrange(() => _scanStartedRequest.ScanType).Returns(ScanType.LocalScan);
            Mock.Arrange(() => _scanStartedRequest.TotalCountOfDiscoveredComputers).Returns(50);
            Mock.Arrange(() => _scanStartedRequest.CountOfScannedComputers).Returns(15);

            IGeneralInfo generalInfo = null;
            IGeneralInfo actualGeneralInfo = new GeneralInfo("enp", "TBD", "TBD", productId)
            {
                EventsInfo = new List<IEventInfo>{
                    new EventInfo{
                        EventGroupIdentifier = eventGroupId,
                        EventIdentifier = eventId,
                        EventValues = new List<IParameter>{
                            new Parameter() {Name = "ev_for_hashing",  Value = eventParamValue,IsApplication = false},
                            new Parameter() { Name = "sc_trg_tp", Value = "0" ,IsApplication = false},
                            new Parameter() { Name = "sc_tg", Value = "1" ,IsApplication = false},
                            new Parameter() {Name = "sc_tp",Value = "3",IsApplication = false},
                            new Parameter() { Name = "pc_cc", Value = "50",IsApplication = false},
                            new Parameter() { Name = "pc_sc", Value = "15",IsApplication = false},
                            new Parameter() {Name = "acid",Value = acId,IsApplication = false},
                            new Parameter() { Name = "sid", Value = sId,IsApplication = false},
                            new Parameter() { Name = "aid", Value = sId,IsApplication = false}
                        }
                    }
                },
                Values = new List<IParameter>()
            };

            var webApiProvider = Mock.Create<ICmcWebApiProvider>();
            Mock.Arrange(() => webApiProvider.SetBaseUri(Arg.AnyUri));

            Mock.Arrange(() => webApiProvider.SendData(Arg.IsAny<IGeneralInfo>()))
                .DoInstead((IGeneralInfo gi) => { generalInfo = gi; });

            var client = new CmcClient(webApiProvider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            client.Launched(_launchedRequest);

            //Act
            client.ScanStarted(_scanStartedRequest);

            for (int j = 0; j < generalInfo.EventsInfo.Count; j++)
            {
                for (int i = 0; i < generalInfo.EventsInfo[j].EventValues.Count; i++)
                {
                    Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Name,
                        actualGeneralInfo.EventsInfo[j].EventValues[i].Name);

                    if (generalInfo.EventsInfo[j].EventValues[i].Name != "sid" &&
                        generalInfo.EventsInfo[j].EventValues[i].Name != "acid" &&
                        generalInfo.EventsInfo[j].EventValues[i].Name != "aid")
                        Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Value,
                            actualGeneralInfo.EventsInfo[j].EventValues[i].Value);
                }
            }
        }

        [Fact(DisplayName = "ScanUpdated SendData Request test.")]
        public void ScanUpdatedSendDataRequestTest()
        {
            //Assert
            var sId = Guid.NewGuid().ToString();
            var acId = Guid.NewGuid().ToString();
            var aId = Guid.NewGuid().ToString();
            var productVersion = "1.5.20801.85";
            var productId = "enp.c1.fat";

            string eventParamValue = "eh_sc_su";
            string eventId = "ei_rp_sc";
            string eventGroupId = "eg_da";

            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.AcId).Returns(acId);
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns(productVersion);
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns(productId);

            _scanUpdatedRequest = Mock.Create<IScanUpdatedRequest>();
            Mock.Arrange(() => _scanUpdatedRequest.CountOfNewUnknownFilesWhichWasFoundAfterTheScanWasFinished).Returns(21);
            Mock.Arrange(() => _scanUpdatedRequest.CountOfNewMaliciousFilesWhichWasFoundAfterTheScanWasFinished).Returns(12);

            IGeneralInfo generalInfo = null;
            IGeneralInfo actualGeneralInfo = new GeneralInfo("enp", "TBD", "TBD", productId)
            {
                EventsInfo = new List<IEventInfo>{
                    new EventInfo{
                        EventGroupIdentifier = eventGroupId,
                        EventIdentifier = eventId,
                        EventValues = new List<IParameter>{
                            new Parameter() {Name = "ev_for_hashing",  Value = eventParamValue,IsApplication = false},
                            new Parameter() { Name = "u_fc", Value = "21" ,IsApplication = false},
                            new Parameter() { Name = "mlc_fc", Value = "12" ,IsApplication = false},
                            new Parameter() {Name = "acid",Value = acId,IsApplication = false},
                            new Parameter() { Name = "sid", Value = sId,IsApplication = false},
                            new Parameter() { Name = "aid", Value = aId,IsApplication = false}
                        }
                    }
                },
                Values = new List<IParameter>()
            };
            var webApiProvider = Mock.Create<ICmcWebApiProvider>();
            Mock.Arrange(() => webApiProvider.SetBaseUri(Arg.AnyUri));

            Mock.Arrange(() => webApiProvider.SendData(Arg.IsAny<IGeneralInfo>()))
                .DoInstead((IGeneralInfo gi) => { generalInfo = gi; });

            var client = new CmcClient(webApiProvider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            client.Launched(_launchedRequest);

            //Act
            client.ScanUpdated(_scanUpdatedRequest);

            for (int j = 0; j < generalInfo.EventsInfo.Count; j++)
            {
                for (int i = 0; i < generalInfo.EventsInfo[j].EventValues.Count; i++)
                {
                    Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Name,
                        actualGeneralInfo.EventsInfo[j].EventValues[i].Name);

                    if (generalInfo.EventsInfo[j].EventValues[i].Name != "sid" &&
                        generalInfo.EventsInfo[j].EventValues[i].Name != "acid" &&
                        generalInfo.EventsInfo[j].EventValues[i].Name != "aid")
                        Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Value,
                            actualGeneralInfo.EventsInfo[j].EventValues[i].Value);
                }
            }
        }

        [Fact(DisplayName = "ScanFinished SendData Request test.")]
        public void ScanFinishedSendDataRequestTest()
        {
            //Assert
            var sId = Guid.NewGuid().ToString();
            var acId = Guid.NewGuid().ToString();
            var productVersion = "1.5.20801.85";
            var productId = "enp.c1.fat";

            string eventParamValue = "eh_fat_sf";
            string eventId = "ei_rp_sc";
            string eventGroupId = "eg_da";

            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.AcId).Returns(acId);
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns(productVersion);
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns(productId);

            _scanFinishedRequest = Mock.Create<IScanFinishedRequest>();
            Mock.Arrange(() => _scanFinishedRequest.ScanStatus).Returns(ScanStatus.Success);
            Mock.Arrange(() => _scanFinishedRequest.CountOfSuccessScans).Returns(25);
            Mock.Arrange(() => _scanFinishedRequest.CountOfFailedScans).Returns(10);
            Mock.Arrange(() => _scanFinishedRequest.CountOfUnknownFiles).Returns(50);
            Mock.Arrange(() => _scanFinishedRequest.CountOfMaliciousFiles).Returns(20);


            IGeneralInfo generalInfo = null;
            IGeneralInfo actualGeneralInfo = new GeneralInfo("enp", "TBD", "TBD", productId)
            {
                EventsInfo = new List<IEventInfo>{
                    new EventInfo{
                        EventGroupIdentifier = eventGroupId,
                        EventIdentifier = eventId,
                        EventValues = new List<IParameter>{
                            new Parameter() {Name = "ev_for_hashing",  Value = eventParamValue,IsApplication = false},

                            new Parameter() { Name = "sc_st", Value = "0" ,IsApplication = false},
                            new Parameter() { Name = "ps_sc_s", Value = "25" ,IsApplication = false},
                            new Parameter() { Name = "ps_sc_f",Value = "10",IsApplication = false},
                            new Parameter() { Name = "u_fc", Value = "50",IsApplication = false},
                            new Parameter() { Name = "mlc_fc", Value = "20",IsApplication = false},

                            new Parameter() {Name = "acid",Value = acId,IsApplication = false},
                            new Parameter() { Name = "sid", Value = sId,IsApplication = false}
                        }
                    }
                },
                Values = new List<IParameter>()
            };

            var webApiProvider = Mock.Create<ICmcWebApiProvider>();
            Mock.Arrange(() => webApiProvider.SetBaseUri(Arg.AnyUri));

            Mock.Arrange(() => webApiProvider.SendData(Arg.IsAny<IGeneralInfo>()))
                .DoInstead((IGeneralInfo gi) => { generalInfo = gi; });

            var client = new CmcClient(webApiProvider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            client.Launched(_launchedRequest);

            //Act
            client.ScanFinished(_scanFinishedRequest);

            for (int j = 0; j < generalInfo.EventsInfo.Count; j++)
            {
                for (int i = 0; i < generalInfo.EventsInfo[j].EventValues.Count; i++)
                {
                    Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Name,
                        actualGeneralInfo.EventsInfo[j].EventValues[i].Name);

                    if (generalInfo.EventsInfo[j].EventValues[i].Name != "sid" &&
                        generalInfo.EventsInfo[j].EventValues[i].Name != "acid")
                        Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Value,
                            actualGeneralInfo.EventsInfo[j].EventValues[i].Value);
                }
            }
        }

        [Fact(DisplayName = "ReportGenerated SendData Request test.")]
        public void ReportGeneratedSendDataRequestTest()
        {
            //Assert
            var sId = Guid.NewGuid().ToString();
            var acId = Guid.NewGuid().ToString();
            var productVersion = "1.5.20801.85";
            var productId = "enp.c1.fat";

            string eventParamValue = "eh_rp_gen";
            string eventId = "ei_rp_info";
            string eventGroupId = "eg_da";

            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.AcId).Returns(acId);
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns(productVersion);
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns(productId);

            _reportGeneratedRequest = Mock.Create<IReportGeneratedRequest>();
            Mock.Arrange(() => _reportGeneratedRequest.ReportKind).Returns(ReportKind.PerDevice);

            IGeneralInfo generalInfo = null;
            IGeneralInfo actualGeneralInfo = new GeneralInfo("enp", "TBD", "TBD", productId)
            {
                EventsInfo = new List<IEventInfo>{
                    new EventInfo{
                        EventGroupIdentifier = eventGroupId,
                        EventIdentifier = eventId,
                        EventValues = new List<IParameter>{
                            new Parameter() {Name = "ev_for_hashing",  Value = eventParamValue,IsApplication = false},

                            new Parameter() { Name = "rp_kd", Value = "1" ,IsApplication = false},

                            new Parameter() {Name = "acid",Value = acId,IsApplication = false},
                            new Parameter() { Name = "sid", Value = sId,IsApplication = false}
                        }
                    }
                },
                Values = new List<IParameter>()
            };

            var webApiProvider = Mock.Create<ICmcWebApiProvider>();
            Mock.Arrange(() => webApiProvider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => webApiProvider.SendData(Arg.IsAny<IGeneralInfo>()))
                .DoInstead((IGeneralInfo gi) => { generalInfo = gi; });
            var client = new CmcClient(webApiProvider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            client.Launched(_launchedRequest);
            //Act
            client.ReportGenerated(_reportGeneratedRequest);

            for (int j = 0; j < generalInfo.EventsInfo.Count; j++)
            {
                for (int i = 0; i < generalInfo.EventsInfo[j].EventValues.Count; i++)
                {
                    Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Name,
                        actualGeneralInfo.EventsInfo[j].EventValues[i].Name);

                    if (generalInfo.EventsInfo[j].EventValues[i].Name != "sid" &&
                        generalInfo.EventsInfo[j].EventValues[i].Name != "acid")
                        Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Value,
                            actualGeneralInfo.EventsInfo[j].EventValues[i].Value);
                }
            }
        }

        [Fact(DisplayName = "ReportPrintedOrSaved SendData Request test.")]
        public void ReportPrintedOrSavedSendDataRequestTest()
        {
            //Assert
            var sId = Guid.NewGuid().ToString();
            var acId = Guid.NewGuid().ToString();
            var productVersion = "1.5.20801.85";
            var productId = "enp.c1.fat";

            string eventParamValue = "eh_rp_svd";
            string eventId = "ei_rp_info";
            string eventGroupId = "eg_da";

            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.AcId).Returns(acId);
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns(productVersion);
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns(productId);

            _reportPrintedOrSavedRequest = Mock.Create<IReportPrintedOrSavedRequest>();
            Mock.Arrange(() => _reportPrintedOrSavedRequest.ReportKind).Returns(ReportKind.PerDevice);
            Mock.Arrange(() => _reportPrintedOrSavedRequest.ReportAction).Returns(ReportAction.Saved);

            IGeneralInfo generalInfo = null;
            IGeneralInfo actualGeneralInfo = new GeneralInfo("enp", "TBD", "TBD", productId)
            {
                EventsInfo = new List<IEventInfo>{
                    new EventInfo{
                        EventGroupIdentifier = eventGroupId,
                        EventIdentifier = eventId,
                        EventValues = new List<IParameter>{
                            new Parameter() {Name = "ev_for_hashing",  Value = eventParamValue,IsApplication = false},

                            new Parameter() { Name = "rp_kd", Value = "1" ,IsApplication = false},
                            new Parameter() { Name = "act", Value = "0" ,IsApplication = false},

                            new Parameter() {Name = "acid",Value = acId,IsApplication = false},
                            new Parameter() { Name = "sid", Value = sId,IsApplication = false}
                        }
                    }
                },
                Values = new List<IParameter>()
            };

            var webApiProvider = Mock.Create<ICmcWebApiProvider>();
            Mock.Arrange(() => webApiProvider.SetBaseUri(Arg.AnyUri));

            Mock.Arrange(() => webApiProvider.SendData(Arg.IsAny<IGeneralInfo>()))
                .DoInstead((IGeneralInfo gi) => { generalInfo = gi; });

            var client = new CmcClient(webApiProvider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            client.Launched(_launchedRequest);

            //Act
            client.ReportPrintedOrSaved(_reportPrintedOrSavedRequest);

            for (int j = 0; j < generalInfo.EventsInfo.Count; j++)
            {
                for (int i = 0; i < generalInfo.EventsInfo[j].EventValues.Count; i++)
                {
                    Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Name,
                        actualGeneralInfo.EventsInfo[j].EventValues[i].Name);

                    if (generalInfo.EventsInfo[j].EventValues[i].Name != "sid" &&
                        generalInfo.EventsInfo[j].EventValues[i].Name != "acid")
                        Assert.Equal(generalInfo.EventsInfo[j].EventValues[i].Value,
                            actualGeneralInfo.EventsInfo[j].EventValues[i].Value);
                }
            }
        }

        [Fact(DisplayName = "AppicationClosed SendData Request test.")]
        public void AppicationClosedSendDataRequestTest()
        {
            //Assert
            var sId = Guid.NewGuid().ToString();
            var acId = Guid.NewGuid().ToString();
            var productVersion = "1.5.20801.85";
            var productId = "enp.c1.fat";

            string eventParamValue = "eh_pc";
            string eventId = "ei_da";
            string eventGroupId = "eg_da";

            _launchedRequest = Mock.Create<ILaunchedRequest>();
            Mock.Arrange(() => _launchedRequest.AcId).Returns(acId);
            Mock.Arrange(() => _launchedRequest.ProductVersion).Returns(productVersion);
            Mock.Arrange(() => _launchedRequest.ProductIdentifier).Returns(productId);

            _applicationClosedRequest = Mock.Create<IApplicationClosedRequest>();
            Mock.Arrange(() => _applicationClosedRequest.SId).Returns(Guid.NewGuid().ToString());

            IGeneralInfo generalInfo = null;
            IGeneralInfo actualGeneralInfo = new GeneralInfo("enp", "TBD", "TBD", productId)
            {
                EventsInfo = new List<IEventInfo>{
                    new EventInfo{
                        EventGroupIdentifier = eventGroupId,
                        EventIdentifier = eventId,
                        EventValues = new List<IParameter>{
                            new Parameter() {Name = "ev_for_hashing",  Value = eventParamValue,IsApplication = false},
                            new Parameter() { Name = "sid", Value = sId,IsApplication = false}
                        }
                    }
                },
                Values = new List<IParameter>()

            };
            var webApiProvider = Mock.Create<ICmcWebApiProvider>();
            Mock.Arrange(() => webApiProvider.SetBaseUri(Arg.AnyUri));
            Mock.Arrange(() => webApiProvider.SendData(Arg.IsAny<IGeneralInfo>())).DoInstead((IGeneralInfo gi) => { generalInfo = gi; });
            var client = new CmcClient(webApiProvider, _keyProviderMock, _systemInfoProvider, _cultureInfoProvider);
            client.Launched(_launchedRequest);

            //Act
            client.AppicationClosed(_applicationClosedRequest);

        }

    }
}
