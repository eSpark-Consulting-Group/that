using System.Data.SqlServerCe;
using APTAT.DAL.Entities;

namespace APTAT.DAL
{
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;

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
            builder.Password = "f8b3cfdacdf529025cfd940255eea5d1f70ea320";
            builder.DataSource = System.IO.Directory.Exists("data") 
                ? "data\\AptDatabase.sdf" : "AptDatabase.sdf";
            builder.PersistSecurityInfo = true;
            _connectionString = builder.ToString();
            _connectionString = _connectionString + ";File Access Retry Timeout=15;Max Database Size=4091;Default Lock Timeout=5000;Max Buffer Size=6400";
            //var sqliteConnectionInitializer = new DropCreateDatabaseIfModelChanges<AptEntities>();
            //Database.SetInitializer(sqliteConnectionInitializer);
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AptEntities, Migrations.Configuration>());
        }

        public AptEntities()
            : base(_connectionString)
        {
        }

        //protected override void OnModelCreating(DbModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Entity<ValkyrieScanInfo>()
        //        .HasRequired(et => et.AptScanOperationResult)
        //        .WithOptional(eo => eo.ValkyrieScanInfo);
        //}

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.
        
        public virtual DbSet<AptScanResult> AptScanResults { get; set; }
        public virtual DbSet<AptScanOperationResult> AptScanOperaionResults { get; set; }
        public virtual DbSet<Entities.Module> Modules { get; set; }
        public virtual DbSet<ValkyrieScanInfo> ValkyrieScans { get; set; }
        public virtual DbSet<ValkyrieFileInfo> ValkyrieFiles { get; set; }
        public virtual DbSet<Email> Emails { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}