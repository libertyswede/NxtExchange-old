using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NxtExchange.DAL;
using NxtLib;

namespace NxtExchange
{
    public interface ITransactionProcessor
    {
        List<InboundTransaction> RemoveUnknownRecipients(IList<InboundTransaction> transactions);
        List<InboundTransaction> ConvertToInboundTransactions(IList<Transaction> transactions);
        List<InboundTransaction> RemoveKnownTransactions(List<InboundTransaction> inboundTransactions);
    }

    public class TransactionProcessor : ITransactionProcessor
    {
        private readonly INxtRepository _repository;

        public TransactionProcessor(INxtRepository repository)
        {
            _repository = repository;
        }

        public List<InboundTransaction> ConvertToInboundTransactions(IList<Transaction> transactions)
        {
            var inboundTransactions = new List<InboundTransaction>();
            foreach (var transaction in transactions.Where(t =>
                t.SubType == TransactionSubType.PaymentOrdinaryPayment &&
                t.Amount.Nqt > 0 &&
                t.Recipient.HasValue))
            {
                Debug.Assert(transaction.TransactionId != null, "transaction.TransactionId != null");

                var inboundTransaction = new InboundTransaction
                {
                    AmountNqt = transaction.Amount.Nqt,
                    Timestamp = transaction.Timestamp,
                    NxtTransactionId = transaction.TransactionId.ToSigned(),
                    NxtRecipientId = transaction.Recipient.ToSigned(),
                    NxtSenderId = transaction.Sender.ToSigned(),
                    Status = TransactionStatusCalculator.GetStatus(transaction)
                };

                inboundTransactions.Add(inboundTransaction);
            }
            return inboundTransactions;
        }

        public List<InboundTransaction> RemoveKnownTransactions(List<InboundTransaction> transactions)
        {
            var existingTransactions = _repository.GetTransactionsByNxtId(transactions.Select(t => t.NxtTransactionId));
            return transactions.Where(t => existingTransactions.All(et => et.NxtTransactionId != t.NxtTransactionId)).ToList();
        }

        public List<InboundTransaction> RemoveUnknownRecipients(IList<InboundTransaction> transactions)
        {
            var filteredTransactions = new List<InboundTransaction>();
            var accounts = _repository.GetAccountsWithNxtId(transactions.Select(t => t.NxtRecipientId));
            foreach (var transaction in transactions)
            {
                var account = accounts.SingleOrDefault(a => a.NxtAccountId == transaction.NxtRecipientId);
                if (account != null)
                {
                    transaction.RecipientAccount = account;
                    filteredTransactions.Add(transaction);
                }
            }
            return filteredTransactions;
        }
    }
}
