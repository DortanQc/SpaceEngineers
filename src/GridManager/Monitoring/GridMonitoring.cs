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
        private readonly List<IMyPowerProducer> _powerBlocks;
        private readonly List<IMyProductionBlock> _productionBlocks;
        private readonly List<IMyTerminalBlock> _storageBlocks;

        public GridMonitoring(
            Action<string> logAction,
            List<IMyPowerProducer> powerBlocks,
            List<IMyTerminalBlock> storageBlocks,
            List<IMyProductionBlock> productionBlocks)
        {
            _logger = logAction;
            _powerBlocks = powerBlocks;
            _storageBlocks = storageBlocks;
            _productionBlocks = productionBlocks;
            MonitoringData = new MonitoringData();

            ScanAllInventory();
            ScanPower();
            ScanProduction();
        }

        public MonitoringData MonitoringData { get; }

        private void ScanPower()
        {
            _powerBlocks.ForEach(block =>
            {
                MonitoringData.CurrentPowerOutput += block.CurrentOutput;
                MonitoringData.MaxPowerOutput += block.MaxOutput;
            });

            MonitoringData.TotalBatteries = _powerBlocks
                .OfType<IMyBatteryBlock>()
                .Count();

            MonitoringData.ChargingBatteries = _powerBlocks
                .OfType<IMyBatteryBlock>()
                .Count(block => block.CurrentInput > block.CurrentOutput);

            MonitoringData.DischargingBatteries = _powerBlocks
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

        private void ScanInventory(IMyInventory inventory)
        {
            var items = new List<MyInventoryItem>();

            MonitoringData.MaxVolume += inventory.MaxVolume;
            MonitoringData.CurrentVolume += inventory.CurrentVolume;
            MonitoringData.CurrentMass += inventory.CurrentMass;

            inventory.GetItems(items);

            items.ForEach(item =>
            {
                MonitoringData.AddToInventory(item);
            });
        }

        private void ScanAllInventory()
        {
            _storageBlocks.ForEach(block =>
            {
                ScanInventory(block.GetInventory());
            });

            _productionBlocks.ForEach(block =>
            {
                ScanInventory(block.OutputInventory);
            });

            _logger("");
            _logger("** Items **");
            _logger($"Max Item Volume Capacity: {MonitoringData.MaxVolume} m^3");
            _logger($"Current Item Volume: {MonitoringData.CurrentVolume} m^3");
            _logger($"Current Item Mass: {MonitoringData.CurrentMass} Kg");

            var components = MonitoringData.GetItems(Item.ItemTypes.Component);
            var ingots = MonitoringData.GetItems(Item.ItemTypes.Ingot);
            var ores = MonitoringData.GetItems(Item.ItemTypes.Ore);
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

        private void ScanProduction()
        {
            _productionBlocks.ForEach(block =>
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
    }
}
