using System.Collections.Generic;
using System.Linq;

namespace MyGridAssistant
{
    public class PowerInfo
    {
        public PowerInfo()
        {
            Batteries = new List<BatteryInfo>();
            Reactors = new List<ReactorInfo>();
            SolarPanels = new List<SolarPanelInfo>();
            WindTurbines = new List<WindTurbineInfo>();
            HydrogenEngines = new List<HydrogenEngineInfo>();
        }

        public List<BatteryInfo> Batteries { get; set; }

        public List<ReactorInfo> Reactors { get; set; }

        public List<SolarPanelInfo> SolarPanels { get; set; }

        public List<WindTurbineInfo> WindTurbines { get; set; }

        public List<HydrogenEngineInfo> HydrogenEngines { get; set; }

        public int AvailableUranium { get; set; }

        public int AvailableIce { get; set; }

        public float CurrentPowerOutput =>
            Batteries.Sum(b => b.CurrentOutput) +
            Reactors.Sum(b => b.CurrentOutput) +
            SolarPanels.Sum(b => b.CurrentOutput) +
            WindTurbines.Sum(b => b.CurrentOutput) +
            HydrogenEngines.Sum(b => b.CurrentOutput);

        public float MaxPowerOutput =>
            Batteries.Sum(b => b.MaxOutput) +
            Reactors.Sum(b => b.MaxOutput) +
            SolarPanels.Sum(b => b.MaxOutput) +
            WindTurbines.Sum(b => b.MaxOutput) +
            HydrogenEngines.Sum(b => b.MaxOutput);
    }

    public class HydrogenEngineInfo
    {
        public string Name { get; set; }

        public float CurrentOutput { get; set; }

        public float MaxOutput { get; set; }

        public double FilledRatio { get; set; }
    }

    public class WindTurbineInfo
    {
        public string Name { get; set; }

        public float CurrentOutput { get; set; }

        public float MaxOutput { get; set; }
    }

    public class SolarPanelInfo
    {
        public string Name { get; set; }

        public float CurrentOutput { get; set; }

        public float MaxOutput { get; set; }
    }

    public class ReactorInfo
    {
        public string Name { get; set; }

        public float CurrentOutput { get; set; }

        public float MaxOutput { get; set; }
    }

    public class BatteryInfo
    {
        public bool IsCharging { get; set; }

        public float CurrentStoredPower { get; set; }

        public float MaxStoredPower { get; set; }

        public string Name { get; set; }

        public float CurrentOutput { get; set; }

        public float MaxOutput { get; set; }
    }
}
