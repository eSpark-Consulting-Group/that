using System;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using APTAT.Core.Storage.Repositories;
using APTAT.DAL.Storage.Core;

namespace APTAT.DAL.Storage
{
    public interface IStorageContext : IStorageContextBase
    {
        IAptScanOperationResultRepository AptScanOperationResultRepository { get; }
        IAptScanResultRepository AptScanResultRepository { get; }
        IModulesRepository ModulesRepository { get; }
    }
    internal sealed class StorageContext : StorageContextBase<AptEntities>, IStorageContext
    {
        public IAptScanOperationResultRepository AptScanOperationResultRepository { get; }
        public IAptScanResultRepository AptScanResultRepository { get; }
        public IModulesRepository ModulesRepository { get; }

        public StorageContext(AptEntities context) : base (context)
        {
            AptScanOperationResultRepository = new AptScanOperationResultRepository(context);
            AptScanResultRepository = new AptScanResultRepository(context);
            ModulesRepository = new ModulesRepository(context);
        }
        
        public StorageContext()
            : this(new AptEntities())
        {
        }
    }
}
