using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class MonitoringData
    {
        private Dictionary<string, int> _items;
        private Dictionary<string, int> _itemsInProduction;

        public MonitoringData()
        {
            Reset();
        }

        public float CurrentPowerOutput { get; set; }

        public float MaxPowerOutput { get; set; }

        public float HydrogenCapacity { get; set; }

        public double HydrogenFilledRatio { get; set; }

        public List<BatteryInfo> Batteries { get; private set; }

        public List<CargoInfo> Cargos { get; private set; }

        public void Reset()
        {
            CurrentPowerOutput = 0f;
            MaxPowerOutput = 0f;
            HydrogenCapacity = 0f;
            HydrogenFilledRatio = 0d;
            _items = new Dictionary<string, int>();
            _itemsInProduction = new Dictionary<string, int>();
            Batteries = new List<BatteryInfo>();
            Cargos = new List<CargoInfo>();
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
