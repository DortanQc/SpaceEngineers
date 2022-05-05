using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class MonitoringData
    {
        private readonly Dictionary<string, int> _items;
        private readonly Dictionary<string, int> _itemsInProduction;

        public MonitoringData()
        {
            MaxVolume = MyFixedPoint.Zero;
            CurrentVolume = MyFixedPoint.Zero;
            CurrentMass = MyFixedPoint.Zero;
            CurrentPowerOutput = 0f;
            MaxPowerOutput = 0f;
            _items = new Dictionary<string, int>();
            _itemsInProduction = new Dictionary<string, int>();
        }

        public MyFixedPoint MaxVolume { get; set; }

        public MyFixedPoint CurrentVolume { get; set; }

        public MyFixedPoint CurrentMass { get; set; }

        public float CurrentPowerOutput { get; set; }

        public float MaxPowerOutput { get; set; }

        public int ChargingBatteries { get; set; }

        public int DischargingBatteries { get; set; }

        public int TotalBatteries { get; set; }

        public float HydrogenCapacity { get; set; }

        public double HydrogenFilledRatio { get; set; }

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
