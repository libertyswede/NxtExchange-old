using System;
using System.Linq;
using System.Threading;
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
        private Block _lastKnownBlock;

        public NxtController(INxtRepository repository, INxtConnector nxtConnector, ITransactionProcessor transactionProcessor)
        {
            _repository = repository;
            _nxtConnector = nxtConnector;
            _transactionProcessor = transactionProcessor;
        }

        public void Start(CancellationToken cancellationToken)
        {
            try
            {
                Init();
                throw new Exception("test");
                ScanBlockchain(cancellationToken);
                CheckForNewTransactions(cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // cancellationToken was cancelled, ignore this exception
            }
        }

        private void Init()
        {
            _lastKnownBlock = _repository.GetLastBlock();
        }

        private void ScanBlockchain(CancellationToken cancellationToken)
        {
            var currentBlock = _lastKnownBlock;
            var currentBlockExistsInBlockchain = true;
            Block nextBlock = null;
            
            do
            {
                try
                {
                    nextBlock = _nxtConnector.GetNextBlock(currentBlock.NxtBlockId.ToUnsigned());
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
                    _repository.RemoveBlockIncludingTransactions(currentBlock.Id);
                    currentBlock = _repository.GetBlockOnHeight(currentBlock.Height - 1);
                    currentBlockExistsInBlockchain = true;
                    _lastKnownBlock = currentBlock;
                }
                else
                {
                    currentBlock = nextBlock;
                    if (currentBlock != null)
                    {
                        _repository.AddBlockIncludeTransactions(currentBlock);
                        _lastKnownBlock = currentBlock;
                    }
                }
            } while (currentBlock != null && !cancellationToken.IsCancellationRequested);
        }

        private void CheckForNewTransactions(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ScanBlockchain(cancellationToken);
                var transactions = _nxtConnector.GetUnconfirmedTransactions();
                var existingTransactions = _repository.GetUnconfirmedTransactions();
                var newTransactions = transactions.Where(t => existingTransactions.All(et => t.NxtTransactionId != et.NxtTransactionId)).ToList();
                newTransactions = _transactionProcessor.FilterTransactionsBasedOnKnownAccounts(newTransactions);
                _repository.AddTransactions(newTransactions);

                Task.Delay(new TimeSpan(0, 0, 10), cancellationToken).Wait(cancellationToken);
            }
        }
    }
}
