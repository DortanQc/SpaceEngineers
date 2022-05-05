﻿using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript.Scripts.InventoryDisplay
{
    public class Program : MyGridProgram
    {
        private const string LCD_BLOC_NAME = "Base - LCD Panel - Core Items";
        private const bool LOG = false;
        private readonly string[] _excludedAssemblersForAutoProduction = { "aaa" };

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
                Threshold = 500,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Motor,
                Threshold = 500,
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
                Threshold = 200,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.LargeTube,
                Threshold = 100,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Girder,
                Threshold = 100,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.BulletproofGlass,
                Threshold = 100,
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
                Threshold = 100,
                IsAutoProduction = true
            },
            new ItemToScan
            {
                ComponentType = Component.Type.Reactor,
                Threshold = 20,
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
                Threshold = 5,
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
                AutoProduceItem();
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
            var stringBuilder = new StringBuilder();
            var hasItemToDisplay = false;

            foreach (var itemToScan in _itemsToScan)
            {
                var item = new Component(itemToScan.ComponentType);
                var storageCount = itemToScan.CountInStorage / item.Divider;
                var productionCount = itemToScan.CountInProduction / item.Divider;

                if (storageCount >= itemToScan.Threshold) continue;

                hasItemToDisplay = true;

                stringBuilder.AppendLine(productionCount > 0
                    ? $"{item.FriendlyName}: {storageCount} / {productionCount}"
                    : $"{item.FriendlyName}: {storageCount}");
            }

            if (hasItemToDisplay == false) stringBuilder.AppendLine("All good...");

            _lcdPanel.WriteText(stringBuilder.ToString());
        }

        private class ItemToScan
        {
            public Component.Type ComponentType { get; set; }

            public long CountInStorage { get; set; }

            public long CountInProduction { get; set; }

            public int Threshold { get; set; }

            public bool IsAutoProduction { get; set; }
        }
    }
}
