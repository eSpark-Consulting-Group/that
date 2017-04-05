using APTAT.Application.Models.Apt;
using APTAT.Core.Descent.Apt;
using APTAT.DeploymentCore;
using FAT.Application.Models;
using System;
using System.IO;
using Telerik.JustMock;
using Xunit;

namespace FAT.Application.Tests
{
    public class ThreatInfoItemTests
    {
        [Theory]
        [InlineData("testname", "testdnsname", "testip", true, true, "testldap")]
        [InlineData("testip", "testdnsname", "testip", false, false, "testldap")]
        public void ComputerConstructorTest(string name, string dnsName, string ipAddress, bool isOnline, bool isLocal, string ldapPath)
        {
            var computer = Mock.Create<IComputer>(Behavior.Strict);
            var guid = Guid.NewGuid();
            Mock.Arrange(() => computer.Name).Returns(name);
            Mock.Arrange(() => computer.DnsName).Returns(dnsName);
            Mock.Arrange(() => computer.IPAddress).Returns(ipAddress);
            Mock.Arrange(() => computer.IsOnline).Returns(isOnline);
            Mock.Arrange(() => computer.IsLocalComputer).Returns(isLocal);
            Mock.Arrange(() => computer.Uid).Returns(guid);
            Mock.Arrange(() => computer.LdapPath).Returns(ldapPath);

            //act
            ThreatInfoItem target = new ThreatInfoItem(computer);

            Assert.True(target.IsComputer);
            Assert.NotNull(target.Items);
            Assert.Empty(target.Items);
            Assert.Null(target.Computers);
            Assert.Null(target.BadFile);
            Assert.Null(target.Parent);
            Assert.Equal(name == ipAddress ? dnsName : name, target.Name);
            Assert.Equal(ipAddress, target.IpAddress);
            Assert.Equal(isOnline, target.IsOnline);
            Assert.Equal(isLocal, target.IsLocal);
            Assert.Equal(guid, target.ComputerGuid);
            Assert.Equal(ldapPath, target.LdapPath);
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

        [Theory]
        [InlineData("testname", 0, "testsize", "testsize2", "testproduct")]
        [InlineData("testname", 0, "testsize", "testsize2", null)]
        public void FileConstructorTest(string name, long size, string sizeSorting, string sizeFormatted, string product)
        {
            var badFile = Mock.Create<IBadFileItem>(Behavior.Strict);
            Mock.Arrange(() => badFile.Name).Returns(name);
            Mock.Arrange(() => badFile.Size).Returns(size);
            Mock.Arrange(() => badFile.SizeSorting).Returns(sizeSorting);
            Mock.Arrange(() => badFile.FileSizeFormatted).Returns(sizeFormatted);
            Mock.Arrange(() => badFile.Product).Returns(product);

            //act
            ThreatInfoItem target = new ThreatInfoItem(badFile);

            Assert.False(target.IsComputer);
            Assert.Null(target.Items);
            Assert.Null(target.Computers);
            Assert.Null(target.DeploymentTarget);
            Assert.Null(target.Parent);
            Assert.Equal(name, target.Name);
            Assert.Equal(size, target.Size);
            Assert.Equal(sizeSorting, target.SizeSorting);
            Assert.Equal(sizeFormatted, target.SizeString);
            Assert.Equal(true, target.IsOnline);
            Assert.EndsWith(string.IsNullOrEmpty(product) ? Path.GetFileName(target.Name) : ")", target.ShortName);
            Assert.Equal(badFile, target.BadFile);
            Assert.Equal(ScanStage.OperationCompleted, target.ScanStage);
        }

        [Theory]
        [InlineData("Items")]
        [InlineData("HasItems")]
        public void AddFileTest(string propertyName)
        {
            var computer = Mock.Create<IComputer>(Behavior.Strict);
            var file = Mock.Create<IThreatInfoItem>(Behavior.Strict);
            var badFile = Mock.Create<IBadFileItem>(Behavior.Strict);

            Mock.Arrange(() => computer.Name).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.DnsName).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.IPAddress).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.IsOnline).Returns(Arg.AnyBool);
            Mock.Arrange(() => computer.IsLocalComputer).Returns(Arg.AnyBool);
            Mock.Arrange(() => computer.Uid).Returns(Arg.AnyGuid);
            Mock.Arrange(() => computer.LdapPath).Returns(Arg.AnyString);

            ThreatInfoItem target = new ThreatInfoItem(computer);

            Mock.ArrangeSet(() => file.Parent = Arg.Matches<IThreatInfoItem>(_ => ReferenceEquals(_, target))).MustBeCalled();
            Mock.Arrange(() => file.BadFile).Returns(badFile);
            Mock.Arrange(() => file.IsComputer).Returns(false);
            Mock.Arrange(() => badFile.Sha1).Returns(string.Empty);

            //act
            Action a = () => target.AddFile(file);

            Assert.PropertyChanged(target, propertyName, a);
            Assert.NotNull(target.Items);
            Assert.Single(target.Items);
            Mock.Assert(file);
        }

        [Theory]
        [InlineData(1000, 1, 2, 3, 4, 5, 99.99d, "teststatus")]
        public void UpdateCountersTest(int totalFiles, int cleanFilesCount, int malwaresFilesCount, int unknownFilesCount,
            int noVerdictFilesCount, int failedToAnalyzeFilesCount, double percentage, string message)
        {
            var computer = Mock.Create<IComputer>(Behavior.Strict);
            Mock.Arrange(() => computer.Name).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.DnsName).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.IPAddress).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.IsOnline).Returns(Arg.AnyBool);
            Mock.Arrange(() => computer.IsLocalComputer).Returns(Arg.AnyBool);
            Mock.Arrange(() => computer.Uid).Returns(Arg.AnyGuid);
            Mock.Arrange(() => computer.LdapPath).Returns(Arg.AnyString);

            ThreatInfoItem target = new ThreatInfoItem(computer);
            
            var scanResultItem = Mock.Create<IScanResultItem>(Behavior.Strict);
            Mock.Arrange(() => scanResultItem.CleanFilesCount).Returns(cleanFilesCount);
            Mock.Arrange(() => scanResultItem.TotalFilesCount).Returns(totalFiles);
            Mock.Arrange(() => scanResultItem.MalwareFilesCount).Returns(malwaresFilesCount);
            Mock.Arrange(() => scanResultItem.UnknownFilesCount).Returns(unknownFilesCount);
            Mock.Arrange(() => scanResultItem.NoVerdictFilesCount).Returns(noVerdictFilesCount);
            Mock.Arrange(() => scanResultItem.FailedToAnalyzeFilesCount).Returns(failedToAnalyzeFilesCount);
            Mock.Arrange(() => scanResultItem.Percentage).Returns(percentage);
            Mock.Arrange(() => scanResultItem.StatusMessage).Returns(message);
            Mock.Arrange(() => scanResultItem.ScanStage).Returns(ScanStage.ScanInProgress);

            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);
            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(0);
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(0);
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(0);
            Mock.ArrangeSet(() => scanCounterDiff.CleanDiff = Arg.Matches<int>(_ => scanResultItem.CleanFilesCount == _)).MustBeCalled();
            Mock.ArrangeSet(() => scanCounterDiff.MalwareDiff = Arg.Matches<int>(_ => scanResultItem.MalwareFilesCount == _)).MustBeCalled();
            Mock.ArrangeSet(() => scanCounterDiff.InProgressDiff = Arg.Matches<int>(_ => 
                scanResultItem.UnknownFilesCount + scanResultItem.NoVerdictFilesCount + scanResultItem.FailedToAnalyzeFilesCount == _)).MustBeCalled();
            Mock.ArrangeSet(() => scanCounterDiff.NotAnalyzedDiff = Arg.AnyInt).OccursNever();
            Mock.ArrangeSet(() => scanCounterDiff.UnknownDiff = Arg.AnyInt).OccursNever();

            //act
            target.UpdateCounters(scanResultItem, scanCounterDiff);

            Assert.Equal(totalFiles, target.TotalFilesCount);     
            Assert.Equal(malwaresFilesCount, target.MalwareFilesCount);
            Assert.Equal(cleanFilesCount, target.CleanFilesCount);
            Assert.Equal((noVerdictFilesCount + failedToAnalyzeFilesCount + unknownFilesCount), target.PendingFilesCount);
            Assert.Equal(percentage / 2, target.TotalProgressPercentage);
            Assert.Equal(percentage, target.RemoteScanningPercentage);
            Assert.Equal(ScanStage.ScanInProgress, target.ScanStage);
            Assert.Equal(message, target.StatusMessageSystem);
            Mock.Assert(scanCounterDiff);
        }

        [Theory]
        [InlineData("VerdictString", CloudVerdict.FailedToAnalyze)]
        [InlineData("IsMalware", CloudVerdict.FirstSeen)]
        [InlineData("IsUnknown", CloudVerdict.Malware)]
        [InlineData("Clean", CloudVerdict.NoVerdict)]
        [InlineData("Failed", CloudVerdict.Safe)]
        [InlineData("Pending", CloudVerdict.Unknown)]
        public void UpdateFileVerdictTest(string propertyName, CloudVerdict verdict)
        {
            var badFile = Mock.Create<IBadFileItem>(Behavior.Strict);
            Mock.Arrange(() => badFile.Name).Returns(Arg.AnyString);
            Mock.Arrange(() => badFile.Size).Returns(Arg.AnyLong);
            Mock.Arrange(() => badFile.SizeSorting).Returns(Arg.AnyString);
            Mock.Arrange(() => badFile.FileSizeFormatted).Returns(Arg.AnyString);
            Mock.Arrange(() => badFile.Product).Returns(Arg.AnyString);
            Mock.ArrangeSet(() => badFile.Cloud = Arg.Matches<CloudVerdict>(_ => verdict == _)).MustBeCalled();

            ThreatInfoItem target = new ThreatInfoItem(badFile);

            //act
            Action a = () => target.UpdateFileVerdict(verdict);

            Assert.PropertyChanged(target, propertyName, a);
            Mock.Assert(badFile);
        }

        [Theory]
        [InlineData("teststatus", "testreason", true, 1f, APTAT.Valkyrie.ValkyrieVerdict.Malware)]
        public void UpdateFileVerdictTest2(string status, string reason, bool isWhitelisted, float probability, APTAT.Valkyrie.ValkyrieVerdict verdict)
        {
            var context = Mock.Create<IFileVerdictContext>(Behavior.Strict);
            var result = Mock.Create<APTAT.Valkyrie.IValkyrieAnalysisResult>(Behavior.Strict);
            var badFile = Mock.Create<IBadFileItem>(Behavior.Strict);

            Mock.Arrange(() => badFile.Name).Returns(Arg.AnyString);
            Mock.Arrange(() => badFile.Size).Returns(Arg.AnyLong);
            Mock.Arrange(() => badFile.SizeSorting).Returns(Arg.AnyString);
            Mock.Arrange(() => badFile.FileSizeFormatted).Returns(Arg.AnyString);
            Mock.Arrange(() => badFile.Product).Returns(Arg.AnyString);
            Mock.Arrange(() => badFile.Cloud).Returns(Arg.IsAny<CloudVerdict>);
            Mock.Arrange(() => result.IsWhiteListed).Returns(isWhitelisted);
            Mock.Arrange(() => result.MalwareProbability).Returns(probability);
            Mock.Arrange(() => result.OperationStatus).Returns(status);
            Mock.Arrange(() => result.SkipReason).Returns(reason);
            Mock.Arrange(() => result.Verdict).Returns(verdict);
            Mock.Arrange(() => context.ValkyrieAnalysisResult).Returns(result);
            Mock.Arrange(() => context.FlsVerdict).Returns((CloudVerdict?)null);

            ThreatInfoItem target = new ThreatInfoItem(badFile);

            //act
            target.UpdateFileVerdict(context);

            Assert.Equal(status, target.OperationStatus);
            Assert.Equal(reason, target.SkipReason);
            Assert.Equal(isWhitelisted, target.IsWhiteListed);
            Assert.Equal(probability, target.MalwareProbabilityValue);
            Assert.Equal(verdict, target.ValkyrieVerdict);
        }

        [Theory]
        [InlineData(UiFinalVerdict.Clean, "cf23df2207d99a74fbe169e3eba035e633b65d94")]
        [InlineData(UiFinalVerdict.Malware, "cf23df2207d99a74fbe169e3eba035e633b65d94")]
        [InlineData(UiFinalVerdict.Unknown, "cf23df2207d99a74fbe169e3eba035e633b65d94")]
        [InlineData(UiFinalVerdict.NotAnalyzed, "cf23df2207d99a74fbe169e3eba035e633b65d94")]
        public void UpdateVerdictForFiles(UiFinalVerdict verdict, string sha1)
        {
            var computer = Mock.Create<IComputer>(Behavior.Strict);
            var file = Mock.Create<IThreatInfoItem>(Behavior.Strict);
            var badFile = Mock.Create<IBadFileItem>(Behavior.Strict);
            var context = Mock.Create<IFileVerdictContext>(Behavior.Strict);
            var scanCounterDiff = Mock.Create<IScanCounterDiff>(Behavior.Strict);

            Mock.Arrange(() => scanCounterDiff.CleanDiff).Returns(Arg.AnyInt);
            Mock.ArrangeSet(() => scanCounterDiff.CleanDiff = Arg.Matches<int>(_ => _ == 1));
            Mock.Arrange(() => scanCounterDiff.MalwareDiff).Returns(Arg.AnyInt);
            Mock.ArrangeSet(() => scanCounterDiff.MalwareDiff = Arg.Matches<int>(_ => _ == 1));
            Mock.Arrange(() => scanCounterDiff.UnknownDiff).Returns(Arg.AnyInt);
            Mock.ArrangeSet(() => scanCounterDiff.UnknownDiff = Arg.Matches<int>(_ => _ == 1));
            Mock.Arrange(() => scanCounterDiff.NotAnalyzedDiff).Returns(Arg.AnyInt);
            Mock.ArrangeSet(() => scanCounterDiff.NotAnalyzedDiff = Arg.Matches<int>(_ => _ == 1));
            Mock.Arrange(() => scanCounterDiff.InProgressDiff).Returns(Arg.AnyInt);
            Mock.ArrangeSet(() => scanCounterDiff.InProgressDiff = Arg.Matches<int>(_ => _ == -1)).MustBeCalled();

            Mock.Arrange(() => computer.Name).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.DnsName).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.IPAddress).Returns(Arg.AnyString);
            Mock.Arrange(() => computer.IsOnline).Returns(Arg.AnyBool);
            Mock.Arrange(() => computer.IsLocalComputer).Returns(Arg.AnyBool);
            Mock.Arrange(() => computer.Uid).Returns(Arg.AnyGuid);
            Mock.Arrange(() => computer.LdapPath).Returns(Arg.AnyString);

            Mock.Arrange(() => context.Sha1).Returns(sha1);

            ThreatInfoItem target = new ThreatInfoItem(computer);
            target.PendingFilesCount = 1;
            bool verdictUpdated = false;

            Mock.ArrangeSet(() => file.Parent = Arg.IsAny<IThreatInfoItem>());
            Mock.Arrange(() => file.BadFile).Returns(badFile);
            Mock.Arrange(() => file.IsComputer).Returns(false);
            Mock.Arrange(() => file.FinalVerdict).Returns(() => !verdictUpdated ? UiFinalVerdict.InProgress : verdict);
            Mock.Arrange(() => file.UpdateFileVerdict(Arg.Matches<IFileVerdictContext>(_ => ReferenceEquals(_, context)))).DoInstead(() => verdictUpdated = true).Returns(true).MustBeCalled();
            Mock.Arrange(() => badFile.Sha1).Returns(sha1);       

            target.AddFile(file);
            //act
            target.UpdateVerdictForFiles(new[] { context }, scanCounterDiff);

            Mock.Assert(scanCounterDiff);
            Mock.Assert(file);
            Assert.Equal(Convert.ToInt32(verdict == UiFinalVerdict.Clean), target.CleanFilesCount);
            Assert.Equal(Convert.ToInt32(verdict == UiFinalVerdict.Malware), target.MalwareFilesCount);
            Assert.Equal(Convert.ToInt32(verdict == UiFinalVerdict.Unknown), target.UnknownFilesCount);
            Assert.Equal(Convert.ToInt32(verdict == UiFinalVerdict.NotAnalyzed), target.NotAnalyzedFilesCount);
            Assert.Equal(0, target.PendingFilesCount);
        }
    }
}