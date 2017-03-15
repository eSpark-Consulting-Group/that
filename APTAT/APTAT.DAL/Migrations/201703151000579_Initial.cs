namespace APTAT.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AptScanOperationResults",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OperationUid = c.Guid(nullable: false),
                        Started = c.DateTime(nullable: false),
                        Finished = c.DateTime(),
                        SourceDescription = c.String(maxLength: 4000),
                        Source = c.Int(),
                        ScanMethod = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.OperationUid, unique: true);
            
            CreateTable(
                "dbo.AptScanResults",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        TargetUid = c.Guid(nullable: false),
                        OperationResultId = c.Int(nullable: false),
                        IpAddress = c.String(maxLength: 20),
                        ComputerName = c.String(nullable: false, maxLength: 1000),
                        OsName = c.String(maxLength: 50),
                        Succeeded = c.Boolean(nullable: false),
                        Canceled = c.Boolean(nullable: false),
                        StatusMessage = c.String(maxLength: 100),
                        OrganizationUnit = c.String(maxLength: 4000),
                        LdapPath = c.String(maxLength: 4000),
                        IsLocal = c.Boolean(nullable: false),
                        IsOnline = c.Boolean(nullable: false),
                        TotalCount = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AptScanOperationResults", t => t.OperationResultId, cascadeDelete: true)
                .Index(t => t.TargetUid)
                .Index(t => t.OperationResultId);
            
            CreateTable(
                "dbo.Modules",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Sha1 = c.String(nullable: false, maxLength: 40),
                        Name = c.String(maxLength: 4000),
                        CloudVerdict = c.Int(nullable: false),
                        VerdictSource = c.Int(nullable: false),
                        Path = c.String(nullable: false, maxLength: 4000),
                        AptScanResultId = c.Long(nullable: false),
                        CloudDate = c.DateTime(),
                        Size = c.Long(nullable: false),
                        Attributes = c.String(maxLength: 6),
                        Created = c.DateTime(),
                        Modified = c.DateTime(),
                        Accessed = c.DateTime(),
                        Signer = c.String(maxLength: 4000),
                        Description = c.String(maxLength: 4000),
                        Version = c.String(maxLength: 300),
                        Copyright = c.String(maxLength: 4000),
                        Product = c.String(maxLength: 4000),
                        FileAttributes = c.Int(nullable: false),
                        FileSizeFormatted = c.String(maxLength: 4000),
                        FileAge = c.String(maxLength: 20),
                        OperationResultId = c.Int(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                        ValkyrieDetails_ValkyrieScanInfoId = c.Int(),
                        ValkyrieDetails_Sha1 = c.String(maxLength: 40),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AptScanResults", t => t.AptScanResultId, cascadeDelete: true)
                .ForeignKey("dbo.ValkyrieFileInfoes", t => new { t.ValkyrieDetails_ValkyrieScanInfoId, t.ValkyrieDetails_Sha1 })
                .Index(t => t.Sha1)
                .Index(t => t.AptScanResultId)
                .Index(t => t.OperationResultId)
                .Index(t => new { t.ValkyrieDetails_ValkyrieScanInfoId, t.ValkyrieDetails_Sha1 });
            
            CreateTable(
                "dbo.ValkyrieFileInfoes",
                c => new
                    {
                        ValkyrieScanInfoId = c.Int(nullable: false),
                        Sha1 = c.String(nullable: false, maxLength: 40),
                        MalwareProbability = c.Single(),
                        OperationStatus = c.String(maxLength: 4000),
                        SkipReason = c.String(maxLength: 4000),
                        Verdict = c.Int(nullable: false),
                        IsWhiteListed = c.Boolean(nullable: false),
                        IsSubmitted = c.Boolean(nullable: false),
                        AnalyzeId = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => new { t.ValkyrieScanInfoId, t.Sha1 })
                .ForeignKey("dbo.ValkyrieScanInfoes", t => t.ValkyrieScanInfoId, cascadeDelete: true)
                .Index(t => t.ValkyrieScanInfoId);
            
            CreateTable(
                "dbo.ValkyrieScanInfoes",
                c => new
                    {
                        AptScanOperationResultId = c.Int(nullable: false),
                        ApiKey = c.String(maxLength: 100),
                        ScanId = c.String(maxLength: 100),
                        Started = c.DateTime(nullable: false),
                        Finished = c.DateTime(),
                        IsCancelled = c.Boolean(),
                        FinishedWithoutErrorHandling = c.DateTime(),
                        ErrorMessage = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.AptScanOperationResultId)
                .ForeignKey("dbo.AptScanOperationResults", t => t.AptScanOperationResultId)
                .Index(t => t.AptScanOperationResultId);
            
            CreateTable(
                "dbo.Emails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Address = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AptScanResults", "OperationResultId", "dbo.AptScanOperationResults");
            DropForeignKey("dbo.Modules", new[] { "ValkyrieDetails_ValkyrieScanInfoId", "ValkyrieDetails_Sha1" }, "dbo.ValkyrieFileInfoes");
            DropForeignKey("dbo.ValkyrieFileInfoes", "ValkyrieScanInfoId", "dbo.ValkyrieScanInfoes");
            DropForeignKey("dbo.ValkyrieScanInfoes", "AptScanOperationResultId", "dbo.AptScanOperationResults");
            DropForeignKey("dbo.Modules", "AptScanResultId", "dbo.AptScanResults");
            DropIndex("dbo.ValkyrieScanInfoes", new[] { "AptScanOperationResultId" });
            DropIndex("dbo.ValkyrieFileInfoes", new[] { "ValkyrieScanInfoId" });
            DropIndex("dbo.Modules", new[] { "ValkyrieDetails_ValkyrieScanInfoId", "ValkyrieDetails_Sha1" });
            DropIndex("dbo.Modules", new[] { "OperationResultId" });
            DropIndex("dbo.Modules", new[] { "AptScanResultId" });
            DropIndex("dbo.Modules", new[] { "Sha1" });
            DropIndex("dbo.AptScanResults", new[] { "OperationResultId" });
            DropIndex("dbo.AptScanResults", new[] { "TargetUid" });
            DropIndex("dbo.AptScanOperationResults", new[] { "OperationUid" });
            DropTable("dbo.Emails");
            DropTable("dbo.ValkyrieScanInfoes");
            DropTable("dbo.ValkyrieFileInfoes");
            DropTable("dbo.Modules");
            DropTable("dbo.AptScanResults");
            DropTable("dbo.AptScanOperationResults");
        }
    }
}
