
namespace APTAT.Core
{
    public interface ISubSystemCatalogProvider
    {
        ISubSystemCatalog GetSubSystemCatalog();
    }

    public class SubSystemFactory
    {
        private static readonly SubSystemFactory Instance = new SubSystemFactory();
        private volatile ISubSystemCatalog _catalog;
        private ISubSystemCatalogProvider _provider;

        private SubSystemFactory()
        {
        }

        public static SubSystemFactory Current
        {
            get { return Instance; }
        }

        public ISubSystemCatalog Catalog
        {
            get
            {
                if (_catalog == null)
                {
                    lock (Instance)
                    {
                        if (_catalog == null)
                            _catalog = CatalogProvider.GetSubSystemCatalog();
                    }
                }
                return _catalog;
            }
        }

        public ISubSystemCatalogProvider CatalogProvider
        {
            get
            {
                if (_provider == null)
                    _provider = new LocalSubSystemCatalogProvider();

                return _provider;
            }
        }

        public T GetSubSystem<T>() where T : ISubSystem
        {
            return Catalog.Find<T>();
        }
    }

    public class LocalSubSystemCatalogProvider : ISubSystemCatalogProvider
    {
        public ISubSystemCatalog GetSubSystemCatalog()
        {
            //var serverPlugins = SubSystemCatalog.GetServerPluginsFromConfig();
            return new SubSystemCatalog(/*serverPlugins*/);
        }
    }
}
