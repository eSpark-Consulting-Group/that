using System;
using DDT.Core;

namespace APTAT.Common
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.Register<IChangeTrackingSubSystem, ChangeTrackingSubSystem>(true);
        }
    }
}
