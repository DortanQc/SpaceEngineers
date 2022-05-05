using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IngameScript
{
    public static class DisplayManager
    {
        public static void Display(MonitoringData monitoringData, List<IMyTextPanel> lcdBlocks)
        {
            DisplayStorageCapacity(monitoringData, lcdBlocks);
            DisplayPowerUsage(monitoringData, lcdBlocks);
        }

        private static void DisplayStorageCapacity(MonitoringData monitoringData, IEnumerable<IMyTextPanel> lcdBlocks)
        {
            var textBuilder = new StringBuilder();

            var remainingCapacity = monitoringData.MaxVolume - monitoringData.CurrentVolume;

            textBuilder
                .AppendLine("** Item's Storage Capacity Statistics **")
                .AppendLine()
                .AppendLine($"Max Volume Capacity: {monitoringData.MaxVolume} m^3")
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
                .AppendLine("** Power Statistics **")
                .AppendLine()
                .AppendLine($"Current Power Output: {monitoringData.CurrentPowerOutput} MW")
                .AppendLine($"Max Power Output: {monitoringData.MaxPowerOutput} MW")
                .AppendLine()
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

        private static bool ForPowerStatisticsDisplay(IMyTextPanel block)
        {
            var customData = new CustomDataManager(block.CustomData);

            return customData.GetPropertyValue("power-usage-statistics") != null;
        }
    }
}
