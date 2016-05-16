using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using APTAT.Core.Facade.Descent.DataContracts.DeploymentTargets;

namespace APTAT.Core.Directory.Resolvers
{
    public interface ITargetsResolver
    {
        DeploymentTargetsCollection Resolve();
        void BeginResolve(object state = null);
        event EventHandler<TargetsResolverEventArgs> TargetsResolved;
    }
    public interface IDomainTargetsResolver : ITargetsResolver
    {
    }
}
