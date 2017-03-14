using APTAT.Common.MachineKey;

namespace APTAT.Common
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<IChangeTrackingSubSystem, ChangeTrackingSubSystem>();
            resolver.RegisterSingleton<IMachineKeyProvider, MachineKeyProvider>();
            resolver.Register<ITimer, Timer>();
        }
    }
}
