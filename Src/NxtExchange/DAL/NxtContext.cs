using System.Data.Entity;

namespace NxtExchange.DAL
{
    public class NxtContext : DbContext
    {
        public DbSet<Block> Blocks { get; set; }
        public DbSet<InboundTransaction> InboundTransactions { get; set; }
    }
}
