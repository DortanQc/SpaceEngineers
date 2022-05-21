using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    public class Program : MyGridProgram
    {
        private const int SCAN_GRID = 0;
        private const int CLEANUP_STORAGES = 1;
        private const int MANAGE_AIRLOCKS = 2;
        private const int SHUT_DOWN_DOORS = 3;
        private const int AUTO_PRODUCE = 4;
        private const int CHECK_FOR_UPDATED_CONFIG = 5;
        private const int DISPLAY_STATS = 6;
        private readonly MenuNavigationSystem _menuNavigationSystem;
        private readonly GridMonitoring _monitoring;
        private readonly Dictionary<int, DateTime> _timerDictionary = new Dictionary<int, DateTime>();
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
        private bool _lastControllerMoveDownAction;
        private bool _lastControllerMoveUpAction;
        private bool _lastControllerSelectAction;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            _blocks = new List<IMyTerminalBlock>();
            _blocksProducingPower = new List<IMyPowerProducer>();
            _blocksWithStorage = new List<IMyTerminalBlock>();
            _blocksProducingItems = new List<IMyProductionBlock>();
            _blocksHoldingGas = new List<IMyGasTank>();
            _doors = new List<IMyDoor>();
            _monitoring = new GridMonitoring(Echo);
            _menuNavigationSystem = new MenuNavigationSystem(Echo);
        }

        private void Init()
        {
            var autoProduceActive = SetupCustomData(Me, CustomDataSettings.AUTO_PRODUCE_ITEMS_ACTIVE, "false");
            var autoShutDownDoors = SetupCustomData(Me, CustomDataSettings.AUTO_SHUTDOWN_DOORS_ACTIVE, "false");
            var autoCleanupStorages = SetupCustomData(Me, CustomDataSettings.AUTO_CLEANUP_STORAGES_ACTIVE, "false");

            _autoProduceActive = InitConfig(autoProduceActive);
            _autoShutDownDoors = InitConfig(autoShutDownDoors);
            _autoCleanupStorages = InitConfig(autoCleanupStorages);

            var bulletproofGlassKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-BulletproofGlass";
            var canvasKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-Canvas";
            var computerKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-ComputerComponent";
            var constructionKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-ConstructionComponent";
            var detectorKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-DetectorComponent";
            var displayKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-Display";
            var explosivesKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-ExplosivesComponent";
            var girderKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-GirderComponent";
            var gravityGeneratorKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-GeneratorComponent";
            var interiorPlateKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-InteriorPlate";
            var largeTubeKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-LargeTube";
            var medicalKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-MedicalComponent";
            var metalGridKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-MetalGrid";
            var motorKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-MotorComponent";
            var powerCellKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-PowerCell";
            var radioCommunicationKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-RadioCommunicationComponent";
            var reactorKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-ReactorComponent";
            var smallTubeKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-SmallTube";
            var solarCellKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-SolarCell";
            var steelPlateKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-SteelPlate";
            var superconductorKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-Superconductor";
            var thrustKeyName = $"{CustomDataSettings.COMPONENT_THRESHOLD}-ThrustComponent";

            var bulletproofGlassValue = SetupCustomData(Me, bulletproofGlassKeyName, "0");
            var canvasValue = SetupCustomData(Me, canvasKeyName, "0");
            var computerComponentValue = SetupCustomData(Me, computerKeyName, "0");
            var constructionComponentValue = SetupCustomData(Me, constructionKeyName, "0");
            var detectorComponentValue = SetupCustomData(Me, detectorKeyName, "0");
            var displayValue = SetupCustomData(Me, displayKeyName, "0");
            var explosivesComponentValue = SetupCustomData(Me, explosivesKeyName, "0");
            var girderComponentValue = SetupCustomData(Me, girderKeyName, "0");
            var gravityGeneratorComponentValue = SetupCustomData(Me, gravityGeneratorKeyName, "0");
            var interiorPlateValue = SetupCustomData(Me, interiorPlateKeyName, "0");
            var largeTubeValue = SetupCustomData(Me, largeTubeKeyName, "0");
            var medicalComponentValue = SetupCustomData(Me, medicalKeyName, "0");
            var metalGridValue = SetupCustomData(Me, metalGridKeyName, "0");
            var motorComponentValue = SetupCustomData(Me, motorKeyName, "0");
            var powerCellValue = SetupCustomData(Me, powerCellKeyName, "0");
            var radioCommunicationComponentValue = SetupCustomData(Me, radioCommunicationKeyName, "0");
            var reactorComponentValue = SetupCustomData(Me, reactorKeyName, "0");
            var smallTubeValue = SetupCustomData(Me, smallTubeKeyName, "0");
            var solarCellValue = SetupCustomData(Me, solarCellKeyName, "0");
            var steelPlateValue = SetupCustomData(Me, steelPlateKeyName, "0");
            var superconductorValue = SetupCustomData(Me, superconductorKeyName, "0");
            var thrustComponentValue = SetupCustomData(Me, thrustKeyName, "0");

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

        private static string SetupCustomData(IMyTerminalBlock block, string key, string defaultValue)
        {
            var customData = new CustomDataManager(block.CustomData);

            var actualValue = customData.GetPropertyValue(key);

            if (actualValue == null)
                CustomDataManager.AddValue(block, key, defaultValue);

            return actualValue;
        }

        private static bool InitConfig(string customDataValue) =>
            (customDataValue ?? string.Empty).Equals("true", StringComparison.InvariantCultureIgnoreCase);

        public void Main(string argument, UpdateType updateSource)
        {
            var action = GetScriptActionRequest(updateSource, argument);

            if (action == MenuNavigationSystem.ScriptActions.Normal)
            {
                WhenItsTimeTo(CHECK_FOR_UPDATED_CONFIG, 5000, Init);

                WhenItsTimeTo(SCAN_GRID, 3000, () =>
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

                WhenItsTimeTo(AUTO_PRODUCE, 5000, () =>
                {
                    if (_autoProduceActive)
                        AutoProducer.Produce(Echo, _monitoring.MonitoringData, _itemsToProduce, _blocksProducingItems);
                });

                WhenItsTimeTo(SHUT_DOWN_DOORS, 1000, () =>
                {
                    if (_autoShutDownDoors)
                        DoorManager.ShutDownDoorWhenOpenedLongerThanExpected(_doors);
                });

                WhenItsTimeTo(CLEANUP_STORAGES, 10000, () =>
                {
                    if (_autoCleanupStorages)
                        AutoCleanup.Cleanup(Echo, _blocksWithStorage, _blocksProducingItems);
                });

                WhenItsTimeTo(MANAGE_AIRLOCKS, 1000, () =>
                {
                    DoorManager.ManageAirLocks(Echo, _doors);
                });

                WhenItsTimeTo(DISPLAY_STATS, 250, () =>
                    DisplayManager.Display(_monitoring.MonitoringData, _itemsToProduce, _blocks, Echo));

                ReactOnControllersAction();
            }
            else
            {
                _menuNavigationSystem.RunAction(action);
            }

            _menuNavigationSystem.RenderMenu(_blocks, _monitoring.MonitoringData);
        }

        private void ReactOnControllersAction()
        {
            _controllers.ForEach(controller =>
            {
                var currentMoveUp = Convert.ToInt32(controller.MoveIndicator.Z) == -1;
                var currentMoveDown = Convert.ToInt32(controller.MoveIndicator.Z) == 1;
                var currentSelect = Convert.ToInt32(controller.MoveIndicator.Y) == 1;

                var moveUpAction = false;
                var moveDownAction = false;
                var selectAction = false;

                if (currentMoveUp)
                    if (_lastControllerMoveUpAction == false)
                        moveUpAction = true;

                if (currentMoveDown)
                    if (_lastControllerMoveDownAction == false)
                        moveDownAction = true;

                if (currentSelect)
                    if (_lastControllerSelectAction == false)
                        selectAction = true;

                _lastControllerMoveUpAction = currentMoveUp;
                _lastControllerMoveDownAction = currentMoveDown;
                _lastControllerSelectAction = currentSelect;

                if (moveUpAction)
                    _menuNavigationSystem.RunAction(MenuNavigationSystem.ScriptActions.NavigationUp);
                else if (moveDownAction)
                    _menuNavigationSystem.RunAction(MenuNavigationSystem.ScriptActions.NavigationDown);
                else if (selectAction)
                    _menuNavigationSystem.RunAction(MenuNavigationSystem.ScriptActions.NavigationSelect);
            });
        }

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
            var customDataManager = new CustomDataManager(block.CustomData);
            var customData = customDataManager.GetPropertyValue(CustomDataSettings.GRID_MANAGER_MENU_ACCESS);

            if (customData == null) return false;

            var index = 0;
            if (customData.Length > 0)
                int.TryParse(customData, out index);

            var textSurface = block as IMyTextSurfaceProvider;

            return textSurface != null && textSurface.SurfaceCount > index;
        }

        private static bool IsAllowedToControlMenu(IMyTerminalBlock block)
        {
            var customDataManager = new CustomDataManager(block.CustomData);
            var customData = customDataManager.GetPropertyValue(CustomDataSettings.GRID_MANAGER_MENU_SHIP_CONTROLLABLE);

            return customData != null;
        }

        private static MenuNavigationSystem.ScriptActions GetScriptActionRequest(
            UpdateType updateSource,
            string argument)
        {
            if (updateSource != UpdateType.Trigger)
                return MenuNavigationSystem.ScriptActions.Normal;

            switch (argument.ToUpper())
            {
                case "UP": return MenuNavigationSystem.ScriptActions.NavigationUp;
                case "DOWN": return MenuNavigationSystem.ScriptActions.NavigationDown;
                case "SELECT": return MenuNavigationSystem.ScriptActions.NavigationSelect;
            }

            return MenuNavigationSystem.ScriptActions.Normal;
        }

        private List<IMyTerminalBlock> ExtractAllTerminalBlocks()
        {
            var blocks = new List<IMyTerminalBlock>();

            GridTerminalSystem.GetBlocks(blocks);

            return blocks.Where(block => block.IsSameConstructAs(Me) && block.IsWorking).ToList();
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

        private void WhenItsTimeTo(int timerKey, int totalMilliSecond, Action action)
        {
            if (_timerDictionary.ContainsKey(timerKey) == false)
                _timerDictionary.Add(timerKey, DateTime.MinValue);

            if (DateTime.Now.Subtract(_timerDictionary[timerKey]).TotalMilliseconds < totalMilliSecond)
                return;

            action();

            _timerDictionary[timerKey] = DateTime.Now;
        }
    }
}
