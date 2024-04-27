namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTotalQuantityIntoProduct : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_Product", "TotalQuantity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tb_Product", "TotalQuantity");
        }
    }
}
