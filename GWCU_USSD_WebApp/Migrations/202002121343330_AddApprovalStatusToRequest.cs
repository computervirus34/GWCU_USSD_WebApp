namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddApprovalStatusToRequest : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserRequests", "IsApproved", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserRequests", "IsApproved");
        }
    }
}
