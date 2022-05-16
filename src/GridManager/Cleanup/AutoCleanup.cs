using Sandbox.ModAPI.Ingame;
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
            IEnumerable<IMyTerminalBlock> storageBlocks,
            IEnumerable<IMyProductionBlock> producingItemBlocks)
        {
            StackItems(storageBlocks, producingItemBlocks);
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
