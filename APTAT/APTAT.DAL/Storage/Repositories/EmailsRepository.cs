using System.Text;
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

namespace APTAT.Core.Storage.Repositories
{
    public interface IEmailRepository : IRepository<Email>
    {
        IQueryable<Email> EntityQuery { get; }
    }
    
    internal sealed class EmailsRepository : Repository<AptEntities, Email, Email>, IEmailRepository
    {
        public EmailsRepository(AptEntities dbContext) : base(dbContext) { }

        public override IQueryable<Email> EntityQuery
        {
            get { return DbContext.Emails; }
        }
        protected override DbSet<Email> DbSet
        {
            get { return DbContext.Emails; }
        }
    }
}
