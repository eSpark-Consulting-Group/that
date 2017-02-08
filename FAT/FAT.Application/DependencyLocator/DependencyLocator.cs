using FAT.Application.Services;
using APTAT.Common;
using APTAT.Core;

namespace FAT.Application
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<ITargetsDiscoveryService, TargetsDiscoveryService>();
            resolver.RegisterSingleton<ISettingsViewModel, SettingsViewModel>();
            resolver.RegisterSingleton<IAptService, AptService>();
            resolver.RegisterSingleton<IStorageService, StorageService>();
            resolver.RegisterSingleton<IFileBrowserService, FileBrowserService>();
            resolver.AllowOverridingRegistrations(true);
            resolver.RegisterSingleton<IDistinctFileHelper,FATFileFilter>();
        }
    }
}
