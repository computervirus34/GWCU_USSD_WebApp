namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addfieldtouse122 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Users", "Email");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Users", "Email", c => c.String());
        }
    }
}
