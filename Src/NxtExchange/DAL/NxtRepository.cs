using System.Threading.Tasks;

namespace NxtExchange.DAL
{
    public interface INxtRepository
    {
        Task AddBlock(Block block);
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
    }
}
