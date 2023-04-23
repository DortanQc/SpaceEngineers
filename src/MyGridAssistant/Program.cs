using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;

namespace MyGridAssistant
{
    public class Program : MyGridProgram
    {
        private readonly AutoCleanup _autoCleanup;
        private readonly AutoProducer _autoProducer;
        private readonly IConfiguration _configuration;
        private readonly DisplayManager _displayManager;
        private readonly DoorManager _doorManager;
        private readonly MyGridAssistantLogger _logger;
        private readonly GridMonitoring _monitoring;
        private readonly Dictionary<TimedAction, DateTime> _timerDictionary = new Dictionary<TimedAction, DateTime>();
        private bool _alreadyRunning;
        private bool _autoCleanupStorages;
        private bool _autoProduceActive;
        private bool _autoShutDownDoors;
        private List<BlockEntity<IMyGasTank>> _blocksHoldingGas;
        private List<BlockEntity<IMyProductionBlock>> _blocksProducingItems;
        private List<BlockEntity<IMyPowerProducer>> _blocksProducingPower;
        private List<BlockEntity<IMyTerminalBlock>> _blocksWithStorage;
        private List<BlockEntity<IMyTerminalBlock>> _blockWithTextSurfaces;
        private List<BlockEntity<IMyDoor>> _doors;
        private List<BlockEntity<IMyPowerProducer>> _foreignBlocksProducingPower;
        private List<Item> _itemsToProduce;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            _logger = new MyGridAssistantLogger(Echo);
            _blocksProducingPower = new List<BlockEntity<IMyPowerProducer>>();
            _foreignBlocksProducingPower = new List<BlockEntity<IMyPowerProducer>>();
            _blocksWithStorage = new List<BlockEntity<IMyTerminalBlock>>();
            _blocksProducingItems = new List<BlockEntity<IMyProductionBlock>>();
            _blocksHoldingGas = new List<BlockEntity<IMyGasTank>>();
            _doors = new List<BlockEntity<IMyDoor>>();
            _blockWithTextSurfaces = new List<BlockEntity<IMyTerminalBlock>>();
            _monitoring = new GridMonitoring(_logger);
            _configuration = new Configuration(Me);

            _autoProducer = new AutoProducer(_logger);
            _displayManager = new DisplayManager(_logger);
            _doorManager = new DoorManager(_logger);
            _autoCleanup = new AutoCleanup(_logger);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (_alreadyRunning)
                return;

            _alreadyRunning = true;

            WhenItsTimeTo(TimedAction.CheckForUpdatedConfig, RefreshConfig);

            WhenItsTimeTo(TimedAction.ScanGrid, () =>
            {
                var allTerminalBlocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocks(allTerminalBlocks);

                var blocks = ExtractLocalTerminalBlocks(allTerminalBlocks);
                var foreignBlocks = ExtractForeignTerminalBlocks(allTerminalBlocks);
                _blocksProducingPower = ExtractPowerBlocks(blocks);
                _foreignBlocksProducingPower = ExtractPowerBlocks(foreignBlocks);
                _blocksWithStorage = ExtractStorageBlocks(blocks);
                _blocksProducingItems = ExtractItemProductionBlocks(blocks);
                _blocksHoldingGas = ExtractGasTanksBlocks(blocks);
                _doors = ExtractDoorBlocks(blocks);
                _blockWithTextSurfaces = ExtractTextSurfaceBlocks(blocks);
            });

            WhenItsTimeTo(TimedAction.MonitorGrid, () =>
            {
                _monitoring.UpdateData(
                    _blocksProducingPower,
                    _foreignBlocksProducingPower,
                    _blocksWithStorage,
                    _blocksProducingItems,
                    _blocksHoldingGas);

                if (_autoProduceActive)
                    _autoProducer.Produce(
                        _monitoring.MonitoringData.GetItems(),
                        _monitoring.MonitoringData.GetItemsInProduction(),
                        _itemsToProduce,
                        _blocksProducingItems);
            });

            WhenItsTimeTo(TimedAction.CleanupStorage, () =>
            {
                if (_autoCleanupStorages)
                    _autoCleanup.Cleanup(
                        _blocksWithStorage,
                        _blocksProducingItems);
            });

            WhenItsTimeTo(TimedAction.DisplayStats, () =>
                _displayManager.Display(_monitoring.MonitoringData, _itemsToProduce, _blockWithTextSurfaces));

            WhenItsTimeTo(TimedAction.ShutDownDoors, () =>
            {
                if (_autoShutDownDoors)
                    _doorManager.ShutDownDoorWhenOpenedLongerThanExpected(_doors);
            });

            WhenItsTimeTo(TimedAction.ManageAirlocks, () =>
                _doorManager.ManageAirLocks(_doors));

            _logger.ShowLogs();
            _alreadyRunning = false;
        }

        private static int GetActionFrequencyInMs(TimedAction action)
        {
            switch (action)
            {
                case TimedAction.ScanGrid: return 10000;
                case TimedAction.MonitorGrid: return 5000;
                case TimedAction.CleanupStorage: return 10000;
                case TimedAction.ManageAirlocks: return 1000;
                case TimedAction.ShutDownDoors: return 1000;
                case TimedAction.CheckForUpdatedConfig: return 10000;
                case TimedAction.DisplayStats: return 250;
                default: throw new Exception($"{nameof(action)} not out of range");
            }
        }

        private void RefreshConfig()
        {
            var autoProduceActive = SetupCustomData(Settings.AUTO_PRODUCE_ITEMS_ACTIVE, "false");
            var autoShutDownDoors = SetupCustomData(Settings.AUTO_SHUTDOWN_DOORS_ACTIVE, "false");
            var autoCleanupStorages = SetupCustomData(Settings.AUTO_CLEANUP_STORAGES_ACTIVE, "false");

            _autoProduceActive = InitConfig(autoProduceActive);
            _autoShutDownDoors = InitConfig(autoShutDownDoors);
            _autoCleanupStorages = InitConfig(autoCleanupStorages);

            var bulletproofGlassKeyName = $"{Settings.COMPONENT_THRESHOLD}-BulletproofGlass";
            var canvasKeyName = $"{Settings.COMPONENT_THRESHOLD}-Canvas";
            var computerKeyName = $"{Settings.COMPONENT_THRESHOLD}-ComputerComponent";
            var constructionKeyName = $"{Settings.COMPONENT_THRESHOLD}-ConstructionComponent";
            var detectorKeyName = $"{Settings.COMPONENT_THRESHOLD}-DetectorComponent";
            var displayKeyName = $"{Settings.COMPONENT_THRESHOLD}-Display";
            var explosivesKeyName = $"{Settings.COMPONENT_THRESHOLD}-ExplosivesComponent";
            var girderKeyName = $"{Settings.COMPONENT_THRESHOLD}-GirderComponent";
            var gravityGeneratorKeyName = $"{Settings.COMPONENT_THRESHOLD}-GeneratorComponent";
            var interiorPlateKeyName = $"{Settings.COMPONENT_THRESHOLD}-InteriorPlate";
            var largeTubeKeyName = $"{Settings.COMPONENT_THRESHOLD}-LargeTube";
            var medicalKeyName = $"{Settings.COMPONENT_THRESHOLD}-MedicalComponent";
            var metalGridKeyName = $"{Settings.COMPONENT_THRESHOLD}-MetalGrid";
            var motorKeyName = $"{Settings.COMPONENT_THRESHOLD}-MotorComponent";
            var powerCellKeyName = $"{Settings.COMPONENT_THRESHOLD}-PowerCell";
            var radioCommunicationKeyName = $"{Settings.COMPONENT_THRESHOLD}-RadioCommunicationComponent";
            var reactorKeyName = $"{Settings.COMPONENT_THRESHOLD}-ReactorComponent";
            var smallTubeKeyName = $"{Settings.COMPONENT_THRESHOLD}-SmallTube";
            var solarCellKeyName = $"{Settings.COMPONENT_THRESHOLD}-SolarCell";
            var steelPlateKeyName = $"{Settings.COMPONENT_THRESHOLD}-SteelPlate";
            var superconductorKeyName = $"{Settings.COMPONENT_THRESHOLD}-Superconductor";
            var thrustKeyName = $"{Settings.COMPONENT_THRESHOLD}-ThrustComponent";
            var gatlingAmmoBoxName = $"{Settings.COMPONENT_THRESHOLD}-GatlingAmmoBox";
            var assaultCannonShellName = $"{Settings.COMPONENT_THRESHOLD}-AssaultCannonShell";
            var largeRailgunSabotName = $"{Settings.COMPONENT_THRESHOLD}-LargeRailgunSabot";

            var bulletproofGlassValue = SetupCustomData(bulletproofGlassKeyName, "0");
            var canvasValue = SetupCustomData(canvasKeyName, "0");
            var computerComponentValue = SetupCustomData(computerKeyName, "0");
            var constructionComponentValue = SetupCustomData(constructionKeyName, "0");
            var detectorComponentValue = SetupCustomData(detectorKeyName, "0");
            var displayValue = SetupCustomData(displayKeyName, "0");
            var explosivesComponentValue = SetupCustomData(explosivesKeyName, "0");
            var girderComponentValue = SetupCustomData(girderKeyName, "0");
            var gravityGeneratorComponentValue = SetupCustomData(gravityGeneratorKeyName, "0");
            var interiorPlateValue = SetupCustomData(interiorPlateKeyName, "0");
            var largeTubeValue = SetupCustomData(largeTubeKeyName, "0");
            var medicalComponentValue = SetupCustomData(medicalKeyName, "0");
            var metalGridValue = SetupCustomData(metalGridKeyName, "0");
            var motorComponentValue = SetupCustomData(motorKeyName, "0");
            var powerCellValue = SetupCustomData(powerCellKeyName, "0");
            var radioCommunicationComponentValue = SetupCustomData(radioCommunicationKeyName, "0");
            var reactorComponentValue = SetupCustomData(reactorKeyName, "0");
            var smallTubeValue = SetupCustomData(smallTubeKeyName, "0");
            var solarCellValue = SetupCustomData(solarCellKeyName, "0");
            var steelPlateValue = SetupCustomData(steelPlateKeyName, "0");
            var superconductorValue = SetupCustomData(superconductorKeyName, "0");
            var thrustComponentValue = SetupCustomData(thrustKeyName, "0");
            var gatlingAmmoBoxValue = SetupCustomData(gatlingAmmoBoxName, "0");
            var assaultCannonShellValue = SetupCustomData(assaultCannonShellName, "0");
            var largeRailgunSabotValue = SetupCustomData(largeRailgunSabotName, "0");

            _itemsToProduce = new List<Item>
            {
                InitAutoProductionValue(bulletproofGlassValue, "BulletproofGlass"),
                InitAutoProductionValue(canvasValue, "Canvas"),
                InitAutoProductionValue(computerComponentValue, "ComputerComponent"),
                InitAutoProductionValue(constructionComponentValue, "ConstructionComponent"),
                InitAutoProductionValue(detectorComponentValue, "DetectorComponent"),
                InitAutoProductionValue(displayValue, "Display"),
                InitAutoProductionValue(explosivesComponentValue, "ExplosivesComponent"),
                InitAutoProductionValue(girderComponentValue, "GirderComponent"),
                InitAutoProductionValue(gravityGeneratorComponentValue, "GravityGeneratorComponent"),
                InitAutoProductionValue(interiorPlateValue, "InteriorPlate"),
                InitAutoProductionValue(largeTubeValue, "LargeTube"),
                InitAutoProductionValue(medicalComponentValue, "MedicalComponent"),
                InitAutoProductionValue(metalGridValue, "MetalGrid"),
                InitAutoProductionValue(motorComponentValue, "MotorComponent"),
                InitAutoProductionValue(powerCellValue, "PowerCell"),
                InitAutoProductionValue(radioCommunicationComponentValue, "RadioCommunicationComponent"),
                InitAutoProductionValue(reactorComponentValue, "ReactorComponent"),
                InitAutoProductionValue(smallTubeValue, "SmallTube"),
                InitAutoProductionValue(solarCellValue, "SolarCell"),
                InitAutoProductionValue(steelPlateValue, "SteelPlate"),
                InitAutoProductionValue(superconductorValue, "Superconductor"),
                InitAutoProductionValue(thrustComponentValue, "ThrustComponent"),
                InitAutoProductionValue(gatlingAmmoBoxValue, "NATO_25x184mmMagazine"),
                InitAutoProductionValue(assaultCannonShellValue, "MediumCalibreAmmo"),
                InitAutoProductionValue(largeRailgunSabotValue, "LargeRailgunAmmo")
            };
        }

        private static Item InitAutoProductionValue(string rawThresholdAmount, string itemType)
        {
            int amount;
            MyDefinitionId itemDefinitionId;

            var isItemDefinitionValid = MyDefinitionId.TryParse($"MyObjectBuilder_BlueprintDefinition/{itemType}", out itemDefinitionId);
            var isAmountValid = int.TryParse(rawThresholdAmount, out amount);

            if (!isItemDefinitionValid)
                throw new Exception($"{itemType} is not a valid Item Type.");

            if (!isAmountValid)
                throw new Exception($"{rawThresholdAmount} is not a valid number for auto producing {itemType}.");

            return new Item(itemDefinitionId, amount);
        }

        private string SetupCustomData(string key, string defaultValue)
        {
            var actualValue = _configuration.GetConfig(key);

            if (actualValue == null)
                _configuration.SetConfig(key, defaultValue);

            return actualValue;
        }

        private static bool InitConfig(string customDataValue) =>
            (customDataValue ?? string.Empty).Equals("true", StringComparison.InvariantCultureIgnoreCase);

        private List<BlockEntity<IMyTerminalBlock>> ExtractForeignTerminalBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks
                .Where(block => block.IsSameConstructAs(Me) == false)
                .Select(block => new BlockEntity<IMyTerminalBlock>(_logger, GridTerminalSystem, block))
                .ToList();

        private List<BlockEntity<IMyTerminalBlock>> ExtractLocalTerminalBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks
                .Where(block => block.IsSameConstructAs(Me))
                .Select(block => new BlockEntity<IMyTerminalBlock>(_logger, GridTerminalSystem, block))
                .ToList();

        private static List<BlockEntity<IMyGasTank>> ExtractGasTanksBlocks(IEnumerable<BlockEntity<IMyTerminalBlock>> blocks) =>
            blocks
                .Where(block => block.IsOfType<IMyGasTank>())
                .Select(block => block.CastTo<IMyGasTank>())
                .ToList();

        private static List<BlockEntity<IMyDoor>> ExtractDoorBlocks(IEnumerable<BlockEntity<IMyTerminalBlock>> blocks) =>
            blocks
                .Where(block => block.IsOfType<IMyDoor>())
                .Where(block =>
                    block.Block
                        .BlockDefinition
                        .TypeIdString
                        .IndexOf("hangar", StringComparison.InvariantCultureIgnoreCase) < 0)
                .Select(block => block.CastTo<IMyDoor>())
                .ToList();

        private static List<BlockEntity<IMyProductionBlock>> ExtractItemProductionBlocks(IEnumerable<BlockEntity<IMyTerminalBlock>> blocks) =>
            blocks
                .Where(block => block.IsOfType<IMyProductionBlock>())
                .Where(block => block.Block.HasInventory)
                .Select(block => block.CastTo<IMyProductionBlock>())
                .ToList();

        private static List<BlockEntity<IMyTerminalBlock>> ExtractStorageBlocks(IEnumerable<BlockEntity<IMyTerminalBlock>> blocks) =>
            blocks
                .Where(block => block.Block.HasInventory)
                .ToList();

        private static List<BlockEntity<IMyPowerProducer>> ExtractPowerBlocks(IEnumerable<BlockEntity<IMyTerminalBlock>> blocks) =>
            blocks
                .Where(block => block.IsOfType<IMyPowerProducer>())
                .Select(block => block.CastTo<IMyPowerProducer>())
                .ToList();

        private static List<BlockEntity<IMyTerminalBlock>> ExtractTextSurfaceBlocks(IEnumerable<BlockEntity<IMyTerminalBlock>> blocks) =>
            blocks
                .Where(block => block.IsOfType<IMyTextSurfaceProvider>())
                .ToList();

        private void WhenItsTimeTo(TimedAction actionKey, Action action)
        {
            var totalMillisecond = GetActionFrequencyInMs(actionKey);

            if (_timerDictionary.ContainsKey(actionKey) == false)
                _timerDictionary.Add(actionKey, DateTime.MinValue);

            if (DateTime.Now.Subtract(_timerDictionary[actionKey]).TotalMilliseconds < totalMillisecond)
                return;

            var begin = DateTime.Now;
            action();
            var end = DateTime.Now;

            var elapsed = end.Subtract(begin).Milliseconds;
            if (elapsed > 10)
                _logger.LogDebug($"{actionKey.ToString()} took {elapsed} ms to complete");

            _timerDictionary[actionKey] = DateTime.Now;
        }

        private enum TimedAction
        {
            ScanGrid = 0,
            CleanupStorage = 1,
            ManageAirlocks = 2,
            ShutDownDoors = 3,
            CheckForUpdatedConfig = 5,
            DisplayStats = 6,
            MonitorGrid
        }
    }
}
