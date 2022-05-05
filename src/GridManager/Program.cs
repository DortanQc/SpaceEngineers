using Sandbox.ModAPI.Ingame;
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
        private GridMonitoring _monitoring;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _menuNavigationSystem = new MenuNavigationSystem();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var action = GetScriptActionRequest(updateSource, argument);
            var blocks = ExtractAllTerminalBlocks();

            if (action == MenuNavigationSystem.ScriptActions.Normal)
            {
                var blocksProducingPower = ExtractPowerBlocks(blocks);
                var blocksWithStorage = ExtractStorageBlocks(blocks);
                var blocksProducingItems = ExtractItemProductionBlocks(blocks);

                Monitor(blocksProducingPower, blocksWithStorage, blocksProducingItems);
                AutoProducer.Produce(Echo, _monitoring.MonitoringData, _itemsToProduce, blocksProducingItems);
                DisplayManager.Display(_monitoring.MonitoringData, _itemsToProduce, blocks);
            }
            else
            {
                _menuNavigationSystem.RunAction(action);
            }

            _menuNavigationSystem.DisplayMenu(blocks);
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

        private void Monitor(
            List<IMyPowerProducer> blocksProducingPower,
            List<IMyTerminalBlock> blocksWithStorage,
            List<IMyProductionBlock> blocksProducingItems)
        {
            _monitoring = new GridMonitoring(Echo, blocksProducingPower, blocksWithStorage, blocksProducingItems);
        }

        private List<IMyTerminalBlock> ExtractAllTerminalBlocks()
        {
            var blocks = new List<IMyTerminalBlock>();

            GridTerminalSystem.GetBlocks(blocks);

            return blocks.Where(block => block.IsSameConstructAs(Me)).ToList();
        }

        private static List<IMyProductionBlock> ExtractItemProductionBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.OfType<IMyProductionBlock>().Where(block => block.HasInventory).ToList();

        private static List<IMyTerminalBlock> ExtractStorageBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.Where(block => block.HasInventory).ToList();

        private static List<IMyPowerProducer> ExtractPowerBlocks(IEnumerable<IMyTerminalBlock> blocks) =>
            blocks.OfType<IMyPowerProducer>().ToList();
    }
}
