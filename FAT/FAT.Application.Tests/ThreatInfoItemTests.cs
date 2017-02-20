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

        //[Fact]
        //public void AddFirstItemShouldRaiseItemsPropertyChanged()
        //{
        //    var computer = Mock.Create<IComputer>(Behavior.Strict);
        //    var uid = Guid.NewGuid();
        //    Mock.Arrange(() => computer.Name).Returns("comp1");
        //    Mock.Arrange(() => computer.IPAddress).Returns("10.8.65.171");
        //    Mock.Arrange(() => computer.IsOnline).Returns(true);
        //    Mock.Arrange(() => computer.IsLocalComputer).Returns(false);
        //    Mock.Arrange(() => computer.Uid).Returns(uid);
        //    Mock.Arrange(() => computer.LdapPath).Returns(String.Empty);
        //    ThreatInfoItem target = new ThreatInfoItem(computer);

        //    bool itemsRaised = false;


        //    var file = Mock.Create<IThreatInfoItem>(Behavior.Strict);
        //    var badFile = Mock.Create<IBadFileItem>(Behavior.Strict);
        //    Mock.Arrange(() => file.BadFile).Returns(badFile);
        //    Mock.Arrange(() => file.IsComputer).Returns(false);
        //    Mock.Arrange(() => badFile.Sha1).Returns("231");

        //    target.AddFile(file);
        //    target.PropertyChanged += (sender, args) =>
        //    {
        //        if (args.PropertyName == "Items")
        //            itemsRaised = true;
        //    };
        //    target.UpdateVerdictForFiles();
        //}
    }
}