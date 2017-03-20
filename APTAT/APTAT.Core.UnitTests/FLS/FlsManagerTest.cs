using System;
using System.Collections.Generic;
using System.Linq;
using APTAT.DeploymentCore.Descent;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Messages;
using Telerik.JustMock;
using Xunit;
using APTAT.Common;
using System.Diagnostics;

namespace APTAT.Core.UnitTests.FLS
{
    public class FlsManagerTest
    {
        [Fact(DisplayName = "0. Update Fls ports test.")]
        public void UpdateFlsPortsTest()
        {
            //Arrange
            var apiProvider = Mock.Create<IFlsApiProvider>(Behavior.Strict);

            var timer = Mock.Create<IScheduler>();
            Mock.Arrange(() => apiProvider.UpdateFlsPorts(Arg.Is(32), Arg.Is(98)))
                .DoNothing().MustBeCalled();

            //Act
            var fls = new FlsManager(apiProvider,timer);
                fls.UpdateFlsPorts(32, 98);
            
            //Assert
            Mock.Assert(apiProvider);
            fls.Dispose();
        }
        [Fact(DisplayName = "0.RequestType0 test.")]
        public void RequestType0Test()
        {
            //Arrange
            var apiProvider = Mock.Create<IFlsApiProvider>(Behavior.Strict);
            var timer = Mock.Create<IScheduler>();
            var flsResponce = Mock.Create<IFlsResponse>();

            var hashes = new String[] { "hash1", "hash2" };
            Mock.Arrange(() => apiProvider.RequestType0(Arg.Matches<String[]>(hashesInput 
                => Enumerable.SequenceEqual(hashesInput.OrderBy(h => h),hashes.OrderBy(h => h)))))
                .Returns(flsResponce)
                .OccursOnce();
            //Act
            var fls = new FlsManager(apiProvider,timer);
                fls.Enqueue(hashes);

            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Mock.Assert(apiProvider);
            fls.Dispose();
        }
        [Fact(DisplayName = "0. Dispose timer test.")]
        public void DisposeTestShouldDisposeTimer()
        {
            //Arrange
            var apiProvider = Mock.Create<IFlsApiProvider>();
            var timer = Mock.Create<IScheduler>(Behavior.Strict);
            Mock.Arrange(() => timer.Dispose()).OccursOnce();
            //Act
            var fls = new FlsManager(apiProvider, timer);
            fls.Dispose();
            //Assert
            Mock.Assert(timer);
        }
        [Fact(DisplayName = "0. Cancel test.")]
        public void CancelTest()
        {
            //Arrange
            var apiProvider = Mock.Create<IFlsApiProvider>(Behavior.Strict);
            var timer = Mock.Create<IScheduler>();
            var manager = Mock.Create<IFlsManager>(Behavior.Strict);
            Mock.Arrange(() => manager.Cancel()).OccursOnce();
            //Act
            var fls = new FlsManager(apiProvider, timer);
            fls.Enqueue(Enumerable.Empty<string>());
            fls.Cancel();
            //Assert
            Mock.Assert(() => timer.Stop(), Occurs.Once());
        }
        [Fact(DisplayName = "1. Call Enqueue should start timer. ")]
        public void EnqueueShouldStartTimerTest()
        {
            //Arrange
            var apiProvider = Mock.Create<IFlsApiProvider>();
            var timer = Mock.Create<IScheduler>(Behavior.Strict);
            var hashes = new String[] { "hash1", "hash2" };

            Mock.Arrange(() => timer.SetInterval(Arg.Matches<TimeSpan>(
                ts => ts.Equals(TimeSpan.FromSeconds(2))))).OccursOnce();
            Mock.Arrange(() => timer.Start(false))
                .Returns(true).OccursOnce();
            Mock.Arrange(() => timer.Dispose());

            //Act
            var fls = new FlsManager(apiProvider, timer);
            fls.Enqueue(hashes);

            //Assert
            Mock.Assert(timer);
            fls.Dispose();
        }
        [Fact(DisplayName = "2. On Fls Analyzed event should rised if analysed not empty.")]
        public void OnFlsAnalyzedEventShouldRisedIfAnalyzedNotEmpty()
        {
            //Arrange
            IFlsResult[] resultsCollection = IFlsResultCollection(10,Descent.Apt.CloudVerdict.Unknown);
            var hashes = resultsCollection.Select(c => c.Sha1).ToArray();
            IScheduler timer;
            IFlsApiProvider apiProvider;

            CreateMocks(out timer, out apiProvider);
            ArrangeFlsProvider(apiProvider, resultsCollection, hashes);            
           
            //Act
            var fls = new FlsManager(apiProvider, timer);
            fls.Enqueue(hashes);

            var rised = false;
            fls.FlsAnalyzed += (sender, args) =>
            {
                rised = true;
            };

            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            //Assert
            Assert.True(rised);
        }
        [Fact(DisplayName = "3. On Fls Analyse event should raised. analysed files not empty.")]
        public void OnFlsAnalyzedEventShouldRisedAllFilesIfAnalyzedNotEmpty()
        {
            //Arrange
            IFlsResult[] resultsCollection = IFlsResultCollection(10);
            var hashes = resultsCollection.Select(c => c.Sha1).ToArray();
            var resultsExspected = resultsCollection.Where(c => c.IsNotAnalyzed()==false).ToArray();

            IFlsResult[] result = null;
            IScheduler timer;
            IFlsApiProvider apiProvider;

            CreateMocks(out timer, out apiProvider);
            ArrangeFlsProvider(apiProvider, resultsCollection, hashes);

            //Act
            var fls = new FlsManager(apiProvider, timer);
            fls.Enqueue(hashes);
            
            fls.FlsAnalyzed += (sender, args) =>
            {
                result = args.Results;
            };

            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.True(Enumerable.SequenceEqual(result, resultsExspected));
        }
        [Fact(DisplayName = "4. On Fls Analyzed event should not raised. GetFailedToAnalyzeHashes return values.")]
        public void OnFlsAnalyzedEventShouldRetrieveAnalisedFilesResult()
        {
            //Arrange
            IFlsResult[] resultsCollection = IFlsResultCollection(10,Descent.Apt.CloudVerdict.FailedToAnalyze);
            var hashes = resultsCollection.Select(c => c.Sha1).ToArray();

            IScheduler timer;
            bool raised=false;
            IFlsApiProvider flsApiProvider;

            CreateMocks(out timer, out flsApiProvider);
            ArrangeFlsProvider(flsApiProvider, resultsCollection, hashes);

            IFlsManager fls = new FlsManager(flsApiProvider, timer);
            fls.FlsAnalyzed += (sender, args) =>
            {
                raised = true;
            };

            //Act
            fls.Enqueue(hashes);

            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.False(raised);
        }
        [Fact(DisplayName = "5. For hashes more than 20 ")]
        public void OnAnalyzeAsyncCallWithHashesMoreThan20ShouldCallRequestType0TestManyTimes()
        {
            //+
            //Arrange
            IFlsResult[] results = IFlsResultCollection();
            var hashes = results.Select(c => c.Sha1).ToArray();

            IScheduler timer;
            IFlsApiProvider apiProvider;

            CreateMocks(out timer, out apiProvider);
            ArrangeFlsProvider(apiProvider, results, hashes);

            //Act
            var rised = false;
            var fls = new FlsManager(apiProvider, timer);
            fls.Enqueue(hashes);

            fls.FlsAnalyzed += (sender, args) =>
            {
                rised = true;
            };

            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            //Assert
            Assert.True(rised);
            Mock.Assert(apiProvider);
        }
        [Fact(DisplayName = "6.On input 30 mixed files check if event raised twice.")]
        public void OnInputMoreThan20FilesCheckIFEventRaisedTwice()
        {
            //+
            //Arrange
            IFlsResult[] results = IFlsResultCollection();
            var hashes = results.Select(c => c.Sha1).ToArray();

            IScheduler timer;
            IFlsApiProvider apiProvider;

            CreateMocks(out timer, out apiProvider);
            ArrangeFlsProvider(apiProvider, results, hashes);

            //Act
            int count = 0;
            var fls = new FlsManager(apiProvider, timer);
            fls.FlsAnalyzed += (sender, args) =>
            {
                count++;
            };

            fls.Enqueue(hashes);

            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.Equal(2, count);
            Mock.Assert(apiProvider);
        }
        [Fact(DisplayName ="7.Enqueue after cancel.")]
        public void OnFlsAnalyzedEventShouldRetrieveAnalisedFilesResultWithCancel()
        {
            //Arrange
            IFlsResult[] results = IFlsResultCollection(10);
            var hashes = results.Select(c => c.Sha1).ToArray();

            IScheduler timer;
            bool raised = false;
            IFlsApiProvider flsApiProvider;

            CreateMocks(out timer, out flsApiProvider);
            ArrangeFlsProvider(flsApiProvider, results, hashes);
            IFlsManager fls = new FlsManager(flsApiProvider, timer);

            fls.FlsAnalyzed += (sender, args) =>
            {
                raised = true;
            };

            fls.Enqueue(hashes);
            fls.Cancel();
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            raised = false;

            //Act
            fls.Enqueue(hashes);
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.True(raised);
        }
        [Fact(DisplayName = "8. Stop Scan before call timer tick.")]
        public void OnFlsAnalyzedEventShouldRetrieveAnalisedFilesResultWithCancelBefore()
        {
            //Arrange
            IFlsResult[] resultsCollection = IFlsResultCollection(10,Descent.Apt.CloudVerdict.Malware);
            var hashes = resultsCollection.Select(c => c.Sha1).ToArray();
            IScheduler timer;
            bool raised = false;
            IFlsApiProvider flsApiProvider;

            CreateMocks(out timer, out flsApiProvider);
            ArrangeFlsProvider(flsApiProvider, resultsCollection, hashes);
            IFlsManager fls = new FlsManager(flsApiProvider, timer);

            fls.FlsAnalyzed += (sender, args) =>
            {
                raised = true;
            };

            //Act
            fls.Enqueue(hashes);
            fls.Cancel();

            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.False(raised);
        }
        [Fact(DisplayName = "9.Result from fls api provider contains Failed and Not Failed together.")]
        public void OnAnalyzeAsyncCallWithHashes10()
        {
            //Arrange
            IFlsResult[] results = IFlsResultCollection(10);
            var hashes = results.Select(c => c.Sha1).ToArray();

            IScheduler timer;
            IFlsApiProvider apiProvider;

            CreateMocks(out timer, out apiProvider);
            ArrangeFlsProvider(apiProvider, results, hashes);

            //Act

            var rised = false;
            var fls = new FlsManager(apiProvider, timer);
            fls.Enqueue(hashes);
            fls.FlsAnalyzed += (sender, args) =>
            {
                rised = true;
            };

            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert

            Assert.True(rised);
        }

        [Fact(DisplayName = "12.Api provider return all analysed.Then sent timer tick.No calls to api provider.")]
        public void OnReturnAllAnalysedAndSentTimerTickShouldNotCallApiProvider()
        {
            //Arrange
            IFlsResult[] resultsCollection = IFlsResultCollection(10,Descent.Apt.CloudVerdict.Safe);
            var hashes = resultsCollection.Select(c => c.Sha1).ToArray();

            //Act
            IScheduler timer;
            IFlsApiProvider apiProvider;
            CreateMocks(out timer, out apiProvider);
            ArrangeFlsProvider(apiProvider, resultsCollection, hashes);

            bool rised = false;

            var fls = new FlsManager(apiProvider, timer);
            fls.FlsAnalyzed += (sender, args) =>
            {
                rised = true;
            };
            fls.Enqueue(hashes);
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            rised = false;
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.False(rised);
            Mock.Assert(() =>apiProvider.RequestType0(Arg.IsAny<string[]>()), Occurs.Once());

        }

        [Fact(DisplayName = "13.Api provider return all failed.Then sent second timer tick.Calls twice to api provider.")]
        public void OnReturnAllFailedAndSentTimerTickCheckIFCalledTwice()
        {
            //Arrange
            IFlsResult[] resultsCollection = IFlsResultCollection(10, Descent.Apt.CloudVerdict.NoVerdict);
            var hashes = resultsCollection.Select(c => c.Sha1).ToArray();
            //Act
            IScheduler timer;
            IFlsApiProvider apiProvider;
            var flsResponce = Mock.Create<IFlsResponse>(Behavior.Strict);

            CreateMocks(out timer, out apiProvider);

            Mock.Arrange(() => flsResponce.GetFailedToAnalyzeHashes())
                    .Returns(hashes);

            Mock.Arrange(() => flsResponce.GetAnalyzed())
                    .Returns(new IFlsResult[0]);

            Mock.Arrange(() => apiProvider.RequestType0(Arg.Matches<String[]>(hashesInput => Enumerable
                    .SequenceEqual(hashesInput.OrderBy(h => h), hashes.OrderBy(h => h)))))
                    .Returns(flsResponce).Occurs(2);
      
            bool rised = false;
            var fls = new FlsManager(apiProvider, timer);
            fls.FlsAnalyzed += (sender, args) =>
            {
                rised = true;
            };
            fls.Enqueue(hashes);
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            rised = false;
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Assert.False(rised);
            Mock.Assert(apiProvider);

        }

        [Fact(DisplayName = "14.Api provider return all failed and all analysed."+
            "Then sent second timer tick.Calls twice to api provider.")]
        public void OnReturnMixedFilesAndSentTimerTickCheckIfCalledTwice()
        {
            //Arrange
            IFlsResult[] resultsCollection = IFlsResultCollection(10);
            var hashes = resultsCollection.Select(c => c.Sha1).ToArray();
            var failedHashes = resultsCollection.Where(c => c.IsNotAnalyzed()).Select(c => c.Sha1).ToArray();

            //Act
            IScheduler timer;
            IFlsApiProvider apiProvider;
            CreateMocks(out timer, out apiProvider);

            ArrangeFlsProvider(apiProvider, resultsCollection, hashes);
            
            var fls = new FlsManager(apiProvider, timer);
            fls.Enqueue(hashes);
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            var flsResponce = Mock.Create<IFlsResponse>();
            Mock.Arrange(() => apiProvider.RequestType0(Arg.Matches<String[]>(hashesInput => Enumerable
                    .SequenceEqual(hashesInput.OrderBy(h => h), failedHashes.OrderBy(h => h)))))
                    .Returns(flsResponce).InSequence().OccursOnce();
            
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Mock.Assert(apiProvider);
        }

        [Fact(DisplayName = "15.On Tick return all failed.Cancel then check if not called api provider.")]
        public void OnTickAndReturnAllFailedAndCancelCheckIfApiProviderIsNotCalled()
        {
            //Arrange
            IFlsResult[] resultsCollection = IFlsResultCollection(5,Descent.Apt.CloudVerdict.NoVerdict);
            var hashes = resultsCollection.Select(c => c.Sha1).ToArray();

            IScheduler timer;
            bool raised = false;
            IFlsApiProvider flsApiProvider;

            CreateMocks(out timer, out flsApiProvider);
            ArrangeFlsProvider(flsApiProvider, resultsCollection, hashes);
            IFlsManager fls = new FlsManager(flsApiProvider, timer);
            fls.FlsAnalyzed += (sender, args) =>
            {
                raised = true;
            };

            //Act
            fls.Enqueue(hashes);
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });
            raised = false;
            fls.Cancel();
            Mock.Raise(() => timer.TimeoutCallback += null, args: new object[] { EventArgs.Empty });

            //Assert
            Mock.Assert(flsApiProvider);
            Assert.False(raised);
        }


        static int nextHash = 0;
        private static IFlsResult[] IFlsResultCollection(int count=30, Descent.Apt.CloudVerdict? verdict = null)
        {
            IFlsResult[] flsResults = new IFlsResult[count];
            for (int i = 0; i <= count - 1; i++)
            {
                string val = ((int)(((i + 1) % 6))).ToString();
                 Descent.Apt.CloudVerdict currentVerdict = verdict ?? 
                (Descent.Apt.CloudVerdict)Enum.Parse(typeof(Descent.Apt.CloudVerdict), val);
            
                  flsResults[i]= Mock.Create<IFlsResult>(Behavior.Strict);
                Mock.Arrange(() => flsResults[i].Cloud).Returns(currentVerdict);
                Mock.Arrange(() => flsResults[i].Sha1).Returns("hash"+ ++nextHash);
            }
            return flsResults;
        }

        private static void CreateMocks(out IScheduler timerOut, out IFlsApiProvider apiProvider)
        {
            apiProvider = Mock.Create<IFlsApiProvider>(Behavior.Strict);
            var timer = Mock.Create<IScheduler>();
            timerOut = timer;

            Mock.Arrange(() => timer.SetInterval(Arg.Matches<TimeSpan>( ts => ts.Equals(TimeSpan.FromSeconds(2)))));

            Mock.Arrange(() => timer.Start(false))
                .Returns(true);
       }
        private static void ArrangeFlsProvider(IFlsApiProvider apiProvider, IFlsResult[] response, string[] input)
        {
            var flsResponce = Mock.Create<IFlsResponse>(Behavior.Strict);
            int size = 20;
            int count = input.Length / size;

            for (int i = 0; i <= count; i++)
            {
                var currentResponse = response.Skip(i * size).Take(size);

                Mock.Arrange(() => flsResponce.GetFailedToAnalyzeHashes())
                    .Returns(currentResponse.Where(c => c.IsNotAnalyzed()).Select(c => c.Sha1).ToArray())
                    .InSequence().OccursOnce();

                Mock.Arrange(() => flsResponce.GetAnalyzed())
                    .Returns(currentResponse.Where(c => !c.IsNotAnalyzed()).ToArray())
                    .InSequence().OccursOnce();

                var inputs= input.Skip(i* size).Take(size).ToArray();

                Mock.Arrange(() => apiProvider.RequestType0(Arg.Matches<String[]>(hashesInput => Enumerable
                    .SequenceEqual(hashesInput.OrderBy(h => h), inputs.OrderBy(h => h)))))
                    .Returns(flsResponce).InSequence().OccursOnce();
            }
        }


    }
}
