using APTAT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.JustMock;
using Valkyrie.WebApiProvider;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;
using Xunit;

namespace APTAT.Valkyrie.Tests
{
    public class ValkyrieClientTests
    {
        [Fact]
        public void StartAnalyzeTest()
        {
            var machineKey = Guid.NewGuid().ToString();
            var keyProviderMock = Mock.Create<IMachineKeyProvider>(Behavior.Strict);
            Mock.Arrange(() => keyProviderMock.GlobalMachineKey).Returns(() => Guid.NewGuid().ToString());
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => apiProviderMock.SubmitEmail(new SubscribeEmailRequest())).IgnoreArguments().Returns(() => new ValkyrieResponse() { ReturnCode = ErrorCodesEnum.Success }).OccursOnce();
            Mock.Arrange(() => apiProviderMock.RegisterWithCamLicense(new RegisterWithCamLicenseRequest())).IgnoreArguments().Returns(()=>new RegisterWithResponse() {ApiKey = Guid.NewGuid().ToString(), ReturnCode = 0 }).OccursOnce();
            Mock.Arrange(() => apiProviderMock.StartScan(new StartScanRequest(""))).IgnoreArguments().Returns(()=>new StartAndFinalizeScanResponse() { ReturnCode = ErrorCodesEnum.Success } ).OccursOnce();
            var timer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => timer.SetInterval(TimeSpan.FromMilliseconds(1000))).IgnoreArguments();
            Mock.Arrange(() => timer.Dispose()).IgnoreArguments();
            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, timer, timer);
            var camRegistration = Mock.Create<ICamRegistration>();
            Mock.Arrange(() => camRegistration.LicenseId).Returns(()=>Guid.NewGuid().ToString());
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            client.AnalyzeStarted += (sender, args)=> { };
           
            //(CamRegistration camRegistration, string login, string password, int machineCount,
        }

        private void Client_AnalyzeStarted(object sender, ValkyrieAnalyzeStartedEventArgs e)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void SubmitEmailTest()
        {
            var machineKey = Guid.NewGuid().ToString();
            var keyProviderMock =  Mock.Create<IMachineKeyProvider>(Behavior.Strict);
            Mock.Arrange(() => keyProviderMock.GlobalMachineKey).Returns(() => Guid.NewGuid().ToString());
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => apiProviderMock.SubmitEmail(new SubscribeEmailRequest())).IgnoreArguments().Returns(() => new ValkyrieResponse() {ReturnCode = ErrorCodesEnum.Success }).OccursOnce();
            var timer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => timer.SetInterval(TimeSpan.FromMilliseconds(1000))).IgnoreArguments();
            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, timer, timer);
            var t = client.SubmitEmail(new []{ "some@mail.com","other_mail@yahoo.com"});
            Assert.Equal(ErrorCodesEnum.Success, t.ReturnCode);
        }

        [Fact]
        public void SetBaseUrlTest()
        {
            var uri = new Uri(@"http://valkyrie.comodo.com");
            var machineKey = Guid.NewGuid().ToString();
            var keyProviderMock = Mock.Create<IMachineKeyProvider>(Behavior.Strict);
            Mock.Arrange(() => keyProviderMock.GlobalMachineKey).Returns(() => Guid.NewGuid().ToString());
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => apiProviderMock.SetBaseUri(uri)).OccursOnce();
            var timer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => timer.SetInterval(TimeSpan.FromMilliseconds(1000))).IgnoreArguments();
            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, timer, timer);
            
            client.SetBaseUri(uri);            
        }

        [Fact]
        public void DisposeTest()
        {
            var machineKey = Guid.NewGuid().ToString();
            var keyProviderMock = Mock.Create<IMachineKeyProvider>(Behavior.Strict);
            Mock.Arrange(() => keyProviderMock.GlobalMachineKey).Returns(() => Guid.NewGuid().ToString());
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);
            var timer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => timer.Dispose()).IgnoreArguments();
            Mock.Arrange(() => timer.SetInterval(TimeSpan.FromMilliseconds(1000))).IgnoreArguments();
            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, timer, timer);
            
            client.Dispose();
        }
    }
}
