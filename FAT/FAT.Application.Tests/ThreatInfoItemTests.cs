using System.Collections.Generic;
using System.Windows;
using APTAT.Application.Services.EventArgs;
using APTAT.Common;
using APTAT.Common.MachineKey;
using APTAT.Core;
using APTAT.Core.Apt;
using APTAT.Core.Descent.Apt;
using APTAT.Core.Properties;
using APTAT.DeploymentCore;
using APTAT.DeploymentCore.Directory;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Valkyrie;
using FAT.Application.Models;
using FAT.Application.Services;
using Valkyrie.WebApiProvider;

namespace FAT.Application.Tests
{
    using APTAT.DeploymentCore.Descent;
    using System;
    using System.Linq;
    using System.Threading;
    using Telerik.JustMock;
    using Xunit;

    public class ThreatInfoItemTests
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        public void ComputerConstructorTest(bool isLocal, bool isOnline)
        {
            var computer = Mock.Create<IComputer>(Behavior.Strict);
            var uid = Guid.NewGuid();
            Mock.Arrange(() => computer.Name).Returns("comp1");
            Mock.Arrange(() => computer.IPAddress).Returns("10.8.65.171");
            Mock.Arrange(() => computer.IsOnline).Returns(isOnline);
            Mock.Arrange(() => computer.IsLocalComputer).Returns(isLocal);
            Mock.Arrange(() => computer.Uid).Returns(uid);
            Mock.Arrange(() => computer.LdapPath).Returns(String.Empty);
            ThreatInfoItem target = new ThreatInfoItem(computer);


            Assert.True(target.IsComputer);
            Assert.NotNull(target.Items);
            Assert.True(target.Items.Count == 0);
            Assert.Null(target.Computers);
            Assert.Null(target.BadFile);
            Assert.Equal(isOnline, target.IsOnline);
            Assert.Equal(isLocal, target.IsLocal);
            Assert.Equal(uid, target.ComputerGuid);
            Assert.Equal(computer, target.DeploymentTarget);

            Assert.Equal(0,target.TotalFilesCount);
            Assert.Equal(0, target.TotalProgressPercentage);
            Assert.Equal(0, target.UnknownFilesCount);
            Assert.Equal(0, target.MalwareFilesCount);
            Assert.Equal(0, target.CleanFilesCount);
            Assert.Equal(0, target.PendingFilesCount);
            Assert.Equal("Initializing...", target.StatusMessage);
            Assert.Equal("Initializing...", target.StatusMessageSystem);
            Assert.Equal(ScanStage.Unknown, target.ScanStage);
            Assert.True(target.ScanStatus.Equals(new ScanStatus(isOnline ? ScanStatusEnum.InProgress : ScanStatusEnum.Offline)));
        }

        [Fact]
        public void AddFirstItemShouldRaiseItemsPropertyChanged()
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

            bool itemsRaised = false;

            target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Items")
                    itemsRaised = true;
            };
            var file = Mock.Create<IThreatInfoItem>(Behavior.Strict);
            var badFile = Mock.Create<IBadFileItem>(Behavior.Strict);

            Mock.ArrangeSet(() => file.Parent = target);
            Mock.Arrange(() => file.BadFile).Returns(badFile);
            Mock.Arrange(() => file.IsComputer).Returns(false);
            Mock.Arrange(() => badFile.Sha1).Returns("231");

            target.AddFile(file);

            Assert.True(itemsRaised);
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