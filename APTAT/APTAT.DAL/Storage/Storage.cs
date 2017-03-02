using APTAT.Core.Storage.Repositories;
using APTAT.DAL.Storage.Core;

namespace APTAT.DAL.Storage
{
    public interface IStorageContext : IStorageContextBase
    {
        IAptScanOperationResultRepository AptScanOperationResultRepository { get; }
        IAptScanResultRepository AptScanResultRepository { get; }
        IModulesRepository ModulesRepository { get; }
        IValkyrieScansRepository ValkyrieScansRepository { get; }
        IValkyrieFilesRepository ValkyrieFilesRepository { get; }
    }
    internal sealed class StorageContext : StorageContextBase<AptEntities>, IStorageContext
    {
        public IAptScanOperationResultRepository AptScanOperationResultRepository { get; }
        public IAptScanResultRepository AptScanResultRepository { get; }
        public IModulesRepository ModulesRepository { get; }
        public IValkyrieScansRepository ValkyrieScansRepository { get; }
        public IValkyrieFilesRepository ValkyrieFilesRepository { get; }

        public StorageContext(AptEntities context) : base (context)
        {
            AptScanOperationResultRepository = new AptScanOperationResultRepository(context);
            AptScanResultRepository = new AptScanResultRepository(context);
            ModulesRepository = new ModulesRepository(context);
            ValkyrieScansRepository = new ValkyrieScansRepository(context);
            ValkyrieFilesRepository = new ValkyrieFilesRepository(context);
        }
        
        public StorageContext()
            : this(new AptEntities())
        {
        }
    }
}
