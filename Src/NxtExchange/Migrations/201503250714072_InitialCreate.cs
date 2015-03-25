namespace NxtExchange.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Blocks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlockId = c.Long(nullable: false),
                        Height = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.BlockId, unique: true, name: "UQ_BlockId");
            
            CreateTable(
                "dbo.InboundTransactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TransactionId = c.Long(nullable: false),
                        AmountNqt = c.Long(nullable: false),
                        Status = c.Int(nullable: false),
                        BlockId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blocks", t => t.BlockId, cascadeDelete: true)
                .Index(t => t.TransactionId, unique: true, name: "UQ_TransactionId")
                .Index(t => t.BlockId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InboundTransactions", "BlockId", "dbo.Blocks");
            DropIndex("dbo.InboundTransactions", new[] { "BlockId" });
            DropIndex("dbo.InboundTransactions", "UQ_TransactionId");
            DropIndex("dbo.Blocks", "UQ_BlockId");
            DropTable("dbo.InboundTransactions");
            DropTable("dbo.Blocks");
        }
    }
}
