
namespace APTAT.DAL.Storage
{
    public interface IEntityStorage
    {
        IStorageContext GetContext();
    }

    public sealed class EntityStorage : IEntityStorage
    {
        public IStorageContext GetContext()
        {
            return new StorageContext();
        }
    }
}
