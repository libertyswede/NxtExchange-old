using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NxtExchange.DAL;
using NxtLib;
using NxtLib.Accounts;
using NxtLib.Blocks;

namespace NxtExchange.Test
{
    [TestClass]
    public class NxtConnectorTest
    {
        private Mock<IServiceFactory> _serviceFactoryMock;
        private Mock<IBlockService> _blockServiceMock;
        private Mock<IAccountService> _accountServiceMock;
        private Mock<IBlockProcessor> _blockProcessorMock;
        private Mock<ITransactionProcessor> _transactionProcessorMock;

        private NxtConnector _nxtConnector;

        [TestInitialize]
        public void TestInit()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>();
            _blockServiceMock = new Mock<IBlockService>();
            _blockProcessorMock = new Mock<IBlockProcessor>();
            _transactionProcessorMock = new Mock<ITransactionProcessor>();
            _accountServiceMock = new Mock<IAccountService>();

            _serviceFactoryMock.Setup(f => f.CreateBlockService()).Returns(_blockServiceMock.Object);
            _serviceFactoryMock.Setup(f => f.CreateAccountService()).Returns(_accountServiceMock.Object);
            _nxtConnector = new NxtConnector(_serviceFactoryMock.Object, _blockProcessorMock.Object, _transactionProcessorMock.Object);
        }

        [TestMethod]
        public void GetNextBlockShouldReturnNextBlock()
        {
            const ulong currentBlockId = 123;
            const ulong nextBlockId = 234;
            var expectedBlock = new Block();
            var getBlockReply = new GetBlockReply<Transaction>();
            _blockServiceMock
                .Setup(s => s.GetBlock(It.Is<BlockLocator>(l => l.BlockId == currentBlockId)))
                .ReturnsAsync(new GetBlockReply<ulong> { NextBlock = nextBlockId });
            _blockServiceMock
                .Setup(s => s.GetBlockIncludeTransactions(It.Is<BlockLocator>(l => l.BlockId == nextBlockId)))
                .ReturnsAsync(getBlockReply);
            _blockProcessorMock
                .Setup(b => b.ConvertBlockAndTransactions(It.Is<Block<Transaction>>(block => block == getBlockReply)))
                .Returns(expectedBlock);

            var actual = _nxtConnector.GetNextBlock(123);

            Assert.AreSame(expectedBlock, actual);
        }

        [TestMethod]
        public void GetNextBlockShouldReturnNullWhenLastKnownBlock()
        {
            const ulong currentBlockId = 123;
            _blockServiceMock
                .Setup(s => s.GetBlock(It.Is<BlockLocator>(l => l.BlockId == currentBlockId)))
                .ReturnsAsync(new GetBlockReply<ulong> { NextBlock = null });

            var actual = _nxtConnector.GetNextBlock(123);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void GetNextBlockShouldThrowOnUnknownBlock()
        {
            const ulong currentBlockId = 123;
            _blockServiceMock
                .Setup(s => s.GetBlock(It.Is<BlockLocator>(l => l.BlockId == 123)))
                .ThrowsAsync(new NxtException(4, null, null, null));

            try
            {
                _nxtConnector.GetNextBlock(currentBlockId);
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException.GetType() == typeof(NxtException))
                {
                    return;
                }
            }

            Assert.Fail("There was no NxtException thrown");
        }

        [TestMethod]
        public void GetNextBlockShouldThrowOnUnknownNextBlock()
        {
            const ulong currentBlockId = 123;
            const ulong nextBlockId = 234;
            _blockServiceMock
                .Setup(s => s.GetBlock(It.Is<BlockLocator>(l => l.BlockId == currentBlockId)))
                .ReturnsAsync(new GetBlockReply<ulong> { NextBlock = nextBlockId });
            _blockServiceMock
                .Setup(s => s.GetBlockIncludeTransactions(It.Is<BlockLocator>(l => l.BlockId == nextBlockId)))
                .ThrowsAsync(new NxtException(4, null, null, null));

            try
            {
                _nxtConnector.GetNextBlock(currentBlockId);
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException.GetType() == typeof(NxtException))
                {
                    return;
                }
            }

            Assert.Fail("There was no NxtException thrown");
        }

        [TestMethod]
        public void GetUnconfirmedTransactionsTest()
        {
            var expected = new List<InboundTransaction>();
            _accountServiceMock
                .Setup(a => a.GetUnconfirmedTransactions(It.IsAny<string>()))
                .ReturnsAsync(new UnconfirmedTransactionsReply());
            _transactionProcessorMock
                .Setup(t => t.ConvertToInboundTransactions(It.IsAny<IList<Transaction>>()))
                .Returns(expected);
            
            var actual = _nxtConnector.GetNewUnconfirmedTransactions();

            Assert.AreSame(expected, actual);
        }
    }
}
