using System;
using System.Linq;
using System.Threading.Tasks;
using NxtExchange.DAL;
using NxtLib;

namespace NxtExchange
{
    public class NxtController
    {
        private readonly INxtRepository _repository;
        private readonly INxtConnector _nxtConnector;
        private readonly ITransactionProcessor _transactionProcessor;
        private readonly IBlockProcessor _blockProcessor;
        private Block _lastKnownBlock;

        public NxtController(INxtRepository repository, INxtConnector nxtConnector, ITransactionProcessor transactionProcessor, IBlockProcessor blockProcessor)
        {
            _repository = repository;
            _nxtConnector = nxtConnector;
            _transactionProcessor = transactionProcessor;
            _blockProcessor = blockProcessor;
        }

        private async Task Init()
        {
            _lastKnownBlock = await _repository.GetLastBlock();
        }

        public async Task Start()
        {
            await Init();
            await TraverseToLatestBlock(_lastKnownBlock);
            await CheckForNewTransactions();
        }

        private async Task CheckForNewTransactions()
        {
            while (true)
            {
                await TraverseToLatestBlock(_lastKnownBlock);
                var transactions = await _nxtConnector.GetUnconfirmedTransactions();
                var existingTransactions = await _repository.GetUnconfirmedTransactions();
                var newTransactions = transactions.Where(t => existingTransactions.All(et => t.NxtTransactionId != et.NxtTransactionId)).ToList();
                newTransactions = await _transactionProcessor.ProcessTransactions(newTransactions);
                await _repository.AddTransactions(newTransactions);
                await Task.Delay(new TimeSpan(0, 0, 10));
            }
        }

        private async Task TraverseToLatestBlock(Block currentBlock)
        {
            var currentBlockExistsInBlockchain = true;
            Block nextBlock = null;
            
            do
            {
                try
                {
                    nextBlock = await _nxtConnector.GetNextBlock(currentBlock.NxtBlockId.ToUnsigned());
                }
                catch (NxtException e)
                {
                    if (e.ErrorCode == 4)
                    {
                        currentBlockExistsInBlockchain = false;
                    }
                    else
                    {
                        throw;
                    }
                }
                if (!currentBlockExistsInBlockchain)
                {
                    await _repository.RemoveBlockIncludingTransactions(currentBlock.Id);
                    currentBlock = await _repository.GetBlockwithHeight(currentBlock.Height - 1);
                    currentBlockExistsInBlockchain = true;
                }
                else
                {
                    currentBlock = nextBlock;
                    if (currentBlock != null)
                    {
                        await _blockProcessor.ProcessBlock(currentBlock);
                    }
                }
            } while (currentBlock != null);
        }
    }
}
