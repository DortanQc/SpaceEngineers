using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript.Scripts
{
    public class Grid
    {
        private readonly bool _activateLog;
        private readonly Action<string> _logger;
        private readonly List<IMyTerminalBlock> _productionBlocks;
        private readonly List<IMyTerminalBlock> _storageBlocks;

        public Grid(
            List<IMyTerminalBlock> storageBlocks,
            List<IMyTerminalBlock> productionBlocks,
            Action<string> logger,
            bool activateLog)
        {
            _storageBlocks = storageBlocks;
            _productionBlocks = productionBlocks;
            _logger = logger;
            _activateLog = activateLog;
        }

        public long GetItemAmountInStorage(Component component)
        {
            var currentAmount = 0L;

            foreach (var block in _storageBlocks)
                currentAmount = CountItemInStorage(component.ItemType.SubtypeId, block, currentAmount);

            foreach (var block in _productionBlocks)
                currentAmount = CountItemInProductionOutputStorage(
                    component.ItemType.SubtypeId,
                    block as IMyProductionBlock,
                    currentAmount);

            return currentAmount;
        }

        public long GetItemAmountInStorage(Resource resource)
        {
            var currentAmount = 0L;

            foreach (var block in _storageBlocks)
                currentAmount = CountItemInStorage(resource.Code, block, currentAmount);

            foreach (var block in _productionBlocks)
                currentAmount = CountItemInProductionOutputStorage(
                    resource.Code,
                    block as IMyProductionBlock,
                    currentAmount);

            return currentAmount;
        }

        public long GetItemAmountInProduction(Component item)
        {
            var currentAmount = 0L;

            foreach (var block in _productionBlocks)
                currentAmount = CountItemInProduction(
                    item,
                    block as IMyProductionBlock,
                    currentAmount);

            return currentAmount;
        }

        private long CountItemInStorage(string code, IMyEntity block, long currentAmount)
        {
            var items = new List<MyInventoryItem>();

            block.GetInventory().GetItems(items, item => item.Type.SubtypeId == code);

            foreach (var item in items)
                Log(item.Type.SubtypeId);

            return currentAmount + items.Sum(item => item.Amount.RawValue);
        }

        private static long CountItemInProduction(Component item, IMyProductionBlock block, long currentAmount)
        {
            var queue = new List<MyProductionItem>();

            block.GetQueue(queue);

            return currentAmount +
                   queue
                       .Where(q => q.BlueprintId.SubtypeId == item.ItemDefinition.SubtypeId)
                       .Sum(itemInProd => itemInProd.Amount.RawValue);
        }

        private long CountItemInProductionOutputStorage(string code, IMyProductionBlock block, long currentAmount)
        {
            var items = new List<MyInventoryItem>();

            block.OutputInventory.GetItems(items, item => item.Type.SubtypeId == code);

            foreach (var item in items)
                Log(item.Type.SubtypeId);

            return currentAmount + items.Sum(item => item.Amount.RawValue);
        }

        public static void ProduceItem(
            Component item,
            decimal amount,
            IReadOnlyCollection<IMyTerminalBlock> assemblers)
        {
            var dict = new Dictionary<long, int>();

            foreach (var assembler in assemblers.OfType<IMyProductionBlock>())
            {
                if (!assembler.CanUseBlueprint(item.ItemDefinition)) continue;

                var queue = new List<MyProductionItem>();
                assembler.GetQueue(queue);

                dict.Add(assembler.EntityId, queue.Count);

                break;
            }

            var bestAssembler = dict
                .OrderByDescending(x => x.Value)
                .Select(orderedAssemblersByQueueCount =>
                    assemblers
                        .OfType<IMyProductionBlock>()
                        .First(x => x.EntityId == orderedAssemblersByQueueCount.Key))
                .First();

            bestAssembler.AddQueueItem(item.ItemDefinition, amount);
        }

        private void Log(string message)
        {
            if (!_activateLog) return;

            _logger(message);
        }
    }
}