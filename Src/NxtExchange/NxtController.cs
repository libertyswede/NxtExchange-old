using System;
using System.Collections.Generic;
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
        private readonly IBlockProcessor _blockProcessor;
        private readonly List<InboundTransaction> _unconfirmedTransactions = new List<InboundTransaction>();
        private Block _lastKnownBlock;

        public NxtController(INxtRepository repository, INxtConnector nxtConnector, IBlockProcessor blockProcessor)
        {
            _repository = repository;
            _nxtConnector = nxtConnector;
            _blockProcessor = blockProcessor;
        }

        public async Task Start()
        {
            await Init();
            await TraverseToLatestBlock(_lastKnownBlock);
            await CheckForNewTransactions();
        }

        public async Task Init()
        {
            _lastKnownBlock = await _repository.GetLastBlock();
        }

        private async Task CheckForNewTransactions()
        {
            while (true)
            {
                await TraverseToLatestBlock(_lastKnownBlock);
                var transactions = await _nxtConnector.GetUnconfirmedTransactions();
                var newTransactions = transactions.Except(_unconfirmedTransactions).ToList();
                var accounts = await _repository.GetAccountsWithNxtId(newTransactions.Select(t => t.NxtRecipientId));
                foreach (var account in accounts)
                {
                    
                }
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
                    _unconfirmedTransactions.Clear();
                }
                else
                {
                    currentBlock = nextBlock;
                    if (currentBlock != null)
                    {
                        await _blockProcessor.ProcessBlock(currentBlock);
                        _unconfirmedTransactions.Clear();
                    }
                }
            } while (currentBlock != null);
        }
    }
}
