using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class GridMonitoring
    {
        private readonly Action<string> _logger;
        private readonly List<IMyPowerProducer> _powerBlocks;
        private readonly List<IMyProductionBlock> _productionBlocks;
        private readonly List<IMyTerminalBlock> _storageBlocks;
        private MonitoringData _monitoringData;

        public GridMonitoring(
            Action<string> logAction,
            List<IMyPowerProducer> powerBlocks,
            List<IMyTerminalBlock> storageBlocks,
            List<IMyProductionBlock> productionBlocks)
        {
            _logger = logAction;
            _powerBlocks = powerBlocks;
            _storageBlocks = storageBlocks;
            _productionBlocks = productionBlocks;
        }

        public void Scan()
        {
            _monitoringData = new MonitoringData();
            ScanAllInventory();
            ScanPower();
        }

        private void ScanPower()
        {
            _powerBlocks.ForEach(block =>
            {
                _monitoringData.CurrentPowerOutput += block.CurrentOutput;
                _monitoringData.MaxPowerOutput += block.MaxOutput;
            });

            _logger("");
            _logger("** Power **");
            _logger($"Current Power Output: {_monitoringData.CurrentPowerOutput} MW");
            _logger($"Max Power Output: {_monitoringData.MaxPowerOutput} MW");

            var shortage = _monitoringData.MaxPowerOutput - _monitoringData.CurrentPowerOutput;
            if (shortage < 0)
                _logger($"Power Shortage of: {shortage} MW");
            else
                _logger($"Power exceeding of: {shortage} MW");
        }

        private void ScanInventory(IMyInventory inventory)
        {
            var items = new List<MyInventoryItem>();

            _monitoringData.MaxVolume += inventory.MaxVolume;
            _monitoringData.CurrentVolume += inventory.CurrentVolume;
            _monitoringData.CurrentMass += inventory.CurrentMass;

            inventory.GetItems(items);

            items.ForEach(item =>
            {
                _monitoringData.AddToInventory(item);
            });
        }

        private void ScanAllInventory()
        {
            _storageBlocks.ForEach(block =>
            {
                ScanInventory(block.GetInventory());
            });

            _productionBlocks.ForEach(block =>
            {
                ScanInventory(block.InputInventory);
            });

            _productionBlocks.ForEach(block =>
            {
                ScanInventory(block.OutputInventory);
            });

            _logger("");
            _logger("** Items **");
            _logger($"Max Item Volume Capacity: {_monitoringData.MaxVolume} m^3");
            _logger($"Current Item Volume: {_monitoringData.CurrentVolume} m^3");
            _logger($"Current Item Mass: {_monitoringData.CurrentVolume} Kg");

            _logger("");
            _logger("** Components **");
            _monitoringData.Components.ForEach(component =>
            {
                _logger($"{component.Name} {component.Amount}");
            });
        }
    }
}
