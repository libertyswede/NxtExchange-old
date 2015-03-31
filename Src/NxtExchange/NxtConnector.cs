using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NxtExchange.DAL;
using NxtLib;
using NxtLib.Accounts;
using NxtLib.Blocks;

namespace NxtExchange
{
    public interface INxtConnector
    {
        Block GetNextBlock(ulong blockId);
        ICollection<InboundTransaction> GetUnconfirmedTransactions();
    }

    public class NxtConnector : INxtConnector
    {
        private readonly IBlockService _blockService;
        private readonly IAccountService _accountService;

        public NxtConnector(IServiceFactory serviceFactory)
        {
            _blockService = serviceFactory.CreateBlockService();
            _accountService = serviceFactory.CreateAccountService();
        }

        public Block GetNextBlock(ulong blockId)
        {
            Block block = null;
            var getBlockResult = _blockService.GetBlock(BlockLocator.BlockId(blockId)).Result;
            if (getBlockResult.NextBlock.HasValue)
            {
                var nextBlockResult = _blockService.GetBlockIncludeTransactions(BlockLocator.BlockId(getBlockResult.NextBlock.Value)).Result;
                block = new Block
                {
                    Height = nextBlockResult.Height,
                    Timestamp = nextBlockResult.Timestamp,
                    InboundTransactions = CreateInboundTransactions(nextBlockResult.Transactions),
                    NxtBlockId = nextBlockResult.BlockId.ToSigned()
                };
                block.InboundTransactions.ToList().ForEach(t => t.Block = block);
            }
            return block;
        }

        public ICollection<InboundTransaction> GetUnconfirmedTransactions()
        {
            var unconfirmedTransactions = _accountService.GetUnconfirmedTransactions().Result;
            return CreateInboundTransactions(unconfirmedTransactions.UnconfirmedTransactions);
        }

        private static ICollection<InboundTransaction> CreateInboundTransactions(IEnumerable<Transaction> transactions)
        {
            var inboundTransactions = new List<InboundTransaction>();
            foreach (var transaction in transactions.Where(t => 
                t.SubType == TransactionSubType.PaymentOrdinaryPayment && 
                t.Amount.Nqt > 0 && 
                t.Recipient.HasValue))
            {
                Debug.Assert(transaction.TransactionId != null, "transaction.TransactionId != null");

                var inboundTransaction = new InboundTransaction
                {
                    AmountNqt = transaction.Amount.Nqt,
                    Timestamp = transaction.Timestamp,
                    NxtTransactionId = transaction.TransactionId.ToSigned(),
                    NxtRecipientId = transaction.Recipient.ToSigned(),
                    NxtSenderId = transaction.Sender.ToSigned(),
                    Status = TransactionStatusCalculator.GetStatus(transaction)
                };

                inboundTransactions.Add(inboundTransaction);
            }
            return inboundTransactions;
        }
    }
}
