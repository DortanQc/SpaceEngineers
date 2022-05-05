using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

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

        private GridMonitoring _monitoring;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Echo("** ************ **");
            Echo("** Grid Manager **");
            Echo("** ************ **");
            Echo($"Script Update Source: {updateSource}");
            Echo($"Script Argument: {argument}");
            Echo("");

            var blocksProducingPower = ExtractPowerBlocks();
            var blocksWithStorage = ExtractStorageBlocks();
            var blocksProducingItems = ExtractItemProductionBlocks();
            var lcdBlocks = ExtractLCDBlocks();

            Monitor(blocksProducingPower, blocksWithStorage, blocksProducingItems);
            AutoProducer.Produce(Echo, _monitoring.MonitoringData, _itemsToProduce, blocksProducingItems);
            DisplayManager.Display(_monitoring.MonitoringData, _itemsToProduce, lcdBlocks);
        }

        private void Monitor(
            List<IMyPowerProducer> blocksProducingPower,
            List<IMyTerminalBlock> blocksWithStorage,
            List<IMyProductionBlock> blocksProducingItems)
        {
            _monitoring = new GridMonitoring(Echo, blocksProducingPower, blocksWithStorage, blocksProducingItems);
        }

        private List<IMyProductionBlock> ExtractItemProductionBlocks()
        {
            var blocks = new List<IMyProductionBlock>();

            GridTerminalSystem.GetBlocksOfType(
                blocks,
                block => block.HasInventory &&
                         block.IsSameConstructAs(Me));

            return blocks;
        }

        private List<IMyTerminalBlock> ExtractStorageBlocks()
        {
            var blocks = new List<IMyTerminalBlock>();

            GridTerminalSystem.GetBlocksOfType(
                blocks,
                block => block.HasInventory &&
                         block.IsSameConstructAs(Me));

            return blocks;
        }

        private List<IMyPowerProducer> ExtractPowerBlocks()
        {
            var blocks = new List<IMyPowerProducer>();

            GridTerminalSystem.GetBlocksOfType(
                blocks,
                block => block.IsSameConstructAs(Me));

            return blocks;
        }

        private List<IMyTextPanel> ExtractLCDBlocks()
        {
            var blocks = new List<IMyTextPanel>();

            GridTerminalSystem.GetBlocksOfType(
                blocks,
                block => block.IsSameConstructAs(Me));

            return blocks;
        }
    }
}
