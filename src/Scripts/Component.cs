using VRage.Game;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript.Scripts
{
    public class Component
    {
        public enum Type
        {
            Construction,
            Girder,
            MetalGrid,
            InteriorPlate,
            SteelPlate,
            SmallTube,
            LargeTube,
            Motor,
            Display,
            BulletproofGlass,
            Computer,
            Reactor,
            Thrust,
            GravityGenerator,
            Medical,
            RadioCommunication,
            Detector,
            SolarCell,
            PowerCell,
            Explosives,
            Superconductor
        }

        private readonly Type _component;

        public Component(Type component)
        {
            _component = component;
            ItemType = new MyItemType("MyObjectBuilder_Component", component.ToString());
            ItemDefinition =
                MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + ComponentToBlueprint(component));
        }

        public string FriendlyName
        {
            get
            {
                switch (_component)
                {
                    case Type.Construction: return "Construction Component";
                    case Type.Girder: return "Girder";
                    case Type.MetalGrid: return "Metal Grid";
                    case Type.InteriorPlate: return "Interior Plate";
                    case Type.SteelPlate: return "Steel Plate";
                    case Type.SmallTube: return "Small Tube";
                    case Type.LargeTube: return "Large Tube";
                    case Type.Motor: return "Motor";
                    case Type.Display: return "Display";
                    case Type.BulletproofGlass: return "Bulletproof Glass";
                    case Type.Computer: return "Computer";
                    case Type.Reactor: return "Reactor";
                    case Type.Thrust: return "Thrust Component";
                    case Type.GravityGenerator: return "Gravity Generator";
                    case Type.Medical: return "Medical Component";
                    case Type.RadioCommunication: return "Radio Communication";
                    case Type.Detector: return "Detector Component";
                    case Type.SolarCell: return "Solar Cell";
                    case Type.PowerCell: return "Power Cell";
                    case Type.Explosives: return "Explosives";
                    case Type.Superconductor: return "Super Conductor";
                    default: return "";
                }
            }
        }

        public int Divider
        {
            get
            {
                switch (_component)
                {
                    default: return 1000000;
                }
            }
        }

        public MyItemType ItemType { get; }

        public MyDefinitionId ItemDefinition { get; }

        private static string ComponentToBlueprint(Type component)
        {
            switch (component)
            {
                case Type.Computer: return "ComputerComponent";
                case Type.Girder: return "GirderComponent";
                case Type.Construction: return "ConstructionComponent";
                case Type.Motor: return "MotorComponent";
                case Type.Reactor: return "ReactorComponent";
                case Type.Thrust: return "ThrustComponent";
                case Type.GravityGenerator: return "GravityGeneratorComponent";
                case Type.Medical: return "MedicalComponent";
                case Type.RadioCommunication: return "RadioCommunicationComponent";
                case Type.Detector: return "DetectorComponent";
                case Type.Explosives: return "ExplosivesComponent";
                default: return component.ToString();
            }
        }
    }
}