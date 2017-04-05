using System;
using System.Data.Entity;
using System.Linq;
using APTAT.DAL;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.Core;
using System.Diagnostics;

using APTAT.DAL.Enums;
using APTAT.DAL.Storage.DTO;
using System.Data.Entity.Core.Objects;
using System.Linq.Expressions;

namespace APTAT.Core.Storage.Repositories
{
    public interface IAptScanOperationResultRepository : IRepository<AptScanOperationResult>
    {
        AptScanOperationResult GetByOperationUid(Guid operationUid);
        IAptScanOperationDTO[] Search(string keyworld);
    }

    internal sealed class AptScanOperationResultRepository
        : Repository<AptEntities, AptScanOperationResult, AptScanOperationResult>, IAptScanOperationResultRepository
    {
        public AptScanOperationResultRepository(AptEntities dbContext)
            : base(dbContext)
        { }

        protected override DbSet<AptScanOperationResult> DbSet
        {
            get { return DbContext.AptScanOperaionResults; }
        }

        public override IQueryable<AptScanOperationResult> EntityQuery
        {
            get { return DbContext.AptScanOperaionResults; }
        }

        public AptScanOperationResult GetByOperationUid(Guid operationUid)
        {
            return DbSet.Where(o => o.OperationUid == operationUid).ExecuteDbQueryFirstOrDefault();
        }

        public static Expression<Func<AptScanOperationResult, bool>> SearchByKeyword(string keyword)
        {
            return result => result.AptScanResults.Any(r => r.ComputerName.ToLower().Contains(keyword)
                     || r.IpAddress.ToLower().Contains(keyword)
                     || r.Modules.Any(m => m.Name.ToLower().Contains(keyword)));
        }

        public IAptScanOperationDTO[] Search(string keyword)
        {
            var results = string.IsNullOrEmpty(keyword) ? 
                DbContext.AptScanOperaionResults : 
                DbContext.AptScanOperaionResults.Where(SearchByKeyword(keyword.ToLower()));

            var query = from i in results

                         let m =
                         (from module in i.AptScanResults.SelectMany(sr => sr.Modules)
                          join valkyrieFile in i.ValkyrieScanInfo.ValkyrieFileInfos
                          on new { sha1 = module.Sha1, oId = module.OperationResultId }
                          equals new { sha1 = valkyrieFile.Sha1, oId = valkyrieFile.ValkyrieScanInfoId }
                          into joined
                          from j in joined.DefaultIfEmpty()
                          select new { Module = module, Valkyrie = j })

                         select new AptScanOperationDTO()
                         {
                             OperationUid = i.OperationUid,
                             Started = i.Started,
                             Finished = i.Finished,
                             ScanMethod = i.ScanMethod,
                             Type = i.Source,
                             DiscovererDescription = i.SourceDescription,
                             FilesScanned = i.AptScanResults.Sum(sr => sr.Succeeded ? sr.TotalCount : 0),
                             Canceled = i.AptScanResults.Any(sr => sr.Canceled),
                             UnknownsCount = m.Count(f1 => (f1.Valkyrie != null
                                    && f1.Valkyrie.Verdict == ValkyrieVerdict.Undetected)
                                    && f1.Module.CloudVerdict != CloudVerdict.Malware),
                             MaliciousCount = m.Count(f1 => (f1.Valkyrie != null
                                    && (f1.Valkyrie.Verdict == ValkyrieVerdict.Malware
                                    || f1.Valkyrie.Verdict == ValkyrieVerdict.PotentiallyUnwantedApplication))
                                    || f1.Module.CloudVerdict == CloudVerdict.Malware),
                             InAnalysisCount = m.Count(f1 => (f1.Valkyrie != null
                                    && (f1.Valkyrie.Verdict == ValkyrieVerdict.PendingFls
                                    || f1.Valkyrie.Verdict == ValkyrieVerdict.InQueue
                                    || f1.Valkyrie.Verdict == ValkyrieVerdict.FirstSeen
                                    || f1.Valkyrie.Verdict == ValkyrieVerdict.Pending))
                                    || f1.Module.CloudVerdict == CloudVerdict.FailedToAnalyze),
                             NotAnalyzedCount = m.Count(f1 => (f1.Valkyrie != null
                                    && f1.Valkyrie.Verdict == ValkyrieVerdict.NotAnalyzed)),
                             ComputersScanned = i.AptScanResults.Count(sr => sr.Succeeded),
                             TotalComputersScanned = i.AptScanResults.Count,
                             ValkyrieOperationInfo = i.ValkyrieScanInfo != null ?
                             new ValkyrieOperationInfo
                             {
                                 ApiKey = i.ValkyrieScanInfo.ApiKey, 
                                 ScanId = i.ValkyrieScanInfo.ScanId,
                                 FinishedOnUi = i.ValkyrieScanInfo.FinishedWithoutErrorHandling
                             } : null

                         };

            return query.ExecuteDbQuery();
        }
    }
}
