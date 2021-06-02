namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class dob : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "dob", c => c.DateTime(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "dob");
        }
    }
}
