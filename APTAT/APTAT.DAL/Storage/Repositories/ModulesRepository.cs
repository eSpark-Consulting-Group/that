using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using APTAT.DAL;
using APTAT.DAL.Entities;
using APTAT.DAL.Storage.Core;
using APTAT.DAL.Enums;

namespace APTAT.Core.Storage.Repositories
{
    public interface IModulesRepository : IRepository<Module>
    {
        void UpdateVerdictByHashAndOperationUid(Guid operationUid, IGrouping<CloudVerdict, string>[] group, VerdictSource verdictSource);
    }

    internal sealed class ModulesRepository
        : Repository<AptEntities, Module, Module>, IModulesRepository
    {
        public ModulesRepository(AptEntities dbContext)
            : base(dbContext)
        { }

        protected override DbSet<Module> DbSet
        {
            get { return DbContext.Modules; }
        }

        public override IQueryable<Module> EntityQuery
        {
            get { return DbContext.Modules; }
        }



        public void UpdateVerdictByHashAndOperationUid(Guid operationUid, IGrouping<CloudVerdict, string>[] group,
            VerdictSource verdictSource)
        {
            Action<string> print = message => Debug.WriteLine(message);
            DbContext.Database.Log = print;
            foreach (var grouping in group)
            {
                try
                {
                       DbContext.Database.ExecuteSqlCommand(@"UPDATE [Modules]
                           SET [CloudVerdict] = @verdict
                              ,[VerdictSource] = @verdictSource
                         WHERE [Sha1] IN (" + GetCommaSeparatedList(grouping) + @") AND EXISTS
                         (
                             SELECT 1
                              FROM AptScanResults cr JOIN
                              AptScanOperationResults opr ON cr.OperationResultId = opr.Id
                              WHERE cr.Id = Modules.AptScanResultId
                              AND opr.OperationUid = @operationUid
                         );", new SqlCeParameter("@operationUid",operationUid),
                         new SqlCeParameter("@verdict", (int)grouping.Key),
                         new SqlCeParameter("@verdictSource", (int)verdictSource));

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            DbContext.SaveChangesWithErrorsHandling(ConcurrencyResolutionStrategy.ClientWins);

        }

        private static string GetCommaSeparatedList(IEnumerable<string> hashes)
        {
            return String.Join(",", hashes.Select(hash => $"'{hash.ToString(CultureInfo.InvariantCulture)}'"));
        }
    }
}
