namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addCountLogin : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "FailedLoginAttempts", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "FailedLoginAttempts");
        }
    }
}
