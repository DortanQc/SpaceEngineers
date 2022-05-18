﻿using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class AutoCleanup
    {
        public static void Cleanup(
            Action<string> echo,
            List<IMyTerminalBlock> storageBlocks,
            List<IMyProductionBlock> producingItemBlocks)
        {
            MoveItemsInProperContainer(storageBlocks, producingItemBlocks);
            StackItems(storageBlocks, producingItemBlocks);
        }

        private static void MoveItemsInProperContainer(
            IReadOnlyCollection<IMyTerminalBlock> storageBlocks,
            IReadOnlyCollection<IMyProductionBlock> producingItemBlocks)
        {
            MoveComponentsToCargo(storageBlocks, producingItemBlocks);
            MoveIngotsToCargo(storageBlocks, producingItemBlocks);
            MoveToolsToCargo(storageBlocks, producingItemBlocks);
            MoveAmmoToCargo(storageBlocks, producingItemBlocks);
            MoveOresToCargo(storageBlocks, producingItemBlocks);
        }

        private static void MoveAmmoToCargo(
            IReadOnlyCollection<IMyTerminalBlock> storageBlocks,
            IEnumerable<IMyProductionBlock> producingItemBlocks)
        {
            var cargos = storageBlocks.Where(IsAmmoStorage).ToList();

            if (!cargos.Any()) return;

            var productionBlockInventories = producingItemBlocks
                .Where(x => !IsAmmoStorage(x))
                .Select(b => b.OutputInventory);

            var storageBlockInventories = storageBlocks
                .Where(x => !IsAmmoStorage(x))
                .Select(b => b.GetInventory());

            var inventories = productionBlockInventories
                .Concat(storageBlockInventories)
                .ToList();

            inventories.ForEach(inventory =>
            {
                var items = new List<MyInventoryItem>();

                inventory.GetItems(items);

                items.Where(item => item.Type.GetItemInfo().IsAmmo)
                    .ToList()
                    .ForEach(item =>
                    {
                        foreach (var cargo in cargos)
                            if (inventory.TransferItemTo(cargo.GetInventory(), item))
                                break;
                    });
            });
        }

        private static void MoveToolsToCargo(
            IReadOnlyCollection<IMyTerminalBlock> storageBlocks,
            IEnumerable<IMyProductionBlock> producingItemBlocks)
        {
            var cargos = storageBlocks.Where(IsToolStorage).ToList();

            if (!cargos.Any()) return;

            var productionBlockInventories = producingItemBlocks
                .Where(x => !IsToolStorage(x))
                .Select(b => b.OutputInventory);

            var storageBlockInventories = storageBlocks
                .Where(x => !IsToolStorage(x))
                .Select(b => b.GetInventory());

            var inventories = productionBlockInventories
                .Concat(storageBlockInventories)
                .ToList();

            inventories.ForEach(inventory =>
            {
                var items = new List<MyInventoryItem>();

                inventory.GetItems(items);

                items.Where(item => item.Type.GetItemInfo().IsTool)
                    .ToList()
                    .ForEach(item =>
                    {
                        foreach (var cargo in cargos)
                            if (inventory.TransferItemTo(cargo.GetInventory(), item))
                                break;
                    });
            });
        }

        private static void MoveComponentsToCargo(
            IReadOnlyCollection<IMyTerminalBlock> storageBlocks,
            IEnumerable<IMyProductionBlock> producingItemBlocks)
        {
            var cargos = storageBlocks.Where(IsComponentStorage).ToList();

            if (!cargos.Any()) return;

            var productionBlockInventories = producingItemBlocks
                .Where(x => !IsComponentStorage(x))
                .Select(b => b.OutputInventory);

            var storageBlockInventories = storageBlocks
                .Where(x => !IsComponentStorage(x))
                .Select(b => b.GetInventory());

            var inventories = productionBlockInventories
                .Concat(storageBlockInventories)
                .ToList();

            inventories.ForEach(inventory =>
            {
                var items = new List<MyInventoryItem>();

                inventory.GetItems(items);

                items.Where(item => item.Type.GetItemInfo().IsComponent)
                    .ToList()
                    .ForEach(item =>
                    {
                        foreach (var cargo in cargos)
                            if (inventory.TransferItemTo(cargo.GetInventory(), item))
                                break;
                    });
            });
        }

        private static void MoveIngotsToCargo(
            IReadOnlyCollection<IMyTerminalBlock> storageBlocks,
            IEnumerable<IMyProductionBlock> producingItemBlocks)
        {
            var cargos = storageBlocks.Where(IsIngotStorage).ToList();

            if (!cargos.Any()) return;

            var productionBlockInventories = producingItemBlocks
                .Where(x => !IsIngotStorage(x))
                .Select(b => b.OutputInventory);

            var storageBlockInventories = storageBlocks
                .Where(block => !(block is IMyProductionBlock))
                .Where(block => !(block is IMyPowerProducer))
                .Where(x => !IsIngotStorage(x))
                .Select(b => b.GetInventory());

            var inventories = productionBlockInventories
                .Concat(storageBlockInventories)
                .ToList();

            inventories.ForEach(inventory =>
            {
                var items = new List<MyInventoryItem>();

                inventory.GetItems(items);

                items.Where(item => item.Type.GetItemInfo().IsIngot)
                    .ToList()
                    .ForEach(item =>
                    {
                        foreach (var cargo in cargos)
                            if (inventory.TransferItemTo(cargo.GetInventory(), item))
                                break;
                    });
            });
        }
        
        private static void MoveOresToCargo(
            IReadOnlyCollection<IMyTerminalBlock> storageBlocks,
            IEnumerable<IMyProductionBlock> producingItemBlocks)
        {
            var cargos = storageBlocks.Where(IsOreStorage).ToList();

            if (!cargos.Any()) return;

            var productionBlockInventories = producingItemBlocks
                .Where(x => !IsOreStorage(x))
                .Select(b => b.OutputInventory);

            var storageBlockInventories = storageBlocks
                .Where(block => !(block is IMyProductionBlock))
                .Where(block => !(block is IMyGasGenerator))
                .Where(x => !IsOreStorage(x))
                .Select(b => b.GetInventory());

            var inventories = productionBlockInventories
                .Concat(storageBlockInventories)
                .ToList();

            inventories.ForEach(inventory =>
            {
                var items = new List<MyInventoryItem>();

                inventory.GetItems(items);

                items.Where(item => item.Type.GetItemInfo().IsOre)
                    .ToList()
                    .ForEach(item =>
                    {
                        foreach (var cargo in cargos)
                            if (inventory.TransferItemTo(cargo.GetInventory(), item))
                                break;
                    });
            });
        }

        private static bool IsComponentStorage(IMyTerminalBlock block)
        {
            var customDataManager = new CustomDataManager(block.CustomData);

            return customDataManager.GetPropertyValue(CustomDataSettings.DEFAULT_STORAGE_FOR_COMPONENTS) != null;
        }

        private static bool IsAmmoStorage(IMyTerminalBlock block)
        {
            var customDataManager = new CustomDataManager(block.CustomData);

            return customDataManager.GetPropertyValue(CustomDataSettings.DEFAULT_STORAGE_FOR_AMMO) != null;
        }

        private static bool IsToolStorage(IMyTerminalBlock block)
        {
            var customDataManager = new CustomDataManager(block.CustomData);

            return customDataManager.GetPropertyValue(CustomDataSettings.DEFAULT_STORAGE_FOR_TOOLS) != null;
        }

        private static bool IsIngotStorage(IMyTerminalBlock block)
        {
            var customDataManager = new CustomDataManager(block.CustomData);

            return customDataManager.GetPropertyValue(CustomDataSettings.DEFAULT_STORAGE_FOR_INGOTS) != null;
        }
        
        private static bool IsOreStorage(IMyTerminalBlock block)
        {
            var customDataManager = new CustomDataManager(block.CustomData);

            return customDataManager.GetPropertyValue(CustomDataSettings.DEFAULT_STORAGE_FOR_ORES) != null;
        }

        private static void StackItems(
            IEnumerable<IMyTerminalBlock> storageBlocks,
            IEnumerable<IMyProductionBlock> producingItemBlocks)
        {
            var inventories = producingItemBlocks
                .Select(b => b.OutputInventory)
                .Concat(storageBlocks.Select(b => b.GetInventory()))
                .ToList();

            inventories.ForEach(inventory =>
            {
                var items = new List<MyInventoryItem>();

                inventory.GetItems(items);

                for (var i = items.Count - 1; i >= 0; i--)
                {
                    var item = items[i];

                    var firstOccurenceIndex = items.FindIndex(x =>
                        x.Type.TypeId == item.Type.TypeId && x.Type.SubtypeId == item.Type.SubtypeId);

                    if (item.Type.GetItemInfo().MaxStackAmount > 1 && firstOccurenceIndex != i)
                        inventory
                            .TransferItemTo(
                                inventory,
                                i,
                                firstOccurenceIndex,
                                true
                            );
                }
            });
        }
    }
}
