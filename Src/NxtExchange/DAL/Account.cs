using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace NxtExchange.DAL
{
    public class Account
    {
        public int Id { get; set; }

        [Index("UQ_NxtAccountId", IsUnique = true)]
        public long NxtAccountId { get; set; }
        public Collection<InboundTransaction> Transactions { get; set; }
    }
}
