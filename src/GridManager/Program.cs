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

        private readonly Item[] _itemsToProduce =
        {
            new Item("MyObjectBuilder_BlueprintDefinition/BulletproofGlass", 100),
            new Item("MyObjectBuilder_BlueprintDefinition/Canvas", 1),
            new Item("MyObjectBuilder_BlueprintDefinition/ComputerComponent", 1000),
            new Item("MyObjectBuilder_BlueprintDefinition/ConstructionComponent", 2000),
            new Item("MyObjectBuilder_BlueprintDefinition/DetectorComponent", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/Display", 100),
            new Item("MyObjectBuilder_BlueprintDefinition/ExplosivesComponent", 100),
            new Item("MyObjectBuilder_BlueprintDefinition/GirderComponent", 500),
            new Item("MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent", 10),
            new Item("MyObjectBuilder_BlueprintDefinition/InteriorPlate", 500),
            new Item("MyObjectBuilder_BlueprintDefinition/LargeTube", 500),
            new Item("MyObjectBuilder_BlueprintDefinition/MedicalComponent", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/MetalGrid", 500),
            new Item("MyObjectBuilder_BlueprintDefinition/MotorComponent", 1000),
            new Item("MyObjectBuilder_BlueprintDefinition/PowerCell", 200),
            new Item("MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/ReactorComponent", 10),
            new Item("MyObjectBuilder_BlueprintDefinition/SmallTube", 1000),
            new Item("MyObjectBuilder_BlueprintDefinition/SolarCell", 200),
            new Item("MyObjectBuilder_BlueprintDefinition/SteelPlate", 5000),
            new Item("MyObjectBuilder_BlueprintDefinition/Superconductor", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/ThrustComponent", 20)
        };

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
            var customData = new CustomDataManager(Me.CustomData);

            var autoProduceActive = customData.GetPropertyValue(CustomDataSettings.AUTO_PRODUCE_ITEMS_ACTIVE);
            var autoShutDownDoors = customData.GetPropertyValue(CustomDataSettings.AUTO_SHUTDOWN_DOORS_ACTIVE);
            var autoCleanupStorages = customData.GetPropertyValue(CustomDataSettings.AUTO_CLEANUP_STORAGES_ACTIVE);

            if (autoProduceActive == null)
                CustomDataManager.AddValue(Me, CustomDataSettings.AUTO_PRODUCE_ITEMS_ACTIVE, "false");

            if (autoShutDownDoors == null)
                CustomDataManager.AddValue(Me, CustomDataSettings.AUTO_SHUTDOWN_DOORS_ACTIVE, "false");

            if (autoCleanupStorages == null)
                CustomDataManager.AddValue(Me, CustomDataSettings.AUTO_CLEANUP_STORAGES_ACTIVE, "false");

            _autoProduceActive = InitConfig(autoProduceActive);
            _autoShutDownDoors = InitConfig(autoShutDownDoors);
            _autoCleanupStorages = InitConfig(autoCleanupStorages);
        }

        private static bool InitConfig(string customDataValue) =>
            (customDataValue ?? string.Empty).Equals("true", StringComparison.InvariantCultureIgnoreCase);

        public void Main(string argument, UpdateType updateSource)
        {
            var action = GetScriptActionRequest(updateSource, argument);

            if (action == MenuNavigationSystem.ScriptActions.Normal)
            {
                WhenItsTimeTo(CHECK_FOR_UPDATED_CONFIG, 5, Init);

                WhenItsTimeTo(SCAN_GRID, 2, () =>
                {
                    _blocks = ExtractAllTerminalBlocks();

                    _blocksProducingPower = ExtractPowerBlocks(_blocks);
                    _blocksWithStorage = ExtractStorageBlocks(_blocks);
                    _blocksProducingItems = ExtractItemProductionBlocks(_blocks);
                    _blocksHoldingGas = ExtractGasTanksBlocks(_blocks);
                    _doors = ExtractDoorBlocks(_blocks);
                    _controllers = ExtractControllersInUse(_blocks);

                    GetMonitoringData(
                        _blocksProducingPower,
                        _blocksWithStorage,
                        _blocksProducingItems,
                        _blocksHoldingGas);

                    DisplayManager.Display(_monitoring.MonitoringData, _itemsToProduce, _blocks, Echo);
                });

                WhenItsTimeTo(AUTO_PRODUCE, 2, () =>
                {
                    if (_autoProduceActive)
                        AutoProducer.Produce(Echo, _monitoring.MonitoringData, _itemsToProduce, _blocksProducingItems);
                });

                WhenItsTimeTo(SHUT_DOWN_DOORS, 1, () =>
                {
                    if (_autoShutDownDoors)
                        DoorManager.ShutDownDoorWhenOpenedLongerThanExpected(_doors);
                });

                WhenItsTimeTo(CLEANUP_STORAGES, 5, () =>
                {
                    if (_autoCleanupStorages)
                        AutoCleanup.Cleanup(Echo, _blocksWithStorage, _blocksProducingItems);
                });

                WhenItsTimeTo(MANAGE_AIRLOCKS, 1, () =>
                {
                    DoorManager.ManageAirLocks(Echo, _doors);
                });

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

        private void GetMonitoringData(
            List<IMyPowerProducer> blocksProducingPower,
            List<IMyTerminalBlock> blocksWithStorage,
            List<IMyProductionBlock> blocksProducingItems,
            IEnumerable<IMyGasTank> blocksHoldingGas)
        {
            _monitoring.UpdateData(
                blocksProducingPower,
                blocksWithStorage,
                blocksProducingItems,
                blocksHoldingGas);
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

        private void WhenItsTimeTo(int timerKey, int totalSecond, Action action)
        {
            if (_timerDictionary.ContainsKey(timerKey) == false)
                _timerDictionary.Add(timerKey, DateTime.MinValue);

            if (DateTime.Now.Subtract(_timerDictionary[timerKey]).TotalSeconds < totalSecond)
                return;

            action();

            _timerDictionary[timerKey] = DateTime.Now;
        }
    }
}
