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
        private Block _lastKnownBlock;

        public NxtController(INxtRepository repository, INxtConnector nxtConnector)
        {
            _repository = repository;
            _nxtConnector = nxtConnector;
        }

        public async Task Start()
        {
            await Init();
        }

        public async Task Init()
        {
            _lastKnownBlock = await _repository.GetLastBlock();
            await TraverseToLatestBlock(_lastKnownBlock);
        }

        private async Task TraverseToLatestBlock(Block currentBlock)
        {
            var blockExists = true;
            var blockId = currentBlock.NxtBlockId.ToUnsigned();
            
            do 
            {
                try
                {
                    currentBlock = await _nxtConnector.GetNextBlock(blockId);
                    if (currentBlock != null)
                    {
                        await ProcessBlock(currentBlock);
                    }
                }
                catch (NxtException e)
                {
                    if (e.ErrorCode == 4)
                    {
                        blockExists = false;
                    }

                }
                if (!blockExists)
                {
                    currentBlock = await Rollback(_lastKnownBlock.Height - 1);
                    blockExists = true;
                }
            } while (currentBlock != null);
        }

        private async Task ProcessBlock(Block block)
        {
            await _repository.AddBlock(block);
            var accounts = await _repository.GetAccountsWithNxtId(block.InboundTransactions.Select(t => t.NxtRecipientId));
            foreach (var inboundTransaction in block.InboundTransactions)
            {
                var account = accounts.SingleOrDefault(a => a.NxtAccountId == inboundTransaction.NxtRecipientId);
                if (account != null)
                {
                    inboundTransaction.RecipientAccount = account;
                    await _repository.AddTransaction(inboundTransaction);
                }
            }
            throw new System.NotImplementedException();
        }

        private async Task<Block> Rollback(int height)
        {
            var block = await _repository.GetBlockOnHeight(height);
            throw new System.NotImplementedException();
        }
    }
}
