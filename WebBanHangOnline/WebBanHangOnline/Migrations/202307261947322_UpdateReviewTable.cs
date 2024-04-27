namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateReviewTable : DbMigration
    {
        public override void Up()
        {
            RenameColumn(table: "dbo.tb_Review", name: "User_Id", newName: "UserId");
            RenameIndex(table: "dbo.tb_Review", name: "IX_User_Id", newName: "IX_UserId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.tb_Review", name: "IX_UserId", newName: "IX_User_Id");
            RenameColumn(table: "dbo.tb_Review", name: "UserId", newName: "User_Id");
        }
    }
}
