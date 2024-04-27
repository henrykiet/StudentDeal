namespace WebBanHangOnline.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class updateIsActiveProducts : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.tb_Order", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.tb_Order", "IsActive");
        }
    }
}
