using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace NxtExchange.DAL
{
    public class Block
    {
        public int Id { get; set; }

        [Index("UQ_NxtBlockId", IsUnique = true)]
        public long NxtBlockId { get; set; }
        public int Height { get; set; }

        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; }
        public virtual ICollection<InboundTransaction> InboundTransactions { get; set; }
    }
}
