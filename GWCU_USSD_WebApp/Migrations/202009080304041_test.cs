namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class test : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.UserTransactions", "IsLoanTopUp", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.UserTransactions", "IsLoanTopUp", c => c.Boolean(nullable: false));
        }
    }
}
