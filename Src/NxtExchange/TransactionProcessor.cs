using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NxtExchange.DAL;

namespace NxtExchange
{
    public interface ITransactionProcessor
    {
        Task<ICollection<InboundTransaction>> ProcessTransactions(ICollection<InboundTransaction> transactions);
    }

    public class TransactionProcessor : ITransactionProcessor
    {
        private readonly INxtRepository _repository;
        public TransactionProcessor(INxtRepository repository)
        {
            _repository = repository;
        }

        public async Task<ICollection<InboundTransaction>> ProcessTransactions(ICollection<InboundTransaction> transactions)
        {
            var filteredTransactions = new List<InboundTransaction>();
            var accounts = await _repository.GetAccountsWithNxtId(transactions.Select(t => t.NxtRecipientId));
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
