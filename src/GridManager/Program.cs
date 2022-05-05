using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    public class Program : MyGridProgram
    {
        private readonly Item[] _itemsToProduce =
        {
            new Item("MyObjectBuilder_BlueprintDefinition/BulletproofGlass", 5),
            new Item("MyObjectBuilder_BlueprintDefinition/Canvas", 1),
            new Item("MyObjectBuilder_BlueprintDefinition/ComputerComponent", 200),
            new Item("MyObjectBuilder_BlueprintDefinition/ConstructionComponent", 200),
            new Item("MyObjectBuilder_BlueprintDefinition/DetectorComponent", 1),
            new Item("MyObjectBuilder_BlueprintDefinition/Display", 20),
            new Item("MyObjectBuilder_BlueprintDefinition/ExplosivesComponent", 1),
            new Item("MyObjectBuilder_BlueprintDefinition/GirderComponent", 100),
            new Item("MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent", 1),
            new Item("MyObjectBuilder_BlueprintDefinition/InteriorPlate", 200),
            new Item("MyObjectBuilder_BlueprintDefinition/LargeTube", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/MedicalComponent", 5),
            new Item("MyObjectBuilder_BlueprintDefinition/MetalGrid", 100),
            new Item("MyObjectBuilder_BlueprintDefinition/MotorComponent", 200),
            new Item("MyObjectBuilder_BlueprintDefinition/PowerCell", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent", 10),
            new Item("MyObjectBuilder_BlueprintDefinition/ReactorComponent", 1),
            new Item("MyObjectBuilder_BlueprintDefinition/SmallTube", 100),
            new Item("MyObjectBuilder_BlueprintDefinition/SolarCell", 50),
            new Item("MyObjectBuilder_BlueprintDefinition/SteelPlate", 200),
            new Item("MyObjectBuilder_BlueprintDefinition/Superconductor", 1),
            new Item("MyObjectBuilder_BlueprintDefinition/ThrustComponent", 1)
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
            Echo($"Script Update Source: {updateSource.ToString()}");
            Echo($"Script Argument: {argument}");
            Echo("");

            var blocksProducingPower = ExtractPowerBlocks();
            var blocksWithStorage = ExtractStorageBlocks();
            var blocksProducingItems = ExtractItemProductionBlocks();

            Monitor(blocksProducingPower, blocksWithStorage, blocksProducingItems);
        }

        private void Monitor(
            List<IMyPowerProducer> blocksProducingPower,
            List<IMyTerminalBlock> blocksWithStorage,
            List<IMyProductionBlock> blocksProducingItems)
        {
            _monitoring = new GridMonitoring(Echo, blocksProducingPower, blocksWithStorage, blocksProducingItems);
            AutoProducer.Produce(Echo, _monitoring.MonitoringData, _itemsToProduce, blocksProducingItems);
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
    }
}
