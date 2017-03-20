using APTAT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Valkyrie.WebApiProvider;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;
using Xunit;

namespace APTAT.Valkyrie.Tests
{
    public class ValkyrieClientTests
    {
        [Fact(DisplayName = "Constructor has to call SetInterval of notifyProgressTimer")]
        public void ConstructorSetsProgressTimerIntervalTest()
        {
            var machineKey = Guid.NewGuid().ToString();
            var keyProviderMock = Mock.Create<IMachineKeyProvider>();
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>();
            var progressNotifyTimer = Mock.Create<IScheduler>(Behavior.Strict);
            var checkStatusTimer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => progressNotifyTimer.SetInterval(TimeSpan.FromMilliseconds(1000))).OccursOnce();

            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, progressNotifyTimer, checkStatusTimer);
            Assert.NotNull(client);
            Mock.AssertAll(progressNotifyTimer);
            Mock.AssertAll(keyProviderMock);
        }

        [Fact(DisplayName = "Constructor has to call GlobalMachineKey from KeyProvider")]
        public void ConstructorGetsMachineKeyTest()
        {
            var machineKey = Guid.NewGuid().ToString();
            var keyProviderMock = Mock.Create<IMachineKeyProvider>(Behavior.Strict);
            Mock.Arrange(() => keyProviderMock.GlobalMachineKey).Returns(() => Guid.NewGuid().ToString()).OccursOnce();
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>();
            var progressNotifyTimer = Mock.Create<IScheduler>();
            var checkStatusTimer = Mock.Create<IScheduler>();
            Mock.Arrange(() => progressNotifyTimer.SetInterval(TimeSpan.FromMilliseconds(1000))).OccursOnce();

            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, progressNotifyTimer, checkStatusTimer);
            Assert.NotNull(client);
            Mock.AssertAll(keyProviderMock);
        }


        [Fact]
        public void StartAnalyzeTest()
        {
            //Arange   
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);

            var apiKey = Guid.NewGuid().ToString();
            Mock.Arrange(() => apiProviderMock.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()))
                .Returns(() => new RegisterWithResponse() { ApiKey = apiKey, ReturnCode = 0 });

            Mock.Arrange(() => apiProviderMock.StartScan(Arg.IsAny<StartScanRequest>()))
                .Returns(() => new StartAndFinalizeScanResponse() { ReturnCode = ErrorCodesEnum.Success });

            Mock.Arrange(() => apiProviderMock.GetBasicInfo(Arg.IsAny<InfoRequest>())).Returns(new BasicInfoResponse());
            Mock.Arrange(() => apiProviderMock.GetBasicInfoEmpty()).Returns(new BasicInfoResponse() { ReturnCode = ErrorCodesEnum.BadRequest });
            var statusTimer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => statusTimer.IsEnabled).Returns(() => false);
            Mock.Arrange(() => statusTimer.SetInterval(Arg.AnyTimeSpan));
            Mock.Arrange(() => statusTimer.Start(Arg.AnyBool)).Returns(() => true);
            
            ValkyrieClient client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMock, Mock.Create<IScheduler>(), statusTimer);
            
            var camRegistration = Mock.Create<ICamRegistration>();
            Mock.Arrange(() => camRegistration.LicenseId).Returns(() => Guid.NewGuid().ToString());

            //Act                       
            ManualResetEventSlim ev2 = new ManualResetEventSlim();
            client.AnalyzeCompleted+= (s, e) => { ev2.Set(); };

            ManualResetEventSlim ev = new ManualResetEventSlim();
            client.AnalyzeStarted += (s, e) => { ev.Set(); };
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            Mock.Raise(() => statusTimer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            ev.Wait(TimeSpan.FromMilliseconds(500));
            ValkyrieFileInfo fileInfo1 = new ValkyrieFileInfo("cf23df2207d99a74fbe169e3eba035e633b65d14", ValkyrieVerdict.Undetected) { Path = @"C:\temp\file.exe" };
            ValkyrieFileInfo fileInfo2 = new ValkyrieFileInfo("cf23df2207d99a74fbe169e3eba035e633b65d14", ValkyrieVerdict.InQueue) { Path = @"C:\temp\file.exe" };
            ValkyrieFileInfo fileInfo3 = new ValkyrieFileInfo("cf23df2207d99a74fbe169e3eba035e633b65d14", ValkyrieVerdict.Undetected) { Path = @"C:\temp\file.exe" };
            ValkyrieFileInfo fileInfo4 = new ValkyrieFileInfo("cf23df2207d99a74fbe169e3eba035e633b65d14", ValkyrieVerdict.Malware) { Path = @"C:\temp\file.exe" };
            client.AddFilesToAnalyze(new List<ValkyrieFileInfo>() { fileInfo1, fileInfo2, fileInfo3, fileInfo4 }, true);
            ev2.Wait(TimeSpan.FromMilliseconds(1000));

            

            //Assert
            Mock.Assert(() => apiProviderMock.StartScan(Arg.Matches<StartScanRequest>((s) => s.ApiKey == apiKey)), Occurs.Once());
            Mock.Assert(() => apiProviderMock.GetBasicInfo(Arg.IsAny<InfoRequest>()), Occurs.Once());
            Mock.AssertAll(apiProviderMock);
        }



        [Fact]
        public void StartAnalyzeCallsStartScanWithCorrectApiKey()
        {
            //Arange   
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);

            var apiKey = Guid.NewGuid().ToString();
            Mock.Arrange(() => apiProviderMock.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()))
                .Returns(() => new RegisterWithResponse() { ApiKey = apiKey, ReturnCode = 0 });

            Mock.Arrange(() => apiProviderMock.StartScan(Arg.IsAny<StartScanRequest>()))
                .Returns(() => new StartAndFinalizeScanResponse() { ReturnCode = ErrorCodesEnum.Success });
            
            ValkyrieClient client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMock, Mock.Create<IScheduler>(), Mock.Create<IScheduler>());
            var camRegistration = Mock.Create<ICamRegistration>();
            Mock.Arrange(() => camRegistration.LicenseId).Returns(() => Guid.NewGuid().ToString());

            //Act                       
            ManualResetEventSlim ev = new ManualResetEventSlim();
            client.AnalyzeCompleted += (s, e) => { ev.Set(); };
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            ev.Wait(TimeSpan.FromMilliseconds(300));

            //Assert
            Mock.Assert(() => apiProviderMock.StartScan(Arg.Matches<StartScanRequest>((s) => s.ApiKey == apiKey)), Occurs.Once());
           // Mock.AssertAll(apiProviderMock);
        }

        [Fact]
        public void StartAnalyzeCallsRegisterWithCamLicenseTest()
        {
            //Arange   
            
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);

            Mock.Arrange(() => apiProviderMock.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()))
                .Returns(() => new RegisterWithResponse() { ApiKey = Guid.NewGuid().ToString(), ReturnCode = 0 });

            Mock.Arrange(() => apiProviderMock.StartScan(Arg.IsAny<StartScanRequest>()))
                .Returns(() => new StartAndFinalizeScanResponse() { ReturnCode = ErrorCodesEnum.Success });


            ValkyrieClient client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMock, Mock.Create<IScheduler>(), Mock.Create<IScheduler>());
            var camRegistration = Mock.Create<ICamRegistration>();
            Mock.Arrange(() => camRegistration.LicenseId).Returns(() => Guid.NewGuid().ToString());
            
            //Act                       
            ManualResetEventSlim ev = new ManualResetEventSlim();
            client.AnalyzeCompleted += (s, e) => { ev.Set(); };
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            ev.Wait(TimeSpan.FromMilliseconds(300));
            
            //Assert
            Mock.Assert(() => apiProviderMock.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()), Occurs.Once());
            Mock.Assert(() => apiProviderMock.RegisterWithCamCredentials(Arg.IsAny<RegisterWithCamCredentials>()), Occurs.Never());            
        }


        [Fact]
        public void StartAnalyzeCallsRegisterWithCamCredenitalsTest()
        {
            //Arange   

            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);

            Mock.Arrange(() => apiProviderMock.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()))
                .Returns(() => new RegisterWithResponse() { ApiKey = Guid.NewGuid().ToString(), ReturnCode = 0 });

            Mock.Arrange(() => apiProviderMock.StartScan(Arg.IsAny<StartScanRequest>()))
                .Returns(() => new StartAndFinalizeScanResponse() { ReturnCode = ErrorCodesEnum.Success });


            ValkyrieClient client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMock, Mock.Create<IScheduler>(), Mock.Create<IScheduler>());

            var camRegistration = Mock.Create<ICamRegistration>();            
            Mock.Arrange(() => camRegistration.UserName).Returns(() => "UserName");
            Mock.Arrange(() => camRegistration.Password).Returns(() => "Password");

            //Act                       
            ManualResetEventSlim ev = new ManualResetEventSlim();
            client.AnalyzeCompleted += (s, e) => { ev.Set(); };
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            ev.Wait(TimeSpan.FromMilliseconds(300));

            //Assert
            Mock.Assert(() => apiProviderMock.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()), Occurs.Never());
            Mock.Assert(() => apiProviderMock.RegisterWithCamCredentials(Arg.IsAny<RegisterWithCamCredentials>()), Occurs.Once());

        }

        private void Client_AnalyzeStarted(object sender, ValkyrieAnalyzeStartedEventArgs e)
        {
            throw new NotImplementedException();
        }

        [Fact(DisplayName = "SubmitEmail of ValkyrieWebApiProvider method should be called")]
        public void SubmitEmailTest()
        {
            //Arange
            var machineKey = Guid.NewGuid().ToString();
            var keyProviderMock = Mock.Create<IMachineKeyProvider>();
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);

            Mock.Arrange(() => apiProviderMock.SubmitEmail(Arg.IsAny<SubscribeEmailRequest>()))
                .Returns(() => new ValkyrieResponse() { ReturnCode = ErrorCodesEnum.Success });
                        
            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, Mock.Create<IScheduler>(), Mock.Create<IScheduler>());

            var emails = new[] { "some@mail.com", "other_mail@yahoo.com" };
            
            //Act
            var t = client.SubmitEmail(emails);
            

            //Assert
            Assert.Equal(ErrorCodesEnum.Success, t.ReturnCode);
            Mock.Assert(() => apiProviderMock.SubmitEmail(Arg.Matches<SubscribeEmailRequest>((request) => Enumerable.SequenceEqual(request.CustomerEmails, emails))),
                Occurs.Once());
        }

       

        [Fact]
        public void SetBaseUrlTest()
        {
            //Arange
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => apiProviderMock.SetBaseUri(Arg.AnyUri));

            var keyProviderMock = Mock.Create<IMachineKeyProvider>();
            var timer = Mock.Create<IScheduler>();

            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, timer, timer);

            //Act
            var uri = new Uri(@"http://valkyrie.comodo.com");
            client.SetBaseUri(uri);
            
            //Assert
            Mock.Assert(() => apiProviderMock.SetBaseUri(uri),Occurs.Once());
        }        

        [Fact]
        public void DisposeTest()
        {
            //Arange
            var keyProviderMock = Mock.Create<IMachineKeyProvider>();
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>();
            var progressNotifyTimer = Mock.Create<IScheduler>(Behavior.Strict);
            var checkStatusTimer = Mock.Create<IScheduler>(Behavior.Strict);            
            Mock.Arrange(() => progressNotifyTimer.Dispose());
            Mock.Arrange(() => checkStatusTimer.Dispose());
            Mock.Arrange(() => progressNotifyTimer.SetInterval(Arg.AnyTimeSpan));
            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, progressNotifyTimer, checkStatusTimer);
            
            //Act
            client.Dispose();
            
            //Assert
            Mock.Assert(() => checkStatusTimer.Dispose(), Occurs.Once());
            Mock.Assert(() => progressNotifyTimer.Dispose(), Occurs.Once());
            Mock.Assert(() => progressNotifyTimer.SetInterval(TimeSpan.FromMilliseconds(1000)), Occurs.Once());
        }

    }
}
