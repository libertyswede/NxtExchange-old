using System;

namespace NxtExchange
{
    public class BlockDoesNotExistException : Exception
    {
        public ulong BlockId { get; private set; }

        public BlockDoesNotExistException(ulong blockId)
        {
            BlockId = blockId;
        }
    }
}