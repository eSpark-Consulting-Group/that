
using System;
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
            var scanCounterManager = new ScanCounterManager();

            Assert.Empty(scanCounterManager.CounterCollection);
            Assert.Equal(scanCounterManager.Total, 0);
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
            var scanCounterManager = new ScanCounterManager();

            var total = cleanCount + malwaresCount + unknownCount + notAnalyzedCount + inProgressCount;

            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(cleanCount);
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(malwaresCount);
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(unknownCount);
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(notAnalyzedCount);
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(inProgressCount);

            scanCounterManager.UpdateCounters(scanCounterDiff);

            Assert.NotEmpty(scanCounterManager.CounterCollection);
            Assert.Equal(scanCounterManager.Total, total);
            Assert.Equal(scanCounterManager.CleanCount, cleanCount);
            Assert.Equal(scanCounterManager.MalwaresCount, malwaresCount);
            Assert.Equal(scanCounterManager.UnknownsCount, unknownCount);
            Assert.Equal(scanCounterManager.NotAnalyzedCount, notAnalyzedCount);
            Assert.Equal(scanCounterManager.InProgressCount, inProgressCount);
        }

        [Theory]
        [InlineData(156, 40, 1, 50, 0, 100)]
        [InlineData(1, 51, 0, 0, 15, 100)]
        [InlineData(2, 0, 77, 0, 0, 100)]
        [InlineData(0, 67, 0, 15, 0, 100)]
        public void UpdateCountersMultiThreadTest(int cleanCount, int malwaresCount, int unknownCount, int notAnalyzedCount, int inProgressCount, int count)
        {
            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);
            var scanCounterManager = new ScanCounterManager();

            var total = cleanCount + malwaresCount + unknownCount + notAnalyzedCount + inProgressCount;

            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(cleanCount);
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(malwaresCount);
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(unknownCount);
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(notAnalyzedCount);
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(inProgressCount);

            var tasks = new Task[count];
            ManualResetEventSlim e = new ManualResetEventSlim(false, count);
            int i = 0;
            do
            {
                tasks[i] = Task.Run(() =>
                {
                    scanCounterManager.UpdateCounters(scanCounterDiff);
                    e.Set();
                });
                i++;
            }
            while (i < count);

            e.Wait();

            Assert.NotEmpty(scanCounterManager.CounterCollection);
            Assert.Equal(scanCounterManager.Total, total * count);
            Assert.Equal(scanCounterManager.CleanCount, cleanCount * count);
            Assert.Equal(scanCounterManager.MalwaresCount, malwaresCount * count);
            Assert.Equal(scanCounterManager.UnknownsCount, unknownCount * count);
            Assert.Equal(scanCounterManager.NotAnalyzedCount, notAnalyzedCount * count);
            Assert.Equal(scanCounterManager.InProgressCount, inProgressCount * count);
        }

        [Fact]
        public void ClearAfterUpdateTest()
        {
            var scanCounterManager = new ScanCounterManager();
            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);
            

            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(1);
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(2);
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(3);
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(4);
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(5);

            scanCounterManager.Clear();

            Assert.Empty(scanCounterManager.CounterCollection);
            Assert.Equal(scanCounterManager.Total, 0);
            Assert.False(scanCounterManager.HasClean);
            Assert.False(scanCounterManager.HasMalwares);
            Assert.False(scanCounterManager.HasUnknowns);
            Assert.False(scanCounterManager.HasNotAnalyzed);
            Assert.False(scanCounterManager.HasInProgress);
        }
    }
}