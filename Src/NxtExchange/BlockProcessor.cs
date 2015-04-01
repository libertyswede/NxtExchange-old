using System.Linq;
using NxtExchange.DAL;

namespace NxtExchange
{
    public interface IBlockProcessor
    {
        Block ConvertBlockAndTransactions(NxtLib.Block<NxtLib.Transaction> nxtBlock);
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

        public Block ConvertBlockAndTransactions(NxtLib.Block<NxtLib.Transaction> nxtBlock)
        {
            var transactions = _transactionProcessor.ConvertToInboundTransactions(nxtBlock.Transactions);
            transactions = _transactionProcessor.FilterTransactionsBasedOnKnownAccounts(transactions);

            var block = new Block
            {
                Height = nxtBlock.Height,
                Timestamp = nxtBlock.Timestamp,
                InboundTransactions = transactions,
                NxtBlockId = nxtBlock.BlockId.ToSigned()
            };
            block.InboundTransactions.ToList().ForEach(t => t.Block = block);
            return block;
        }
    }
}
