using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace NxtExchange.DAL
{
    public class NxtContext : DbContext
    {
        public DbSet<Block> Blocks { get; set; }
        public DbSet<InboundTransaction> InboundTransactions { get; set; }

        public NxtContext()
        {
            EnsureDateTimeKindIsUtc();
        }

        private void EnsureDateTimeKindIsUtc()
        {
            ((IObjectContextAdapter) this).ObjectContext.ObjectMaterialized +=
                (sender, e) => DateTimeKindAttribute.Apply(e.Entity);
        }
    }
}
