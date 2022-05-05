using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;

namespace IngameScript
{
    public static class AutoProducer
    {
        private static Action<string> _logger;

        public static void Produce(
            Action<string> logger,
            MonitoringData data,
            IEnumerable<Item> itemsToProduce,
            IEnumerable<IMyProductionBlock> blocksProducingItems)
        {
            _logger = logger;
            var allowedBlockForProduction = GetAllowedBlocksForProduction(blocksProducingItems);
            var allItemsInInventory = data.GetItems();
            var allItemsInProduction = data.GetItemsInProduction();

            foreach (var item in itemsToProduce)
            {
                var inventoryCount = allItemsInInventory
                    .Where(i => i.ItemDefinition == item.ItemDefinition)
                    .Sum(x => x.Amount);

                var producingCount = allItemsInProduction
                    .Where(i => i.ItemDefinition == item.ItemDefinition)
                    .Sum(x => x.Amount);

                var count = inventoryCount + producingCount;

                if (count >= item.Amount) continue;

                var delta = item.Amount - count;

                if (delta > 0)
                    ProduceItem(item.ItemDefinition, delta, allowedBlockForProduction);
            }
        }

        private static List<IMyProductionBlock> GetAllowedBlocksForProduction(IEnumerable<IMyProductionBlock> blocks)
        {
            return blocks.Where(block =>
                {
                    var customData = new CustomDataManager(block.CustomData);

                    var isIgnored = customData.GetPropertyValue("ignore-auto-production");

                    if (isIgnored == null) return true;

                    _logger("");
                    _logger($"block {block.CustomName} ignored for auto producing items");
                    _logger("");

                    return false;
                })
                .ToList();
        }

        private static void ProduceItem(
            MyDefinitionId itemDefinitionId,
            decimal amount,
            IReadOnlyCollection<IMyProductionBlock> blocksProducingItems)
        {
            var best = GetBestBlockForJob(itemDefinitionId, blocksProducingItems);

            if (best != null)
            {
                best.AddQueueItem(itemDefinitionId, amount);
                _logger($"");
                _logger($"Queued item: {amount} {itemDefinitionId.SubtypeName} into {best.CustomName}");
            }
            else
            {
                _logger($"");
                _logger($"No Block found for producing {amount} {itemDefinitionId.SubtypeName}");
            }
        }

        private static IMyProductionBlock GetBestBlockForJob(
            MyDefinitionId itemDefinitionId,
            IReadOnlyCollection<IMyProductionBlock> blocksProducingItems)
        {
            var dict = new Dictionary<long, int>();

            foreach (var block in blocksProducingItems)
            {
                if (!block.CanUseBlueprint(itemDefinitionId)) continue;

                var queue = new List<MyProductionItem>();
                block.GetQueue(queue);

                var count = queue.Sum(q => q.Amount.ToIntSafe());
                dict.Add(block.EntityId, count);
            }

            var best = dict
                .OrderBy(x => x.Value)
                .Select(orderedBlocks =>
                    blocksProducingItems.First(x => x.EntityId == orderedBlocks.Key))
                .FirstOrDefault();

            return best;
        }
    }
}
