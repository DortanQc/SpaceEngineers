using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private const string LCD_GROUP_NAME = "Base - LCD Panel - Resources";

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

        private readonly List<IMyTextSurface> _lcdPanels = new List<IMyTextSurface>();
        private Grid _grid;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            try
            {
                var lcdGroups = GridTerminalSystem.GetBlockGroupWithName(LCD_GROUP_NAME);

                lcdGroups?.GetBlocksOfType(_lcdPanels);

                if (_lcdPanels == null || !_lcdPanels.Any())
                    throw new Exception($"{LCD_GROUP_NAME} block name does noes exists");

                WriteText("Ready...");
            }
            catch (Exception ex)
            {
                Echo("Error");
                Echo(ex.ToString());
                Runtime.UpdateFrequency = UpdateFrequency.None;
            }
        }

        private void WriteText(string message)
        {
            _lcdPanels.ForEach(lcd => lcd.WriteText(message));
        }

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
                WriteText("ERRORS...");
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

            _grid = new Grid(storageBlocks, productionBlocks, Echo);
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
            var contentStringBuilder = new StringBuilder();
            var hasItemToDisplay = false;

            foreach (var itemToScan in _itemsToScan)
            {
                var item = new Resource(itemToScan.ResourceType);
                var storageCount = itemToScan.CountInStorage / item.Divider;

                if (storageCount >= itemToScan.Threshold) continue;

                hasItemToDisplay = true;

                contentStringBuilder.AppendLine($"   {item.FriendlyName}: {storageCount}");
            }

            var stringBuilder = new StringBuilder();

            if (hasItemToDisplay == false)
            {
                stringBuilder.AppendLine("All good...");
            }
            else
            {
                stringBuilder.AppendLine("** Ore Under Threshold **");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(contentStringBuilder.ToString());
            }

            WriteText(stringBuilder.ToString());
        }

        private class ResourceToScan
        {
            public Resource.Type ResourceType { get; set; }

            public long CountInStorage { get; set; }

            public int Threshold { get; set; }
        }

        private class Grid
        {
            private readonly Action<string> _logger;
            private readonly List<IMyTerminalBlock> _productionBlocks;
            private readonly List<IMyTerminalBlock> _storageBlocks;

            public Grid(
                List<IMyTerminalBlock> storageBlocks,
                List<IMyTerminalBlock> productionBlocks,
                Action<string> logger)
            {
                _storageBlocks = storageBlocks;
                _productionBlocks = productionBlocks;
                _logger = logger;
            }

            public long GetItemAmountInStorage(Resource resource)
            {
                var currentAmount = 0L;

                foreach (var block in _storageBlocks)
                    currentAmount = CountItemInStorage(resource.Code, block, currentAmount);

                foreach (var block in _productionBlocks)
                    currentAmount = CountItemInProductionOutputStorage(
                        resource.Code,
                        block as IMyProductionBlock,
                        currentAmount);

                return currentAmount;
            }

            private static long CountItemInStorage(string code, IMyEntity block, long currentAmount)
            {
                var items = new List<MyInventoryItem>();

                block.GetInventory().GetItems(items, item => item.Type.SubtypeId == code);

                return currentAmount + items.Sum(item => item.Amount.RawValue);
            }

            private static long CountItemInProductionOutputStorage(
                string code,
                IMyProductionBlock block,
                long currentAmount)
            {
                var items = new List<MyInventoryItem>();

                block.OutputInventory.GetItems(items, item => item.Type.SubtypeId == code);

                return currentAmount + items.Sum(item => item.Amount.RawValue);
            }
        }

        private class Resource
        {
            public enum Type
            {
                Stone,
                Iron,
                Nickel,
                Cobalt,
                Magnesium,
                Silicon,
                Silver,
                Gold,
                Platinum,
                Uranium,
                Ice
            }

            private readonly Type _resource;

            public Resource(Type resource)
            {
                _resource = resource;
            }

            public string Code => _resource.ToString();

            public string FriendlyName
            {
                get
                {
                    switch (_resource)
                    {
                        case Type.Stone: return "Stone / Gravel";
                        case Type.Iron: return "Iron";
                        case Type.Nickel: return "Nickel";
                        case Type.Cobalt: return "Cobalt";
                        case Type.Magnesium: return "Magnesium";
                        case Type.Silicon: return "Silicon";
                        case Type.Silver: return "Silver";
                        case Type.Gold: return "Gold";
                        case Type.Platinum: return "Platinum";
                        case Type.Uranium: return "Uranium";
                        case Type.Ice: return "Ice";
                        default: return "";
                    }
                }
            }

            public int Divider
            {
                get
                {
                    switch (_resource)
                    {
                        default: return 1000000;
                    }
                }
            }
        }
    }
}
