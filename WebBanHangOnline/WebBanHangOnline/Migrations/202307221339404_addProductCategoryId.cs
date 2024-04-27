namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProductCategoryId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_News", "ProductCategoryId", c => c.Int(nullable: false));
            AddColumn("dbo.tb_Posts", "ProductCategoryId", c => c.Int(nullable: false));
            CreateIndex("dbo.tb_News", "ProductCategoryId");
            CreateIndex("dbo.tb_Posts", "ProductCategoryId");
            AddForeignKey("dbo.tb_News", "ProductCategoryId", "dbo.tb_ProductCategory", "Id", cascadeDelete: true);
            AddForeignKey("dbo.tb_Posts", "ProductCategoryId", "dbo.tb_ProductCategory", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tb_Posts", "ProductCategoryId", "dbo.tb_ProductCategory");
            DropForeignKey("dbo.tb_News", "ProductCategoryId", "dbo.tb_ProductCategory");
            DropIndex("dbo.tb_Posts", new[] { "ProductCategoryId" });
            DropIndex("dbo.tb_News", new[] { "ProductCategoryId" });
            DropColumn("dbo.tb_Posts", "ProductCategoryId");
            DropColumn("dbo.tb_News", "ProductCategoryId");
        }
    }
}
