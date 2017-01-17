using System;

namespace APTAT.Common
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<IChangeTrackingSubSystem, ChangeTrackingSubSystem>();
        }
    }
}
