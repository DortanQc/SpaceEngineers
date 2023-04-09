using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;

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
            List<Item> allItemsInInventory,
            List<Item> allItemsInProduction,
            IEnumerable<Item> itemsToProduce,
            IEnumerable<IMyProductionBlock> blocksProducingItems)
        {
            var allowedBlockForProduction = GetAllowedBlocksForProduction(blocksProducingItems);

            foreach (var itemToProduce in itemsToProduce)
            {
                var ownedInventoryCount = allItemsInInventory
                    .Where(ownerItem => ownerItem.IsSameAs(itemToProduce))
                    .Sum(x => x.Amount);

                var producingCount = allItemsInProduction
                    .Where(producingItem => producingItem.IsSameAs(itemToProduce))
                    .Sum(x => x.Amount);

                var count = ownedInventoryCount + producingCount;

                if (count >= itemToProduce.Amount) continue;

                var delta = itemToProduce.Amount - count;

                if (delta > 0)
                    ProduceItem(itemToProduce, delta, allowedBlockForProduction);
            }
        }

        private static List<IMyProductionBlock> GetAllowedBlocksForProduction(IEnumerable<IMyProductionBlock> blocks)
        {
            return blocks.Where(block =>
                {
                    var isIgnored = Configuration.GetBlockConfiguration(block, Settings.EXCLUDE_FROM_AUTO_PRODUCTION);

                    if (isIgnored != null)
                    {
                        _logger.LogDebug($"block {block.CustomName} ignored for auto producing items", 3);

                        return false;
                    }

                    var assemblerBlock = block as IMyAssembler;

                    if (assemblerBlock == null)
                        return true;

                    if (assemblerBlock.Mode == MyAssemblerMode.Assembly) return true;

                    _logger.LogDebug($"block {block.CustomName} ignored because set in `Disassembly` mode", 3);

                    return false;
                })
                .ToList();
        }

        private static void ProduceItem(Item itemToProduce, decimal amount, IReadOnlyCollection<IMyProductionBlock> blocksProducingItems)
        {
            var best = GetBestBlockForJob(itemToProduce, blocksProducingItems);

            if (best == null)
            {
                _logger.LogDebug($"No production block found for producing {amount} {itemToProduce.ItemDefinitionId.SubtypeName}");

                return;
            }

            best.AddQueueItem(itemToProduce.ItemDefinitionId, amount);
            _logger.LogDebug($"Queued item: {amount} {itemToProduce.ItemDefinitionId.SubtypeName} into {best.CustomName}");
        }

        private static IMyProductionBlock GetBestBlockForJob(Item itemToProduce, IReadOnlyCollection<IMyProductionBlock> blocksProducingItems)
        {
            var dict = new Dictionary<long, int>();

            if (!blocksProducingItems.Any())
                return null;

            foreach (var block in blocksProducingItems)
            {
                if (!block.CanUseBlueprint(itemToProduce.ItemDefinitionId))
                    continue;

                var actualQueue = new List<MyProductionItem>();
                block.GetQueue(actualQueue);

                var actualQueueCount = actualQueue.Sum(q => q.Amount.ToIntSafe());
                dict.Add(block.EntityId, actualQueueCount);
            }

            if (!dict.Any())
                return null;

            var bestBlockId = dict.OrderBy(x => x.Value).First().Key;

            return blocksProducingItems.First(x => x.EntityId == bestBlockId);
        }
    }
}
