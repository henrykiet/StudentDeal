namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddReviewTable1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_Review", "User_Id", c => c.String(maxLength: 128));
            CreateIndex("dbo.tb_Review", "User_Id");
            AddForeignKey("dbo.tb_Review", "User_Id", "dbo.AspNetUsers", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.tb_Review", "User_Id", "dbo.AspNetUsers");
            DropIndex("dbo.tb_Review", new[] { "User_Id" });
            DropColumn("dbo.tb_Review", "User_Id");
        }
    }
}
