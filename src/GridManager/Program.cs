using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    public class Program : MyGridProgram
    {
        private GridMonitoring _monitoring;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            var blocksProducingPower = ExtractPowerBlocks();
            var blocksWithStorage = ExtractStorageBlocks();
            var blocksProducingItems = ExtractItemProductionBlocks();

            _monitoring = new GridMonitoring(Echo, blocksProducingPower, blocksWithStorage, blocksProducingItems);

            _monitoring.Scan();
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
