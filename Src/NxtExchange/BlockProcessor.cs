using System.Linq;
using NxtExchange.DAL;

namespace NxtExchange
{
    public interface IBlockProcessor
    {
        Block ProcessBlock(Block block);
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

        public Block ProcessBlock(Block block)
        {
            block.InboundTransactions = _transactionProcessor.ProcessTransactions(block.InboundTransactions.ToList());
            _repository.AddBlockIncludeTransactions(block);
            return block;
        }
    }
}
