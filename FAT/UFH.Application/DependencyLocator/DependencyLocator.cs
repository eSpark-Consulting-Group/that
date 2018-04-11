using System.Security.AccessControl;
using UFH.Application.Services;
using APTAT.Common;
using APTAT.Core;
using APTAT.Core.CMC;
using UFH.Application.Models;
using APTAT.Core.Reports;

namespace UFH.Application
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

            resolver.RegisterSingleton<ICmcClient, CmcClient>();
            resolver.RegisterSingleton<IStatisticsService, StatisticsService>();
            resolver.RegisterSingleton<ICmcWebApiProvider, CmcWebApiProvider>();
            resolver.RegisterSingleton<ISystemInfoProvider, SystemInfoProvider>();
            resolver.RegisterSingleton<ICultureInfoProvider, CultureInfoProvider>();

            //overriding APTAT.Core registrations 
            resolver.AllowOverridingRegistrations(true);
            resolver.RegisterSingleton<IDistinctFileHelper, FatFileFilter>();
            resolver.RegisterSingleton<IReportBuilderFactory, ReportBuilderFactoryFat>();

        }
    }
}
