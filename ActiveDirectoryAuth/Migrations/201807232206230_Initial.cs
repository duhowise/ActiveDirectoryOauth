namespace ActiveDirectoryAuth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
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
            
            CreateTable(
                "ADCONTEXT.USERPSK",
                c => new
                    {
                        EMPLOYEEID = c.String(nullable: false, maxLength: 256, unicode: false),
                        PSK = c.String(maxLength: 256, unicode: false),
                    })
                .PrimaryKey(t => t.EMPLOYEEID);
            
        }
        
        public override void Down()
        {
            DropTable("ADCONTEXT.USERPSK");
            DropTable("ADCONTEXT.DIRECTORYSETUP");
        }
    }
}
