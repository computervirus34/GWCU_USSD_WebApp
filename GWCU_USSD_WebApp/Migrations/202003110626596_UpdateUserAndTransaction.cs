namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUserAndTransaction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserTransactions", "IsReversed", c => c.Boolean(nullable: false));
            DropColumn("dbo.Users", "CreditAvailable");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "CreditAvailable", c => c.Single(nullable: false));
            DropColumn("dbo.UserTransactions", "IsReversed");
        }
    }
}
