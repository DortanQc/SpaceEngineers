using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace MyGridAssistant
{
    public class DisplayManager
    {
        private static readonly Dictionary<string, TopMarginInfo> TopMarginDictionary =
            new Dictionary<string, TopMarginInfo>();

        private readonly IMyGridAssistantLogger _logger;

        public DisplayManager(IMyGridAssistantLogger logger)
        {
            _logger = logger;
        }

        public void Display(
            MonitoringData monitoringData,
            List<Item> itemsWithThreshold,
            List<IMyTerminalBlock> allBlocks)
        {
            DisplayItemInventory(monitoringData, itemsWithThreshold, allBlocks);
            DisplayStorageCapacity(monitoringData, allBlocks);
            DisplayElectricalUsage(monitoringData, allBlocks);
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
                IConfiguration configuration = new Configuration(block);
                var customData = configuration.GetConfig(customDataKeyToLookup);

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
                    Configuration = configuration
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

            var textSurfaceBlock = GetDisplayBlocks(blocks, Settings.STATS_IN_PRODUCTION);

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

            itemsToDisplay
                .Where(i => itemTypes.Contains(i.ItemType))
                .OrderBy(i => i.ItemType)
                .ThenByDescending(i => i.Amount)
                .ToList()
                .ForEach(item =>
                {
                    if (ShouldHideItem(block.Configuration, item))
                        return;

                    if (ShouldHideBecauseMetThreshold(item, itemsWithThreshold, monitoringData, block.Configuration))
                        return;

                    itemTypes
                        .Where(i => i == item.ItemType)
                        .OrderBy(i => i)
                        .ToList()
                        .ForEach(type =>
                        {
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
            IConfiguration configuration)
        {
            var hideWhenMetThreshold =
                configuration.GetConfig(Settings.EXCLUDE_FROM_STATS_INVENTORY_WHEN_OVER_THRESHOLD) !=
                null;

            var itemMetThreshold = IsItemMetThreshold(
                item,
                itemsWithThreshold,
                monitoringData);

            var itemHasThresholdDefined = HasItemThresholdDefined(item, itemsWithThreshold);

            return hideWhenMetThreshold && itemHasThresholdDefined && itemMetThreshold;
        }

        private static bool ShouldHideItem(IConfiguration customData, Item item)
        {
            var show = customData.GetConfig($"{Settings.EXCLUDE_ITEM_FROM_STATS_INVENTORY}-{item.ItemSubType}") == null;

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

            var displayAllSurfaceBlocks = GetDisplayBlocks(blocks, Settings.STATS_INVENTORY);
            var displayComponentSurfaceBlocks = GetDisplayBlocks(blocks, Settings.STATS_INVENTORY_COMPONENTS);
            var displayOreSurfaceBlocks = GetDisplayBlocks(blocks, Settings.STATS_INVENTORY_ORES);
            var displayIngotSurfaceBlocks = GetDisplayBlocks(blocks, Settings.STATS_INVENTORY_INGOTS);
            var displayToolsSurfaceBlocks = GetDisplayBlocks(blocks, Settings.STATS_INVENTORY_TOOLS);
            var displayAmmunitionSurfaceBlocks = GetDisplayBlocks(blocks, Settings.STATS_INVENTORY_AMMUNITION);

            var surfaceByTypes = Combine(
                displayAllSurfaceBlocks,
                displayComponentSurfaceBlocks,
                displayOreSurfaceBlocks,
                displayIngotSurfaceBlocks,
                displayToolsSurfaceBlocks,
                displayAmmunitionSurfaceBlocks);

            surfaceByTypes.ForEach(surfaceByType =>
            {
                DisplayInventoryForTypes(
                    surfaceByType.Surface,
                    itemsToDisplay,
                    itemsWithThreshold,
                    monitoringData,
                    surfaceByType.ItemTypes);
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
            var allSurfaces = allSurfaceBlocks.Select(surface => new SurfaceByType
                {
                    Surface = surface,
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
                .Concat(componentSurfaceBlocks.Select(surface => new SurfaceByType
                {
                    Surface = surface,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Component,
                        Item.ItemTypes.Unknown
                    }
                }))
                .Concat(oreSurfaceBlocks.Select(surface => new SurfaceByType
                {
                    Surface = surface,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Ore,
                        Item.ItemTypes.Unknown
                    }
                }))
                .Concat(ingotSurfaceBlocks.Select(surface => new SurfaceByType
                {
                    Surface = surface,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Ingot,
                        Item.ItemTypes.Unknown
                    }
                }))
                .Concat(toolsSurfaceBlocks.Select(surface => new SurfaceByType
                {
                    Surface = surface,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Tools,
                        Item.ItemTypes.Unknown
                    }
                }))
                .Concat(ammunitionSurfaceBlocks.Select(surface => new SurfaceByType
                {
                    Surface = surface,
                    ItemTypes = new[]
                    {
                        Item.ItemTypes.Ammunition,
                        Item.ItemTypes.Unknown
                    }
                }))
                .ToList();

            var dictionary = new Dictionary<long, SurfaceByType>();

            foreach (var surfaceByType in allSurfaces)
                if (dictionary.ContainsKey(surfaceByType.Surface.BlockId))
                {
                    var actualTypes = dictionary[surfaceByType.Surface.BlockId].ItemTypes.ToList();
                    var typesToAdd = surfaceByType.ItemTypes.ToList();

                    var newItemTypes = new List<Item.ItemTypes>(actualTypes);

                    foreach (var itemType in typesToAdd.Where(itemType => !newItemTypes.Exists(newItemType => newItemType == itemType)))
                        newItemTypes.Add(itemType);

                    dictionary[surfaceByType.Surface.BlockId].ItemTypes = newItemTypes;
                }
                else
                {
                    var typesToAdd = surfaceByType.ItemTypes.ToList();

                    dictionary.Add(
                        surfaceByType.Surface.BlockId,
                        new SurfaceByType
                        {
                            Surface = surfaceByType.Surface,
                            ItemTypes = typesToAdd
                        });
                }

            return dictionary.Select(d => new SurfaceByType
                {
                    Surface = allSurfaces.First(b => b.Surface.BlockId == d.Key).Surface,
                    ItemTypes = d.Value.ItemTypes
                })
                .ToList();
        }

        private static List<Item> BuildItemsToDisplay(
            ICollection<Item> itemsInInventory,
            IEnumerable<Item> itemsWithThreshold)
        {
            var result = new List<Item>(itemsInInventory);

            foreach (var itemWithThreshold in itemsWithThreshold)
            {
                var isItemWithThresholdFound = itemsInInventory.Any(itemToDisplay => itemToDisplay.IsSameAs(itemWithThreshold));

                if (!isItemWithThresholdFound)
                    itemsInInventory.Add(new Item(itemWithThreshold.ItemDefinitionId, 0));
            }

            return result.OrderBy(item => item.ItemType).ThenBy(item => item.Name).ToList();
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

            var textSurfaceBlock = GetDisplayBlocks(blocks, Settings.STATS_STORAGE_CAPACITY);

            textSurfaceBlock.ForEach(block => block.TextSurface.WriteText(textBuilder));
        }

        private void DisplayElectricalUsage(MonitoringData monitoringData, IEnumerable<IMyTerminalBlock> blocks)
        {
            const float LEFT_MARGIN = 10f;
            const float GAP_BETWEEN_SECTIONS = 20f;

            var textSurfaceBlock = GetDisplayBlocks(blocks, Settings.STATS_POWER_USAGE);

            textSurfaceBlock.ForEach(surface =>
            {
                var withDetails = surface.Configuration.GetConfig(Settings.HIDE_POWER_USAGE_DETAILS) != null;
                var topMargin = GetTopMargin(surface.BlockId, surface.TextSurface.Name);
                var keepRatio = surface.Configuration.GetConfig(Settings.LCD_WIDTH_RATIO) != null;

                var textSurface = surface.TextSurface;
                var engin = new GraphicEngine(textSurface, keepRatio, _logger);
                var fullBarSize = textSurface.SurfaceSize.X - LEFT_MARGIN * 2;
                var mediumBarSize = textSurface.SurfaceSize.X - 20f - LEFT_MARGIN * 2;
                var smallBarSize = textSurface.SurfaceSize.X - 30f - LEFT_MARGIN * 2;

                engin.BackgroundColor = new Color(0, 0, 0, 255);

                var currentYPos = engin.AddProgressBar(
                    "Electrical Consumption",
                    fullBarSize,
                    30f,
                    1f,
                    LEFT_MARGIN,
                    topMargin,
                    monitoringData.PowerConsumption.CurrentPowerOutput / monitoringData.PowerConsumption.MaxPowerOutput
                );

                currentYPos += 10f;

                if (monitoringData.PowerConsumption.WindTurbines.Any())
                {
                    var currentOutput = monitoringData.PowerConsumption.WindTurbines.Sum(t => t.CurrentOutput);
                    var currentTotalOutput = monitoringData.PowerConsumption.CurrentPowerOutput;
                    double fillRatio = currentTotalOutput == 0
                        ? 0
                        : currentOutput / currentTotalOutput;

                    currentYPos = engin.AddProgressBar(
                        $"[{monitoringData.PowerConsumption.WindTurbines.Count}] Wind Turbines",
                        mediumBarSize,
                        22f,
                        .75f,
                        LEFT_MARGIN + 20f,
                        currentYPos + GAP_BETWEEN_SECTIONS,
                        fillRatio
                    );

                    currentYPos += 5f;

                    var currentTotalTurbineOutput = monitoringData.PowerConsumption.WindTurbines.Sum(t => t.CurrentOutput);

                    if (withDetails)
                        monitoringData.PowerConsumption.WindTurbines
                            .OrderBy(x => x.Name)
                            .ToList()
                            .ForEach(turbine =>
                            {
                                double fill = currentTotalTurbineOutput == 0
                                    ? 0
                                    : turbine.CurrentOutput / currentTotalTurbineOutput;

                                currentYPos = engin.AddProgressBar(
                                    turbine.Name,
                                    smallBarSize,
                                    5f,
                                    .50f,
                                    LEFT_MARGIN + 30f,
                                    currentYPos + 1f,
                                    fill
                                );
                            });
                }

                if (monitoringData.PowerConsumption.SolarPanels.Any())
                {
                    var currentOutput = monitoringData.PowerConsumption.SolarPanels.Sum(t => t.CurrentOutput);
                    var currentTotalOutput = monitoringData.PowerConsumption.CurrentPowerOutput;
                    double fillRatio = currentTotalOutput == 0
                        ? 0
                        : currentOutput / currentTotalOutput;

                    currentYPos = engin.AddProgressBar(
                        $"[{monitoringData.PowerConsumption.SolarPanels.Count}] Solar Panels",
                        mediumBarSize,
                        22f,
                        .75f,
                        LEFT_MARGIN + 20f,
                        currentYPos + GAP_BETWEEN_SECTIONS,
                        fillRatio
                    );

                    currentYPos += 5f;

                    var currentTotalSolarPanelsOutput =
                        monitoringData.PowerConsumption.SolarPanels.Sum(t => t.CurrentOutput);

                    if (withDetails)
                        monitoringData.PowerConsumption.SolarPanels
                            .OrderBy(x => x.Name)
                            .ToList()
                            .ForEach(solarPanel =>
                            {
                                double fill = currentTotalSolarPanelsOutput == 0
                                    ? 0
                                    : solarPanel.CurrentOutput / currentTotalSolarPanelsOutput;

                                currentYPos = engin.AddProgressBar(
                                    solarPanel.Name,
                                    smallBarSize,
                                    5f,
                                    .50f,
                                    LEFT_MARGIN + 30f,
                                    currentYPos + 1f,
                                    fill
                                );
                            });
                }

                if (monitoringData.PowerConsumption.Batteries.Any())
                {
                    var currentOutput = monitoringData.PowerConsumption.Batteries.Sum(t => t.CurrentOutput);
                    var currentTotalOutput = monitoringData.PowerConsumption.CurrentPowerOutput;
                    double fillRatio = currentTotalOutput == 0
                        ? 0
                        : currentOutput / currentTotalOutput;

                    currentYPos = engin.AddProgressBar(
                        $"[{monitoringData.PowerConsumption.Batteries.Count}] Batteries",
                        mediumBarSize,
                        22f,
                        .75f,
                        LEFT_MARGIN + 20f,
                        currentYPos + GAP_BETWEEN_SECTIONS,
                        fillRatio
                    );

                    currentYPos += 5f;

                    var currentTotalBatteriesOutput = monitoringData.PowerConsumption.Batteries.Sum(t => t.CurrentOutput);

                    if (withDetails)
                        monitoringData.PowerConsumption.Batteries
                            .OrderBy(x => x.Name)
                            .ToList()
                            .ForEach(battery =>
                            {
                                double fill = currentTotalBatteriesOutput == 0
                                    ? 0
                                    : battery.CurrentOutput / currentTotalBatteriesOutput;

                                currentYPos = engin.AddProgressBar(
                                    battery.Name,
                                    smallBarSize,
                                    5f,
                                    .50f,
                                    LEFT_MARGIN + 30f,
                                    currentYPos + 1f,
                                    fill
                                );
                            });

                    currentYPos += 10f;

                    engin.AddSprite(
                        "IconEnergy",
                        15,
                        15,
                        textSurface.ScriptBackgroundColor,
                        LEFT_MARGIN + 10f,
                        currentYPos,
                        TextAlignment.LEFT,
                        0f,
                        false
                    );

                    currentYPos = engin.AddText(
                        "Stored Power",
                        .75f,
                        LEFT_MARGIN + 30f,
                        currentYPos,
                        TextAlignment.LEFT,
                        textSurface.ScriptForegroundColor
                    );

                    currentYPos += 5f;

                    monitoringData.PowerConsumption.Batteries
                        .OrderBy(x => x.Name)
                        .ToList()
                        .ForEach(battery =>
                        {
                            double fill = battery.MaxStoredPower == 0
                                ? 0
                                : battery.CurrentStoredPower / battery.MaxStoredPower;

                            currentYPos = engin.AddProgressBar(
                                battery.Name,
                                smallBarSize,
                                5f,
                                .50f,
                                LEFT_MARGIN + 30f,
                                currentYPos + 1f,
                                fill,
                                battery.IsCharging
                                    ? Color.Green
                                    : Color.Red
                            );
                        });

                    monitoringData.ForeignPowerConsumption.Batteries
                        .OrderBy(x => x.Name)
                        .ToList()
                        .ForEach(battery =>
                        {
                            double fill = battery.MaxStoredPower == 0
                                ? 0
                                : battery.CurrentStoredPower / battery.MaxStoredPower;

                            if (fill < .99f)
                                currentYPos = engin.AddProgressBar(
                                    battery.Name,
                                    smallBarSize,
                                    5f,
                                    .50f,
                                    LEFT_MARGIN + 30f,
                                    currentYPos + 1f,
                                    fill,
                                    battery.IsCharging
                                        ? Color.DarkGreen
                                        : Color.DarkRed
                                );
                        });
                }

                if (monitoringData.PowerConsumption.Reactors.Any())
                {
                    var currentOutput = monitoringData.PowerConsumption.Reactors.Sum(t => t.CurrentOutput);
                    var currentTotalOutput = monitoringData.PowerConsumption.CurrentPowerOutput;
                    double fillRatio = currentTotalOutput == 0
                        ? 0
                        : currentOutput / currentTotalOutput;

                    currentYPos = engin.AddProgressBar(
                        $"[{monitoringData.PowerConsumption.Reactors.Count}] Reactors",
                        mediumBarSize,
                        22f,
                        .75f,
                        LEFT_MARGIN + 20f,
                        currentYPos + GAP_BETWEEN_SECTIONS,
                        fillRatio
                    );

                    currentYPos += 5f;

                    engin.AddText(
                        $"Available Uranium Ingots: {monitoringData.PowerConsumption.AvailableUranium.ToString()}",
                        .75f,
                        LEFT_MARGIN + 50f,
                        currentYPos,
                        TextAlignment.LEFT,
                        textSurface.ScriptForegroundColor
                    );

                    currentYPos = engin.AddSprite(
                        "MyObjectBuilder_Ingot/Uranium",
                        15,
                        15,
                        textSurface.ScriptBackgroundColor,
                        LEFT_MARGIN + 30f,
                        currentYPos,
                        TextAlignment.LEFT,
                        0f,
                        false
                    );

                    currentYPos += 10f;

                    var currentTotalReactorsOutput = monitoringData.PowerConsumption.Reactors.Sum(t => t.CurrentOutput);

                    if (withDetails)
                        monitoringData.PowerConsumption.Reactors
                            .OrderBy(x => x.Name)
                            .ToList()
                            .ForEach(reactor =>
                            {
                                double fill = currentTotalReactorsOutput == 0
                                    ? 0
                                    : reactor.CurrentOutput / currentTotalReactorsOutput;

                                currentYPos = engin.AddProgressBar(
                                    reactor.Name,
                                    smallBarSize,
                                    5f,
                                    .50f,
                                    LEFT_MARGIN + 30f,
                                    currentYPos + 1f,
                                    fill
                                );
                            });
                }

                if (monitoringData.PowerConsumption.HydrogenEngines.Any())
                {
                    var currentOutput = monitoringData.PowerConsumption.HydrogenEngines.Sum(t => t.CurrentOutput);
                    var currentTotalOutput = monitoringData.PowerConsumption.CurrentPowerOutput;
                    double fillRatio = currentTotalOutput == 0
                        ? 0
                        : currentOutput / currentTotalOutput;

                    currentYPos = engin.AddProgressBar(
                        $"[{monitoringData.PowerConsumption.HydrogenEngines.Count}] - Hydrogen Engines",
                        mediumBarSize,
                        22f,
                        .75f,
                        LEFT_MARGIN + 20f,
                        currentYPos + GAP_BETWEEN_SECTIONS,
                        fillRatio
                    );

                    currentYPos += 5f;

                    engin.AddText(
                        $"Available Ice Ore: {monitoringData.PowerConsumption.AvailableIce.ToString()}",
                        .75f,
                        LEFT_MARGIN + 50f,
                        currentYPos,
                        TextAlignment.LEFT,
                        textSurface.ScriptForegroundColor
                    );

                    currentYPos = engin.AddSprite(
                        "MyObjectBuilder_Ore/Ice",
                        15,
                        15,
                        textSurface.ScriptBackgroundColor,
                        LEFT_MARGIN + 30f,
                        currentYPos,
                        TextAlignment.LEFT,
                        0f,
                        false
                    );

                    var hydroFillRatio = monitoringData.HydrogenTanks.Sum(h => h.FilledRatio);
                    var availableTank = monitoringData.HydrogenTanks.Count;

                    var ratio = availableTank == 0
                        ? 0
                        : hydroFillRatio / availableTank;

                    engin.AddText(
                        $"Available Hydrogen in tanks: {ratio * 100:F2} %",
                        .75f,
                        LEFT_MARGIN + 50f,
                        currentYPos + 1f,
                        TextAlignment.LEFT,
                        textSurface.ScriptForegroundColor
                    );

                    currentYPos = engin.AddSprite(
                        "IconHydrogen",
                        15,
                        15,
                        textSurface.ScriptBackgroundColor,
                        LEFT_MARGIN + 30f,
                        currentYPos + 1f,
                        TextAlignment.LEFT,
                        0f,
                        false
                    );

                    currentYPos += 10f;

                    var currentTotalEngineOutput = monitoringData.PowerConsumption.HydrogenEngines.Sum(t => t.CurrentOutput);

                    if (withDetails)
                        monitoringData.PowerConsumption.HydrogenEngines
                            .OrderBy(x => x.Name)
                            .ToList()
                            .ForEach(engine =>
                            {
                                double fill = currentTotalEngineOutput == 0
                                    ? 0
                                    : engine.CurrentOutput / currentTotalEngineOutput;

                                currentYPos = engin.AddProgressBar(
                                    engine.Name,
                                    smallBarSize,
                                    5f,
                                    .50f,
                                    LEFT_MARGIN + 30f,
                                    currentYPos + 1f,
                                    fill
                                );
                            });
                }

                SetTopMargin(surface.BlockId, textSurface.Name, currentYPos, textSurface.SurfaceSize.Y);

                engin.Draw();
            });
        }

        private static void SetTopMargin(
            long surfaceBlockId,
            string surfaceName,
            float currentYPos,
            float surfaceHeight)
        {
            var key = $"{surfaceBlockId.ToString()}-{surfaceName}";
            var marginInfo = TopMarginDictionary[key];

            if (marginInfo.Direction == Directions.Forward)
            {
                if (currentYPos > surfaceHeight - 10f)
                    marginInfo.CurrentMargin -= 5f;
                else marginInfo.Direction = Directions.Backward;
            }
            else
            {
                if (marginInfo.CurrentMargin == 10f)
                    marginInfo.Direction = Directions.Forward;
                else marginInfo.CurrentMargin += 5f;
            }

            TopMarginDictionary[key] = marginInfo;
        }

        private static float GetTopMargin(long surfaceBlockId, string surfaceName)
        {
            var key = $"{surfaceBlockId.ToString()}-{surfaceName}";

            if (!TopMarginDictionary.ContainsKey(key))
                TopMarginDictionary.Add(
                    key,
                    new TopMarginInfo
                    {
                        Direction = Directions.Forward,
                        CurrentMargin = 10f
                    });

            return TopMarginDictionary[key].CurrentMargin;
        }

        private void DisplayHydrogenStatistics(
            MonitoringData monitoringData,
            IEnumerable<IMyTerminalBlock> blocks)
        {
            var textSurfaceBlock = GetDisplayBlocks(blocks, Settings.STATS_HYDROGEN_USAGE);

            textSurfaceBlock.ForEach(surface =>
            {
                var topMargin = GetTopMargin(surface.BlockId, surface.TextSurface.Name);

                const float GAP_BETWEEN_SECTIONS = 20f;
                const float LEFT_MARGIN = 10f;
                const float ICON_SIZE = 30f;
                const float BAR_LEFT_BEGIN = LEFT_MARGIN + ICON_SIZE;
                var isFirst = true;

                var ratio = surface.Configuration.GetConfig(Settings.LCD_WIDTH_RATIO) != null;

                var textSurface = surface.TextSurface;
                var engin = new GraphicEngine(textSurface, ratio, _logger);
                var barSize = textSurface.SurfaceSize.X - 90f;

                engin.BackgroundColor = new Color(0, 0, 0, 255);

                var currentYPos = topMargin;

                foreach (var tank in monitoringData.HydrogenTanks.OrderBy(x => x.Name))
                {
                    if (isFirst == false)
                        currentYPos += GAP_BETWEEN_SECTIONS;

                    var rowYPos = engin.AddProgressBar(
                        tank.Name,
                        barSize,
                        30f,
                        1f,
                        BAR_LEFT_BEGIN,
                        currentYPos,
                        tank.FilledRatio);

                    engin.AddSprite(
                        "IconHydrogen",
                        ICON_SIZE,
                        ICON_SIZE,
                        textSurface.ScriptBackgroundColor,
                        LEFT_MARGIN,
                        currentYPos + 25f,
                        TextAlignment.LEFT,
                        0f,
                        false
                    );

                    isFirst = false;
                    currentYPos = rowYPos;
                }

                SetTopMargin(surface.BlockId, textSurface.Name, currentYPos, textSurface.SurfaceSize.Y);

                engin.Draw();
            });
        }

        private enum Directions
        {
            Forward,
            Backward
        }

        private class TopMarginInfo
        {
            public Directions Direction { get; set; }

            public float CurrentMargin { get; set; }
        }
    }
}
