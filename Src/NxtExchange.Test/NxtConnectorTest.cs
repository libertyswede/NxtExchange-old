using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NxtLib;
using NxtLib.Blocks;

namespace NxtExchange.Test
{
    [TestClass]
    public class NxtConnectorTest
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<IBlockService> _blockServiceMock;
        private NxtConnector _nxtConnector;
        private Mock<IBlockProcessor> _blockProcessorMock;
        private Mock<ITransactionProcessor> _transactionProcessorMock;

        [TestInitialize]
        public void TestInit()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>();
            _blockServiceMock = new Mock<IBlockService>();
            _blockProcessorMock = new Mock<IBlockProcessor>();
            _transactionProcessorMock = new Mock<ITransactionProcessor>();
            _serviceFactoryMock.Setup(f => f.CreateBlockService()).Returns(_blockServiceMock.Object);
            _nxtConnector = new NxtConnector(_serviceFactoryMock.Object, _blockProcessorMock.Object, _transactionProcessorMock.Object);
        }

        [TestMethod]
        public void GetNextBlockShouldReturnNextBlock()
        {
            const ulong currentBlockId = 123;
            const ulong expectedBlockId = 234;
            SetupBlockServiceMock(currentBlockId, expectedBlockId, new List<Transaction>());

            var nextBlock = _nxtConnector.GetNextBlock(123);

            Assert.AreEqual(expectedBlockId, nextBlock.NxtBlockId.ToUnsigned());
        }

        [TestMethod]
        public void GetNextBlockShouldReturnTransaction()
        {
            const ulong currentBlockId = 123;
            const ulong expectedBlockId = 234;
            var transaction = new Transaction
            {
                Amount = Amount.OneNxt,
                Recipient = 123,
                SubType = TransactionSubType.PaymentOrdinaryPayment
            };
            SetupBlockServiceMock(currentBlockId, expectedBlockId, new List<Transaction>());

            var nextBlock = _nxtConnector.GetNextBlock(123);

            Assert.AreEqual(expectedBlockId, nextBlock.NxtBlockId.ToUnsigned());
        }

        [TestMethod]
        public void GetNextBlockShouldReturnNullWhenLastKnownBlock()
        {
            _blockServiceMock
                .Setup(s => s.GetBlock(It.Is<BlockLocator>(l => l.BlockId == 123)))
                .ReturnsAsync(new GetBlockReply<ulong> { NextBlock = null });

            var nextBlock = _nxtConnector.GetNextBlock(123);

            Assert.IsNull(nextBlock);
        }

        [TestMethod]
        public void GetNextBlockShouldThrowOnUnknownBlock()
        {
            _blockServiceMock
                .Setup(s => s.GetBlock(It.Is<BlockLocator>(l => l.BlockId == 123)))
                .ThrowsAsync(new NxtException(4, null, null, null));

            try
            {
                _nxtConnector.GetNextBlock(123);
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException.GetType() == typeof(NxtException))
                {
                    return;
                }
            }

            Assert.Fail("There was no NxtException.");
        }

        private void SetupBlockServiceMock(ulong getBlockId, ulong nextBlockId, List<Transaction> transactions)
        {
            _blockServiceMock
                .Setup(s => s.GetBlock(It.Is<BlockLocator>(l => l.BlockId == getBlockId)))
                .ReturnsAsync(new GetBlockReply<ulong> { NextBlock = nextBlockId });
            _blockServiceMock
                .Setup(s => s.GetBlockIncludeTransactions(It.Is<BlockLocator>(l => l.BlockId == nextBlockId)))
                .ReturnsAsync(new GetBlockReply<Transaction> { BlockId = nextBlockId, Transactions = transactions });
        }
    }
}
