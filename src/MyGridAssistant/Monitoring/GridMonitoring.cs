using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace MyGridAssistant
{
    public class GridMonitoring
    {
        private readonly IMyGridAssistantLogger _logger;

        public GridMonitoring(IMyGridAssistantLogger logger)
        {
            _logger = logger;
            MonitoringData = new MonitoringData(_logger);
        }

        public MonitoringData MonitoringData { get; }

        public void UpdateData(
            IEnumerable<BlockEntity<IMyPowerProducer>> powerBlocks,
            IEnumerable<BlockEntity<IMyPowerProducer>> foreignPowerBlocks,
            List<BlockEntity<IMyTerminalBlock>> withStorageBlocks,
            IEnumerable<BlockEntity<IMyProductionBlock>> productionBlocks,
            IEnumerable<BlockEntity<IMyGasTank>> gasTanks)
        {
            MonitoringData.Reset();

            ScanStorageCapacity(withStorageBlocks);
            ScanAllInventory(withStorageBlocks);
            ScanPower(powerBlocks);
            ScanForeignPower(foreignPowerBlocks);
            ScanProduction(productionBlocks);
            ScanHydrogen(gasTanks);
        }

        private void ScanHydrogen(IEnumerable<BlockEntity<IMyGasTank>> gasTanks)
        {
            MonitoringData.HydrogenTanks.AddRange(gasTanks
                .Where(block => block.Exists())
                .Select(block => block.Block)
                .Where(tank => tank.BlockDefinition.SubtypeName.ToUpper().Contains("HYDROGEN"))
                .Select(tank =>
                    new HydrogenTankInfo
                    {
                        Name = tank.CustomName,
                        Capacity = tank.Capacity,
                        FilledRatio = tank.FilledRatio
                    }
                ));

            _logger.LogInfo("GridMonitoring.ScanHydrogen_1", "");
            _logger.LogInfo("GridMonitoring.ScanHydrogen_2", "** Hydrogen **");
            _logger.LogInfo("GridMonitoring.ScanHydrogen_3", $"Current Hydrogen Capacity: {MonitoringData.HydrogenTanks.Sum(h => h.Capacity):0.##}");
            _logger.LogInfo("GridMonitoring.ScanHydrogen_4",
                $"Current Hydrogen Filled Ratio: {MonitoringData.HydrogenTanks.Sum(h => h.FilledRatio) / MonitoringData.HydrogenTanks.Count:0.##} %");
        }

        private void ScanPower(IEnumerable<BlockEntity<IMyPowerProducer>> powerBlocks)
        {
            var uraniumCount = MonitoringData.GetItems(Item.ItemTypes.Ingot)
                .Where(i => i.ItemSubType == Item.ItemSubTypes.Uranium)
                .Sum(i => i.Amount);

            var iceCount = MonitoringData.GetItems(Item.ItemTypes.Ore)
                .Where(i => i.ItemSubType == Item.ItemSubTypes.Ice)
                .Sum(i => i.Amount);

            MonitoringData.PowerConsumption.AvailableUranium = uraniumCount;
            MonitoringData.PowerConsumption.AvailableIce = iceCount;

            powerBlocks
                .Where(block => block.Exists())
                .Select(block => block.Block)
                .ToList()
                .ForEach(block =>
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
                });

            _logger.LogInfo("GridMonitoring.ScanPower_1", "");
            _logger.LogInfo("GridMonitoring.ScanPower_2", "** Power **");
            _logger.LogInfo("GridMonitoring.ScanPower_3", $"Battery Count: {MonitoringData.PowerConsumption.Batteries.Count}");
            _logger.LogInfo("GridMonitoring.ScanPower_4", $"Solar Panel Count: {MonitoringData.PowerConsumption.SolarPanels.Count}");
            _logger.LogInfo("GridMonitoring.ScanPower_5", $"Reactor Count: {MonitoringData.PowerConsumption.Reactors.Count}");
            _logger.LogInfo("GridMonitoring.ScanPower_6", $"Hydrogen Engine Count: {MonitoringData.PowerConsumption.HydrogenEngines.Count}");
            _logger.LogInfo("GridMonitoring.ScanPower_7", $"Wind Turret Count: {MonitoringData.PowerConsumption.WindTurbines.Count}");
            _logger.LogInfo("GridMonitoring.ScanPower_8", "");
            _logger.LogInfo("GridMonitoring.ScanPower_9", $"Current Power Output: {MonitoringData.PowerConsumption.CurrentPowerOutput} MW");
            _logger.LogInfo("GridMonitoring.ScanPower_10", $"Max Power Output: {MonitoringData.PowerConsumption.MaxPowerOutput} MW");
            _logger.LogInfo("GridMonitoring.ScanPower_11", "");
            _logger.LogInfo("GridMonitoring.ScanPower_12", $"Available Uranium: {MonitoringData.PowerConsumption.AvailableUranium}");
            _logger.LogInfo("GridMonitoring.ScanPower_13", $"Available Ice: {MonitoringData.PowerConsumption.AvailableIce}");
        }

        private void ScanForeignPower(IEnumerable<BlockEntity<IMyPowerProducer>> powerBlocks)
        {
            powerBlocks
                .Where(block => block.Exists())
                .Select(block => block.Block)
                .ToList()
                .ForEach(block =>
                {
                    var battery = block as IMyBatteryBlock;

                    if (battery != null)
                        MonitoringData.ForeignPowerConsumption.Batteries.Add(new BatteryInfo
                        {
                            Name = battery.CustomName,
                            IsCharging = battery.IsCharging,
                            CurrentStoredPower = battery.CurrentStoredPower,
                            MaxStoredPower = battery.MaxStoredPower,
                            CurrentOutput = battery.CurrentOutput,
                            MaxOutput = battery.MaxOutput
                        });
                });

            _logger.LogInfo("GridMonitoring.ScanForeignPower_1", "");
            _logger.LogInfo("GridMonitoring.ScanForeignPower_2", "** Power **");
            _logger.LogInfo("GridMonitoring.ScanForeignPower_3", $"Foreign Battery Count: {MonitoringData.ForeignPowerConsumption.Batteries.Count}");
        }

        private void ScanStorageCapacity(IEnumerable<BlockEntity<IMyTerminalBlock>> withStorageBlocks)
        {
            withStorageBlocks
                .Where(block => block.Exists() && block.IsOfType<IMyCargoContainer>())
                .ToList()
                .ForEach(block =>
                {
                    var inventory = block.Block.GetInventory();

                    MonitoringData.Cargos.Add(new CargoInfo
                    {
                        Name = block.Block.CustomName,
                        MaxVolume = inventory.MaxVolume,
                        CurrentVolume = inventory.CurrentVolume
                    });
                });

            _logger.LogInfo("GridMonitoring.ScanStorageCapacity_1", "");
            _logger.LogInfo("GridMonitoring.ScanStorageCapacity_2", "** Items **");
            _logger.LogInfo("GridMonitoring.ScanStorageCapacity_3", $"Max Storage Capacity: {MonitoringData.Cargos.Sum(x => (float)x.MaxVolume)} m^3");
            _logger.LogInfo("GridMonitoring.ScanStorageCapacity_4", $"Current Storage Volume: {MonitoringData.Cargos.Sum(x => (float)x.CurrentVolume)} m^3");
        }

        private void ScanAllInventory(IEnumerable<BlockEntity<IMyTerminalBlock>> blocksWithStorage)
        {
            blocksWithStorage
                .Where(block => block.Exists())
                .ToList()
                .ForEach(block =>
                {
                    var inventoryCount = block.Block.InventoryCount;

                    for (var index = 0; index < inventoryCount; index++)
                        ScanInventory(block.Block.GetInventory(index));
                });

            var components = MonitoringData.GetItems(Item.ItemTypes.Component);
            var ingots = MonitoringData.GetItems(Item.ItemTypes.Ingot);
            var ores = MonitoringData.GetItems(Item.ItemTypes.Ore);
            var tools = MonitoringData.GetItems(Item.ItemTypes.Tools);
            var unknowns = MonitoringData.GetItems(Item.ItemTypes.Unknown);

            _logger.LogInfo("GridMonitoring.ScanAllInventory_1", $"Components found: {components.Count.ToString()}");
            _logger.LogInfo("GridMonitoring.ScanAllInventory_2", $"Ingots found: {ingots.Count.ToString()}");
            _logger.LogInfo("GridMonitoring.ScanAllInventory_3", $"Ores found: {ores.Count.ToString()}");
            _logger.LogInfo("GridMonitoring.ScanAllInventory_4", $"Tools found: {tools.Count.ToString()}");
            _logger.LogInfo("GridMonitoring.ScanAllInventory_5", $"Unknowns found: {unknowns.Count.ToString()}");
        }

        private void ScanProduction(IEnumerable<BlockEntity<IMyProductionBlock>> productionBlocks)
        {
            productionBlocks
                .Where(block => block.Exists())
                .Select(block => block.Block)
                .ToList()
                .ForEach(block =>
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

            _logger.LogInfo("GridMonitoring.ScanProduction_1", $"Components in production found: {components.Count.ToString()}");
            _logger.LogInfo("GridMonitoring.ScanProduction_2", $"Ingots in production found: {ingots.Count.ToString()}");
            _logger.LogInfo("GridMonitoring.ScanProduction_3", $"Unknown in production found: {unknowns.Count.ToString()}");
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
    }
}
