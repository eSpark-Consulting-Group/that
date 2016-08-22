using System;
using APTAT.DeploymentCore.Facade.Descent.DataContracts.DeploymentTargets;
using System.Threading.Tasks;

namespace APTAT.DeploymentCore.Directory.Resolvers
{
    public interface ITargetsResolver
    {
        DeploymentTargetsCollection Resolve();
        void BeginResolve(object state = null);
        Task<TargetsResolverEventArgs> ResolveAsync(object state = null);

        event EventHandler<TargetsResolverEventArgs> TargetsResolved;
    }
    public interface IDomainTargetsResolver : ITargetsResolver
    {
    }
}
