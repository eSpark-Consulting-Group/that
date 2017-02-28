using System.Data.SqlServerCe;
using System.Reflection;
using APTAT.DAL.Entities;

namespace APTAT.DAL
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class AptEntities : DbContext
    {
        // Your context has been configured to use a 'AptEntities' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'APTAT.DAL.AptEntities' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'AptEntities' 
        // connection string in the application configuration file.
        private static readonly string _connectionString;
        static AptEntities()
        {
            var builder = new SqlCeConnectionStringBuilder();
            builder.Password = "Test1231";
            builder.DataSource = "AptDatabase.sdf";
            builder.PersistSecurityInfo = true;
            _connectionString = builder.ToString();
            var sqliteConnectionInitializer = new DropCreateDatabaseIfModelChanges<AptEntities>();
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public AptEntities()
            : base(_connectionString)
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.
        
        public virtual DbSet<AptScanResult> AptScanResults { get; set; }
        public virtual DbSet<AptScanOperationResult> AptScanOperaionResults { get; set; }
        public virtual DbSet<Entities.Module> Modules { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}