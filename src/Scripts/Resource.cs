namespace IngameScript.Scripts
{
    public class Resource
    {
        public enum Type
        {
            Stone,
            Iron,
            Nickel,
            Cobalt,
            Magnesium,
            Silicon,
            Silver,
            Gold,
            Platinum,
            Uranium,
            Ice
        }

        private readonly Type _resource;

        public Resource(Type resource)
        {
            _resource = resource;
        }

        public string Code => _resource.ToString();

        public string FriendlyName
        {
            get
            {
                switch (_resource)
                {
                    case Type.Stone: return "Stone / Gravel";
                    case Type.Iron: return "Iron";
                    case Type.Nickel: return "Nickel";
                    case Type.Cobalt: return "Cobalt";
                    case Type.Magnesium: return "Magnesium";
                    case Type.Silicon: return "Silicon";
                    case Type.Silver: return "Silver";
                    case Type.Gold: return "Gold";
                    case Type.Platinum: return "Platinum";
                    case Type.Uranium: return "Uranium";
                    case Type.Ice: return "Ice";
                    default: return "";
                }
            }
        }

        public int Divider
        {
            get
            {
                switch (_resource)
                {
                    default: return 1000000;
                }
            }
        }
    }
}