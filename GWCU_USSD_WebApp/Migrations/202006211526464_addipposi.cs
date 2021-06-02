namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addipposi : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "ipposi", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "ipposi");
        }
    }
}
