
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telerik.JustMock;
using Xunit;
using UFH.Application.Models;
using System.Threading.Tasks;
using System.Collections.Specialized;

namespace UFH.Application.Tests
{

    public class ScanCounterManagerTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var scanCounterCollection = Mock.Create<IScanCounterCollection>(Behavior.Strict);
            var scanCounterFactory = Mock.Create<IScanCounterFactory>(Behavior.Strict);

            Mock.ArrangeSet(() => scanCounterCollection.ExternalEnumerator
                = Arg.Matches<Func<IEnumerable<IScanCounter>>>(_ => !_().Any())).MustBeCalled();

            //act
            var scanCounterManager = new ScanCounterManager(scanCounterCollection, scanCounterFactory);

            Assert.Same(scanCounterCollection, scanCounterManager.CounterCollection);
            Assert.Equal(0, scanCounterManager.Total);
            Assert.False(scanCounterManager.HasClean);
            Assert.False(scanCounterManager.HasMalwares);
            Assert.False(scanCounterManager.HasUnknowns);
            Assert.False(scanCounterManager.HasNotAnalyzed);
            Assert.False(scanCounterManager.HasInProgress);
            Mock.Assert(scanCounterCollection);
        }

        [Theory]
        [InlineData(17, 1, 12, 8, 0)]
        [InlineData(1, 122, 0, 1, 5)]
        [InlineData(1, 0, 6, 0, 8)]
        [InlineData(0, 11, 0, 122, 0)]
        [InlineData(0, 0, 0, 0, 0)]
        [InlineData(1, 1, 1, 1, 1)]
        public void UpdateCountersTest(int cleanCount, int malwaresCount, int unknownCount,
            int notAnalyzedCount, int inProgressCount)
        {
            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);
            
            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(cleanCount).MustBeCalled();
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(malwaresCount).MustBeCalled();
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(unknownCount).MustBeCalled();
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(notAnalyzedCount).MustBeCalled();
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(inProgressCount).MustBeCalled();

            bool addRaised = false;
            var scanCounterCollection = new ScanCounterCollection();
            scanCounterCollection.CollectionChanged += (o, e) => addRaised = e.Action == NotifyCollectionChangedAction.Add;
            var scanCounterManager = new ScanCounterManager(scanCounterCollection, new ScanCounterFactory());
            var total = cleanCount + malwaresCount + unknownCount + notAnalyzedCount + inProgressCount;

            //act
            scanCounterManager.UpdateCounters(scanCounterDiff);

            Assert.NotEmpty(scanCounterManager.CounterCollection);
            Assert.Equal(total, scanCounterManager.Total);
            Assert.Equal(cleanCount, scanCounterManager.CleanCount);
            Assert.Equal(malwaresCount, scanCounterManager.MalwaresCount);
            Assert.Equal(unknownCount, scanCounterManager.UnknownsCount);
            Assert.Equal(notAnalyzedCount, scanCounterManager.NotAnalyzedCount);
            Assert.Equal(inProgressCount, scanCounterManager.InProgressCount);
            Assert.True(addRaised);
            Mock.Assert(scanCounterDiff);
        }

        [Theory]
        [InlineData(156, 40, 1, 50, 0, 100)]
        [InlineData(1, 51, 0, 0, 15, 80)]
        [InlineData(2, 0, 77, 0, 0, 200)]
        [InlineData(0, 67, 0, 15, 0, 150)]
        public void UpdateCountersMultiThreadTest(int cleanCount, int malwaresCount, int unknownCount, int notAnalyzedCount, int inProgressCount, int threadCount)
        {
            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);
            
            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(cleanCount).OnAllThreads();
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(malwaresCount).OnAllThreads();
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(unknownCount).OnAllThreads();
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(notAnalyzedCount).OnAllThreads();
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(inProgressCount).OnAllThreads();

            var scanCounterManager = new ScanCounterManager(new ScanCounterCollection(), new ScanCounterFactory());
            var total = cleanCount + malwaresCount + unknownCount + notAnalyzedCount + inProgressCount;
            var countdown = new CountdownEvent(threadCount);

            //act
            foreach (var e in Enumerable.Range(1, threadCount))
            {
                Task.Run(() =>
                {
                    scanCounterManager.UpdateCounters(scanCounterDiff);
                    countdown.Signal();
                });
            }

            countdown.Wait();
            countdown.Dispose();

            Assert.NotEmpty(scanCounterManager.CounterCollection);
            Assert.Equal(total * threadCount, scanCounterManager.Total);
            Assert.Equal(cleanCount * threadCount, scanCounterManager.CleanCount);
            Assert.Equal(malwaresCount * threadCount, scanCounterManager.MalwaresCount);
            Assert.Equal(unknownCount * threadCount, scanCounterManager.UnknownsCount);
            Assert.Equal(notAnalyzedCount * threadCount, scanCounterManager.NotAnalyzedCount);
            Assert.Equal(inProgressCount * threadCount, scanCounterManager.InProgressCount);
            Mock.Assert(scanCounterDiff);
        }

        [Fact]
        public void ClearTest()
        {
            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);

            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(1);
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(1);
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(1);
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(1);
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(1);

            bool resetRaised = false;
            var scanCounterCollection = new ScanCounterCollection();
            scanCounterCollection.CollectionChanged += (o, e) => resetRaised = e.Action == NotifyCollectionChangedAction.Reset;
            var scanCounterManager = new ScanCounterManager(scanCounterCollection, new ScanCounterFactory());

            //act
            scanCounterManager.UpdateCounters(scanCounterDiff);
            scanCounterManager.Clear();

            Assert.Empty(scanCounterManager.CounterCollection);
            Assert.Equal(0, scanCounterManager.Total);
            Assert.False(scanCounterManager.HasClean);
            Assert.False(scanCounterManager.HasMalwares);
            Assert.False(scanCounterManager.HasUnknowns);
            Assert.False(scanCounterManager.HasNotAnalyzed);
            Assert.False(scanCounterManager.HasInProgress);
            Assert.True(resetRaised);
        }

        [Fact]
        public void SelectedOrdersChangedTest()
        {
            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);

            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(0);
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(0);
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(0);
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(0);
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(0);

            var orders = new int[0];
            var scanCounterManager = new ScanCounterManager(new ScanCounterCollection(), new ScanCounterFactory());
            scanCounterManager.UpdateCounters(scanCounterDiff);
            scanCounterManager.SelectedOrdersChanged = o => orders = o;

            //act
            scanCounterManager.CounterCollection.First().IsSelected = true;

            Assert.NotEmpty(orders);
        }
    }
}