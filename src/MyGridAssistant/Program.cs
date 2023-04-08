using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGridAssistant
{
    public class Program : MyGridProgram
    {
        private readonly IConfiguration _configuration;
        private readonly MenuNavigationSystem _menuNavigationSystem;
        private readonly GridMonitoring _monitoring;
        private readonly Dictionary<TimedAction, DateTime> _timerDictionary = new Dictionary<TimedAction, DateTime>();
        private bool _autoCleanupStorages;
        private bool _autoProduceActive;
        private bool _autoShutDownDoors;
        private List<IMyTerminalBlock> _blocks;
        private List<IMyGasTank> _blocksHoldingGas;
        private List<IMyProductionBlock> _blocksProducingItems;
        private List<IMyPowerProducer> _blocksProducingPower;
        private List<IMyTerminalBlock> _blocksWithStorage;
        private List<IMyShipController> _controllers;
        private List<IMyDoor> _doors;
        private Item[] _itemsToProduce;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            _blocks = new List<IMyTerminalBlock>();
            _blocksProducingPower = new List<IMyPowerProducer>();
            _blocksWithStorage = new List<IMyTerminalBlock>();
            _blocksProducingItems = new List<IMyProductionBlock>();
            _blocksHoldingGas = new List<IMyGasTank>();
            _doors = new List<IMyDoor>();
            _monitoring = new GridMonitoring(Echo);
            _menuNavigationSystem = new MenuNavigationSystem(Echo);
            _configuration = new Configuration(Me);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var action = GetScriptActionRequest(updateSource, argument);

            if (action == MenuNavigationSystem.ScriptActions.Normal)
            {
                WhenItsTimeTo(TimedAction.CheckForUpdatedConfig, RefreshConfig);

                WhenItsTimeTo(TimedAction.ScanGrid, () =>
                {
                    _blocks = ExtractAllTerminalBlocks();

                    _blocksProducingPower = ExtractPowerBlocks(_blocks);
                    _blocksWithStorage = ExtractStorageBlocks(_blocks);
                    _blocksProducingItems = ExtractItemProductionBlocks(_blocks);
                    _blocksHoldingGas = ExtractGasTanksBlocks(_blocks);
                    _doors = ExtractDoorBlocks(_blocks);
                    _controllers = ExtractControllersInUse(_blocks);

                    _monitoring.UpdateData(
                        _blocks,
                        _blocksProducingPower,
                        _blocksWithStorage,
                        _blocksProducingItems,
                        _blocksHoldingGas);
                });

                WhenItsTimeTo(TimedAction.AutoProduce, () =>
                {
                    if (_autoProduceActive)
                        AutoProducer.Produce(Echo, _monitoring.MonitoringData, _itemsToProduce, _blocksProducingItems);
                });

                WhenItsTimeTo(TimedAction.ShutDownDoors, () =>
                {
                    if (_autoShutDownDoors)
                        DoorManager.ShutDownDoorWhenOpenedLongerThanExpected(_doors);
                });

                WhenItsTimeTo(TimedAction.CleanupStorage, () =>
                {
                    if (_autoCleanupStorages)
                        AutoCleanup.Cleanup(Echo, _blocksWithStorage, _blocksProducingItems);
                });

                WhenItsTimeTo(TimedAction.ManageAirlocks, () =>
                {
                    DoorManager.ManageAirLocks(Echo, _doors);
                });

                WhenItsTimeTo(TimedAction.DisplayStats, () =>
                    DisplayManager.Display(_monitoring.MonitoringData, _itemsToProduce, _blocks, Echo));
            }
            else
            {
                _menuNavigationSystem.RunAction(action);
            }

            _menuNavigationSystem.RenderMenu(_blocks, _monitoring.MonitoringData);
        }

        private static int GetActionFrequencyInMs(TimedAction action)
        {
            switch (action)
            {
                case TimedAction.ScanGrid: return 3000;
                case TimedAction.CleanupStorage: return 10000;
                case TimedAction.ManageAirlocks:
                case TimedAction.ShutDownDoors:
                    return 1000;
                case TimedAction.AutoProduce:
                case TimedAction.CheckForUpdatedConfig:
                    return 5000;
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

            _itemsToProduce = new[]
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
                InitAutoProductionValue(thrustComponentValue, "ThrustComponent")
            };
        }

        private static Item InitAutoProductionValue(string customDataValue, string objectName)
        {
            int amount;
            int.TryParse(customDataValue, out amount);

            return new Item($"MyObjectBuilder_BlueprintDefinition/{objectName}", amount);
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

        private static List<IMyShipController> ExtractControllersInUse(IEnumerable<IMyTerminalBlock> blocks)
        {
            var controllersBlocks = blocks
                .OfType<IMyShipController>()
                .Where(c => c.IsUnderControl);

            return controllersBlocks
                .Where(HasDisplayMenuBlocks)
                .Where(IsAllowedToControlMenu)
                .ToList();
        }

        private static bool HasDisplayMenuBlocks(IMyTerminalBlock block)
        {
            var customData = Configuration.GetBlockConfiguration(block, Settings.GRID_MANAGER_MENU_ACCESS);

            if (customData == null) return false;

            var index = 0;
            if (customData.Length > 0)
                int.TryParse(customData, out index);

            var textSurface = block as IMyTextSurfaceProvider;

            return textSurface != null && textSurface.SurfaceCount > index;
        }

        private static bool IsAllowedToControlMenu(IMyTerminalBlock block)
        {
            var customData = Configuration.GetBlockConfiguration(block, Settings.GRID_MANAGER_MENU_SHIP_CONTROLLABLE);
            
            return customData != null;
        }

        private static MenuNavigationSystem.ScriptActions GetScriptActionRequest(UpdateType updateSource, string argument)
        {
            if (updateSource != UpdateType.Trigger)
                return MenuNavigationSystem.ScriptActions.Normal;

            switch (argument.ToUpper())
            {
                case "UP": return MenuNavigationSystem.ScriptActions.NavigationUp;
                case "DOWN": return MenuNavigationSystem.ScriptActions.NavigationDown;
                case "SELECT": return MenuNavigationSystem.ScriptActions.NavigationSelect;
                default: return MenuNavigationSystem.ScriptActions.Normal;
            }
        }

        private List<IMyTerminalBlock> ExtractAllTerminalBlocks()
        {
            var blocks = new List<IMyTerminalBlock>();

            GridTerminalSystem.GetBlocks(blocks);

            return blocks.Where(block => block.IsSameConstructAs(Me)).ToList();
        }

        private static List<IMyGasTank> ExtractGasTanksBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.OfType<IMyGasTank>().ToList();

        private static List<IMyDoor> ExtractDoorBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.OfType<IMyDoor>()
                .Where(block =>
                    block
                        .BlockDefinition
                        .TypeIdString
                        .IndexOf("hangar", StringComparison.InvariantCultureIgnoreCase) <
                    0)
                .ToList();

        private static List<IMyProductionBlock> ExtractItemProductionBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.OfType<IMyProductionBlock>().Where(block => block.HasInventory).ToList();

        private static List<IMyTerminalBlock> ExtractStorageBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.Where(block => block.HasInventory).ToList();

        private static List<IMyPowerProducer> ExtractPowerBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.OfType<IMyPowerProducer>().ToList();

        private void WhenItsTimeTo(TimedAction actionKey, Action action)
        {
            var totalMillisecond = GetActionFrequencyInMs(actionKey);

            if (_timerDictionary.ContainsKey(actionKey) == false)
                _timerDictionary.Add(actionKey, DateTime.MinValue);

            if (DateTime.Now.Subtract(_timerDictionary[actionKey]).TotalMilliseconds < totalMillisecond)
                return;

            action();

            _timerDictionary[actionKey] = DateTime.Now;
        }

        private enum TimedAction
        {
            ScanGrid = 0,
            CleanupStorage = 1,
            ManageAirlocks = 2,
            ShutDownDoors = 3,
            AutoProduce = 4,
            CheckForUpdatedConfig = 5,
            DisplayStats = 6
        }
    }
}
