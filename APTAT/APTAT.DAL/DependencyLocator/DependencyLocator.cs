using APTAT.Common;
using APTAT.Core.Storage;
using APTAT.DAL.Storage;

namespace APTAT.DAL.DependencyLocator
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<IEntityStorage, EntityStorage>();
        }
    }
}
