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

        [TestInitialize]
        public void TestInit()
        {
            _serviceFactoryMock = new Mock<IServiceFactory>();
            _blockServiceMock = new Mock<IBlockService>();
            _serviceFactoryMock.Setup(f => f.CreateBlockService()).Returns(_blockServiceMock.Object);
            _nxtConnector = new NxtConnector(_serviceFactoryMock.Object);
        }

        [TestMethod]
        public void GetNextBlockShouldReturnNextBlock()
        {
            const ulong expectedBlockId = 234;
            _blockServiceMock
                .Setup(s => s.GetBlock(It.Is<BlockLocator>(l => l.BlockId == 123)))
                .ReturnsAsync(new GetBlockReply<ulong> { NextBlock = expectedBlockId });
            _blockServiceMock
                .Setup(s => s.GetBlockIncludeTransactions(It.Is<BlockLocator>(l => l.BlockId == expectedBlockId)))
                .ReturnsAsync(new GetBlockReply<Transaction> { BlockId = expectedBlockId, Transactions = new List<Transaction>() });

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
                .ThrowsAsync(new NxtException(4, string.Empty, string.Empty, string.Empty));

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
    }
}
