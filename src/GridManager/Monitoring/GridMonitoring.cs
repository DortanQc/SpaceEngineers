using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
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

        private void ScanHydrogen(IEnumerable<IMyGasTank> gasTanks)
        {
            gasTanks
                .Where(tank => tank.BlockDefinition.SubtypeName.ToUpper().Contains("HYDROGEN"))
                .ToList()
                .ForEach(tank =>
                {
                    MonitoringData.HydrogenTanks.Add(new HydrogenTankInfo
                    {
                        Name = tank.CustomName,
                        Capacity = tank.Capacity,
                        FilledRatio = tank.FilledRatio
                    });
                });

            _logger("");
            _logger("** Hydrogen **");
            _logger($"Current Hydrogen Capacity: {MonitoringData.HydrogenTanks.Sum(h => h.Capacity):0.##}");
            _logger(
                $"Current Hydrogen Filled Ratio: {MonitoringData.HydrogenTanks.Sum(h => h.FilledRatio) / MonitoringData.HydrogenTanks.Count:0.##} %");
        }

        private void ScanPower(List<IMyPowerProducer> powerBlocks)
        {
            var uraniumCount = MonitoringData.GetItems(Item.ItemTypes.Ingot)
                .Where(i => i.ItemSubType == Item.ItemSubTypes.Uranium)
                .Sum(i => i.Amount);

            var iceCount = MonitoringData.GetItems(Item.ItemTypes.Ore)
                .Where(i => i.ItemSubType == Item.ItemSubTypes.Ice)
                .Sum(i => i.Amount);

            powerBlocks.ForEach(block =>
            {
                var battery = block as IMyBatteryBlock;
                var solarPanel = block as IMySolarPanel;
                var reactor = block as IMyReactor;

                if (battery != null)
                    MonitoringData.PowerConsumption.Batteries.Add(new BatteryInfo
                    {
                        Name = battery.CustomName,
                        IsCharging = battery.IsCharging,
                        CurrentStoredPower = battery.CurrentStoredPower,
                        MaxStoredPower = battery.MaxStoredPower,
                        CurrentOutput = battery.CurrentOutput,
                        MaxOutput = battery.MaxOutput
                    });

                if (solarPanel != null)
                    MonitoringData.PowerConsumption.SolarPanels.Add(new SolarPanelInfo
                    {
                        Name = solarPanel.CustomName,
                        CurrentOutput = solarPanel.CurrentOutput,
                        MaxOutput = solarPanel.MaxOutput
                    });

                if (reactor != null)
                    MonitoringData.PowerConsumption.Reactors.Add(new ReactorInfo
                    {
                        Name = reactor.CustomName,
                        CurrentOutput = reactor.CurrentOutput,
                        MaxOutput = reactor.MaxOutput
                    });

                if (block.GetType().Name.IndexOf("MyHydrogenEngine", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    MonitoringData.PowerConsumption.HydrogenEngines.Add(new HydrogenEngineInfo
                    {
                        Name = block.CustomName,
                        CurrentOutput = block.CurrentOutput,
                        MaxOutput = block.MaxOutput,
                        FilledRatio = (block as IMyGasTank)?.FilledRatio ?? 0
                    });

                if (block.GetType().Name.IndexOf("MyWindTurbine", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    MonitoringData.PowerConsumption.WindTurbines.Add(new WindTurbineInfo
                    {
                        Name = block.CustomName,
                        CurrentOutput = block.CurrentOutput,
                        MaxOutput = block.MaxOutput
                    });

                MonitoringData.PowerConsumption.AvailableUranium = uraniumCount;
                MonitoringData.PowerConsumption.AvailableIce = iceCount;
            });

            _logger("");
            _logger("** Power **");
            _logger($"Battery Count: {MonitoringData.PowerConsumption.Batteries.Count}");
            _logger($"Solar Panel Count: {MonitoringData.PowerConsumption.SolarPanels.Count}");
            _logger($"Reactor Count: {MonitoringData.PowerConsumption.Reactors.Count}");
            _logger($"Hydrogen Engine Count: {MonitoringData.PowerConsumption.HydrogenEngines.Count}");
            _logger($"Wind Turret Count: {MonitoringData.PowerConsumption.WindTurbines.Count}");
            _logger("");
            _logger($"Current Power Output: {MonitoringData.PowerConsumption.CurrentPowerOutput} MW");
            _logger($"Max Power Output: {MonitoringData.PowerConsumption.MaxPowerOutput} MW");
            _logger("");
            _logger($"Available Uranium: {MonitoringData.PowerConsumption.AvailableUranium}");
            _logger($"Available Ice: {MonitoringData.PowerConsumption.AvailableIce}");
        }

        private void ScanStorageCapacity(IEnumerable<IMyTerminalBlock> storageBlocks)
        {
            storageBlocks
                .OfType<IMyCargoContainer>()
                .ToList()
                .ForEach(block =>
                {
                    var inventory = block.GetInventory();

                    MonitoringData.Cargos.Add(new CargoInfo
                    {
                        Name = block.CustomName,
                        MaxVolume = inventory.MaxVolume,
                        CurrentVolume = inventory.CurrentVolume
                    });
                });

            _logger("");
            _logger("** Items **");
            _logger($"Max Storage Capacity: {MonitoringData.Cargos.Sum(x => (float)x.MaxVolume)} m^3");
            _logger($"Current Storage Volume: {MonitoringData.Cargos.Sum(x => (float)x.CurrentVolume)} m^3");
        }

        private void ScanAllInventory(List<IMyTerminalBlock> allBlocks)
        {
            allBlocks.ForEach(block =>
            {
                if (block.InventoryCount == 0) return;

                Enumerable.Range(0, block.InventoryCount)
                    .ToList()
                    .ForEach(inventoryIndex =>
                    {
                        ScanInventory(block.GetInventory(inventoryIndex));
                    });
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

        public void UpdateData(
            List<IMyTerminalBlock> allBlocks,
            List<IMyPowerProducer> powerBlocks,
            List<IMyTerminalBlock> storageBlocks,
            List<IMyProductionBlock> productionBlocks,
            IEnumerable<IMyGasTank> gasTanks)
        {
            MonitoringData.Reset();

            ScanStorageCapacity(storageBlocks);
            ScanAllInventory(allBlocks);
            ScanPower(powerBlocks);
            ScanProduction(productionBlocks);
            ScanHydrogen(gasTanks);
        }
    }
}
