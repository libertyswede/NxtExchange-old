using System.Threading.Tasks;
using NxtExchange.DAL;

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
            await _nxtConnector.GetNextBlock(_lastKnownBlock.GetBlockId());
        }
    }
}
