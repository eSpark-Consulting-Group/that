using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using APTAT.Common;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace APTAT.DeploymentCore.UnitTests.Facade.DescentTest.CommandsTests
{
    

    public class FindADTargetsCommandTests
    {


        private string _dnsName = "new dns name";
        readonly IAdEntry rootEntryMock;
        readonly IAdEntry compEntryMock;
        readonly IAdEntry ouEntryMock;
        readonly IActiveDirectoryObserver ado;
        readonly INetworkObserver networkObserver;

        string domain = "test";
        string path = "search path";

        public FindADTargetsCommandTests()
        {
            InitMocks(domain, path, out rootEntryMock, out compEntryMock, out ouEntryMock, out ado, out networkObserver);
            var x = ado.GetDomainRootEntry(domain, null, null);
        }

        [Fact]
        public void ExecuteSearchRootTest()
        {
            //InitCatalog();
            FindTargetsRequest request = new FindTargetsRequest();
            DomainSearchCriterion criteria = new DomainSearchCriterion
            {
                Domain = domain,
                FromRoot = true,
                ParentAddress = "anyaddress"
            };
            request.SearchCriterion = criteria;

            FindDomainTargetsCommand target = new FindDomainTargetsCommand(ado, request);

            Mock.Arrange(() => ado.GetDomainRootEntry(domain, null, null)).Returns(rootEntryMock).OccursOnce();
            Mock.Arrange(() => ado.GetChildEntries(rootEntryMock.FullPath, null, null)).Returns(new IAdEntry[0] ).OccursOnce();

            target.Execute();

            Assert.Equal(1, target.Response.Targets.Count);
            var computerTarget = target.Response.Targets.OfType<DomainTarget>().FirstOrDefault(trg => trg.EntryType == DomainEntryType.Root);
            Assert.NotNull(computerTarget);
            Assert.Equal(rootEntryMock.Name, computerTarget.Name);
            Assert.Equal(rootEntryMock.DnsName, computerTarget.DnsName);
            Assert.Equal(rootEntryMock.FullPath, computerTarget.LdapPath);
        }

        [Fact]
        public void ExecuteSearchNotRootTest()
        {
            //InitCatalog();
            FindTargetsRequest request = new FindTargetsRequest();
            DomainSearchCriterion criteria = new DomainSearchCriterion
            {
                Domain = domain,
                FromRoot = false,
                ParentAddress = path
            };
            request.SearchCriterion = criteria;

            FindDomainTargetsCommand target = new FindDomainTargetsCommand(ado, request);

            Mock.Arrange(() => ado.GetDomainRootEntry(Arg.IsAny<string>(), Arg.IsAny<string>(), Arg.IsAny<string>())).OccursNever();

            Mock.Arrange(() => ado.GetChildEntries(path, null, null)).Returns(new[] { compEntryMock, ouEntryMock }).OccursAtLeast(2,"xx");

            target.Execute();
            Assert.Equal(2, target.Response.Targets.Count);
            var computerTarget = target.Response.Targets.OfType<DomainTarget>().First(trg => trg.EntryType == DomainEntryType.Computer);
            Assert.Equal(compEntryMock.FullPath, computerTarget.LdapPath);
            Assert.Equal(compEntryMock.Name, computerTarget.Name);
            Assert.Equal(compEntryMock.DnsName, computerTarget.DnsName);
            var ouTarget = target.Response.Targets.OfType<DomainTarget>().First(trg => trg.EntryType == DomainEntryType.OrganizationUnit);
            Assert.Equal(ouEntryMock.FullPath, ouTarget.LdapPath);
            Assert.Equal(ouEntryMock.Name, ouTarget.Name);
        }

        private void InitMocks(string domain, string ldapPath, out IAdEntry rootEntryMock, out IAdEntry compEntryMock,
            out IAdEntry ouEntryMock, out IActiveDirectoryObserver activeDirectoryObserver, out INetworkObserver networkObserver)
        {
            var rootEntryMockInternal = Mock.Create<IAdEntry>();
            Mock.Arrange(() => rootEntryMockInternal.FullPath).Returns(ldapPath);
            Mock.Arrange(() => rootEntryMockInternal.Name).Returns("root name");
            Mock.Arrange(() => rootEntryMockInternal.DnsName).Returns("root dns name");
            Mock.Arrange(() => rootEntryMockInternal.Type).Returns(AdEntryType.AdRoot);
            rootEntryMock = rootEntryMockInternal;

            var compEntryMockInternal = Mock.Create<IAdEntry>();
            Mock.Arrange(() => compEntryMockInternal.FullPath).Returns("comp path");
            Mock.Arrange(() => compEntryMockInternal.Name).Returns("comp name");
            Mock.Arrange(() => compEntryMockInternal.DnsName).Returns("comp dns name");
            Mock.Arrange(() => compEntryMockInternal.Type).Returns(AdEntryType.Computer);
            Mock.Arrange(() => compEntryMockInternal.Sid).Returns("2");
            compEntryMock = compEntryMockInternal;

            var ouEntryMockInternal = Mock.Create<IAdEntry>();
            Mock.Arrange(() => ouEntryMockInternal.FullPath).Returns("ou path");
            Mock.Arrange(() => ouEntryMockInternal.Name).Returns("ou name");
            Mock.Arrange(() => ouEntryMockInternal.DnsName).Returns("ou dns name");
            Mock.Arrange(() => ouEntryMockInternal.Type).Returns(AdEntryType.OrganizationUnit);
            Mock.Arrange(() => ouEntryMockInternal.Sid).Returns("1");
            ouEntryMock = ouEntryMockInternal;

            var activeDirectoryObserverInternal = Mock.Create<IActiveDirectoryObserver>();
            Mock.Arrange(() => activeDirectoryObserverInternal.GetDomainRootEntry(domain, null, null))
                .Returns(rootEntryMock);
            Mock.Arrange(() => activeDirectoryObserverInternal.GetChildEntries(ldapPath, null, null))
                .Returns(new[] { compEntryMock, ouEntryMock });
            Mock.Arrange(() => activeDirectoryObserverInternal.GetChildEntries(ldapPath, "container", null, null))
                .Returns(new IAdEntry[] { });
            activeDirectoryObserver = activeDirectoryObserverInternal;

            var networkObservalInternal = Mock.Create<INetworkObserver>();
            Mock.Arrange(
                () => networkObservalInternal.SendPingAsync(compEntryMockInternal.Name, Arg.IsAny<Action<IPStatus, IPAddress>>()))
                .DoInstead(
                    (string name, Action<IPStatus, IPAddress> callback) =>
                        callback(IPStatus.Success, IPAddress.Loopback));
            Mock.Arrange(
                () => networkObservalInternal.ResolveDnsByIp(Arg.IsAny<IPAddress>()))
                .Returns(_dnsName);
            networkObserver = networkObservalInternal;
        }
    }
}
