namespace GWCU_USSD_WebApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                {
                    UserID = c.Int(nullable: false, identity: true),
                    UserDetailID = c.Int(nullable: false),
                    Username = c.String(),
                    PasswordHash = c.String(),
                    PhoneNumber = c.String(),
                    FirstName = c.String(),
                    LastName = c.String(),
                    IsAccountEnabled = c.Boolean(nullable: false),
                    CreditLimit = c.Single(nullable: false),
                    CreditUtilized = c.Single(nullable: false),
                    CreditAvailable = c.Single(nullable: false),
                    InterestRates = c.String(),
                    PaybackPeriods = c.String()
                })
                .PrimaryKey(t => t.UserID);

            CreateTable(
                "dbo.UserBanks",
                c => new
                {
                    UserBankID = c.Int(nullable: false, identity: true),
                    UserID = c.Int(nullable: false),
                    BankName = c.String(),
                    AccountNumber = c.String(),
                })
                .PrimaryKey(t => t.UserBankID)
                .ForeignKey("dbo.Users", t => t.UserID, cascadeDelete: true)
                .Index(t => t.UserID);

            CreateTable(
                "dbo.UserRequests",
                c => new
                {
                    UserRequestID = c.Int(nullable: false, identity: true),
                    UserBankID = c.Int(nullable: false),
                    DateTime = c.DateTime(nullable: false),
                    Amount = c.Single(nullable: false),
                    InterestRate = c.Single(nullable: false),
                    PaybackPeriod = c.Single(nullable: false),
                    User_UserID = c.Int(),
                })
                .PrimaryKey(t => t.UserRequestID)
                .ForeignKey("dbo.UserBanks", t => t.UserBankID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_UserID)
                .Index(t => t.UserBankID)
                .Index(t => t.User_UserID);

            CreateTable(
                "dbo.UserTransactions",
                c => new
                {
                    UserTransactionID = c.Int(nullable: false, identity: true),
                    DateTime = c.DateTime(nullable: false),
                    ReferenceNumber = c.String(),
                    Debit = c.Single(nullable: false),
                    Credit = c.Single(nullable: false),
                    Balance = c.Single(nullable: false),
                    AmountToBePaid = c.Single(nullable: false),
                    AmountDisbursed = c.Single(nullable: false),
                    AmountEarned = c.Single(nullable: false),
                    UserRequestID = c.Int(nullable: false),
                    User_UserID = c.Int(),
                })
                .PrimaryKey(t => t.UserTransactionID)
                .ForeignKey("dbo.UserRequests", t => t.UserRequestID, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_UserID)
                .Index(t => t.UserRequestID)
                .Index(t => t.User_UserID);

            CreateTable(
                "dbo.AspNetRoles",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Name = c.String(nullable: false, maxLength: 256),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");

            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                {
                    UserId = c.String(nullable: false, maxLength: 128),
                    RoleId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);

            CreateTable(
                "dbo.AspNetUsers",
                c => new
                {
                    Id = c.String(nullable: false, maxLength: 128),
                    Email = c.String(maxLength: 256),
                    EmailConfirmed = c.Boolean(nullable: false),
                    PasswordHash = c.String(),
                    SecurityStamp = c.String(),
                    PhoneNumber = c.String(),
                    PhoneNumberConfirmed = c.Boolean(nullable: false),
                    TwoFactorEnabled = c.Boolean(nullable: false),
                    LockoutEndDateUtc = c.DateTime(),
                    LockoutEnabled = c.Boolean(nullable: false),
                    AccessFailedCount = c.Int(nullable: false),
                    UserName = c.String(nullable: false, maxLength: 256),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");

            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserId = c.String(nullable: false, maxLength: 128),
                    ClaimType = c.String(),
                    ClaimValue = c.String(),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                {
                    LoginProvider = c.String(nullable: false, maxLength: 128),
                    ProviderKey = c.String(nullable: false, maxLength: 128),
                    UserId = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.UserTransactions", "User_UserID", "dbo.Users");
            DropForeignKey("dbo.UserTransactions", "UserRequestID", "dbo.UserRequests");
            DropForeignKey("dbo.UserRequests", "User_UserID", "dbo.Users");
            DropForeignKey("dbo.UserRequests", "UserBankID", "dbo.UserBanks");
            DropForeignKey("dbo.UserBanks", "UserID", "dbo.Users");
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.UserTransactions", new[] { "User_UserID" });
            DropIndex("dbo.UserTransactions", new[] { "UserRequestID" });
            DropIndex("dbo.UserRequests", new[] { "User_UserID" });
            DropIndex("dbo.UserRequests", new[] { "UserBankID" });
            DropIndex("dbo.UserBanks", new[] { "UserID" });
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.UserTransactions");
            DropTable("dbo.UserRequests");
            DropTable("dbo.UserBanks");
            DropTable("dbo.Users");
        }
    }
}
