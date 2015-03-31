using System.Linq;
using System.Threading.Tasks;
using NxtExchange.DAL;

namespace NxtExchange
{
    public interface IBlockProcessor
    {
        Task<Block> ProcessBlock(Block block);
    }

    public class BlockProcessor : IBlockProcessor
    {
        private readonly INxtRepository _repository;
        private readonly ITransactionProcessor _transactionProcessor;

        public BlockProcessor(INxtRepository repository, ITransactionProcessor transactionProcessor)
        {
            _repository = repository;
            _transactionProcessor = transactionProcessor;
        }

        public async Task<Block> ProcessBlock(Block block)
        {
            block.InboundTransactions = await _transactionProcessor.ProcessTransactions(block.InboundTransactions.ToList());
            await _repository.AddBlockIncludeTransactions(block);
            return block;
        }
    }
}
