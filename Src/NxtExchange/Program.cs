using System;
using System.Threading;
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
            var blockProcessor = new BlockProcessor(transactionProcessor);
            var connector = new NxtConnector(new ServiceFactory(NxtUri), blockProcessor, transactionProcessor);
            var controller = new NxtController(repository, connector, transactionProcessor);

            var cts = new CancellationTokenSource();
            Task controllerTask = new Task(controller.Start(cts.Token));

            Task.Factory.StartNew(() => controller.Start(cts.Token), TaskCreationOptions.LongRunning);

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
            Console.WriteLine("Shutting down executing NxtController task... ");
            cts.Cancel();
        }
    }
}
