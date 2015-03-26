using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace NxtExchange.DAL
{
    public interface INxtRepository
    {
        Task AddBlock(Block block);
        Task<Block> GetLastBlock();
        Task<Block> GetBlockOnHeight(int height);
        Task<List<Account>>  GetAccountsWithNxtId(IEnumerable<long> nxtAccountIds);
        Task AddTransaction(InboundTransaction transaction);
    }

    public class NxtRepository : INxtRepository
    {
        public async Task AddBlock(Block block)
        {
            using (var context = new NxtContext())
            {
                context.Blocks.Add(block);
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

        public async Task<Block> GetBlockOnHeight(int height)
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

        public Task AddTransaction(InboundTransaction transaction)
        {
            
        }
    }
}
