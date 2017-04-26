using System.Security.AccessControl;
using FAT.Application.Services;
using APTAT.Common;
using APTAT.Core;
using APTAT.Core.CMC;
using FAT.Application.Models;
using APTAT.Core.Reports;

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
            resolver.RegisterSingleton<IDataService, DataService>();

            resolver.Register<IScanOperationsManager, ScanOperationsManager>();
            resolver.Register<IScanOperationCollection, ScanOperationCollection>();
            resolver.Register<IScanCounterCollection, ScanCounterCollection>();
            resolver.Register<IScanCounterFactory, ScanCounterFactory>();
            resolver.Register<IScanCounterManager, ScanCounterManager>();
            resolver.Register<IScanTimer, ScanTimer>();

            resolver.Register<ICmcClient, CmcClient>();
            resolver.Register<IStatisticsService, StatisticsService>();
            resolver.Register<ICmcWebApiProvider, CmcWebApiProvider>();
            resolver.Register<ISystemInfoProvider, SystemInfoProvider>();
            resolver.Register<ICultureInfoProvider, CultureInfoProvider>();

            //overriding APTAT.Core registrations 
            resolver.AllowOverridingRegistrations(true);
            resolver.RegisterSingleton<IDistinctFileHelper, FatFileFilter>();
            resolver.RegisterSingleton<IReportBuilderFactory, ReportBuilderFactoryFat>();

        }
    }
}
