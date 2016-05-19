
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
        private readonly ISubSystemCatalogProvider _provider = new LocalSubSystemCatalogProvider();

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
                            _catalog = _provider.GetSubSystemCatalog();
                    }
                }
                return _catalog;
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
            return new SubSystemCatalog();
        }
    }
}
