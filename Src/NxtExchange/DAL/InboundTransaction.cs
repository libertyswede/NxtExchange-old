using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace NxtExchange.DAL
{
    public enum TransactionStatus
    {
        Pending,     // 0-10 confirmations
        Confirmed,   // >= 10 confirmations
        Secured,     // > 720 confirmations
        Removed      // TX did exist, but was removed from blockchain after for eg. processing a fork or blockchain reorganization
    }

    public class InboundTransaction
    {
        public int Id { get; set; }

        [Index("UQ_TransactionId", IsUnique = true)]
        public long TransactionId { get; set; }

        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; }
        public long AmountNqt { get; set; }
        public TransactionStatus Status { get; set; }
        public int BlockId { get; set; }
        public virtual Block Block { get; set; }
        public long RecipientId { get; set; }
        public long SenderId { get; set; }
    }
}
