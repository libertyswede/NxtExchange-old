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

        [Index("UQ_NxtTransactionId", IsUnique = true)]
        public long NxtTransactionId { get; set; }

        [DateTimeKind(DateTimeKind.Utc)]
        public DateTime Timestamp { get; set; }
        public long AmountNqt { get; set; }
        public TransactionStatus Status { get; set; }
        public int? BlockId { get; set; }
        public virtual Block Block { get; set; }
        public long NxtRecipientId { get; set; }
        public long NxtSenderId { get; set; }
        public int? RecipientAccountId { get; set; }
        public Account RecipientAccount { get; set; }
    }
}
