using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class Program : MyGridProgram
    {
        private MonitoringData _monitoringData;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _monitoringData = new MonitoringData();
            ScanInventory();
            ScanPower();

            SaveData();
        }

        private void SaveData()
        {
            _monitoringData.Save();
        }

        private void ScanPower()
        {
            var blocksProducingPower = new List<IMyPowerProducer>();

            GridTerminalSystem.GetBlocksOfType(
                blocksProducingPower,
                block => block.IsSameConstructAs(Me));

            blocksProducingPower.ForEach(block =>
            {
                _monitoringData.CurrentOutput += block.CurrentOutput;
                _monitoringData.MaxOutput += block.MaxOutput;
            });
        }

        private void ScanInventory()
        {
            var blocksWithInventory = new List<IMyTerminalBlock>();

            GridTerminalSystem.GetBlocksOfType(
                blocksWithInventory,
                block => block.HasInventory &&
                         block.IsSameConstructAs(Me));

            blocksWithInventory.ForEach(block =>
            {
                var inventory = block.GetInventory();
                var items = new List<MyInventoryItem>();

                _monitoringData.MaxVolume += inventory.MaxVolume;
                _monitoringData.CurrentVolume += inventory.CurrentVolume;
                _monitoringData.CurrentMass += inventory.CurrentMass;

                inventory.GetItems(items);

                items.ForEach(item =>
                {
                    _monitoringData.AddToInventory(item);
                });
            });
        }
    }
}
