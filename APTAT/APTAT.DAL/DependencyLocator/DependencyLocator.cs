using APTAT.Common;
using APTAT.DAL.Storage;
using System;

namespace APTAT.DAL.DependencyLocator
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingletonLazy<IEntityStorage, EntityStorage>();
        }
    }
}
