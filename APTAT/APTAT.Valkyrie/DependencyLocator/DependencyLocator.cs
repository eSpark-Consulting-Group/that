using APTAT.Common;

namespace APTAT.Valkyrie.DependencyLocator
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<IValkyrieClient, ValkyrieClient>();
        }
    }
}
