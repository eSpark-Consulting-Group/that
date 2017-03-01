using FAT.Application.Services;
using APTAT.Common;
using APTAT.Core;
using FAT.Application.Models;

namespace FAT.Application
{
    public class DependencyLocator : IDependencyLocatorModule
    {
        public void RegisterDependencies(ICompositionHelper resolver)
        {
            resolver.RegisterSingleton<ITargetsDiscoveryService, TargetsDiscoveryService>();
            resolver.RegisterSingleton<IProxySettingsViewModel, ProxySettingsViewModel>();
            resolver.RegisterSingleton<IAptService, AptService>();
            resolver.RegisterSingleton<IStorageService, StorageService>();
            resolver.RegisterSingleton<IFileBrowserService, FileBrowserService>();
            resolver.RegisterSingleton<IScanCounterManager, ScanCounterManager>();
            resolver.RegisterSingleton<IScanCounterCollection, ScanCounterCollection>();
            resolver.RegisterSingleton<IScanCounterFactory, ScanCounterFactory>();
            resolver.AllowOverridingRegistrations(true);
            resolver.RegisterSingleton<IDistinctFileHelper,FATFileFilter>();
        }
    }
}
