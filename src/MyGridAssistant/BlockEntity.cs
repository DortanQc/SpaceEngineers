using Sandbox.ModAPI.Ingame;
using System;

namespace MyGridAssistant
{
    public class BlockEntity<T>
        where T : class, IMyTerminalBlock
    {
        private readonly IMyGridTerminalSystem _gridTerminalSystem;
        private readonly IMyGridAssistantLogger _logger;

        public BlockEntity(IMyGridAssistantLogger logger, IMyGridTerminalSystem gridTerminalSystem, T block)
        {
            _logger = logger;
            _gridTerminalSystem = gridTerminalSystem;
            Block = block;
        }

        public T Block { get; }

        public bool Exists()
        {
            var foundBlock = (T)_gridTerminalSystem.GetBlockWithId(Block.EntityId);

            return foundBlock != null;
        }

        public bool IsOfType<TType>() =>
            Block is TType;

        public BlockEntity<TNewType> CastTo<TNewType>()
            where TNewType : class, IMyTerminalBlock
        {
            var blockOfType = Block as TNewType;

            if (blockOfType == null)
                throw new Exception($"Block {Block.CustomName} is not of type {typeof(TNewType)}");

            return new BlockEntity<TNewType>(_logger, _gridTerminalSystem, blockOfType);
        }
    }
}
