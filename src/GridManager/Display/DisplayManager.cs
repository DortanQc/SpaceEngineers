using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IngameScript
{
    public class DisplayManager
    {
        public static void Display(
            MonitoringData monitoringData,
            IEnumerable<Item> itemsWithThreshold,
            List<IMyTerminalBlock> allBlocks)
        {
            DisplayItemInventory(monitoringData, itemsWithThreshold.ToList(), allBlocks);
            DisplayStorageCapacity(monitoringData, allBlocks);
            DisplayPowerUsage(monitoringData, allBlocks);
            DisplayProduction(monitoringData, allBlocks);
            DisplayHydrogenStatistics(monitoringData, allBlocks);
        }

        private static List<Surface> GetDisplayBlocks(
            IEnumerable<IMyTerminalBlock> blocks,
            string customDataKeyToLookup)
        {
            var results = new List<Surface>();

            foreach (var block in blocks)
            {
                var customDataManager = new CustomDataManager(block.CustomData);
                var customData = customDataManager.GetPropertyValue(customDataKeyToLookup);

                if (customData == null) continue;

                var index = 0;
                if (customData.Length > 0)
                    int.TryParse(customData, out index);

                var textSurface = block as IMyTextSurfaceProvider;

                if (textSurface == null || textSurface.SurfaceCount <= index) continue;

                var result = new Surface
                {
                    BlockId = block.EntityId,
                    TextSurface = textSurface.GetSurface(index),
                    BlockCustomData = customDataManager
                };

                results.Add(result);
            }

            return results;
        }

        private static void DisplayProduction(MonitoringData monitoringData, IEnumerable<IMyTerminalBlock> blocks)
        {
            var textBuilder = new StringBuilder();

            textBuilder
                .AppendLine("** Currently in production **")
                .AppendLine();

            monitoringData
                .GetItemsInProduction()
                .ForEach(inProd =>
                {
                    textBuilder.AppendLine(inProd.ItemType == Item.ItemTypes.Ingot
                        ? $"{ToFriendlyQuantity(inProd.Amount / inProd.Volume)} {inProd.Name}"
                        : $"{ToFriendlyQuantity(inProd.Amount)} {inProd.Name}");
                });

            var textSurfaceBlock = GetDisplayBlocks(blocks, CustomDataSettings.STATS_IN_PRODUCTION);

            textSurfaceBlock.ForEach(block => block.TextSurface.WriteText(textBuilder));
        }

        private static string ToFriendlyQuantity(float quantity) =>
            quantity >= 1000
                ? $"{quantity / 1000:0.##}K"
                : quantity.ToString("0.##");

        private static void DisplayInventoryForTypes(
            Surface block,
            List<Item> itemsToDisplay,
            IReadOnlyCollection<Item> itemsWithThreshold,
            MonitoringData monitoringData,
            IEnumerable<Item.ItemTypes> itemTypes)
        {
            var textBuilder = new StringBuilder();
            var alreadyHasType = new Dictionary<Item.ItemTypes, bool>();
            var hasSomethingToDisplay = false;

            itemsToDisplay.ForEach(item =>
            {
                if (ShouldHideItem(block.BlockCustomData, item))
                    return;

                if (ShouldHideBecauseMetThreshold(item, itemsWithThreshold, monitoringData, block.BlockCustomData))
                    return;

                itemTypes
                    .ToList()
                    .ForEach(type =>
                    {
                        if (item.ItemType != type)
                            return;

                        if (!alreadyHasType.ContainsKey(type))
                            alreadyHasType.Add(type, false);

                        alreadyHasType[type] = DisplayItems(
                            type,
                            item,
                            alreadyHasType[type],
                            textBuilder,
                            ref hasSomethingToDisplay);
                    });
            });

            block.TextSurface.WriteText(textBuilder);
        }

        private static bool ShouldHideBecauseMetThreshold(
            Item item,
            IReadOnlyCollection<Item> itemsWithThreshold,
            MonitoringData monitoringData,
            CustomDataManager customData)
        {
            var hideWhenMetThreshold = customData.GetPropertyValue(CustomDataSettings.EXCLUDE_FROM_STATS_INVENTORY_WHEN_OVER_THRESHOLD) != null;

            var itemMetThreshold = IsItemMetThreshold(
                item,
                itemsWithThreshold,
                monitoringData);

            var itemHasThresholdDefined = HasItemThresholdDefined(item, itemsWithThreshold);

            return hideWhenMetThreshold && itemHasThresholdDefined && itemMetThreshold;
        }

        private static bool ShouldHideItem(CustomDataManager customData, Item item)
        {
            var show = customData.GetPropertyValue($"{CustomDataSettings.EXCLUDE_ITEM_FROM_STATS_INVENTORY}-{item.ItemSubType}") == null;

            return show == false;
        }

        private static bool DisplayItems(
            Item.ItemTypes itemType,
            Item item,
            bool alreadyHasItem,
            StringBuilder textBuilder,
            ref bool hasSomethingToDisplay)
        {
            if (item.ItemType == itemType)
                if (!alreadyHasItem)
                {
                    if (hasSomethingToDisplay)
                        textBuilder.AppendLine();

                    textBuilder.AppendLine($"** Owned {itemType.ToString()} **");
                    alreadyHasItem = true;
                }

            hasSomethingToDisplay = true;
            textBuilder.AppendLine($"{ToFriendlyQuantity(item.Amount)} {item.Name}");

            return alreadyHasItem;
        }

        private static void DisplayItemInventory(
            MonitoringData monitoringData,
            IReadOnlyCollection<Item> itemsWithThreshold,
            IReadOnlyCollection<IMyTerminalBlock> blocks)
        {
            var itemsToDisplay = BuildItemsToDisplay(monitoringData.GetItems(), itemsWithThreshold);

            var displayAllSurfaceBlocks = GetDisplayBlocks(blocks, CustomDataSettings.STATS_INVENTORY);
            var displayComponentSurfaceBlocks = GetDisplayBlocks(blocks, CustomDataSettings.STATS_INVENTORY_COMPONENTS);
            var displayOreSurfaceBlocks = GetDisplayBlocks(blocks, CustomDataSettings.STATS_INVENTORY_ORES);
            var displayIngotSurfaceBlocks = GetDisplayBlocks(blocks, CustomDataSettings.STATS_INVENTORY_INGOTS);
            var displayToolsSurfaceBlocks = GetDisplayBlocks(blocks, CustomDataSettings.STATS_INVENTORY_TOOLS);
            var displayAmmunitionSurfaceBlocks = GetDisplayBlocks(blocks, CustomDataSettings.STATS_INVENTORY_AMMUNITION);

            var combinations = Combine(
                displayAllSurfaceBlocks,
                displayComponentSurfaceBlocks,
                displayOreSurfaceBlocks,
                displayIngotSurfaceBlocks,
                displayToolsSurfaceBlocks,
                displayAmmunitionSurfaceBlocks);

            combinations.ForEach(combination =>
            {
                DisplayInventoryForTypes(
                    combination.Block,
                    itemsToDisplay,
                    itemsWithThreshold,
                    monitoringData,
                    combination.ItemTypes);
            });
        }

        private static List<SurfaceByType> Combine(
            IEnumerable<Surface> allSurfaceBlocks,
            IEnumerable<Surface> componentSurfaceBlocks,
            IEnumerable<Surface> oreSurfaceBlocks,
            IEnumerable<Surface> ingotSurfaceBlocks,
            IEnumerable<Surface> toolsSurfaceBlocks,
            IEnumerable<Surface> ammunitionSurfaceBlocks)
        {
            var allBlocks = allSurfaceBlocks
                .Select(b => new SurfaceByType
                {
                    Block = b,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Ammunition,
                        Item.ItemTypes.Component,
                        Item.ItemTypes.Consumable,
                        Item.ItemTypes.Ingot,
                        Item.ItemTypes.Ore,
                        Item.ItemTypes.Tools,
                        Item.ItemTypes.Unknown
                    }
                })
                .Concat(componentSurfaceBlocks.Select(b => new SurfaceByType
                {
                    Block = b,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Component,
                        Item.ItemTypes.Unknown
                    }
                }))
                .Concat(oreSurfaceBlocks.Select(b => new SurfaceByType
                {
                    Block = b,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Ore,
                        Item.ItemTypes.Unknown
                    }
                }))
                .Concat(ingotSurfaceBlocks.Select(b => new SurfaceByType
                {
                    Block = b,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Ingot,
                        Item.ItemTypes.Unknown
                    }
                }))
                .Concat(toolsSurfaceBlocks.Select(b => new SurfaceByType
                {
                    Block = b,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Tools,
                        Item.ItemTypes.Unknown
                    }
                }))
                .Concat(ammunitionSurfaceBlocks.Select(b => new SurfaceByType
                {
                    Block = b,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Ammunition,
                        Item.ItemTypes.Unknown
                    }
                }))
                .ToList();

            var dictionary = new Dictionary<long, SurfaceByType>();

            allBlocks.ForEach(block =>
            {
                if (dictionary.ContainsKey(block.Block.BlockId))
                {
                    var actualTypes = dictionary[block.Block.BlockId].ItemTypes.ToList();
                    var typesToAdd = block.ItemTypes.ToList();
                    var newTypesToAdd = new List<Item.ItemTypes>(actualTypes);

                    typesToAdd.ForEach(type =>
                    {
                        if (actualTypes.All(x => x != type))
                            newTypesToAdd.Add(type);
                    });

                    dictionary[block.Block.BlockId].ItemTypes = newTypesToAdd;
                }
                else
                {
                    var typesToAdd = block.ItemTypes.ToList();

                    dictionary.Add(
                        block.Block.BlockId,
                        new SurfaceByType
                        {
                            Block = block.Block,
                            ItemTypes = typesToAdd
                        });
                }
            });

            return dictionary.Select(d => new SurfaceByType
                {
                    Block = allBlocks.First(b => b.Block.BlockId == d.Key).Block,
                    ItemTypes = d.Value.ItemTypes
                })
                .ToList();
        }

        private static List<Item> BuildItemsToDisplay(
            IEnumerable<Item> itemsInInventory,
            IEnumerable<Item> itemsWithThreshold)
        {
            var itemsToDisplay = itemsInInventory.ToList();

            foreach (var itemWithThreshold in itemsWithThreshold)
                if (!itemsToDisplay.Any(itemToDisplay =>
                        itemToDisplay.ItemType == itemWithThreshold.ItemType &&
                        itemToDisplay.ItemSubType == itemWithThreshold.ItemSubType))
                    itemsToDisplay.Add(new Item(itemWithThreshold.ItemDefinition.ToString(), 0));

            return itemsToDisplay.OrderBy(item => item.ItemType).ThenBy(item => item.Name).ToList();
        }

        private static bool HasItemThresholdDefined(Item item, IEnumerable<Item> itemsWithThreshold)
        {
            return itemsWithThreshold.Any(itemWithThreshold =>
                itemWithThreshold.ItemType == item.ItemType &&
                itemWithThreshold.ItemSubType == item.ItemSubType);
        }

        private static bool IsItemMetThreshold(
            Item itemToDisplay,
            IEnumerable<Item> itemsWithThreshold,
            MonitoringData data)
        {
            var item = itemsWithThreshold.FirstOrDefault(itemWithThreshold =>
                itemWithThreshold.ItemType == itemToDisplay.ItemType &&
                itemWithThreshold.ItemSubType == itemToDisplay.ItemSubType);

            if (item == null) return true;

            var inventoryCount = data.GetItems()
                .Where(i =>
                    i.ItemType == itemToDisplay.ItemType &&
                    i.ItemSubType == itemToDisplay.ItemSubType)
                .Sum(x => x.Amount);

            return inventoryCount >= item.Amount;
        }

        private static void DisplayStorageCapacity(
            MonitoringData monitoringData,
            IEnumerable<IMyTerminalBlock> blocks)
        {
            var textBuilder = new StringBuilder();

            var remainingCapacity = monitoringData.MaxVolume - monitoringData.CurrentVolume;

            textBuilder
                .AppendLine("** Item's Storage Capacity **")
                .AppendLine()
                .AppendLine($"Max Storage Capacity: {monitoringData.MaxVolume} m^3")
                .AppendLine($"Current Volume: {monitoringData.CurrentVolume} m^3")
                .AppendLine($"Remaining Capacity: {remainingCapacity} m^3")
                .AppendLine()
                .AppendLine($"Current Mass: {monitoringData.CurrentMass} Kg");

            var textSurfaceBlock = GetDisplayBlocks(blocks, CustomDataSettings.STATS_STORAGE_CAPACITY);

            textSurfaceBlock.ForEach(block => block.TextSurface.WriteText(textBuilder));
        }

        private static void DisplayPowerUsage(MonitoringData monitoringData, IEnumerable<IMyTerminalBlock> blocks)
        {
            var textBuilder = new StringBuilder();
            var shortage = monitoringData.MaxPowerOutput - monitoringData.CurrentPowerOutput;

            textBuilder
                .AppendLine("** Power Usage **")
                .AppendLine()
                .AppendLine($"Current Power Output: {monitoringData.CurrentPowerOutput} MW")
                .AppendLine($"Max Power Output: {monitoringData.MaxPowerOutput} MW")
                .AppendLine()
                .AppendLine($"Total Batteries: {monitoringData.TotalBatteries}");

            if (monitoringData.ChargingBatteries > 0)
                textBuilder.AppendLine($"Total Charging Batteries: {monitoringData.ChargingBatteries}");

            if (monitoringData.DischargingBatteries > 0)
                textBuilder.AppendLine($"Total Discharging Batteries: {monitoringData.DischargingBatteries}");

            textBuilder.AppendLine()
                .AppendLine(shortage < 0
                    ? $"Power shortage of: {shortage} MW"
                    : $"Power exceeding of: {shortage} MW");

            var textSurfaceBlock = GetDisplayBlocks(blocks, CustomDataSettings.STATS_POWER_USAGE);

            textSurfaceBlock.ForEach(block => block.TextSurface.WriteText(textBuilder));
        }

        private static void DisplayHydrogenStatistics(
            MonitoringData monitoringData,
            IEnumerable<IMyTerminalBlock> blocks)
        {
            var textBuilder = new StringBuilder();
            var shortage = monitoringData.MaxPowerOutput - monitoringData.CurrentPowerOutput;

            textBuilder
                .AppendLine("** Hydrogen **")
                .AppendLine()
                .AppendLine($"Current Hydrogen Capacity: {monitoringData.HydrogenCapacity:0.##}")
                .AppendLine($"Current Hydrogen Filled Ratio: {monitoringData.HydrogenFilledRatio * 100:0.##} %");

            var textSurfaceBlock = GetDisplayBlocks(blocks, CustomDataSettings.STATS_HYDROGEN_USAGE);

            textSurfaceBlock.ForEach(block => block.TextSurface.WriteText(textBuilder));
        }
    }
}
