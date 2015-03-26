using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            Block block = null;
            var getBlockResult = await _blockService.GetBlock(BlockLocator.BlockId(blockId));
            if (getBlockResult.NextBlock.HasValue)
            {
                var nextBlockResult = await _blockService.GetBlockIncludeTransactions(BlockLocator.BlockId(getBlockResult.NextBlock.Value));
                block = new Block
                {
                    Height = nextBlockResult.Height,
                    Timestamp = nextBlockResult.Timestamp,
                    InboundTransactions = CreateInboundTransactions(nextBlockResult.Transactions),
                    BlockId = nextBlockResult.BlockId.ToSigned()
                };
            }
            return block;
        }

        private ICollection<InboundTransaction> CreateInboundTransactions(List<Transaction> transactions)
        {
            var inboundTransactions = new List<InboundTransaction>();
            foreach (var transaction in transactions.Where(t => t.SubType == TransactionSubType.PaymentOrdinaryPayment && t.Amount.Nqt > 0))
            {
                Debug.Assert(transaction.TransactionId != null, "transaction.TransactionId != null");

                var inboundTransaction = new InboundTransaction
                {
                    AmountNqt = transaction.Amount.Nqt,
                    Timestamp = transaction.Timestamp,
                    TransactionId = transaction.TransactionId.ToSigned(),
                    RecipientId = transaction.Recipient.ToSigned(),
                    SenderId = transaction.Sender.ToSigned(),
                    Status = TransactionStatusCalculator.GetStatus(transaction)
                };

                inboundTransactions.Add(inboundTransaction);
            }
            return inboundTransactions;
        }
    }
}
