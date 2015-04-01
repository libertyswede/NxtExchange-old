using System;
using System.Diagnostics;
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
            Task.Factory.StartNew(() => controller.Start(cts.Token), TaskCreationOptions.LongRunning)
                .ContinueWith(HandleException, TaskContinuationOptions.OnlyOnFaulted)
                .ContinueWith(TaskFinished, TaskContinuationOptions.NotOnFaulted);

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
            Console.WriteLine("Shutting down executing NxtController task... ");
            cts.Cancel();
        }

        private static void TaskFinished(Task task)
        {
            Environment.Exit(0);
        }

        private static void HandleException(Task task)
        {
            Debug.Assert(task.Exception != null, "task.Exception != null");

            foreach (var exception in task.Exception.InnerExceptions)
            {
                Console.WriteLine("Unhandled exception: " + exception.Message);
            }
        }
    }
}
