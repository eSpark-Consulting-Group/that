using System;
using APTAT.DeploymentCore.Directory.Discoverers;
using APTAT.DeploymentCore.Directory.Resolvers;
using APTAT.DeploymentCore.Facade.Descent.Commands;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.SearchCriteria.EndpointAddress;
using APTAT.DeploymentCore.Facade.Descent.MessageContracts;

namespace APTAT.DeploymentCore.Directory
{
    public class TargetsManager : ITargetsManager
    {
        private readonly ICommandFactory _commandFactory;

        public TargetsManager(ICommandFactory commandFactory)
        {
            _commandFactory = commandFactory;
        }
        public IDomainDiscoverer GetDomainDiscoverer(string ldapPath, bool fromRoot, string domain, string login, string password)
        {
            IServiceCommand<FindTargetsResponse> command = GetDomainDiscoveryCommand(fromRoot, ldapPath, domain, login, password);
            return new DomainDiscoverer(command);
        }

        private IServiceCommand<FindTargetsResponse> GetDomainDiscoveryCommand(bool fromRoot, string ldapPath, string domain, string login, string password)
        {
            FindTargetsRequest findTargetsRequest = new FindTargetsRequest();
            DomainSearchCriterion searchCriterion = new DomainSearchCriterion();

            if (fromRoot)
                searchCriterion.FromRoot = true;
            else
                if (String.IsNullOrEmpty(ldapPath))
                throw new ArgumentException("Ldap path is not specified.");
            searchCriterion.Domain = domain;
            searchCriterion.Login = login;
            searchCriterion.Password = password;
            searchCriterion.ParentAddress = ldapPath;

            findTargetsRequest.SearchCriterion = searchCriterion;
            return _commandFactory.CreateCommand<FindTargetsResponse>(findTargetsRequest);
        }

        public IWorkgroupDiscoverer GetWorkgroupDiscoverer(string workgroupName)
        {
            IServiceCommand<FindTargetsResponse> command = GetWorkgroupDiscoveryCommand(workgroupName);
            return new WorkgroupDiscoverer(command, GetEnumerateWorkgroupsCommand());
        }

        private IServiceCommand<EnumerateWorkgroupsResponse> GetEnumerateWorkgroupsCommand()
        {
            return  _commandFactory.CreateCommand<EnumerateWorkgroupsResponse>();
        }

        private IServiceCommand<FindTargetsResponse> GetWorkgroupDiscoveryCommand(string workgroupName)
        {
            FindTargetsRequest findTargetsRequest = new FindTargetsRequest();
            WorkgroupSearchCriterion searchCriterion = new WorkgroupSearchCriterion();

            searchCriterion.WorkgroupName = workgroupName;

            findTargetsRequest.SearchCriterion = searchCriterion;
            return _commandFactory.CreateCommand<FindTargetsResponse>(findTargetsRequest);
        }

        public IIpAddressDiscoverer GetIpAddressDiscoverer(EndpointAddressCollection addresses)
        {
            IServiceCommand<FindTargetsResponse> command = GetIpAddressDiscoveryCommand(addresses);
            return new IpAddressDiscoverer(command);
        }

        private IServiceCommand<FindTargetsResponse> GetIpAddressDiscoveryCommand(EndpointAddressCollection addresses)
        {
            FindTargetsRequest findTargetsRequest = new FindTargetsRequest();
            EndpointSearchCriterion searchCriterion = new EndpointSearchCriterion();

            searchCriterion.EndpointAddresses = addresses;

            findTargetsRequest.SearchCriterion = searchCriterion;
            return _commandFactory.CreateCommand<FindTargetsResponse>(findTargetsRequest);
        }

        public DeploymentTargetsCollection DiscoverDomain(string ldapPath, bool fromRoot, string domain, string login, string password)
        {
            IServiceCommand<FindTargetsResponse> command = GetDomainDiscoveryCommand(fromRoot, ldapPath, domain, login, password);
            command.Execute();
            return command.Response.Targets;
        }

        public IDomainTargetsResolver GetDomainTargetsResolver(DeploymentTargetsCollection targets, string login, string password)
        {
            IServiceCommand<GetTargetsSummaryResponse> command = GetDomainTargetsResolverCommand(targets, login, password);
            return new DomainTargetsResolver(command);
        }

        private IServiceCommand<GetTargetsSummaryResponse> GetDomainTargetsResolverCommand(DeploymentTargetsCollection targets, string login, string password)
        {
            if (targets == null || targets.Count == 0)
                throw new ArgumentException("Targets list is empty.");

            GetTargetsSummaryRequest request = new GetTargetsSummaryRequest { Targets = targets, DomainAccessLogin = login, DomainAccessPassword = password };
            return _commandFactory.CreateCommand<GetTargetsSummaryResponse>(request);
        }

        public DeploymentTargetsCollection CheckTargetsAvailability(DeploymentTargetsCollection targets, string login, string password)
        {
            var command = GetDomainTargetsResolverCommand(targets, login, password);
            command.Execute();
            return command.Response.Targets;
        }
    }
}
