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
            var blockId = currentBlock.GetBlockId();
            
            do 
            {
                try
                {
                    var nextBlock = await _nxtConnector.GetNextBlock(blockId);
                    if (nextBlock != null)
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
                    await Rollback(_lastKnownBlock.Height - 1);
                }
            } while (blockExists && !hasNext);
        }

        private async Task Rollback(int height)
        {
            var block = await _repository.GetBlockOnHeight(height);
        }
    }
}
