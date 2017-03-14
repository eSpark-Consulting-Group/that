using System;
using System.Data.Entity;
using System.Linq;
using APTAT.DAL;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.Core;
using APTAT.DAL.Enums;
using APTAT.DAL.Storage.DTO;

namespace APTAT.Core.Storage.Repositories
{
    public interface IAptScanResultRepository : IRepository<AptScanResult>
    {
        AptScanResult GetByTargetUidAndOperationUid(Guid targetUid, Guid operationUid);
        AptScanResultDTO[] GetByOperationUid(Guid operationUid);

    }

    internal sealed class AptScanResultRepository
        : Repository<AptEntities, AptScanResult, AptScanResult>, IAptScanResultRepository
    {
        public AptScanResultRepository(AptEntities dbContext)
            : base(dbContext)
        { }

        public AptScanResult GetByTargetUidAndOperationUid(Guid targetUid, Guid operationUid)
        {
            return DbSet.Include(c => c.OperationResult).Where(c => c.TargetUid == targetUid &&
                                                             c.OperationResult.OperationUid == operationUid)
                .ExecuteDbQuerySingleOrDefault();
        }

        public AptScanResultDTO[] GetByOperationUid(Guid operationUid)
        {

            var query = DbSet
                .Where(c => c.OperationResult.OperationUid == operationUid)
                .Select(c => new
                {
                    Modules = c.Modules.GroupJoin(DbContext.ValkyrieFiles,
                          m => new { sha1 = m.Sha1, oId = m.OperationResultId },
                          v => new { sha1 = v.Sha1, oId = v.ValkyrieScanInfoId },
                    (m, vs) => new { Module = m, Valkyrie = vs.DefaultIfEmpty().FirstOrDefault() }
                          ),
                    Data = c
                }).ToArray();

            var result = query.Select(r =>
               new AptScanResultDTO
               {
                   ScanResult = r.Data,
                   Modules = r.Modules.Select(c => new ModuleWithValkyrie
                   {
                       ValkyrieDetails = c.Valkyrie,
                       Accessed = c.Module.Accessed,
                       AptScanResult = c.Module.AptScanResult,
                       AptScanResultId = c.Module.AptScanResultId,
                       Attributes = c.Module.Attributes,
                       CloudDate = c.Module.CloudDate,
                       CloudVerdict = c.Module.CloudVerdict,
                       Copyright = c.Module.Copyright,
                       Created = c.Module.Created,
                       Description = c.Module.Description,
                       FileAge = c.Module.FileAge,
                       FileAttributes = c.Module.FileAttributes,
                       FileSizeFormatted = c.Module.FileSizeFormatted,
                       Id = c.Module.Id,
                       Modified = c.Module.Modified,
                       Name = c.Module.Name,
                       Path = c.Module.Path,
                       Product = c.Module.Product,
                       Sha1 = c.Module.Sha1,
                       Signer = c.Module.Signer,
                       Size = c.Module.Size,
                       VerdictSource = c.Module.VerdictSource,
                       Version = c.Module.Version
                   }),

                   TotalCount = r.Data.TotalCount,
                   UnknownsCount = r.Modules.Count(f1 => (f1.Valkyrie != null
                                    && f1.Valkyrie.Verdict == ValkyrieVerdict.Undetected)
                                    && f1.Module.CloudVerdict != CloudVerdict.Malware),
                   MaliciousCount = r.Modules.Count(f1 => (f1.Valkyrie != null
                                    && (f1.Valkyrie.Verdict == ValkyrieVerdict.Malware
                                    || f1.Valkyrie.Verdict == ValkyrieVerdict.PotentiallyUnwantedApplication))
                                    || f1.Module.CloudVerdict == CloudVerdict.Malware),
                   InAnalysisCount = r.Modules.Count(f1 => (f1.Valkyrie != null
                                    && (f1.Valkyrie.Verdict == ValkyrieVerdict.PendingFls
                                    || f1.Valkyrie.Verdict == ValkyrieVerdict.InQueue
                                    || f1.Valkyrie.Verdict == ValkyrieVerdict.FirstSeen
                                    || f1.Valkyrie.Verdict == ValkyrieVerdict.Pending))
                                    || f1.Module.CloudVerdict == CloudVerdict.FailedToAnalyze),
                   NotAnalyzedCount = r.Modules.Count(f1 => (f1.Valkyrie != null
                                    && f1.Valkyrie.Verdict == ValkyrieVerdict.NotAnalyzed)),
               }).ToArray();
            
            return result;

        }

        protected override DbSet<AptScanResult> DbSet
        {
            get { return DbContext.AptScanResults; }
        }

        public override IQueryable<AptScanResult> EntityQuery
        {
            get { return DbContext.AptScanResults; }
        }
    }
}
