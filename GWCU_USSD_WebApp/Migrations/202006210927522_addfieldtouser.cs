namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addfieldtouser : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "Institution", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "Institution");
        }
    }
}
