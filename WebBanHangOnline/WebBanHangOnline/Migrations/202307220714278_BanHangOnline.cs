namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BanHangOnline : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.tb_Order", "Phone", c => c.String());
            DropColumn("dbo.AspNetUsers", "WalletPoint");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "WalletPoint", c => c.Single(nullable: false));
            AlterColumn("dbo.tb_Order", "Phone", c => c.String(nullable: false));
        }
    }
}
