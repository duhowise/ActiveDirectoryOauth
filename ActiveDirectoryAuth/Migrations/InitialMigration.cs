namespace ActiveDirectoryAuth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "ADCONTEXT.DIRECTORYSETUP",
                c => new
                    {
                        ID = c.Decimal(nullable: false, precision: 10, scale: 0, identity: true),
                        DOMAINNAME = c.String(maxLength: 256, unicode: false),
                        IPADDRESS = c.String(maxLength: 256, unicode: false),
                        SERVICEUSERNAME = c.String(maxLength: 256, unicode: false),
                        SERVICEPASSWORD = c.String(maxLength: 256, unicode: false),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("ADCONTEXT.DIRECTORYSETUP");
        }
    }
}
