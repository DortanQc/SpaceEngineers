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
            const float LEFT_OFFSET = 60f;
            const float TOP_OFFSET = 20f;

            var textSurfaceBlock = GetDisplayBlocks(blocks, CustomDataSettings.STATS_HYDROGEN_USAGE);

            textSurfaceBlock.ForEach(surface =>
            {
                var textSurface = surface.TextSurface;

                var barSize = textSurface.SurfaceSize.X - 90f;
                var smallScreenOffset = 0f;

                if (textSurface.SurfaceSize.Y == 320.00)
                    smallScreenOffset = 90f;

                using (var frame = textSurface.DrawFrame())
                {
                    //BACKGROUND
                    var sprite = new MySprite(
                        SpriteType.TEXTURE,
                        "SquareSimple",
                        size: new Vector2(512.0f, 512.0f),
                        color: new Color(0, 0, 0, 255))
                    {
                        Position = new Vector2(256.0f, 256.0f)
                    };

                    frame.Add(sprite);

                    var i = 0f;

                    foreach (var tank in monitoringData.HydrogenTanks.OrderBy(x => x.Name))
                    {
                        //DISPLAY NAME            
                        sprite = MySprite.CreateText(
                            tank.Name,
                            "debug",
                            textSurface.ScriptForegroundColor,
                            1f,
                            TextAlignment.LEFT);

                        sprite.Position = new Vector2(
                            LEFT_OFFSET + 50f,
                            TOP_OFFSET + smallScreenOffset + i * 85f);

                        frame.Add(sprite);

                        //ICON                  
                        sprite = new MySprite(
                            SpriteType.TEXTURE,
                            "IconHydrogen",
                            size: new Vector2(30f, 30f),
                            color: textSurface.ScriptBackgroundColor)
                        {
                            Position = new Vector2(30f, TOP_OFFSET + 50f + smallScreenOffset + i * 85f)
                        };

                        frame.Add(sprite);

                        //BAR BACKGROUND
                        sprite = new MySprite(
                            SpriteType.TEXTURE,
                            "SquareSimple",
                            size: new Vector2(barSize, 30f),
                            color: new Color(10, 10, 10, 200),
                            alignment: TextAlignment.LEFT)
                        {
                            Position = new Vector2(LEFT_OFFSET, TOP_OFFSET + smallScreenOffset + 50f + i * 85f)
                        };

                        frame.Add(sprite);

                        //BAR FOREGROUND
                        sprite = new MySprite(
                            SpriteType.TEXTURE,
                            "SquareSimple",
                            size: new Vector2(Convert.ToSingle(tank.FilledRatio) * barSize, 30f),
                            color: textSurface.ScriptBackgroundColor,
                            alignment: TextAlignment.LEFT)
                        {
                            Position = new Vector2(LEFT_OFFSET, TOP_OFFSET + smallScreenOffset + 50f + i * 85f)
                        };

                        frame.Add(sprite);

                        //BAR SEPARATORS
                        sprite = new MySprite(
                            SpriteType.TEXTURE,
                            "SquareSimple",
                            size: new Vector2(3f, 30f),
                            color: new Color(0, 0, 0, 255),
                            alignment: TextAlignment.LEFT)
                        {
                            Position = new Vector2(
                                LEFT_OFFSET + barSize / 4,
                                TOP_OFFSET + smallScreenOffset + 50f + i * 85f)
                        };

                        frame.Add(sprite);

                        sprite = new MySprite(
                            SpriteType.TEXTURE,
                            "SquareSimple",
                            size: new Vector2(3f, 30f),
                            color: new Color(0, 0, 0, 255),
                            alignment: TextAlignment.LEFT)
                        {
                            Position = new Vector2(LEFT_OFFSET + barSize / 4 * 2, TOP_OFFSET + 50f + i * 85f)
                        };

                        frame.Add(sprite);

                        sprite = new MySprite(
                            SpriteType.TEXTURE,
                            "SquareSimple",
                            size: new Vector2(3f, 30f),
                            color: new Color(0, 0, 0, 255),
                            alignment: TextAlignment.LEFT)
                        {
                            Position = new Vector2(LEFT_OFFSET + barSize / 4 * 3,
                                TOP_OFFSET + smallScreenOffset + 50f + i * 85f)
                        };

                        frame.Add(sprite);

                        //BAR END
                        sprite = new MySprite(
                            SpriteType.TEXTURE,
                            "Triangle",
                            size: new Vector2(38f, 80f),
                            color: new Color(0, 0, 0, 255),
                            alignment: TextAlignment.LEFT)
                        {
                            Position = new Vector2(LEFT_OFFSET + 6, TOP_OFFSET + smallScreenOffset + 41f + i * 85f),
                            RotationOrScale = 1.3f
                        };

                        frame.Add(sprite);

                        //FILL RATIO
                        sprite = MySprite.CreateText(
                            (tank.FilledRatio * 100).ToString("F2") + "%",
                            "debug",
                            textSurface.ScriptForegroundColor,
                            .75f,
                            TextAlignment.RIGHT);

                        sprite.Position = new Vector2(
                            LEFT_OFFSET + barSize,
                            TOP_OFFSET + smallScreenOffset + 7f + i * 85f);

                        frame.Add(sprite);

                        i++;
                    }
                }
            });
        }
    }
}
