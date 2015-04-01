using System.Collections.Generic;
using NxtExchange.DAL;
using NxtLib;
using NxtLib.Accounts;
using NxtLib.Blocks;

namespace NxtExchange
{
    public interface INxtConnector
    {
        Block GetNextBlock(ulong blockId);
        List<InboundTransaction> GetNewUnconfirmedTransactions();
    }

    public class NxtConnector : INxtConnector
    {
        private readonly IBlockProcessor _blockProcessor;
        private readonly ITransactionProcessor _transactionProcessor;
        private readonly IBlockService _blockService;
        private readonly IAccountService _accountService;

        public NxtConnector(IServiceFactory serviceFactory, IBlockProcessor blockProcessor, ITransactionProcessor transactionProcessor)
        {
            _blockProcessor = blockProcessor;
            _transactionProcessor = transactionProcessor;
            _blockService = serviceFactory.CreateBlockService();
            _accountService = serviceFactory.CreateAccountService();
        }

        public Block GetNextBlock(ulong blockId)
        {
            try
            {
                Block block = null;
                var getBlockResult = _blockService.GetBlock(BlockLocator.ByBlockId(blockId)).Result;
                if (getBlockResult.NextBlock.HasValue)
                {
                    var nextBlockResult = _blockService.GetBlockIncludeTransactions(BlockLocator.ByBlockId(getBlockResult.NextBlock.Value)).Result;
                    block = _blockProcessor.ConvertBlockAndTransactions(nextBlockResult);
                }
                return block;
            }
            catch (NxtException e)
            {
                if (e.ErrorCode == 4)
                {
                    throw new BlockDoesNotExistException(blockId);
                }
                throw;
            }
        }

        public List<InboundTransaction> GetNewUnconfirmedTransactions()
        {
            var unconfirmedTransactions = _accountService.GetUnconfirmedTransactions().Result;
            var inboundTransactions = _transactionProcessor.ConvertToInboundTransactions(unconfirmedTransactions.UnconfirmedTransactions);
            inboundTransactions = _transactionProcessor.RemoveUnknownRecipients(inboundTransactions);
            return _transactionProcessor.RemoveKnownTransactions(inboundTransactions);
        }
    }
}
