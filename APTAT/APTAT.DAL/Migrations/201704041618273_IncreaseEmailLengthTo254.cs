namespace APTAT.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncreaseEmailLengthTo254 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Emails", "Address", c => c.String(maxLength: 254));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Emails", "Address", c => c.String(maxLength: 128));
        }
    }
}
