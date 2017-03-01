
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Telerik.JustMock;
using Xunit;
using FAT.Application.Models;
using System.Threading.Tasks;

namespace FAT.Application.Tests
{

    public class ScanCountersTests
    {
        [Fact]
        public void ConstructorTest()
        {
            var scanCounterCollection = Mock.Create<IScanCounterCollection>(Behavior.Strict);
            var scanCounterFactory = Mock.Create<IScanCounterFactory>(Behavior.Strict);

            Mock.ArrangeSet(() => scanCounterCollection.ExternalEnumerator
                = Arg.Matches<Func<IEnumerable<IScanCounter>>>(fx => !fx().Any()));

            var scanCounterManager = new ScanCounterManager(scanCounterCollection, scanCounterFactory);

            Assert.Same(scanCounterCollection, scanCounterManager.CounterCollection);
            Assert.Equal(0, scanCounterManager.Total);
            Assert.False(scanCounterManager.HasClean);
            Assert.False(scanCounterManager.HasMalwares);
            Assert.False(scanCounterManager.HasUnknowns);
            Assert.False(scanCounterManager.HasNotAnalyzed);
            Assert.False(scanCounterManager.HasInProgress);
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
            var scanCounterManager = new ScanCounterManager(new ScanCounterCollection(), new ScanCounterFactory());

            var total = cleanCount + malwaresCount + unknownCount + notAnalyzedCount + inProgressCount;

            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(cleanCount);
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(malwaresCount);
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(unknownCount);
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(notAnalyzedCount);
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(inProgressCount);

            scanCounterManager.UpdateCounters(scanCounterDiff);

            Assert.NotEmpty(scanCounterManager.CounterCollection);
            Assert.Equal(total, scanCounterManager.Total);
            Assert.Equal(cleanCount, scanCounterManager.CleanCount);
            Assert.Equal(malwaresCount, scanCounterManager.MalwaresCount);
            Assert.Equal(unknownCount, scanCounterManager.UnknownsCount);
            Assert.Equal(notAnalyzedCount, scanCounterManager.NotAnalyzedCount);
            Assert.Equal(inProgressCount, scanCounterManager.InProgressCount);
        }

        [Theory]
        [InlineData(156, 40, 1, 50, 0, 100)]
        [InlineData(1, 51, 0, 0, 15, 80)]
        [InlineData(2, 0, 77, 0, 0, 200)]
        [InlineData(0, 67, 0, 15, 0, 150)]
        public void UpdateCountersMultiThreadTest(int cleanCount, int malwaresCount, int unknownCount, int notAnalyzedCount, int inProgressCount, int threadCount)
        {
            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);
            var scanCounterManager = new ScanCounterManager(new ScanCounterCollection(), new ScanCounterFactory());

            var total = cleanCount + malwaresCount + unknownCount + notAnalyzedCount + inProgressCount;

            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(cleanCount);
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(malwaresCount);
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(unknownCount);
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(notAnalyzedCount);
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(inProgressCount);

            using (var e = new CountdownEvent(threadCount))
            {
                foreach(var e2 in Enumerable.Range(1, threadCount))
                {
                    Task.Run(() =>
                    {
                        scanCounterManager.UpdateCounters(scanCounterDiff);
                        e.Signal();
                    });
                }
                e.Wait();
            }

            Assert.NotEmpty(scanCounterManager.CounterCollection);
            Assert.Equal(total * threadCount, scanCounterManager.Total);
            Assert.Equal(cleanCount * threadCount, scanCounterManager.CleanCount);
            Assert.Equal(malwaresCount * threadCount, scanCounterManager.MalwaresCount);
            Assert.Equal(unknownCount * threadCount, scanCounterManager.UnknownsCount);
            Assert.Equal(notAnalyzedCount * threadCount, scanCounterManager.NotAnalyzedCount);
            Assert.Equal(inProgressCount * threadCount, scanCounterManager.InProgressCount);
        }

        [Fact]
        public void ClearTest()
        {
            var scanCounterManager = new ScanCounterManager(new ScanCounterCollection(), new ScanCounterFactory());
            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);

            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(1);
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(2);
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(3);
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(4);
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(5);

            scanCounterManager.UpdateCounters(scanCounterDiff);
            scanCounterManager.Clear();

            Assert.Empty(scanCounterManager.CounterCollection);
            Assert.Equal(0, scanCounterManager.Total);
            Assert.False(scanCounterManager.HasClean);
            Assert.False(scanCounterManager.HasMalwares);
            Assert.False(scanCounterManager.HasUnknowns);
            Assert.False(scanCounterManager.HasNotAnalyzed);
            Assert.False(scanCounterManager.HasInProgress);
        }
    }
}