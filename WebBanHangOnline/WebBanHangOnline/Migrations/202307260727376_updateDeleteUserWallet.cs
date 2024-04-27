namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateDeleteUserWallet : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tb_Wallet", "Id", "dbo.AspNetUsers");
            AddColumn("dbo.tb_Wallet", "ApplicationUser_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.tb_Wallet", "ApplicationUser_Id");
            AddForeignKey("dbo.tb_Wallet", "ApplicationUser_Id", "dbo.AspNetUsers", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tb_Wallet", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.tb_Wallet", new[] { "ApplicationUser_Id" });
            DropColumn("dbo.tb_Wallet", "ApplicationUser_Id");
            AddForeignKey("dbo.tb_Wallet", "Id", "dbo.AspNetUsers", "Id");
        }
    }
}
