using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class GridMonitoring
    {
        private readonly Action<string> _logger;

        public GridMonitoring(Action<string> logAction)
        {
            _logger = logAction;
            MonitoringData = new MonitoringData();
        }

        public MonitoringData MonitoringData { get; }

        private void ScanHydrogen(IReadOnlyCollection<IMyGasTank> gasTanks)
        {
            gasTanks
                .Where(tank => tank.BlockDefinition.SubtypeName.ToUpper().Contains("HYDROGEN"))
                .ToList()
                .ForEach(tank =>
                {
                    MonitoringData.HydrogenCapacity += tank.Capacity;
                    MonitoringData.HydrogenFilledRatio += tank.FilledRatio * 100 / gasTanks.Count;
                });

            _logger("");
            _logger("** Hydrogen **");
            _logger($"Current Hydrogen Capacity: {MonitoringData.HydrogenCapacity:0.##}");
            _logger($"Current Hydrogen Filled Ratio: {MonitoringData.HydrogenFilledRatio:0.##} %");
        }

        private void ScanPower(List<IMyPowerProducer> powerBlocks)
        {
            powerBlocks.ForEach(block =>
            {
                MonitoringData.CurrentPowerOutput += block.CurrentOutput;
                MonitoringData.MaxPowerOutput += block.MaxOutput;
            });

            MonitoringData.TotalBatteries = powerBlocks
                .OfType<IMyBatteryBlock>()
                .Count();

            MonitoringData.ChargingBatteries = powerBlocks
                .OfType<IMyBatteryBlock>()
                .Count(block => block.CurrentInput > block.CurrentOutput);

            MonitoringData.DischargingBatteries = powerBlocks
                .OfType<IMyBatteryBlock>()
                .Count(block => block.CurrentInput < block.CurrentOutput);

            _logger("");
            _logger("** Power **");
            _logger($"Current Power Output: {MonitoringData.CurrentPowerOutput} MW");
            _logger($"Max Power Output: {MonitoringData.MaxPowerOutput} MW");
            _logger("");
            _logger($"Total Batteries: {MonitoringData.TotalBatteries}");
            if (MonitoringData.ChargingBatteries > 0)
                _logger($"Total Charging Batteries: {MonitoringData.ChargingBatteries}");

            if (MonitoringData.DischargingBatteries > 0)
                _logger($"Total Discharging Batteries: {MonitoringData.DischargingBatteries}");

            _logger("");
            var shortage = MonitoringData.MaxPowerOutput - MonitoringData.CurrentPowerOutput;
            _logger(shortage < 0
                ? $"Power Shortage of: {shortage} MW"
                : $"Power exceeding of: {shortage} MW");
        }

        private void ScanStorageCapacity(IEnumerable<IMyTerminalBlock> storageBlocks)
        {
            storageBlocks
                .OfType<IMyCargoContainer>()
                .ToList()
                .ForEach(block =>
                {
                    ScanStorageCapacity(block.GetInventory());
                });

            _logger("");
            _logger("** Items **");
            _logger($"Max Storage Capacity: {MonitoringData.MaxVolume} m^3");
            _logger($"Current Storage Volume: {MonitoringData.CurrentVolume} m^3");
            _logger($"Current Storage Mass: {MonitoringData.CurrentMass} Kg");
        }

        private void ScanAllInventory(List<IMyTerminalBlock> storageBlocks, List<IMyProductionBlock> productionBlocks)
        {
            storageBlocks.ForEach(block =>
            {
                ScanInventory(block.GetInventory());
            });

            productionBlocks.ForEach(block =>
            {
                ScanInventory(block.OutputInventory);
            });

            var components = MonitoringData.GetItems(Item.ItemTypes.Component);
            var ingots = MonitoringData.GetItems(Item.ItemTypes.Ingot);
            var ores = MonitoringData.GetItems(Item.ItemTypes.Ore);
            var tools = MonitoringData.GetItems(Item.ItemTypes.Tools);
            var unknowns = MonitoringData.GetItems(Item.ItemTypes.Unknown);

            if (components.Count > 0)
            {
                _logger("");
                _logger("** Components **");
                components.ForEach(component =>
                {
                    _logger($"{component.Name} {component.Amount}");
                });
            }

            if (ingots.Count > 0)
            {
                _logger("");
                _logger("** Ingots **");
                ingots.ForEach(ingot =>
                {
                    _logger($"{ingot.Name} {ingot.Amount}");
                });
            }

            if (ores.Count > 0)
            {
                _logger("");
                _logger("** Ores **");
                ores.ForEach(ore =>
                {
                    _logger($"{ore.Name} {ore.Amount}");
                });
            }

            if (tools.Count > 0)
            {
                _logger("");
                _logger("** Tools **");
                tools.ForEach(tool =>
                {
                    _logger($"{tool.Name} {tool.Amount}");
                });
            }

            if (unknowns.Count > 0)
            {
                _logger("");
                _logger("** Unknown Items **");
                unknowns.ForEach(unknown =>
                {
                    _logger($"{unknown.Name} {unknown.Amount}");
                });
            }
        }

        private void ScanProduction(List<IMyProductionBlock> productionBlocks)
        {
            productionBlocks.ForEach(block =>
            {
                var items = new List<MyProductionItem>();

                block.GetQueue(items);

                items.ForEach(item =>
                {
                    MonitoringData.AddToQueuedList(item);
                });
            });

            var components = MonitoringData.GetItemsInProduction(Item.ItemTypes.Component);
            var ingots = MonitoringData.GetItemsInProduction(Item.ItemTypes.Ingot);
            var unknowns = MonitoringData.GetItemsInProduction(Item.ItemTypes.Unknown);

            if (components.Count > 0)
            {
                _logger("");
                _logger("** Components Production **");
                components.ForEach(component =>
                {
                    _logger($"{component.Name} {component.Amount}");
                });
            }

            if (ingots.Count > 0)
            {
                _logger("");
                _logger("** Ingots Production **");
                ingots.ForEach(ingot =>
                {
                    _logger($"{ingot.Name} {ingot.Amount * ingot.Volume}");
                });
            }

            if (unknowns.Count > 0)
            {
                _logger("");
                _logger("** Unknown Items In Production **");
                unknowns.ForEach(unknown =>
                {
                    _logger($"{unknown.Name} {unknown.Amount}");
                });
            }
        }

        private void ScanInventory(IMyInventory inventory)
        {
            var items = new List<MyInventoryItem>();

            inventory.GetItems(items);

            items.ForEach(item =>
            {
                MonitoringData.AddToInventory(item);
            });
        }

        private void ScanStorageCapacity(IMyInventory inventory)
        {
            MonitoringData.MaxVolume += inventory.MaxVolume;
            MonitoringData.CurrentVolume += inventory.CurrentVolume;
            MonitoringData.CurrentMass += inventory.CurrentMass;
        }

        public void UpdateData(
            List<IMyPowerProducer> powerBlocks,
            List<IMyTerminalBlock> storageBlocks,
            List<IMyProductionBlock> productionBlocks,
            List<IMyGasTank> gasTanks)
        {
            MonitoringData.Reset();
            ScanStorageCapacity(storageBlocks);
            ScanAllInventory(storageBlocks, productionBlocks);
            ScanPower(powerBlocks);
            ScanProduction(productionBlocks);
            ScanHydrogen(gasTanks);
        }
    }
}
