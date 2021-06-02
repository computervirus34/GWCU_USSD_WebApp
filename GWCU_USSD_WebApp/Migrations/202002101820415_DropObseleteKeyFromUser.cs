namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DropObseleteKeyFromUser : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Users", "UserDetailID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "UserDetailID", c => c.Int(nullable: false));
        }
    }
}
