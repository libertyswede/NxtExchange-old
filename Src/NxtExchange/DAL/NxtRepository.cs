using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace NxtExchange.DAL
{
    public interface INxtRepository
    {
        Task AddBlockIncludeTransactions(Block block);
        Task<Block> GetLastBlock();
        Task<Block> GetBlockwithHeight(int height);
        Task<List<Account>>  GetAccountsWithNxtId(IEnumerable<long> nxtAccountIds);
        Task<List<InboundTransaction>> GetUnconfirmedTransactions();
        Task RemoveBlockIncludingTransactions(int blockId);
        Task AddTransactions(List<InboundTransaction> transactions);
    }

    public class NxtRepository : INxtRepository
    {
        public async Task AddBlockIncludeTransactions(Block block)
        {
            using (var context = new NxtContext())
            {
                context.Blocks.Add(block);
                foreach (var transaction in block.InboundTransactions)
                {
                    transaction.Block = block;
                    context.InboundTransactions.Add(transaction);
                }
                await context.SaveChangesAsync();
            }
        }

        public async Task AddTransactions(List<InboundTransaction> transactions)
        {
            using (var context = new NxtContext())
            {
                transactions.ForEach(t => context.InboundTransactions.Add(t));
                await context.SaveChangesAsync();
            }
        }

        public async Task<Block> GetLastBlock()
        {
            using (var context = new NxtContext())
            {
                return await context.Blocks.OrderByDescending(b => b.Timestamp).FirstAsync();
            }
        }

        public async Task<Block> GetBlockwithHeight(int height)
        {
            using (var context = new NxtContext())
            {
                return await context.Blocks.SingleOrDefaultAsync(b => b.Height == height);
            }
        }

        public async Task<List<Account>> GetAccountsWithNxtId(IEnumerable<long> nxtAccountIds)
        {
            using (var context = new NxtContext())
            {
                return await context.Accounts.Where(a => nxtAccountIds.Contains(a.NxtAccountId)).ToListAsync();
            }
        }

        public async Task<List<InboundTransaction>> GetUnconfirmedTransactions()
        {
            using (var context = new NxtContext())
            {
                return await context.InboundTransactions.Where(t => t.BlockId == null).ToListAsync();
            }
        }

        public async Task RemoveBlockIncludingTransactions(int blockId)
        {
            using (var context = new NxtContext())
            {
                var transactions = await context.InboundTransactions.Where(t => t.BlockId == blockId).ToListAsync();
                transactions.ForEach(t => context.InboundTransactions.Remove(t));

                var block = new Block {Id = blockId};
                context.Blocks.Attach(block);
                context.Blocks.Remove(block);
                
                await context.SaveChangesAsync();
            }
        }
    }
}
