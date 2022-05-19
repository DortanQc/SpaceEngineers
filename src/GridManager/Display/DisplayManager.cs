using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    public class DisplayManager
    {
        private static Action<string> _logger;

        public static void Display(
            MonitoringData monitoringData,
            IEnumerable<Item> itemsWithThreshold,
            List<IMyTerminalBlock> allBlocks,
            Action<string> logger)
        {
            _logger = logger;
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
            IEnumerable<Item> itemsToDisplay,
            IReadOnlyCollection<Item> itemsWithThreshold,
            MonitoringData monitoringData,
            IEnumerable<Item.ItemTypes> itemTypes)
        {
            var textBuilder = new StringBuilder();
            var alreadyHasType = new Dictionary<Item.ItemTypes, bool>();
            var hasSomethingToDisplay = false;

            itemsToDisplay.OrderByDescending(i => i.Amount)
                .ToList()
                .ForEach(item =>
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
            var hideWhenMetThreshold =
                customData.GetPropertyValue(CustomDataSettings.EXCLUDE_FROM_STATS_INVENTORY_WHEN_OVER_THRESHOLD) !=
                null;

            var itemMetThreshold = IsItemMetThreshold(
                item,
                itemsWithThreshold,
                monitoringData);

            var itemHasThresholdDefined = HasItemThresholdDefined(item, itemsWithThreshold);

            return hideWhenMetThreshold && itemHasThresholdDefined && itemMetThreshold;
        }

        private static bool ShouldHideItem(CustomDataManager customData, Item item)
        {
            var show = customData.GetPropertyValue(
                           $"{CustomDataSettings.EXCLUDE_ITEM_FROM_STATS_INVENTORY}-{item.ItemSubType}") ==
                       null;

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
            var displayAmmunitionSurfaceBlocks = GetDisplayBlocks(
                blocks,
                CustomDataSettings.STATS_INVENTORY_AMMUNITION);

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

            var remainingCapacity =
                monitoringData.Cargos.Sum(c => (float)c.MaxVolume) -
                monitoringData.Cargos.Sum(c => (float)c.CurrentVolume);

            textBuilder
                .AppendLine("** Storage Capacity **")
                .AppendLine()
                .AppendLine($"Max Storage Capacity: {monitoringData.Cargos.Sum(c => (float)c.MaxVolume):0.##} m^3")
                .AppendLine($"Current Volume: {monitoringData.Cargos.Sum(c => (float)c.CurrentVolume):0.##} m^3")
                .AppendLine($"Remaining Capacity: {remainingCapacity:0.##} m^3")
                .AppendLine(
                    $"Filled Ratio: {monitoringData.Cargos.Sum(c => (float)c.CurrentVolume) * 100 / monitoringData.Cargos.Sum(c => (float)c.MaxVolume):0.##} %");

            if (monitoringData.Cargos.Any())
                textBuilder
                    .AppendLine()
                    .AppendLine("** Containers ** ")
                    .AppendLine();

            monitoringData.Cargos.OrderBy(b => b.Name)
                .ToList()
                .ForEach(cargo =>
                {
                    textBuilder
                        .Append(cargo.Name)
                        .Append(" - ")
                        .AppendLine(
                            $"{cargo.CurrentVolume.ToIntSafe() * 100 / cargo.MaxVolume.ToIntSafe():0.##} %");
                });

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
                .AppendLine($"Max Power Output: {monitoringData.MaxPowerOutput:0.##} MW")
                .AppendLine($"Current Power Output: {monitoringData.CurrentPowerOutput:0.##} MW")
                .AppendLine(shortage < 0
                    ? $"Shortage of: {shortage:0.##} MW"
                    : $"Exceeding of: {shortage:0.##} MW")
                .AppendLine(
                    $"Usage Ratio: {monitoringData.CurrentPowerOutput * 100 / monitoringData.MaxPowerOutput:0.##} %");

            if (monitoringData.Batteries.Any())
                textBuilder
                    .AppendLine()
                    .AppendLine("** Batteries ** ")
                    .AppendLine();

            monitoringData.Batteries.OrderBy(b => b.Name)
                .ToList()
                .ForEach(battery =>
                {
                    textBuilder
                        .Append(battery.Name)
                        .Append(" - ")
                        .Append(battery.IsCharging
                            ? "C"
                            : "D")
                        .Append(" - ")
                        .AppendLine(
                            $"{battery.CurrentStoredPower * 100 / battery.MaxStoredPower:0.##} %");
                });

            var textSurfaceBlock = GetDisplayBlocks(blocks, CustomDataSettings.STATS_POWER_USAGE);

            textSurfaceBlock.ForEach(block => block.TextSurface.WriteText(textBuilder));
        }

        private static void DisplayHydrogenStatistics(
            MonitoringData monitoringData,
            IEnumerable<IMyTerminalBlock> blocks)
        {
            var textSurfaceBlock = GetDisplayBlocks(blocks, CustomDataSettings.STATS_HYDROGEN_USAGE);

            textSurfaceBlock.ForEach(surface =>
            {
                const float LEFT_MARGIN = 30f;
                const float TOP_MARGIN = 20f;
                const float ICON_SIZE = 30f;
                const float TEXT_LEFT_BEGIN = LEFT_MARGIN + 80f;
                const float BAR_LEFT_BEGIN = LEFT_MARGIN + ICON_SIZE;
                const float ROW_TOP_BEGIN = 85f;
                const float BAR_TOP_BEGIN = 50f;
                const float BAR_HEIGHT = 30f;

                var ratio = surface.BlockCustomData.GetPropertyValue(CustomDataSettings.LCD_WIDTH_RATIO) != null;

                var textSurface = surface.TextSurface;
                var engin = new GraphicEngine(textSurface, ratio);
                var barSize = textSurface.SurfaceSize.X - 90f;

                engin.BackgroundColor = new Color(0, 0, 0, 255);

                var i = 0;

                foreach (var tank in monitoringData.HydrogenTanks.OrderBy(x => x.Name))
                {
                    var rowYPos = i * ROW_TOP_BEGIN + TOP_MARGIN;

                    engin.AddText(
                        tank.Name,
                        1f,
                        TEXT_LEFT_BEGIN,
                        rowYPos,
                        TextAlignment.LEFT,
                        textSurface.ScriptForegroundColor
                    );

                    engin.AddSprite(
                        "IconHydrogen",
                        ICON_SIZE,
                        ICON_SIZE,
                        textSurface.ScriptBackgroundColor,
                        LEFT_MARGIN,
                        rowYPos + BAR_TOP_BEGIN,
                        TextAlignment.CENTER,
                        0f,
                        false
                    );

                    engin.AddSprite(
                        "SquareSimple",
                        barSize,
                        BAR_HEIGHT,
                        new Color(10, 10, 10, 200),
                        BAR_LEFT_BEGIN,
                        rowYPos + BAR_TOP_BEGIN,
                        TextAlignment.LEFT,
                        0f,
                        false
                    );

                    engin.AddSprite(
                        "SquareSimple",
                        Convert.ToSingle(tank.FilledRatio) * barSize,
                        BAR_HEIGHT,
                        textSurface.ScriptBackgroundColor,
                        BAR_LEFT_BEGIN,
                        rowYPos + BAR_TOP_BEGIN,
                        TextAlignment.LEFT,
                        0f,
                        false
                    );

                    engin.AddSprite(
                        "SquareSimple",
                        3f,
                        BAR_HEIGHT,
                        new Color(0, 0, 0, 255),
                        BAR_LEFT_BEGIN + barSize / 4,
                        rowYPos + BAR_TOP_BEGIN,
                        TextAlignment.LEFT,
                        0f,
                        false
                    );

                    engin.AddSprite(
                        "SquareSimple",
                        3f,
                        BAR_HEIGHT,
                        new Color(0, 0, 0, 255),
                        BAR_LEFT_BEGIN + barSize / 4 * 2,
                        rowYPos + BAR_TOP_BEGIN,
                        TextAlignment.LEFT,
                        0f,
                        false
                    );

                    engin.AddSprite(
                        "SquareSimple",
                        3f,
                        BAR_HEIGHT,
                        new Color(0, 0, 0, 255),
                        BAR_LEFT_BEGIN + barSize / 4 * 3,
                        rowYPos + BAR_TOP_BEGIN,
                        TextAlignment.LEFT,
                        0f,
                        false
                    );

                    engin.AddSprite(
                        "Triangle",
                        38f,
                        80f,
                        new Color(0, 0, 0, 255),
                        BAR_LEFT_BEGIN + 6,
                        rowYPos + 41f,
                        TextAlignment.LEFT,
                        1.3f,
                        true
                    );

                    engin.AddText(
                        (tank.FilledRatio * 100).ToString("F2") + "%",
                        .75f,
                        barSize + BAR_LEFT_BEGIN,
                        7f + rowYPos,
                        TextAlignment.RIGHT,
                        textSurface.ScriptForegroundColor);

                    i++;
                }

                engin.Draw();
            });
        }
    }
}
