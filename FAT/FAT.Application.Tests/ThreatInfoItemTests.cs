using APTAT.DeploymentCore;
using FAT.Application.Models;
using System;
using System.ComponentModel;
using Telerik.JustMock;
using Xunit;

namespace FAT.Application.Tests
{
    public class ThreatInfoItemTests
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        public void ComputerConstructorTest(bool isLocal, bool isOnline)
        {
            var computer = Mock.Create<IComputer>(Behavior.Strict);
            Mock.Arrange(() => computer.Name).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.DnsName).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.IPAddress).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.IsOnline).Returns(isOnline);
            Mock.Arrange(() => computer.IsLocalComputer).Returns(isLocal);
            Mock.Arrange(() => computer.Uid).Returns(Arg.AnyGuid);
            Mock.Arrange(() => computer.LdapPath).Returns(Arg.AnyString);
            ThreatInfoItem target = new ThreatInfoItem(computer);

            Assert.True(target.IsComputer);
            Assert.NotNull(target.Items);
            Assert.Empty(target.Items);
            Assert.Null(target.Computers);
            Assert.Null(target.BadFile);
            Assert.Null(target.Parent);
            Assert.Equal(isOnline, target.IsOnline);
            Assert.Equal(isLocal, target.IsLocal);
            Assert.Equal(computer, target.DeploymentTarget);

            Assert.Equal(0, target.TotalFilesCount);
            Assert.Equal(0, target.TotalProgressPercentage);
            Assert.Equal(0, target.UnknownFilesCount);
            Assert.Equal(0, target.MalwareFilesCount);
            Assert.Equal(0, target.CleanFilesCount);
            Assert.Equal(0, target.PendingFilesCount);
            Assert.Equal(0, target.NotAnalyzedFilesCount);
            Assert.Equal("Initializing...", target.StatusMessage);
            Assert.Equal("Initializing...", target.StatusMessageSystem);
            Assert.Equal(ScanStage.Unknown, target.ScanStage);
            Assert.True(target.ScanStatus.Equals(new ScanStatus(isOnline ? ScanStatusEnum.InProgress : ScanStatusEnum.Offline)));
        }

        [Fact]
        public void AddFileTest()
        {
            var computer = Mock.Create<IComputer>(Behavior.Strict);
            var file = Mock.Create<IThreatInfoItem>(Behavior.Strict);
            var badFile = Mock.Create<IBadFileItem>(Behavior.Strict);
            var nonstrict = Mock.Create<INonStrictHelper>();

            Mock.Arrange(() => computer.Name).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.DnsName).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.IPAddress).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.IsOnline).Returns(Arg.AnyBool);
            Mock.Arrange(() => computer.IsLocalComputer).Returns(Arg.AnyBool);
            Mock.Arrange(() => computer.Uid).Returns(Arg.AnyGuid);
            Mock.Arrange(() => computer.LdapPath).Returns(Arg.AnyString);

            ThreatInfoItem target = new ThreatInfoItem(computer);
            PropertyChangedEventHandler handler = (sender, args) =>
            {
                if (args.PropertyName == "Items" || args.PropertyName == "HasItems")
                    nonstrict.Action();
            };

            Mock.Arrange(() => nonstrict.Action()).DoNothing().OccursAtLeast(2);
            Mock.ArrangeSet(() => file.Parent = Arg.Matches<IThreatInfoItem>(_ => ReferenceEquals(_, target))).MustBeCalled();
            Mock.Arrange(() => file.BadFile).Returns(badFile);
            Mock.Arrange(() => file.IsComputer).Returns(false);
            Mock.Arrange(() => badFile.Sha1).Returns(string.Empty);

            target.PropertyChanged += handler;
            target.AddFile(file);

            Assert.NotNull(target.Items);
            Assert.Single(target.Items);
            Assert.True(target.IsComputer);
            Assert.All(target.Items, _ => Assert.False(_.IsComputer));

            Mock.Assert(file);
            Mock.Assert(nonstrict);
        }

        [Theory]
        [InlineData(1000, 13,32,23,12,10,32.21)]
        [InlineData(100, 1, 0, 0, 0, 1, 2.21)]
        [InlineData(1000, 0, 0, 0, 0, 0, 1.21)]
        [InlineData(1000, 0, 1, 1, 1, 0, 1.21)]
        public void UpdateCountersTest(int totalFiles, int cleanFilesCount, int malwaresFilesCount, int unknownFilesCount,
            int noVerdictFilesCount, int failedToAnalyzeFilesCount, double percentage)
        {
            var computer = Mock.Create<IComputer>(Behavior.Strict);
            var uid = Guid.NewGuid();
            Mock.Arrange(() => computer.Name).Returns("comp1");
            Mock.Arrange(() => computer.IPAddress).Returns("10.8.65.171");
            Mock.Arrange(() => computer.IsOnline).Returns(true);
            Mock.Arrange(() => computer.IsLocalComputer).Returns(false);
            Mock.Arrange(() => computer.Uid).Returns(uid);
            Mock.Arrange(() => computer.LdapPath).Returns(String.Empty);
            ThreatInfoItem target = new ThreatInfoItem(computer);
            
            var scanResultItem = Mock.Create<IScanResultItem>(Behavior.Strict);
            Mock.Arrange(() => scanResultItem.CleanFilesCount).Returns(cleanFilesCount);
            Mock.Arrange(() => scanResultItem.TotalFilesCount).Returns(totalFiles);
            Mock.Arrange(() => scanResultItem.MalwareFilesCount).Returns(malwaresFilesCount);
            Mock.Arrange(() => scanResultItem.UnknownFilesCount).Returns(unknownFilesCount);
            Mock.Arrange(() => scanResultItem.NoVerdictFilesCount).Returns(noVerdictFilesCount);
            Mock.Arrange(() => scanResultItem.FailedToAnalyzeFilesCount).Returns(failedToAnalyzeFilesCount);
            Mock.Arrange(() => scanResultItem.Percentage).Returns(percentage);
            Mock.Arrange(() => scanResultItem.StatusMessage).Returns(String.Empty);
            Mock.Arrange(() => scanResultItem.ScanStage).Returns(ScanStage.ScanInProgress);

            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);
            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(0);
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(0);
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(0);
            Mock.ArrangeSet(() => scanCounterDiff.CleanDiff = cleanFilesCount);
            Mock.ArrangeSet(() => scanCounterDiff.MalwareDiff =malwaresFilesCount);
            Mock.ArrangeSet(() => scanCounterDiff.InProgressDiff = (noVerdictFilesCount + 
                failedToAnalyzeFilesCount + unknownFilesCount));
            

            target.UpdateCounters(scanResultItem, scanCounterDiff);
            Assert.Equal(totalFiles, target.TotalFilesCount);
            Assert.Equal(percentage/2, target.TotalProgressPercentage);
            Assert.Equal(malwaresFilesCount, target.MalwareFilesCount);
            Assert.Equal(cleanFilesCount, target.CleanFilesCount);
            Assert.Equal((noVerdictFilesCount +
                failedToAnalyzeFilesCount + unknownFilesCount), target.PendingFilesCount);
            Assert.Equal(ScanStage.ScanInProgress, target.ScanStage);

        }
    }
}