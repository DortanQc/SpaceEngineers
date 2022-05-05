using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IngameScript
{
    public static class DisplayManager
    {
        public static void Display(
            MonitoringData monitoringData,
            IEnumerable<Item> itemsWithThreshold,
            List<IMyTextPanel> lcdBlocks)
        {
            DisplayItemInventory(monitoringData, itemsWithThreshold.ToList(), lcdBlocks);
            DisplayStorageCapacity(monitoringData, lcdBlocks);
            DisplayPowerUsage(monitoringData, lcdBlocks);
            DisplayProduction(monitoringData, lcdBlocks);
        }

        private static void DisplayProduction(MonitoringData monitoringData, IEnumerable<IMyTextPanel> lcdBlocks)
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
                        ? $"{inProd.Amount / inProd.Volume} {inProd.Name}"
                        : $"{inProd.Amount} {inProd.Name}");
                });

            lcdBlocks
                .Where(ForProductionDisplay)
                .ToList()
                .ForEach(block => block.WriteText(textBuilder));
        }

        private static void DisplayItemInventory(
            MonitoringData monitoringData,
            IReadOnlyCollection<Item> itemsWithThreshold,
            List<IMyTextPanel> lcdBlocks)
        {
            lcdBlocks.ForEach(block =>
            {
                var textBuilder = new StringBuilder();
                var customData = new CustomDataManager(block.CustomData);
                var displayAll = customData.GetPropertyValue("display-all-inventory") != null;
                var displayComponents = customData.GetPropertyValue("display-all-component") != null;
                var displayOres = customData.GetPropertyValue("display-all-ore") != null;
                var displayIngots = customData.GetPropertyValue("display-all-ingot") != null;
                var displayTools = customData.GetPropertyValue("display-all-tool") != null;
                var displayAmmunition = customData.GetPropertyValue("display-all-ammunition") != null;
                var hideWhenMetThreshold = customData.GetPropertyValue("hide-when-meets-threshold") != null;

                var hasUnknowns = false;
                var hasComponents = false;
                var hasOres = false;
                var hasIngots = false;
                var hasTools = false;
                var hasAmmunition = false;
                var hasSomethingToDisplay = false;

                var itemsToDisplay = BuildItemsToDisplay(monitoringData.GetItems(), itemsWithThreshold);

                itemsToDisplay.ForEach(item =>
                {
                    if (item.ItemType == Item.ItemTypes.Unknown)
                        if (displayAll ||
                            displayComponents ||
                            displayOres ||
                            displayIngots ||
                            displayTools ||
                            displayAmmunition)
                        {
                            if (!hasUnknowns)
                            {
                                if (hasSomethingToDisplay) textBuilder.AppendLine();

                                textBuilder.AppendLine("** Unknown Items **");
                                hasUnknowns = true;
                            }

                            hasSomethingToDisplay = true;
                            textBuilder.AppendLine($"{item.Amount} {item.Name}");
                        }

                    var show = customData.GetPropertyValue($"hide-{item.ItemSubType}") == null;

                    if (show == false)
                        return;

                    var itemMetThreshold = IsItemMetThreshold(
                        item,
                        itemsWithThreshold,
                        monitoringData);

                    var itemHasThresholdDefined = HasItemThresholdDefined(item, itemsWithThreshold);

                    if (hideWhenMetThreshold && itemHasThresholdDefined && itemMetThreshold)
                        return;

                    switch (item.ItemType)
                    {
                        case Item.ItemTypes.Component:
                            if (displayAll || displayComponents)
                            {
                                if (!hasComponents)
                                {
                                    if (hasSomethingToDisplay) textBuilder.AppendLine();

                                    textBuilder.AppendLine("** Owned Components **");
                                    hasComponents = true;
                                }

                                hasSomethingToDisplay = true;
                                textBuilder.AppendLine($"{item.Amount} {item.Name}");
                            }

                            break;
                        case Item.ItemTypes.Ore:
                            if (displayAll || displayOres)
                            {
                                if (!hasOres)
                                {
                                    if (hasSomethingToDisplay) textBuilder.AppendLine();

                                    textBuilder.AppendLine("** Owned Ores **");
                                    hasOres = true;
                                }

                                hasSomethingToDisplay = true;
                                textBuilder.AppendLine($"{item.Amount} {item.Name}");
                            }

                            break;
                        case Item.ItemTypes.Ingot:
                            if (displayAll || displayIngots)
                            {
                                if (!hasIngots)
                                {
                                    if (hasSomethingToDisplay) textBuilder.AppendLine();

                                    textBuilder.AppendLine("** Owned Ingots **");
                                    hasIngots = true;
                                }

                                hasSomethingToDisplay = true;
                                textBuilder.AppendLine($"{item.Amount} {item.Name}");
                            }

                            break;
                        case Item.ItemTypes.Tools:
                            if (displayAll || displayTools)
                            {
                                if (!hasTools)
                                {
                                    if (hasSomethingToDisplay) textBuilder.AppendLine();

                                    textBuilder.AppendLine("** Owned Tools **");
                                    hasTools = true;
                                }

                                hasSomethingToDisplay = true;
                                textBuilder.AppendLine($"{item.Amount} {item.Name}");
                            }

                            break;
                        case Item.ItemTypes.Ammunition:
                            if (displayAll || displayAmmunition)
                            {
                                if (!hasAmmunition)
                                {
                                    if (hasSomethingToDisplay) textBuilder.AppendLine();

                                    textBuilder.AppendLine("** Owned Ammunition **");
                                    hasAmmunition = true;
                                }

                                hasSomethingToDisplay = true;
                                textBuilder.AppendLine($"{item.Amount} {item.Name}");
                            }

                            break;
                    }
                });

                block.WriteText(textBuilder);
            });
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

            return itemsToDisplay
                .OrderBy(item => item.ItemType)
                .ThenBy(item => item.Name)
                .ToList();
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

        private static void DisplayStorageCapacity(MonitoringData monitoringData, IEnumerable<IMyTextPanel> lcdBlocks)
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

            lcdBlocks
                .Where(ForCargoCapacityStatisticsDisplay)
                .ToList()
                .ForEach(block => block.WriteText(textBuilder));
        }

        private static void DisplayPowerUsage(MonitoringData monitoringData, IEnumerable<IMyTextPanel> lcdBlocks)
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

            lcdBlocks
                .Where(ForPowerStatisticsDisplay)
                .ToList()
                .ForEach(block => block.WriteText(textBuilder));
        }

        private static bool ForCargoCapacityStatisticsDisplay(IMyTextPanel block)
        {
            var customData = new CustomDataManager(block.CustomData);

            return customData.GetPropertyValue("storage-capacity-statistics") != null;
        }

        private static bool ForProductionDisplay(IMyTextPanel block)
        {
            var customData = new CustomDataManager(block.CustomData);

            return customData.GetPropertyValue("in-production-statistics") != null;
        }

        private static bool ForPowerStatisticsDisplay(IMyTextPanel block)
        {
            var customData = new CustomDataManager(block.CustomData);

            return customData.GetPropertyValue("power-usage-statistics") != null;
        }
    }
}
