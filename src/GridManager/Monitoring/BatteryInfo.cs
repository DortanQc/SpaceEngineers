namespace IngameScript
{
    public class BatteryInfo
    {
        public bool IsCharging { get; set; }

        public float CurrentStoredPower { get; set; }

        public float MaxStoredPower { get; set; }

        public string Name { get; set; }
    }
}