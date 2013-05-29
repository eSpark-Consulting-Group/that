using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APTAT.Core.Facade.Descent.Commands;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;
using APTAT.Core.Facade.Descent.DataContracts.SearchCriteria;
using APTAT.Core.Facade.Descent.MessageContracts;

namespace APTAT.Core
{
    public class AptAtManager
    {
        private readonly CommandFactory _commandFactory = new CommandFactory();

        public DeploymentTargetsCollection DiscoverDomain(string ldapPath, bool fromRoot, string domain, string login, string password)
        {
            FindTargetsRequest findTargetsRequest = new FindTargetsRequest();
            DomainSearchCriterion searchCriterion = new DomainSearchCriterion();

            if (fromRoot)
                searchCriterion.FromRoot = fromRoot;
            else
                if (String.IsNullOrEmpty(ldapPath))
                    throw new ArgumentException("Ldap path is not specified.");

            searchCriterion.ParentAddress = ldapPath;
            findTargetsRequest.SearchCriterion = searchCriterion;
            var command = _commandFactory.CreateCommand<FindTargetsResponse>(findTargetsRequest);
            command.Execute();
            return command.Response.Targets;
        }

        public DeploymentTargetsCollection CheckTargetsAvailability(DeploymentTargetsCollection targets)
        {
            if(targets == null || targets.Count == 0)
                throw new ArgumentException("Targets list is empty.");

            GetTargetsSummaryRequest request = new GetTargetsSummaryRequest();
            request.Targets = targets;
            var command = _commandFactory.CreateCommand<GetTargetsSummaryResponse>(request);
            command.Execute();
            return command.Response.Targets;
        }
    }
}
