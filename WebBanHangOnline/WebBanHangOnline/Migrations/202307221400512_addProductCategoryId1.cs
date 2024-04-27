namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addProductCategoryId1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.tb_News", "CategoryId", "dbo.tb_Category");
            DropForeignKey("dbo.tb_Posts", "CategoryId", "dbo.tb_Category");
            DropIndex("dbo.tb_News", new[] { "CategoryId" });
            DropIndex("dbo.tb_Posts", new[] { "CategoryId" });
            AlterColumn("dbo.tb_News", "CategoryId", c => c.Int());
            AlterColumn("dbo.tb_Posts", "CategoryId", c => c.Int());
            CreateIndex("dbo.tb_News", "CategoryId");
            CreateIndex("dbo.tb_Posts", "CategoryId");
            AddForeignKey("dbo.tb_News", "CategoryId", "dbo.tb_Category", "Id");
            AddForeignKey("dbo.tb_Posts", "CategoryId", "dbo.tb_Category", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tb_Posts", "CategoryId", "dbo.tb_Category");
            DropForeignKey("dbo.tb_News", "CategoryId", "dbo.tb_Category");
            DropIndex("dbo.tb_Posts", new[] { "CategoryId" });
            DropIndex("dbo.tb_News", new[] { "CategoryId" });
            AlterColumn("dbo.tb_Posts", "CategoryId", c => c.Int(nullable: false));
            AlterColumn("dbo.tb_News", "CategoryId", c => c.Int(nullable: false));
            CreateIndex("dbo.tb_Posts", "CategoryId");
            CreateIndex("dbo.tb_News", "CategoryId");
            AddForeignKey("dbo.tb_Posts", "CategoryId", "dbo.tb_Category", "Id", cascadeDelete: true);
            AddForeignKey("dbo.tb_News", "CategoryId", "dbo.tb_Category", "Id", cascadeDelete: true);
        }
    }
}
