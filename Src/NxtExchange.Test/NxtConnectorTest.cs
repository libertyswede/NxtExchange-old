using System;
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
            const ulong expectedBlockId = (ulong)Int64.MaxValue + 3;
            SetupBlockService(expectedBlockId);

            var nextBlock = _nxtConnector.GetNextBlock(123);

            Assert.AreEqual(expectedBlockId, nextBlock.NxtBlockId.ToUnsigned());
        }

        [TestMethod]
        public void GetNextBlockShouldReturnNullWhenLastKnownBlock()
        {
            SetupBlockService(null);

            var nextBlock = _nxtConnector.GetNextBlock(123);

            Assert.IsNull(nextBlock);
        }

        [TestMethod]
        [ExpectedException(typeof(NxtException))]
        public void GetNextBlockShouldThrowOnUnknownBlock()
        {
            //_blockServiceMock
            //    .Setup(s => s.GetBlock(It.Is<BlockLocator>(l => l.QueryParameters["block"].Equals("123"))))
            //    .ThrowsAsync(new NxtException(4, string.Empty, string.Empty, string.Empty));

            _nxtConnector.GetNextBlock(123);
        }

        private void SetupBlockService(ulong? expectedBlockId)
        {
            //_blockServiceMock
            //    .Setup(s => s.GetBlock(It.Is<BlockLocator>(l => l.QueryParameters["block"].Equals("123"))))
            //    .ReturnsAsync(new GetBlockReply<ulong> {NextBlock = expectedBlockId});
        }
    }
}
