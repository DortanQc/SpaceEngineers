using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript.Scripts.ResourceDisplay
{
    public class Program : MyGridProgram
    {
        private const string LCD_BLOC_NAME = "Base - LCD Panel - Resources";
        private const bool LOG = false;

        private readonly ResourceToScan[] _itemsToScan =
        {
            new ResourceToScan { ResourceType = Resource.Type.Cobalt, Threshold = 300 },
            new ResourceToScan { ResourceType = Resource.Type.Gold, Threshold = 2000 },
            new ResourceToScan { ResourceType = Resource.Type.Ice, Threshold = 5000 },
            new ResourceToScan { ResourceType = Resource.Type.Iron, Threshold = 1000 },
            new ResourceToScan { ResourceType = Resource.Type.Magnesium, Threshold = 200 },
            new ResourceToScan { ResourceType = Resource.Type.Nickel, Threshold = 1000 },
            new ResourceToScan { ResourceType = Resource.Type.Platinum, Threshold = 200 },
            new ResourceToScan { ResourceType = Resource.Type.Silicon, Threshold = 1000 },
            new ResourceToScan { ResourceType = Resource.Type.Silver, Threshold = 200 },
            new ResourceToScan { ResourceType = Resource.Type.Uranium, Threshold = 200 },
            new ResourceToScan { ResourceType = Resource.Type.Stone, Threshold = 1000 }
        };

        private readonly IMyTextSurface _lcdPanel;
        private Grid _grid;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            try
            {
                _lcdPanel = GridTerminalSystem.GetBlockWithName(LCD_BLOC_NAME) as IMyTextSurface;

                if (_lcdPanel == null)
                    throw new Exception($"{LCD_BLOC_NAME} block name does noes exists");

                _lcdPanel.WriteText("Ready...");
            }
            catch (Exception ex)
            {
                Echo("Error");
                Echo(ex.ToString());
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
        }

        public void Save() { }

        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                InitGrid();
                Scan();
                BuildText();
            }
            catch (Exception ex)
            {
                _lcdPanel.WriteText("ERRORS...");
                Echo("Error");
                Echo(ex.ToString());

                throw;
            }
        }

        private void InitGrid()
        {
            var storageBlocks = new List<IMyTerminalBlock>();
            var productionBlocks = new List<IMyTerminalBlock>();

            GridTerminalSystem.GetBlocksOfType<IMyCubeBlock>(storageBlocks, block => block.HasInventory);
            GridTerminalSystem.GetBlocksOfType<IMyProductionBlock>(productionBlocks);

            _grid = new Grid(storageBlocks, productionBlocks, Echo, LOG);
        }

        private void Scan()
        {
            ResetQuantities();

            foreach (var itemToScan in _itemsToScan)
            {
                var resource = new Resource(itemToScan.ResourceType);

                itemToScan.CountInStorage = _grid.GetItemAmountInStorage(resource);
            }
        }

        private void ResetQuantities()
        {
            foreach (var itemToScan in _itemsToScan)
                itemToScan.CountInStorage = 0L;
        }

        private void BuildText()
        {
            var stringBuilder = new StringBuilder();
            var hasItemToDisplay = false;

            foreach (var itemToScan in _itemsToScan)
            {
                var item = new Resource(itemToScan.ResourceType);
                var storageCount = itemToScan.CountInStorage / item.Divider;

                if (storageCount >= itemToScan.Threshold) continue;

                hasItemToDisplay = true;

                stringBuilder.AppendLine($"{item.FriendlyName}: {storageCount}");
            }

            if (hasItemToDisplay == false) stringBuilder.AppendLine("All good...");

            _lcdPanel.WriteText(stringBuilder.ToString());
        }

        private class ResourceToScan
        {
            public Resource.Type ResourceType { get; set; }

            public long CountInStorage { get; set; }

            public int Threshold { get; set; }
        }
    }
}