using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    public class Program : MyGridProgram
    {
        private readonly Item[] _itemsToProduce =
        {
            new Item("MyObjectBuilder_BlueprintDefinition/BulletproofGlass", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/Canvas", 1),
            new Item("MyObjectBuilder_BlueprintDefinition/ComputerComponent", 500),
            new Item("MyObjectBuilder_BlueprintDefinition/ConstructionComponent", 1000),
            new Item("MyObjectBuilder_BlueprintDefinition/DetectorComponent", 20),
            new Item("MyObjectBuilder_BlueprintDefinition/Display", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/ExplosivesComponent", 10),
            new Item("MyObjectBuilder_BlueprintDefinition/GirderComponent", 300),
            new Item("MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent", 10),
            new Item("MyObjectBuilder_BlueprintDefinition/InteriorPlate", 300),
            new Item("MyObjectBuilder_BlueprintDefinition/LargeTube", 200),
            new Item("MyObjectBuilder_BlueprintDefinition/MedicalComponent", 20),
            new Item("MyObjectBuilder_BlueprintDefinition/MetalGrid", 300),
            new Item("MyObjectBuilder_BlueprintDefinition/MotorComponent", 500),
            new Item("MyObjectBuilder_BlueprintDefinition/PowerCell", 100),
            new Item("MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/ReactorComponent", 10),
            new Item("MyObjectBuilder_BlueprintDefinition/SmallTube", 300),
            new Item("MyObjectBuilder_BlueprintDefinition/SolarCell", 100),
            new Item("MyObjectBuilder_BlueprintDefinition/SteelPlate", 2000),
            new Item("MyObjectBuilder_BlueprintDefinition/Superconductor", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/ThrustComponent", 20)
        };

        private readonly MenuNavigationSystem _menuNavigationSystem;
        private readonly GridMonitoring _monitoring;
        private List<IMyTerminalBlock> _blocks;
        private List<IMyShipController> _controllers;
        private bool _lastControllerMoveDownAction;
        private bool _lastControllerMoveUpAction;
        private bool _lastControllerSelectAction;
        private DateTime _lastScanTime = DateTime.MinValue;

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
                if (DateTime.Now.Subtract(_lastScanTime).TotalSeconds >= 2)
                {
                    _lastScanTime = DateTime.Now;
                    _blocks = ExtractAllTerminalBlocks();

                    var blocksProducingPower = ExtractPowerBlocks(_blocks);
                    var blocksWithStorage = ExtractStorageBlocks(_blocks);
                    var blocksProducingItems = ExtractItemProductionBlocks(_blocks);
                    var blocksHoldingGas = ExtractGasTanksBlocks(_blocks);
                    _controllers = ExtractControllersInUse(_blocks);

                    GetMonitoringData(blocksProducingPower, blocksWithStorage, blocksProducingItems, blocksHoldingGas);

                    AutoProducer.Produce(Echo, _monitoring.MonitoringData, _itemsToProduce, blocksProducingItems);

                    DisplayManager.Display(_monitoring.MonitoringData, _itemsToProduce, _blocks);
                }

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
            if (updateSource == UpdateType.Trigger)
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

        private static List<IMyProductionBlock> ExtractItemProductionBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.OfType<IMyProductionBlock>().Where(block => block.HasInventory).ToList();

        private static List<IMyTerminalBlock> ExtractStorageBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.Where(block => block.HasInventory).ToList();

        private static List<IMyPowerProducer> ExtractPowerBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.OfType<IMyPowerProducer>().ToList();
    }
}
