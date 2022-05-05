using System.Collections.Generic;
using System.Linq;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class MonitoringData
    {
        private readonly CustomDataManager _dataManager = new CustomDataManager();
        private readonly Dictionary<string, int> _components;

        public MonitoringData()
        {
            MaxVolume = MyFixedPoint.Zero;
            CurrentVolume = MyFixedPoint.Zero;
            CurrentMass = MyFixedPoint.Zero;
            CurrentPowerOutput = 0f;
            MaxPowerOutput = 0f;
            _components = new Dictionary<string, int>();
        }

        public List<Component> Components
        {
            get
            {
                return _components
                    .Select(c => new Component(c.Key, c.Value))
                    .ToList();
            }
        }

        public MyFixedPoint MaxVolume
        {
            get
            {
                var value = _dataManager.GetPropertyValue(nameof(MaxVolume));

                return MyFixedPoint.DeserializeStringSafe(value);
            }
            set { _dataManager.AddValue(nameof(MaxVolume), value.SerializeString()); }
        }

        public MyFixedPoint CurrentVolume
        {
            get
            {
                var value = _dataManager.GetPropertyValue(nameof(CurrentVolume));

                return MyFixedPoint.DeserializeStringSafe(value);
            }
            set { _dataManager.AddValue(nameof(CurrentVolume), value.SerializeString()); }
        }

        public MyFixedPoint CurrentMass
        {
            get
            {
                var value = _dataManager.GetPropertyValue(nameof(CurrentMass));

                return MyFixedPoint.DeserializeStringSafe(value);
            }
            set { _dataManager.AddValue(nameof(CurrentMass), value.SerializeString()); }
        }

        public float CurrentPowerOutput
        {
            get
            {
                var value = _dataManager.GetPropertyValue(nameof(CurrentPowerOutput));

                return float.Parse(value);
            }
            set { _dataManager.AddValue(nameof(CurrentPowerOutput), value.ToString()); }
        }

        public float MaxPowerOutput
        {
            get
            {
                var value = _dataManager.GetPropertyValue(nameof(MaxPowerOutput));

                return float.Parse(value);
            }
            set { _dataManager.AddValue(nameof(MaxPowerOutput), value.ToString()); }
        }

        public void AddToInventory(MyInventoryItem item)
        {
            if (item.Type.GetItemInfo().IsComponent)
                if (_components.ContainsKey(item.Type.SubtypeId))
                    _components[item.Type.SubtypeId] += item.Amount.ToIntSafe();
                else
                    _components.Add(item.Type.SubtypeId, item.Amount.ToIntSafe());
        }
    }
}
