namespace NxtExchange.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initialcreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Blocks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NxtBlockId = c.Long(nullable: false),
                        Height = c.Int(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.NxtBlockId, unique: true, name: "UQ_NxtBlockId");
            
            CreateTable(
                "dbo.InboundTransactions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NxtTransactionId = c.Long(nullable: false),
                        Timestamp = c.DateTime(nullable: false),
                        AmountNqt = c.Long(nullable: false),
                        Status = c.Int(nullable: false),
                        BlockId = c.Int(nullable: false),
                        NxtRecipientId = c.Long(nullable: false),
                        NxtSenderId = c.Long(nullable: false),
                        RecipientAccountId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blocks", t => t.BlockId, cascadeDelete: true)
                .ForeignKey("dbo.Accounts", t => t.RecipientAccountId)
                .Index(t => t.NxtTransactionId, unique: true, name: "UQ_NxtTransactionId")
                .Index(t => t.BlockId)
                .Index(t => t.RecipientAccountId);
            
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NxtAccountId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.NxtAccountId, unique: true, name: "UQ_NxtAccountId");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.InboundTransactions", "RecipientAccountId", "dbo.Accounts");
            DropForeignKey("dbo.InboundTransactions", "BlockId", "dbo.Blocks");
            DropIndex("dbo.Accounts", "UQ_NxtAccountId");
            DropIndex("dbo.InboundTransactions", new[] { "RecipientAccountId" });
            DropIndex("dbo.InboundTransactions", new[] { "BlockId" });
            DropIndex("dbo.InboundTransactions", "UQ_NxtTransactionId");
            DropIndex("dbo.Blocks", "UQ_NxtBlockId");
            DropTable("dbo.Accounts");
            DropTable("dbo.InboundTransactions");
            DropTable("dbo.Blocks");
        }
    }
}
