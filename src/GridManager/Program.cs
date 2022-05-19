using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    public class Program : MyGridProgram
    {
        private const int RUN_MONITORING = 0;
        private const int CLEANUP_STORAGES = 1;
        private const int MANAGE_AIRLOCKS = 2;

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
        private List<IMyTerminalBlock> _blocks;
        private List<IMyShipController> _controllers;
        private bool _lastControllerMoveDownAction;
        private bool _lastControllerMoveUpAction;
        private bool _lastControllerSelectAction;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            _blocks = new List<IMyTerminalBlock>();
            _monitoring = new GridMonitoring(Echo);
            _menuNavigationSystem = new MenuNavigationSystem(Echo);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var action = GetScriptActionRequest(updateSource, argument);

            if (action == MenuNavigationSystem.ScriptActions.Normal)
            {
                WhenItsTimeTo(RUN_MONITORING, 2, () =>
                {
                    _blocks = ExtractAllTerminalBlocks();

                    var blocksProducingPower = ExtractPowerBlocks(_blocks);
                    var blocksWithStorage = ExtractStorageBlocks(_blocks);
                    var blocksProducingItems = ExtractItemProductionBlocks(_blocks);
                    var blocksHoldingGas = ExtractGasTanksBlocks(_blocks);
                    var doors = ExtractDoorBlocks(_blocks);
                    _controllers = ExtractControllersInUse(_blocks);

                    GetMonitoringData(blocksProducingPower, blocksWithStorage, blocksProducingItems, blocksHoldingGas);

                    AutoProducer.Produce(Echo, _monitoring.MonitoringData, _itemsToProduce, blocksProducingItems);

                    DisplayManager.Display(_monitoring.MonitoringData, _itemsToProduce, _blocks, Echo);

                    DoorManager.ShutDownDoorWhenOpenedLongerThanExpected(doors);
                });

                WhenItsTimeTo(MANAGE_AIRLOCKS, 1, () =>
                {
                    var doors = ExtractDoorBlocks(_blocks);
                    DoorManager.ManageAirLocks(Echo, doors);
                });

                WhenItsTimeTo(CLEANUP_STORAGES, 5, () =>
                {
                    var blocksWithStorage = ExtractStorageBlocks(_blocks);
                    var blocksProducingItems = ExtractItemProductionBlocks(_blocks);

                    AutoCleanup.Cleanup(Echo, blocksWithStorage, blocksProducingItems);
                });

                ReactOnControllersAction();
            }
            else
            {
                _menuNavigationSystem.RunAction(action);
            }

            _menuNavigationSystem.RenderMenu(_blocks, _monitoring.MonitoringData);
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
            List<IMyGasTank> blocksHoldingGas)
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
