using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript.Scripts.InventoryDisplay
{
    public class Program : MyGridProgram
    {
        private const string LCD_GROUP_NAME = "Station - LCD Panels - Components";
        private readonly string[] _excludedAssemblersForAutoProduction = { };

        private readonly ItemToScan[] _itemsToScan =
        {
            new ItemToScan
            {
                ComponentType = Component.Type.SteelPlate,
                Threshold = 5000,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Computer,
                Threshold = 2000,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Motor,
                Threshold = 1000,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Construction,
                Threshold = 500,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.InteriorPlate,
                Threshold = 500,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.SmallTube,
                Threshold = 500,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.LargeTube,
                Threshold = 300,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Girder,
                Threshold = 500,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.BulletproofGlass,
                Threshold = 300,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Display,
                Threshold = 100,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.MetalGrid,
                Threshold = 300,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Reactor,
                Threshold = 100,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Thrust,
                Threshold = 20,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.GravityGenerator,
                Threshold = 10,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Medical,
                Threshold = 10,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.RadioCommunication,
                Threshold = 5,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Detector,
                Threshold = 20,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.SolarCell,
                Threshold = 75,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.PowerCell,
                Threshold = 200,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Explosives,
                Threshold = 5,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Superconductor,
                Threshold = 50,
                IsAutoProduction = true
            }
        };

        private readonly List<IMyTextSurface> _lcdPanels = new List<IMyTextSurface>();
        private Grid _grid;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        private void WriteText(string message)
        {
            _lcdPanels?.ForEach(lcd => lcd.WriteText(message));
        }

        public void Main(string argument, UpdateType updateSource)
        {
            try
            {
                InitLCD();
                InitGrid();
                Scan();
                AutoProduceItem();
                Cleanup();
                BuildText();
            }
            catch (Exception ex)
            {
                WriteText("ERRORS...");
                Echo("Error");
                Echo(ex.ToString());
                Runtime.UpdateFrequency = UpdateFrequency.None;

                throw;
            }
        }

        private void InitLCD()
        {
            var lcdGroups = GridTerminalSystem.GetBlockGroupWithName(LCD_GROUP_NAME);

            lcdGroups?.GetBlocksOfType(_lcdPanels);

            if (_lcdPanels == null || !_lcdPanels.Any())
                Echo($"{LCD_GROUP_NAME} block name does noes exists");
        }

        private void InitGrid()
        {
            var storageBlocks = new List<IMyTerminalBlock>();
            var productionBlocks = new List<IMyTerminalBlock>();

            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(storageBlocks, block => block.HasInventory);
            GridTerminalSystem.GetBlocksOfType<IMyProductionBlock>(productionBlocks);

            _grid = new Grid(storageBlocks, productionBlocks, Echo);
        }

        private void Scan()
        {
            ResetQuantities();

            foreach (var itemToScan in _itemsToScan)
            {
                var item = new Component(itemToScan.ComponentType);

                itemToScan.CountInStorage = _grid.GetItemAmountInStorage(item);
                itemToScan.CountInProduction = _grid.GetItemAmountInProduction(item);
            }
        }

        private void AutoProduceItem()
        {
            foreach (var itemToScan in _itemsToScan)
            {
                var item = new Component(itemToScan.ComponentType);

                if (!itemToScan.IsAutoProduction) continue;

                var count = (itemToScan.CountInStorage + itemToScan.CountInProduction) / item.Divider;

                if (count >= itemToScan.Threshold) continue;

                var assemblyBlocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblyBlocks);

                var delta = itemToScan.Threshold - count;

                var allowedAssemblyBlocks = assemblyBlocks
                    .Where(a => !_excludedAssemblersForAutoProduction.Contains(a.CustomName))
                    .ToList();

                Grid.ProduceItem(item, delta, allowedAssemblyBlocks);
            }
        }

        private void Cleanup()
        {
            var assemblyBlocks = new List<IMyTerminalBlock>();
            var storageBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(assemblyBlocks);
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(storageBlocks, block => block.HasInventory);

            _grid.CleanupProductionInputInventory(assemblyBlocks);
            _grid.CleanupProductionOutputInventory(assemblyBlocks);
            _grid.CleanupStorageInventory(storageBlocks);
        }

        private void ResetQuantities()
        {
            foreach (var itemToScan in _itemsToScan)
            {
                itemToScan.CountInStorage = 0L;
                itemToScan.CountInProduction = 0L;
            }
        }

        private void BuildText()
        {
            var contentStringBuilder = new StringBuilder();
            var hasItemToDisplay = false;

            foreach (var itemToScan in _itemsToScan)
            {
                var item = new Component(itemToScan.ComponentType);
                var storageCount = itemToScan.CountInStorage / item.Divider;
                var productionCount = itemToScan.CountInProduction / item.Divider;

                if (storageCount >= itemToScan.Threshold) continue;

                hasItemToDisplay = true;

                contentStringBuilder.AppendLine(productionCount > 0
                    ? $"   {item.FriendlyName}: {storageCount} (owned) / {productionCount} (prod.)"
                    : $"   {item.FriendlyName}: {storageCount} (owned)");
            }

            var stringBuilder = new StringBuilder();

            if (hasItemToDisplay == false)
            {
                stringBuilder.AppendLine("All good...");
            }
            else
            {
                stringBuilder.AppendLine("** Components Under Threshold **");
                stringBuilder.AppendLine();
                stringBuilder.AppendLine(contentStringBuilder.ToString());
            }

            WriteText(stringBuilder.ToString());
        }

        private class ItemToScan
        {
            public Component.Type ComponentType { get; set; }

            public long CountInStorage { get; set; }

            public long CountInProduction { get; set; }

            public int Threshold { get; set; }

            public bool IsAutoProduction { get; set; }
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

            public long GetItemAmountInStorage(Component component)
            {
                var currentAmount = 0L;

                foreach (var block in _storageBlocks)
                    currentAmount = CountItemInStorage(component.ItemType.SubtypeId, block, currentAmount);

                foreach (var block in _productionBlocks)
                    currentAmount = CountItemInProductionOutputStorage(
                        component.ItemType.SubtypeId,
                        block as IMyProductionBlock,
                        currentAmount);

                return currentAmount;
            }

            public long GetItemAmountInProduction(Component item)
            {
                var currentAmount = 0L;

                foreach (var block in _productionBlocks)
                    currentAmount = CountItemInProduction(
                        item,
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

            private static long CountItemInProduction(Component item, IMyProductionBlock block, long currentAmount)
            {
                var queue = new List<MyProductionItem>();

                block.GetQueue(queue);

                return currentAmount +
                       queue.Where(q => q.BlueprintId.SubtypeId == item.ItemDefinition.SubtypeId)
                           .Sum(itemInProd => itemInProd.Amount.RawValue);
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

            public static void ProduceItem(
                Component item,
                decimal amount,
                IReadOnlyCollection<IMyTerminalBlock> assemblers)
            {
                var dict = new Dictionary<long, int>();

                foreach (var assembler in assemblers.OfType<IMyProductionBlock>())
                {
                    if (!assembler.CanUseBlueprint(item.ItemDefinition)) continue;

                    var queue = new List<MyProductionItem>();
                    assembler.GetQueue(queue);

                    dict.Add(assembler.EntityId, queue.Count);
                }

                var bestAssembler = dict.OrderBy(x => x.Value)
                    .Select(orderedAssemblersByQueueCount =>
                        assemblers.OfType<IMyProductionBlock>()
                            .First(x =>
                                x.EntityId == orderedAssemblersByQueueCount.Key))
                    .First();

                bestAssembler.AddQueueItem(item.ItemDefinition, amount);
            }

            public void CleanupProductionInputInventory(IEnumerable<IMyTerminalBlock> assemblers)
            {
                foreach (var assembler in assemblers.OfType<IMyProductionBlock>())
                {
                    var items = new List<MyInventoryItem>();

                    assembler.InputInventory.GetItems(items);

                    for (var i = 0; i < items.Count; i++)
                    for (var j = 0; j < _storageBlocks.Count; j++)
                        if (assembler.IsQueueEmpty)
                            if (assembler.InputInventory.TransferItemTo(
                                    _storageBlocks[j].GetInventory(),
                                    i,
                                    null,
                                    true))
                                break;
                }
            }

            public void CleanupProductionOutputInventory(IEnumerable<IMyTerminalBlock> assemblers)
            {
                foreach (var assembler in assemblers.OfType<IMyProductionBlock>())
                {
                    var items = new List<MyInventoryItem>();

                    assembler.OutputInventory.GetItems(items);

                    for (var i = 0; i < items.Count; i++)
                    for (var j = 0; j < _storageBlocks.Count; j++)
                        if (assembler.OutputInventory.TransferItemTo(
                                _storageBlocks[j].GetInventory(),
                                i,
                                null,
                                true))
                            break;
                }
            }

            public void CleanupStorageInventory(IEnumerable<IMyTerminalBlock> storages)
            {
                foreach (var cargo in storages.OfType<IMyCargoContainer>())
                {
                    var customData = cargo.CustomData;
                    var aaa = customData.Split(
                            new string[] { "\r\n", "\r", "\n" },
                            StringSplitOptions.None)
                        .ToList();

                    aaa.ForEach(a => _logger("-> " + a));
                    var items = new List<MyInventoryItem>();

                    cargo.GetInventory().GetItems(items);

                    for (var i = 0; i < items.Count; i++)
                        cargo.GetInventory()
                            .TransferItemTo(
                                cargo.GetInventory(),
                                i,
                                null,
                                true);
                }
            }
        }

        private class Component
        {
            public enum Type
            {
                Construction,
                Girder,
                MetalGrid,
                InteriorPlate,
                SteelPlate,
                SmallTube,
                LargeTube,
                Motor,
                Display,
                BulletproofGlass,
                Computer,
                Reactor,
                Thrust,
                GravityGenerator,
                Medical,
                RadioCommunication,
                Detector,
                SolarCell,
                PowerCell,
                Explosives,
                Superconductor
            }

            private readonly Type _component;

            public Component(Type component)
            {
                _component = component;
                ItemType = new MyItemType("MyObjectBuilder_Component", component.ToString());
                ItemDefinition =
                    MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + ComponentToBlueprint(component));
            }

            public string FriendlyName
            {
                get
                {
                    switch (_component)
                    {
                        case Type.Construction: return "Construction Component";
                        case Type.Girder: return "Girder";
                        case Type.MetalGrid: return "Metal Grid";
                        case Type.InteriorPlate: return "Interior Plate";
                        case Type.SteelPlate: return "Steel Plate";
                        case Type.SmallTube: return "Small Tube";
                        case Type.LargeTube: return "Large Tube";
                        case Type.Motor: return "Motor";
                        case Type.Display: return "Display";
                        case Type.BulletproofGlass: return "Bulletproof Glass";
                        case Type.Computer: return "Computer";
                        case Type.Reactor: return "Reactor";
                        case Type.Thrust: return "Thrust Component";
                        case Type.GravityGenerator: return "Gravity Generator";
                        case Type.Medical: return "Medical Component";
                        case Type.RadioCommunication: return "Radio Communication";
                        case Type.Detector: return "Detector Component";
                        case Type.SolarCell: return "Solar Cell";
                        case Type.PowerCell: return "Power Cell";
                        case Type.Explosives: return "Explosives";
                        case Type.Superconductor: return "Super Conductor";
                        default: return "";
                    }
                }
            }

            public int Divider
            {
                get
                {
                    switch (_component)
                    {
                        default: return 1000000;
                    }
                }
            }

            public MyItemType ItemType { get; }

            public MyDefinitionId ItemDefinition { get; }

            private static string ComponentToBlueprint(Type component)
            {
                switch (component)
                {
                    case Type.Computer: return "ComputerComponent";
                    case Type.Girder: return "GirderComponent";
                    case Type.Construction: return "ConstructionComponent";
                    case Type.Motor: return "MotorComponent";
                    case Type.Reactor: return "ReactorComponent";
                    case Type.Thrust: return "ThrustComponent";
                    case Type.GravityGenerator: return "GravityGeneratorComponent";
                    case Type.Medical: return "MedicalComponent";
                    case Type.RadioCommunication: return "RadioCommunicationComponent";
                    case Type.Detector: return "DetectorComponent";
                    case Type.Explosives: return "ExplosivesComponent";
                    default: return component.ToString();
                }
            }
        }
    }
}
