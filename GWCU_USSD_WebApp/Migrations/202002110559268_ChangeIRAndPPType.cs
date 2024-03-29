namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangeIRAndPPType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserRequests", "InterestRate", c => c.String());
            AlterColumn("dbo.UserRequests", "PaybackPeriod", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserRequests", "PaybackPeriod", c => c.Single(nullable: false));
            AlterColumn("dbo.UserRequests", "InterestRate", c => c.Single(nullable: false));
        }
    }
}
