
namespace APTAT.DAL.Storage
{
    public interface IEntityStorage
    {
        IStorageContext Context { get; }
    }

    public sealed class EntityStorage : IEntityStorage
    {
        public EntityStorage()
        {

        }

        public IStorageContext Context
        {
            get
            {
                return new StorageContext();
            }
        }
    }
}
