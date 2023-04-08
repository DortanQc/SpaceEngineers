using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace MyGridAssistant
{
    public class MonitoringData
    {
        private Dictionary<string, int> _items;
        private Dictionary<string, int> _itemsInProduction;

        public MonitoringData()
        {
            Reset();
        }

        public PowerInfo PowerConsumption { get; private set; }

        public List<CargoInfo> Cargos { get; private set; }

        public List<HydrogenTankInfo> HydrogenTanks { get; private set; }

        public void Reset()
        {
            _items = new Dictionary<string, int>();
            _itemsInProduction = new Dictionary<string, int>();
            PowerConsumption = new PowerInfo();
            Cargos = new List<CargoInfo>();
            HydrogenTanks = new List<HydrogenTankInfo>();
        }

        public List<Item> GetItemsInProduction()
        {
            return _itemsInProduction
                .Select(c => new Item(c.Key, c.Value))
                .ToList();
        }

        public List<Item> GetItemsInProduction(Item.ItemTypes type)
        {
            return _itemsInProduction
                .Select(c => new Item(c.Key, c.Value))
                .Where(c => c.ItemType == type)
                .ToList();
        }

        public List<Item> GetItems()
        {
            return _items
                .Select(c => new Item(c.Key, c.Value))
                .ToList();
        }

        public List<Item> GetItems(Item.ItemTypes type)
        {
            return _items
                .Select(c => new Item(c.Key, c.Value))
                .Where(c => c.ItemType == type)
                .ToList();
        }

        public void AddToInventory(MyInventoryItem item)
        {
            var key = item.Type.ToString();

            if (_items.ContainsKey(key))
                _items[key] += item.Amount.ToIntSafe();
            else
                _items.Add(key, item.Amount.ToIntSafe());
        }

        public void AddToQueuedList(MyProductionItem queuedItem)
        {
            var key = queuedItem.BlueprintId.ToString();

            if (_itemsInProduction.ContainsKey(key))
                _itemsInProduction[key] += queuedItem.Amount.ToIntSafe();
            else
                _itemsInProduction.Add(key, queuedItem.Amount.ToIntSafe());
        }
    }
}
