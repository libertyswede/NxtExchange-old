using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace NxtExchange.DAL
{
    public interface INxtRepository
    {
        Block GetLastBlock();
        Block GetBlockOnHeight(int height);
        List<Account> GetAccountsWithNxtId(IEnumerable<long> nxtAccountIds);
        List<InboundTransaction> GetUnconfirmedTransactions();
        void AddBlockIncludeTransactions(Block block);
        void AddTransactions(List<InboundTransaction> transactions);
        void RemoveBlockIncludingTransactions(int blockId);
        List<InboundTransaction> GetTransactionsByNxtId(IEnumerable<long> transactionIds);
    }

    public class NxtRepository : INxtRepository
    {
        public Block GetLastBlock()
        {
            using (var context = new NxtContext())
            {
                return context.Blocks
                    .Include(b => b.InboundTransactions)
                    .OrderByDescending(b => b.Height)
                    .First();
            }
        }

        public Block GetBlockOnHeight(int height)
        {
            using (var context = new NxtContext())
            {
                return context.Blocks
                    .Include(b => b.InboundTransactions)
                    .SingleOrDefault(b => b.Height == height);
            }
        }

        public List<Account> GetAccountsWithNxtId(IEnumerable<long> nxtAccountIds)
        {
            using (var context = new NxtContext())
            {
                return context.Accounts.Where(a => nxtAccountIds.Contains(a.NxtAccountId)).ToList();
            }
        }

        public List<InboundTransaction> GetUnconfirmedTransactions()
        {
            using (var context = new NxtContext())
            {
                return context.InboundTransactions.Where(t => t.BlockId == null).ToList();
            }
        }

        public void AddBlockIncludeTransactions(Block block)
        {
            using (var context = new NxtContext())
            {
                context.Blocks.Add(block);
                foreach (var transaction in block.InboundTransactions)
                {
                    transaction.Block = block;
                    context.InboundTransactions.Add(transaction);
                }
                context.SaveChanges();
            }
        }

        public void AddTransactions(List<InboundTransaction> transactions)
        {
            using (var context = new NxtContext())
            {
                transactions.ForEach(t => context.InboundTransactions.Add(t));
                context.SaveChanges();
            }
        }

        public void RemoveBlockIncludingTransactions(int blockId)
        {
            using (var context = new NxtContext())
            {
                var transactions = context.InboundTransactions.Where(t => t.BlockId == blockId).ToList();
                transactions.ForEach(t => context.InboundTransactions.Remove(t));

                var block = new Block {Id = blockId};
                context.Blocks.Attach(block);
                context.Blocks.Remove(block);
                
                context.SaveChanges();
            }
        }

        public List<InboundTransaction> GetTransactionsByNxtId(IEnumerable<long> transactionIds)
        {
            using (var context = new NxtContext())
            {
                return context.InboundTransactions.Where(t => transactionIds.Contains(t.NxtTransactionId)).ToList();
            }
        }
    }
}
