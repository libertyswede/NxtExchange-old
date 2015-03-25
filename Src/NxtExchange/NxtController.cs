using NxtExchange.DAL;

namespace NxtExchange
{
    public class NxtController
    {
        private readonly INxtRepository _repository;
        private readonly INxtConnector _nxtConnector;

        public NxtController(INxtRepository repository, INxtConnector nxtConnector)
        {
            _repository = repository;
            _nxtConnector = nxtConnector;
        }
    }
}
