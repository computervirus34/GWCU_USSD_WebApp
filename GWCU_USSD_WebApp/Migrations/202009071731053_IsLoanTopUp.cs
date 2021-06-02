namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsLoanTopUp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserTransactions", "IsLoanTopUp", c => c.Boolean(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserTransactions", "IsLoanTopUp");
        }
    }
}
