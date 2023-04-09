using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace MyGridAssistant
{
    public class MonitoringData
    {
        private Dictionary<MyItemType, Item> _items;
        private Dictionary<MyDefinitionId, Item> _itemsInProduction;

        public MonitoringData()
        {
            Reset();
        }

        public PowerInfo PowerConsumption { get; private set; }

        public List<CargoInfo> Cargos { get; private set; }

        public List<HydrogenTankInfo> HydrogenTanks { get; private set; }

        public void Reset()
        {
            _items = new Dictionary<MyItemType, Item>();
            _itemsInProduction = new Dictionary<MyDefinitionId, Item>();
            PowerConsumption = new PowerInfo();
            Cargos = new List<CargoInfo>();
            HydrogenTanks = new List<HydrogenTankInfo>();
        }

        public List<Item> GetItemsInProduction()
        {
            return _itemsInProduction
                .Select(c => c.Value)
                .ToList();
        }

        public List<Item> GetItemsInProduction(Item.ItemTypes type)
        {
            return _itemsInProduction
                .Where(c => c.Value.ItemType == type)
                .Select(c => c.Value)
                .ToList();
        }

        public List<Item> GetItems()
        {
            return _items
                .Select(c => c.Value)
                .ToList();
        }

        public List<Item> GetItems(Item.ItemTypes type)
        {
            return _items
                .Where(c => c.Value.ItemType == type)
                .Select(c => c.Value)
                .ToList();
        }

        public void AddToInventory(MyInventoryItem item)
        {
            if (_items.ContainsKey(item.Type))
                _items[item.Type].Amount += item.Amount.ToIntSafe();
            else
                _items.Add(item.Type, new Item(item.Type, item.Amount.ToIntSafe()));
        }

        public void AddToQueuedList(MyProductionItem queuedItem)
        {
            if (_itemsInProduction.ContainsKey(queuedItem.BlueprintId))
                _itemsInProduction[queuedItem.BlueprintId].Amount += queuedItem.Amount.ToIntSafe();
            else
                _itemsInProduction.Add(queuedItem.BlueprintId, new Item(queuedItem.BlueprintId, queuedItem.Amount.ToIntSafe()));
        }
    }
}
