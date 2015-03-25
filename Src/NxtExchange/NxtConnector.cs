using System.Threading.Tasks;
using NxtExchange.DAL;
using NxtLib;
using NxtLib.Blocks;

namespace NxtExchange
{
    public interface INxtConnector
    {
        Task<Block> GetNextBlock(ulong blockId);
    }

    public class NxtConnector : INxtConnector
    {
        private readonly IBlockService _blockService;

        public NxtConnector(IServiceFactory serviceFactory)
        {
            _blockService = serviceFactory.CreateBlockService();
        }

        public async Task<Block> GetNextBlock(ulong blockId)
        {
            var getBlockResult = await _blockService.GetBlock(BlockLocator.BlockId(blockId));
            return null;
        }
    }
}
