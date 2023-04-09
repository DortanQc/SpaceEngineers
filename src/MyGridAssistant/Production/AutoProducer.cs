using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;

namespace MyGridAssistant
{
    public class AutoProducer
    {
        private static IMyGridAssistantLogger _logger;

        public AutoProducer(IMyGridAssistantLogger logger)
        {
            _logger = logger;
        }

        public void Produce(
            MonitoringData data,
            IEnumerable<Item> itemsToProduce,
            IEnumerable<IMyProductionBlock> blocksProducingItems)
        {
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
                    var isIgnored = Configuration.GetBlockConfiguration(block, Settings.EXCLUDE_FROM_AUTO_PRODUCTION);

                    if (isIgnored == null) 
                        return true;

                    _logger.LogDebug($"block {block.CustomName} ignored for auto producing items", 3);

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

            best.AddQueueItem(itemDefinitionId, amount);
            _logger.LogDebug($"Queued item: {amount} {itemDefinitionId.SubtypeName} into {best.CustomName}");
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
