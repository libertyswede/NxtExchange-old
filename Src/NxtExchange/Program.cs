using System.Threading.Tasks;
using NxtExchange.DAL;
using NxtLib;

namespace NxtExchange
{
    class Program
    {
        private const string NxtUri = "http://localhost:6876/nxt";

        static void Main()
        {
            var repository = new NxtRepository();
            var transactionProcessor = new TransactionProcessor(repository);
            var blockProcessor = new BlockProcessor(repository, transactionProcessor);
            var connector = new NxtConnector(new ServiceFactory(NxtUri));
            var controller = new NxtController(repository, connector, transactionProcessor, blockProcessor);
            Task.WaitAll(controller.Start());
        }
    }
}
