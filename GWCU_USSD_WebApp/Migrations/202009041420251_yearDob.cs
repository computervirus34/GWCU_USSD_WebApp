namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class yearDob : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "yearDob", c => c.Int(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "yearDob");
        }
    }
}
