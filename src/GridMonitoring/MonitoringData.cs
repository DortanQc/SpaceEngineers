using System.Collections.Generic;
using System.Globalization;
using VRage;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    public class MonitoringData
    {
        private readonly Dictionary<string, int> _components;
        private readonly CustomDataManager _dataManager = new CustomDataManager();

        public MonitoringData()
        {
            MaxVolume = MyFixedPoint.Zero;
            CurrentVolume = MyFixedPoint.Zero;
            CurrentMass = MyFixedPoint.Zero;
            CurrentOutput = 0f;
            MaxOutput = 0f;
            _components = new Dictionary<string, int>();
        }

        public MyFixedPoint MaxVolume
        {
            get
            {
                var value = _dataManager.GetStringProperty(nameof(MaxVolume));

                return MyFixedPoint.DeserializeStringSafe(value);
            }
            set { _dataManager.AddValue(nameof(MaxVolume), value.SerializeString()); }
        }

        public MyFixedPoint CurrentVolume
        {
            get
            {
                var value = _dataManager.GetStringProperty(nameof(CurrentVolume));

                return MyFixedPoint.DeserializeStringSafe(value);
            }
            set { _dataManager.AddValue(nameof(CurrentVolume), value.SerializeString()); }
        }

        public MyFixedPoint CurrentMass
        {
            get
            {
                var value = _dataManager.GetStringProperty(nameof(CurrentMass));

                return MyFixedPoint.DeserializeStringSafe(value);
            }
            set { _dataManager.AddValue(nameof(CurrentMass), value.SerializeString()); }
        }

        public float CurrentOutput
        {
            get
            {
                var value = _dataManager.GetStringProperty(nameof(CurrentOutput));

                return float.Parse(value);
            }
            set { _dataManager.AddValue(nameof(CurrentOutput), value.ToString(CultureInfo.InvariantCulture)); }
        }

        public float MaxOutput
        {
            get
            {
                var value = _dataManager.GetStringProperty(nameof(MaxOutput));

                return float.Parse(value);
            }
            set { _dataManager.AddValue(nameof(MaxOutput), value.ToString(CultureInfo.InvariantCulture)); }
        }

        public void AddToInventory(MyInventoryItem item)
        {
            if (item.Type.GetItemInfo().IsComponent)
                if (_components.ContainsKey(item.Type.SubtypeId))
                    _components[item.Type.SubtypeId] += item.Amount.ToIntSafe();
                else
                    _components.Add(item.Type.SubtypeId, item.Amount.ToIntSafe());
        }

        public void Save() { }
    }
}
