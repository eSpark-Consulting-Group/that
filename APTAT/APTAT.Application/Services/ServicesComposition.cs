using APTAT.Common;

namespace APTAT.Application.Services
{
    public class ServicesComposition
    {
        public static void Init()
        {
            CompositionHelper.Instance.Register<ITargetsDiscoveryService, TargetsDiscoveryService>();
            CompositionHelper.Instance.Register<IAptService, AptService>();
            CompositionHelper.Instance.Register<IStorageService, StorageService>();
            CompositionHelper.Instance.Register<IWmiService, WmiService>();

            CompositionHelper.Instance.Verify();
        }
    }
}
