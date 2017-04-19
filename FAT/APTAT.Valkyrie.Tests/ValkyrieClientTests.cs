using APTAT.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Telerik.JustMock;
using Valkyrie.WebApiProvider;
using Valkyrie.WebApiProvider.Contracts;
using Valkyrie.WebApiProvider.Enums;
using Xunit;

namespace APTAT.Valkyrie.Tests
{
    public class ValkyrieClientTests
    {
        [Fact(DisplayName = "Constructor has to call SetInterval of notifyProgressTimer and checkStatusTimer")]
        public void ConstructorSetsProgressTimerIntervalTest()
        {
            var keyProviderMock = Mock.Create<IMachineKeyProvider>();
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);
            var progressNotifyTimer = Mock.Create<IScheduler>(Behavior.Strict);
            var checkStatusTimer = Mock.Create<IScheduler>(Behavior.Strict);
            var delayTimer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => progressNotifyTimer.SetInterval(TimeSpan.FromSeconds(1))).OccursOnce();
            Mock.Arrange(() => checkStatusTimer.SetInterval(TimeSpan.FromMinutes(3))).OccursOnce();
            Mock.Arrange(() => delayTimer.SetInterval(TimeSpan.FromSeconds(2))).OccursOnce();

            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, progressNotifyTimer, checkStatusTimer, delayTimer);

            Mock.AssertAll(progressNotifyTimer);
            Mock.AssertAll(checkStatusTimer);
            Mock.AssertAll(delayTimer);
        }

        [Fact(DisplayName = "Constructor has to call GlobalMachineKey from KeyProvider")]
        public void ConstructorGetsMachineKeyTest()
        {
            var machineKey = Guid.NewGuid().ToString();
            var keyProviderMock = Mock.Create<IMachineKeyProvider>(Behavior.Strict);
            Mock.Arrange(() => keyProviderMock.GlobalMachineKey).Returns(() => machineKey).OccursOnce();
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>();
            var progressNotifyTimer = Mock.Create<IScheduler>();
            var checkStatusTimer = Mock.Create<IScheduler>();
            var delayTimer = Mock.Create<IScheduler>();

            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, progressNotifyTimer, checkStatusTimer, delayTimer);

            Mock.AssertAll(keyProviderMock);
        }


        private static ValkyrieClient CreateAptClientForEventTests(out IScheduler delayScheduler, out IValkyrieWebApiProvider apiProviderMock)
        {
            //Arange   
            var apiProviderMockInternal = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);

            var apiKey = Guid.NewGuid().ToString();
            Mock.Arrange(() => apiProviderMockInternal.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()))
                .Returns(() => new RegisterWithResponse() { ApiKey = apiKey, ReturnCode = 0 });

            Mock.Arrange(() => apiProviderMockInternal.StartScan(Arg.IsAny<StartScanRequest>()))
                .Returns(() => new StartAndFinalizeScanResponse() { ReturnCode = ErrorCodesEnum.Success, AptScanId = Guid.NewGuid().ToString() });
            var basicResponse = Mock.Create<IBasicInfoResponse>();
            Mock.Arrange(() => basicResponse.ReturnCode).Returns(ErrorCodesEnum.BadRequest);
            Mock.Arrange(() => apiProviderMockInternal.GetBasicInfoEmpty()).Returns(basicResponse);
            delayScheduler = Mock.Create<IScheduler>();
            var client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMockInternal, Mock.Create<IScheduler>(), Mock.Create<IScheduler>(),
                delayScheduler);
            apiProviderMock = apiProviderMockInternal;
            return client;

        }


        [Fact]
        public void StartAnalyzeRisesAnalyzeStarted()
        {
            IScheduler delayScheduler;
            IValkyrieWebApiProvider apiProvider;
            //Arange   
            var client = CreateAptClientForEventTests(out delayScheduler, out apiProvider);
            bool isEventRised = false;

            //Act                       
            var ev = new ManualResetEventSlim();

            client.AnalyzeStarted += (s, e) =>
            {
                isEventRised = true;
                ev.Set();
            };

            ICamRegistration camRegistration = CreateCamRegistrationMock();
            Mock.Arrange(() => delayScheduler.Start(false))
                .Raises(() => delayScheduler.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            Assert.True(ev.Wait(TimeSpan.FromMilliseconds(150)), "timeout");
            //Assert            
            Assert.True(isEventRised, "isEventRised");
        }

        [Fact]
        public void StartAnalyzeRisesAnalyzeCompleted()
        {
            IScheduler delayScheduler;
            IValkyrieWebApiProvider apiProvider;
            //Arange   
            var client = CreateAptClientForEventTests(out delayScheduler, out apiProvider);
            bool isEventRised = false;

            //Act                       
            var ev = new ManualResetEventSlim();

            client.AnalyzeCompleted += (s, e) =>
            {
                isEventRised = true;
                ev.Set();
            }; 

            ICamRegistration camRegistration = CreateCamRegistrationMock();
            Mock.Arrange(() => delayScheduler.Start(false))
                .Raises(() => delayScheduler.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            Assert.True(ev.Wait(TimeSpan.FromMilliseconds(150)), "timeout");

            //Assert            
            Assert.True(isEventRised, "isEventRised");
        }
        [Fact]
        public void SecondStartAnalyzeShouldSubmitEmails()
        {
            IScheduler delayScheduler;
            IValkyrieWebApiProvider apiProvider;
            //Arange   
            var client = CreateAptClientForEventTests(out delayScheduler, out apiProvider);
            bool isEventRised = false;
                
            var ev = new ManualResetEventSlim();
            client.AnalyzeCompleted += (s, e) =>
            {
                isEventRised = true;
                ev.Set();
            };
            var emails = new[] {"email"};
            Mock.Arrange(() => apiProvider.SubmitEmail(
                 Arg.Matches<SubscribeEmailRequest>(s => s.CustomerEmails.SequenceEqual(emails) && s.SalesmanEmails == null)))
                .Returns(() => new ValkyrieResponse() { ReturnCode = ErrorCodesEnum.Success });
            ICamRegistration camRegistration = CreateCamRegistrationMock();
            Mock.Arrange(() => delayScheduler.Start(false))
                .Raises(() => delayScheduler.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            Assert.True(ev.Wait(TimeSpan.FromMilliseconds(200)), "timeout");
            ev.Reset();
            client.SubmitEmail(emails);
            //Act       
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            Assert.True(ev.Wait(TimeSpan.FromMilliseconds(200)), "timeout");
            //Assert            
            Mock.Assert(() => apiProvider.SubmitEmail(Arg.IsAny<SubscribeEmailRequest>()),Occurs.Exactly(2));
        }

        [Fact]
        public void SecondStartAnalyzeShouldntIfSubmitWasFailed()
        {
            IScheduler delayScheduler;
            IValkyrieWebApiProvider apiProvider;
            //Arange   
            var client = CreateAptClientForEventTests(out delayScheduler, out apiProvider);
            bool isEventRised = false;

            var ev = new ManualResetEventSlim();
            client.AnalyzeCompleted += (s, e) =>
            {
                isEventRised = true;
                ev.Set();
            };
            var emails = new[] { "email" };
            Mock.Arrange(() => apiProvider.SubmitEmail(
                    Arg.Matches<SubscribeEmailRequest>(s => s.CustomerEmails.SequenceEqual(emails) && s.SalesmanEmails == null)))
                .Returns(() => new ValkyrieResponse() { ReturnCode = ErrorCodesEnum.InvalidApiKey });
            ICamRegistration camRegistration = CreateCamRegistrationMock();
            Mock.Arrange(() => delayScheduler.Start(false))
                .Raises(() => delayScheduler.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            Assert.True(ev.Wait(TimeSpan.FromMilliseconds(200)), "timeout");
            ev.Reset();
            client.SubmitEmail(emails);
            //Act       
            Mock.Reset();
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            Assert.True(ev.Wait(TimeSpan.FromMilliseconds(200)), "timeout");
            //Assert            
            Mock.Assert(() => apiProvider.SubmitEmail(Arg.IsAny<SubscribeEmailRequest>()), Occurs.Never());
        }
        private static ICamRegistration CreateCamRegistrationMock()
        {
            var camRegistration = Mock.Create<ICamRegistration>();
            Mock.Arrange(() => camRegistration.LicenseId).Returns(() => Guid.NewGuid().ToString());
            return camRegistration;
        }


        [Fact]
        public void StartAnalyzeCallsGetBasicInfoEmptyTest()
        {
            var apiProviderMock = CreateApiProvider();

            ValkyrieClient client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMock, Mock.Create<IScheduler>(), Mock.Create<IScheduler>(), Mock.Create<IScheduler>());
            var camRegistration = CreateCamRegistrationLoginPwd();

            //Act                       
            ManualResetEventSlim ev = new ManualResetEventSlim();
            client.AnalyzeCompleted += (s, e) => { ev.Set(); };
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            ev.Wait(TimeSpan.FromMilliseconds(300));
            //Assert            
            Mock.Assert(() => apiProviderMock.GetBasicInfoEmpty(), Occurs.Once());
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

            ValkyrieClient client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMock, Mock.Create<IScheduler>(), Mock.Create<IScheduler>(), Mock.Create<IScheduler>());
            var camRegistration = CreateCamRegistrationMock();
            var basicResponse = Mock.Create<IBasicInfoResponse>();
            Mock.Arrange(() => basicResponse.ReturnCode).Returns(ErrorCodesEnum.BadRequest);
            Mock.Arrange(() => apiProviderMock.GetBasicInfoEmpty()).Returns(basicResponse);
            //Act                       
            ManualResetEventSlim ev = new ManualResetEventSlim();
            client.AnalyzeStarted += (s, e) => { ev.Set(); };
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            ev.Wait(TimeSpan.FromMilliseconds(300));

            //Assert
            Mock.Assert(() => apiProviderMock.StartScan(Arg.Matches<StartScanRequest>((s) => s.ApiKey == apiKey)), Occurs.Once());

            Mock.AssertAll(apiProviderMock);
        }


        [Fact]
        public void StartAnalyzeCallsRegisterWithCamLicenseTest()
        {
            var camRegistration = CreateCamRegistrationLicense();
            IValkyrieWebApiProvider apiProviderMock = CreateApiProvider();
            ValkyrieClient client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMock, Mock.Create<IScheduler>(), 
                Mock.Create<IScheduler>(), Mock.Create<IScheduler>());
            if (camRegistration == null)
                camRegistration = CreateCamRegistrationLoginPwd();

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
            var camRegistration = CreateCamRegistrationLoginPwd();
            IValkyrieWebApiProvider apiProviderMock = CreateApiProvider();

            ValkyrieClient client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMock, Mock.Create<IScheduler>(), 
                Mock.Create<IScheduler>(), Mock.Create<IScheduler>());
            if (camRegistration == null)
                camRegistration = CreateCamRegistrationLoginPwd();

            //Act                       
            ManualResetEventSlim ev = new ManualResetEventSlim();
            client.AnalyzeCompleted += (s, e) => { ev.Set(); };
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            ev.Wait(TimeSpan.FromMilliseconds(300));

            //Assert
            Mock.Assert(() => apiProviderMock.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()), Occurs.Never());
            Mock.Assert(() => apiProviderMock.RegisterWithCamCredentials(Arg.IsAny<RegisterWithCamCredentials>()), Occurs.Once());
        }

        [Theory(DisplayName = "SubmitEmail of ValkyrieWebApiProvider method should be called")]
        [InlineData(ErrorCodesEnum.Success)]
        [InlineData(ErrorCodesEnum.InvalidApiKey)]
        public void SubmitEmailTest(ErrorCodesEnum status)
        {
            //Arange
            IScheduler delayScheduler;
            IValkyrieWebApiProvider apiProvider;
            var client = CreateAptClientForEventTests(out delayScheduler, out apiProvider);
            bool isEventRised = false;

            var ev = new ManualResetEventSlim();
            client.AnalyzeCompleted += (s, e) =>
            {
                isEventRised = true;
                ev.Set();
            };
            var emails = new[] { "some@mail.com", "other_mail@yahoo.com" };

            Mock.Arrange(() => apiProvider.SubmitEmail(Arg.IsAny<SubscribeEmailRequest>()))
                .Returns(() => new ValkyrieResponse() { ReturnCode = status });

            ICamRegistration camRegistration = CreateCamRegistrationMock();
            Mock.Arrange(() => delayScheduler.Start(false))
                .Raises(() => delayScheduler.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            Assert.True(ev.Wait(TimeSpan.FromMilliseconds(200)), "timeout");

            //Act       
            var t = client.SubmitEmail(emails);

            //Assert
            Mock.Assert(() => apiProvider.SubmitEmail(Arg.Matches<SubscribeEmailRequest>((request) => 
                request.CustomerEmails.SequenceEqual(emails) && request.SalesmanEmails == null)),
                Occurs.Once());
            Assert.Equal(status, t.ReturnCode);
        }

        [Fact(DisplayName = "SubmitEmail of ValkyrieWebApiProvider method should be called")]
        public void SubmitEmailShouldntCallApiProviderBeforeStartAnalyze()
        {
            //Arange
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => apiProviderMock.SetBaseUri(Arg.AnyUri));

            var keyProviderMock = Mock.Create<IMachineKeyProvider>();
            var timer = Mock.Create<IScheduler>();

            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, timer, timer, Mock.Create<IScheduler>());

            var emails = new[] { "some@mail.com", "other_mail@yahoo.com" };

            //Act  
            try
            {
                var t = client.SubmitEmail(emails);
            }
            catch (Exception)
            {
            }

            //Assert
            Mock.Assert(() => apiProviderMock.SubmitEmail(Arg.IsAny<SubscribeEmailRequest>()), Occurs.Never());
        }

        [Fact(DisplayName = "SubmitEmail of ValkyrieWebApiProvider method should be called")]
        public void SubmitEmailShouldThrowExceptionBeforeStartAnalyze()
        {
            //Arange
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => apiProviderMock.SetBaseUri(Arg.AnyUri));

            var keyProviderMock = Mock.Create<IMachineKeyProvider>();
            var timer = Mock.Create<IScheduler>();

            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, timer, timer, Mock.Create<IScheduler>());

            var emails = new[] { "some@mail.com", "other_mail@yahoo.com" };

            //Act  
            var ex = Assert.Throws<InvalidOperationException>(() => client.SubmitEmail(emails));
            
            //Assert
            Assert.Equal("Analyze isn't started", ex.Message);
        }



        [Fact]
        public void SetBaseUrlTest()
        {
            //Arange
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);
            Mock.Arrange(() => apiProviderMock.SetBaseUri(Arg.AnyUri));

            var keyProviderMock = Mock.Create<IMachineKeyProvider>();
            var timer = Mock.Create<IScheduler>();

            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, timer, timer, Mock.Create<IScheduler>());

            //Act
            var uri = new Uri(@"http://myUrl");
            client.SetBaseUri(uri);

            //Assert
            Mock.Assert(() => apiProviderMock.SetBaseUri(uri), Occurs.Once());
        }

        [Fact]
        public void DisposeTest()
        {
            //Arange
            var keyProviderMock = Mock.Create<IMachineKeyProvider>();
            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>();
            var progressNotifyTimer = Mock.Create<IScheduler>(Behavior.Strict);
            var checkStatusTimer = Mock.Create<IScheduler>(Behavior.Strict);
            var delayTimer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => progressNotifyTimer.Dispose());
            Mock.Arrange(() => checkStatusTimer.Dispose());
            Mock.Arrange(() => delayTimer.Dispose());
            Mock.Arrange(() => progressNotifyTimer.SetInterval(Arg.AnyTimeSpan));
            Mock.Arrange(() => checkStatusTimer.SetInterval(Arg.AnyTimeSpan));
            Mock.Arrange(() => delayTimer.SetInterval(Arg.AnyTimeSpan));

            ValkyrieClient client = new ValkyrieClient(keyProviderMock, apiProviderMock, progressNotifyTimer, checkStatusTimer, delayTimer);

            //Act
            client.Dispose();

            //Assert
            Mock.Assert(() => checkStatusTimer.Dispose(), Occurs.Once());
            Mock.Assert(() => progressNotifyTimer.Dispose(), Occurs.Once());
            Mock.Assert(() => delayTimer.Dispose(), Occurs.Once());
        }

        [Fact]
        public void AddFilesToAnalyzeShouldCallBasicInfoTest()
        {
            //Arange   
            var files = ValkyrieFileInfos();

            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);

            var apiKey = Guid.NewGuid().ToString();
            Mock.Arrange(() => apiProviderMock.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()))
                .Returns(() => new RegisterWithResponse() { ApiKey = apiKey, ReturnCode = 0 });

            Mock.Arrange(() => apiProviderMock.StartScan(Arg.IsAny<StartScanRequest>()))
                .Returns(() => new StartAndFinalizeScanResponse() { ReturnCode = ErrorCodesEnum.Success, AptScanId = Guid.NewGuid().ToString() });

            ManualResetEventSlim ev = new ManualResetEventSlim();
            CountdownEvent cev = new CountdownEvent(files.Count);
            List<string> calledBasicInfoHashes = new List<string>();
            Mock.Arrange(() => apiProviderMock.GetBasicInfo(Arg.IsAny<InfoRequest>()))
                .Returns((InfoRequest x) =>
                {
                    calledBasicInfoHashes.Add(x.SHA1);
                    cev.Signal();
                    var response = Mock.Create<IBasicInfoResponse>();
                    Mock.Arrange(() => response.SHA1).Returns(x.SHA1);
                    return response;
                });
            var basicResponse = Mock.Create<IBasicInfoResponse>();
            Mock.Arrange(() => basicResponse.ReturnCode).Returns(ErrorCodesEnum.BadRequest);
            Mock.Arrange(() => apiProviderMock.GetBasicInfoEmpty()).Returns(basicResponse);
            var statusTimer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => statusTimer.IsEnabled).Returns(() => false);
            Mock.Arrange(() => statusTimer.SetInterval(Arg.AnyTimeSpan));
            Mock.Arrange(() => statusTimer.Start(Arg.AnyBool)).Returns(() => true);

            var progressNotify = Mock.Create<IScheduler>();
            var delayScheduler = Mock.Create<IScheduler>();
            Mock.Arrange(() => delayScheduler.Start(false))
                .Raises(() => delayScheduler.TimeoutCallback += null, args: new object[] { EventArgs.Empty });


            ValkyrieClient client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMock, progressNotify, statusTimer, delayScheduler);

            var camRegistration = Mock.Create<ICamRegistration>();
            Mock.Arrange(() => camRegistration.LicenseId).Returns(() => Guid.NewGuid().ToString());

            //Act                       
            int completedEventCalled = 0;
            client.AnalyzeCompleted += (s, e) =>
            {
                Interlocked.Increment(ref completedEventCalled);
                ev.Set();
            };
            
            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            Assert.True(ev.Wait(TimeSpan.FromMilliseconds(250)));
            
            client.AddFilesToAnalyze(files.ToList(), true);       
            Assert.True(cev.Wait(TimeSpan.FromMilliseconds(250)));

            //Assert
            int numbersOfCallsShouldBe = files.Count;
            var hashes = files.Select(f => f.Sha1).ToArray();
            Mock.Assert(() => apiProviderMock.StartScan(Arg.Matches<StartScanRequest>((s) => s.ApiKey == apiKey)), Occurs.Once());
            Mock.Assert(() => apiProviderMock.GetBasicInfo(Arg.Matches<InfoRequest>((f) => hashes.Contains(f.SHA1))), Occurs.Exactly(numbersOfCallsShouldBe));
            var calledDistinct = calledBasicInfoHashes.Distinct().OrderBy(b => b).ToList();
            Assert.Equal(hashes.OrderBy(a => a).ToList(), calledDistinct);
            Assert.Equal(numbersOfCallsShouldBe, calledBasicInfoHashes.Count);
            Assert.Equal(1, completedEventCalled);
            Mock.AssertAll(apiProviderMock);
        }
        


        [Fact]
        public void ProgressNotifyEventTest()
        {
            //Arange   
            var files = ValkyrieFileInfos();

            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);

            var apiKey = Guid.NewGuid().ToString();
            Mock.Arrange(() => apiProviderMock.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()))
                .Returns(() => new RegisterWithResponse() { ApiKey = apiKey, ReturnCode = 0 });

            Mock.Arrange(() => apiProviderMock.StartScan(Arg.IsAny<StartScanRequest>()))
                .Returns(() => new StartAndFinalizeScanResponse() { ReturnCode = ErrorCodesEnum.Success, AptScanId = Guid.NewGuid().ToString() });

            ManualResetEventSlim ev = new ManualResetEventSlim();
            CountdownEvent cev = new CountdownEvent(files.Count);
            List<string> calledBasicInfoHashes = new List<string>();
            Mock.Arrange(() => apiProviderMock.GetBasicInfo(Arg.IsAny<InfoRequest>()))
                .Returns((InfoRequest x) =>
                {
                    calledBasicInfoHashes.Add(x.SHA1);
                    cev.Signal();
                    var response = Mock.Create<IBasicInfoResponse>();
                    Mock.Arrange(() => response.SHA1).Returns(x.SHA1);
                    return response;
                });
            var basicResponse = Mock.Create<IBasicInfoResponse>();
            Mock.Arrange(() => basicResponse.ReturnCode).Returns(ErrorCodesEnum.BadRequest);
            Mock.Arrange(() => apiProviderMock.GetBasicInfoEmpty()).Returns(basicResponse);
            var statusTimer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => statusTimer.IsEnabled).Returns(() => true);
            Mock.Arrange(() => statusTimer.Stop());
            Mock.Arrange(() => statusTimer.SetInterval(TimeSpan.FromMinutes(3)));
            Mock.Arrange(() => statusTimer.SetInterval(TimeSpan.FromMilliseconds(50)));
            Mock.Arrange(() => statusTimer.Start(false));

            var progressNotify = Mock.Create<IScheduler>();
            Mock.Arrange(() => statusTimer.Start(Arg.AnyBool)).Returns(() => true);
            var delayScheduler = Mock.Create<IScheduler>();
            Mock.Arrange(() => delayScheduler.Start(false))
                .Raises(() => delayScheduler.TimeoutCallback += null, args: new object[] { EventArgs.Empty });


            ValkyrieClient client = new ValkyrieClient(Mock.Create<IMachineKeyProvider>(), apiProviderMock, progressNotify, statusTimer, delayScheduler);
            
            var camRegistration = Mock.Create<ICamRegistration>();
            Mock.Arrange(() => camRegistration.LicenseId).Returns(() => Guid.NewGuid().ToString());

            //Act                       
            int completedEventCalled = 0;
            client.AnalyzeCompleted += (s, e) =>
            {
                Interlocked.Increment(ref completedEventCalled);
                ev.Set();
            };

            IValkyrieAnalysisResult[] results = new IValkyrieAnalysisResult[0];
            client.AnalyzeProgressChanged += (s, e) =>
            {
                results = e.ProgressInfo.Results;
                ev.Set();
            };

            client.StartAnalyze(camRegistration, "user", "test", 1, new System.Net.NetworkCredential(), "FAT");
            Assert.True(ev.Wait(TimeSpan.FromMilliseconds(250)));
            
            ev.Reset();

            client.AddFilesToAnalyze(files.ToList(), true);
            Assert.True(cev.Wait(TimeSpan.FromMilliseconds(250)));
            
            Mock.Raise(() => progressNotify.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            Assert.True(ev.Wait(TimeSpan.FromMilliseconds(250)));

            //Assert
            Assert.Equal(files.Count, results.Length);
            foreach (var file in files)
            {
                Assert.NotNull(results.FirstOrDefault(v => v.Sha1.Equals(file.Sha1, StringComparison.OrdinalIgnoreCase)));
            }
            Assert.Equal(1, completedEventCalled);
            Mock.AssertAll(apiProviderMock);
        }


        private static List<IValkyrieFileInfo> ValkyrieFileInfos(int count = 4)
        {
            var list = new List<IValkyrieFileInfo>(count);
            for (int i = 1; i <= count; i++)
            {
                var file = Mock.Create<IValkyrieFileInfo>(Behavior.Strict);
                Mock.Arrange(() => file.Path).Returns($@"C:\temp\file{i}.exe");
                Mock.Arrange(() => file.GetFullPath()).Returns($@"C:\temp\file{i}.exe");
                Mock.Arrange(() => file.Sha1).Returns($@"{i}");
                Mock.Arrange(() => file.FlsCloud).Returns(CloudVerdict.Unknown);
                Mock.Arrange(() => file.PreviousValkyrieVerdict).Returns(ValkyrieVerdict.InQueue);
                Mock.ArrangeSet(() => file.PreviousValkyrieVerdict = Arg.IsAny<ValkyrieVerdict>());
                list.Add(file);
            }
            return list;
        }

        #region Mock Helpers
        private ICamRegistration CreateCamRegistrationLicense()
        {
            var camRegistration = Mock.Create<ICamRegistration>();
            Mock.Arrange(() => camRegistration.LicenseId).Returns(() => Guid.NewGuid().ToString());
            return camRegistration;
        }
        private static ICamRegistration CreateCamRegistrationLoginPwd()
        {
            var camRegistration = Mock.Create<ICamRegistration>();
            Mock.Arrange(() => camRegistration.UserName).Returns(() => "UserName");
            Mock.Arrange(() => camRegistration.Password).Returns(() => "Password");
            return camRegistration;
        }

        private IValkyrieWebApiProvider CreateApiProvider()
        {
            //Arange   

            var apiProviderMock = Mock.Create<IValkyrieWebApiProvider>(Behavior.Strict);

            Mock.Arrange(() => apiProviderMock.RegisterWithCamLicense(Arg.IsAny<RegisterWithCamLicenseRequest>()))
                .Returns(() => new RegisterWithResponse() { ApiKey = Guid.NewGuid().ToString(), ReturnCode = 0 });

            Mock.Arrange(() => apiProviderMock.RegisterWithCamCredentials(Arg.IsAny<RegisterWithCamCredentials>()))
                            .Returns(() => new RegisterWithResponse() { ApiKey = Guid.NewGuid().ToString(), ReturnCode = 0 });

            Mock.Arrange(() => apiProviderMock.StartScan(Arg.IsAny<StartScanRequest>()))
                .Returns(() => new StartAndFinalizeScanResponse() { ReturnCode = ErrorCodesEnum.Success });

            var basicResponse = Mock.Create<IBasicInfoResponse>();
            Mock.Arrange(() => basicResponse.ReturnCode).Returns(ErrorCodesEnum.BadRequest);
            Mock.Arrange(() => apiProviderMock.GetBasicInfoEmpty()).Returns(basicResponse);

            return apiProviderMock;
        }

        #endregion

    }
}
