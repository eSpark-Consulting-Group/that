using APTAT.Common;
using Valkyrie.WebApiProvider;

namespace APTAT.Valkyrie.DependencyLocator
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<IValkyrieWebApiProvider, ValkyrieWebApiProvider>();
            resolver.RegisterSingleton<IValkyrieClient, ValkyrieClient>();
        }
    }
}
