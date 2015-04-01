using System;
using System.Threading;
using System.Threading.Tasks;
using NxtExchange.DAL;

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

            while (currentBlock != null && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    currentBlock = TryAddNextBlock(currentBlock);
                }
                catch (BlockDoesNotExistException)
                {
                    currentBlock = Rollback(currentBlock);
                }
                _lastKnownBlock = currentBlock;
            }
        }

        private Block Rollback(Block currentBlock)
        {
            _repository.RemoveBlockIncludingTransactions(currentBlock.Id);
            var previousBlock = _repository.GetBlockOnHeight(currentBlock.Height - 1);
            return previousBlock;
        }

        private Block TryAddNextBlock(Block currentBlock)
        {
            var nextBlock = _nxtConnector.GetNextBlock(currentBlock.NxtBlockId.ToUnsigned());
            if (nextBlock != null)
            {
                _repository.AddBlockIncludeTransactions(nextBlock);
            }
            return nextBlock;
        }

        private void CheckForNewTransactions(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                ScanBlockchain(cancellationToken);
                var transactions = _nxtConnector.GetNewUnconfirmedTransactions();
                _repository.AddTransactions(transactions);

                Task.Delay(new TimeSpan(0, 0, 10), cancellationToken).Wait(cancellationToken);
            }
        }
    }
}
