using VRage.Game;

namespace IngameScript
{
    public class Item
    {
        public enum ItemTypes
        {
            Unknown,
            Component,
            Ore,
            Ingot,
            Tools,
            Ammunition
        }

        public Item(string itemType, int amount)
        {
            Amount = amount;
            Build(itemType);
        }

        public MyDefinitionId ItemDefinition { get; private set; }

        public int Amount { get; }

        public string Name { get; private set; }

        public float Volume { get; private set; }

        public ItemTypes ItemType { get; private set; }

        private void Build(string itemType)
        {
            switch (itemType)
            {
                case "MyObjectBuilder_BlueprintDefinition/BulletproofGlass":
                case "MyObjectBuilder_Component/BulletproofGlass":
                    ItemType = ItemTypes.Component;
                    Name = "Bulletproof Glass";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/BulletproofGlass");
                    Volume = 8;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/Canvas":
                case "MyObjectBuilder_Component/Canvas":
                    ItemType = ItemTypes.Component;
                    Name = "Canvas";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Canvas");
                    Volume = 8;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/ComputerComponent":
                case "MyObjectBuilder_Component/Computer":
                    Name = "Computer";
                    ItemType = ItemTypes.Component;
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ComputerComponent");
                    Volume = 1;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/ConstructionComponent":
                case "MyObjectBuilder_Component/Construction":
                    ItemType = ItemTypes.Component;
                    Name = "Construction Comp";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ConstructionComponent");
                    Volume = 2;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/DetectorComponent":
                case "MyObjectBuilder_Component/Detector":
                    ItemType = ItemTypes.Component;
                    Name = "Detector Comp";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/DetectorComponent");
                    Volume = 6;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/Display":
                case "MyObjectBuilder_Component/Display":
                    ItemType = ItemTypes.Component;
                    Name = "Display";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Display");
                    Volume = 6;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/ExplosivesComponent":
                case "MyObjectBuilder_Component/Explosives":
                    ItemType = ItemTypes.Component;
                    Name = "Explosives";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ExplosivesComponent");
                    Volume = 2;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/GirderComponent":
                case "MyObjectBuilder_Component/Girder":
                    ItemType = ItemTypes.Component;
                    Name = "Girder";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/GirderComponent");
                    Volume = 2;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent":
                case "MyObjectBuilder_Component/GravityGenerator":
                    ItemType = ItemTypes.Component;
                    Name = "Gravity Gen. Comp";
                    ItemDefinition =
                        MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent");

                    Volume = 200;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/InteriorPlate":
                case "MyObjectBuilder_Component/InteriorPlate":
                    ItemType = ItemTypes.Component;
                    Name = "Interior Plate";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/InteriorPlate");
                    Volume = 5;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/LargeTube":
                case "MyObjectBuilder_Component/LargeTube":
                    ItemType = ItemTypes.Component;
                    Name = "Large Steel Tube";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/LargeTube");
                    Volume = 38;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/MedicalComponent":
                case "MyObjectBuilder_Component/Medical":
                    ItemType = ItemTypes.Component;
                    Name = "Medical Comp";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MedicalComponent");
                    Volume = 160;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/MetalGrid":
                case "MyObjectBuilder_Component/MetalGrid":
                    ItemType = ItemTypes.Component;
                    Name = "Metal Grid";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MetalGrid");
                    Volume = 15;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/MotorComponent":
                case "MyObjectBuilder_Component/Motor":
                    ItemType = ItemTypes.Component;
                    Name = "Motor";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MotorComponent");
                    Volume = 8;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/PowerCell":
                case "MyObjectBuilder_Component/PowerCell":
                    ItemType = ItemTypes.Component;
                    Name = "Power Cell";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/PowerCell");
                    Volume = 45;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent":
                case "MyObjectBuilder_Component/RadioCommunication":
                    ItemType = ItemTypes.Component;
                    Name = "Radio Comm. Comp";
                    ItemDefinition =
                        MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent");

                    Volume = 70;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/ReactorComponent":
                case "MyObjectBuilder_Component/Reactor":
                    ItemType = ItemTypes.Component;
                    Name = "Reactor Comp";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ReactorComponent");
                    Volume = 8;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/SmallTube":
                case "MyObjectBuilder_Component/SmallTube":
                    ItemType = ItemTypes.Component;
                    Name = "Small Steel Tube";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SmallTube");
                    Volume = 2;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/SolarCell":
                case "MyObjectBuilder_Component/SolarCell":
                    ItemType = ItemTypes.Component;
                    Name = "Solar Cell";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SolarCell");
                    Volume = 20;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/SteelPlate":
                case "MyObjectBuilder_Component/SteelPlate":
                    ItemType = ItemTypes.Component;
                    Name = "Steel Plate";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SteelPlate");
                    Volume = 3;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/Superconductor":
                case "MyObjectBuilder_Component/Superconductor":
                    ItemType = ItemTypes.Component;
                    Name = "Superconductor";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Superconductor");
                    Volume = 8;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/ThrustComponent":
                case "MyObjectBuilder_Component/Thrust":
                    ItemType = ItemTypes.Component;
                    Name = "Thruster Comp";
                    ItemDefinition = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ThrustComponent");
                    Volume = 10;

                    break;

                case "MyObjectBuilder_BlueprintDefinition/CobaltOreToIngot":
                case "MyObjectBuilder_Ingot/Cobalt":
                    ItemType = ItemTypes.Ingot;
                    Name = "Cobalt Ingot";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.112f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/GoldOreToIngot":
                case "MyObjectBuilder_Ingot/Gold":
                    ItemType = ItemTypes.Ingot;
                    Name = "Gold Ingot";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.052f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/StoneOreToIngot":
                case "MyObjectBuilder_Ingot/Stone":
                    ItemType = ItemTypes.Ingot;
                    Name = "Gravel";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/IronOreToIngot":
                case "MyObjectBuilder_Ingot/Iron":
                    ItemType = ItemTypes.Ingot;
                    Name = "Iron Ingot";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.127f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/MagnesiumOreToIngot":
                case "MyObjectBuilder_Ingot/Magnesium":
                    ItemType = ItemTypes.Ingot;
                    Name = "Magnesium Powder";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.575f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/NickelOreToIngot":
                case "MyObjectBuilder_Ingot/Nickel":
                    ItemType = ItemTypes.Ingot;
                    Name = "Nickel Ingot";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.112f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/PlatinumOreToIngot":
                case "MyObjectBuilder_Ingot/Platinum":
                    ItemType = ItemTypes.Ingot;
                    Name = "Platinum Ingot";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.047f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/SiliconOreToIngot":
                case "MyObjectBuilder_Ingot/Silicon":
                    ItemType = ItemTypes.Ingot;
                    Name = "Silicon Wafer";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.429f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/SilverOreToIngot":
                case "MyObjectBuilder_Ingot/Silver":
                    ItemType = ItemTypes.Ingot;
                    Name = "Silver Ingot";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.095f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/UraniumOreToIngot":
                case "MyObjectBuilder_Ingot/Uranium":
                    ItemType = ItemTypes.Ingot;
                    Name = "Uranium Ingot";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.052f;

                    break;

                case "MyObjectBuilder_Ore/Cobalt":
                    ItemType = ItemTypes.Ore;
                    Name = "Cobalt Ore";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Gold":
                    ItemType = ItemTypes.Ore;
                    Name = "Gold Ore";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Ice":
                    ItemType = ItemTypes.Ore;
                    Name = "Ice";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Iron":
                    ItemType = ItemTypes.Ore;
                    Name = "Iron Ore";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Magnesium":
                    ItemType = ItemTypes.Ore;
                    Name = "Magnesium Ore";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Nickel":
                    ItemType = ItemTypes.Ore;
                    Name = "Nickel Ore";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Platinum":
                    ItemType = ItemTypes.Ore;
                    Name = "Platinum Ore";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Scrap":
                    ItemType = ItemTypes.Ore;
                    Name = "Scrap Ore";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Silicon":
                    ItemType = ItemTypes.Ore;
                    Name = "Silicon Ore";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Silver":
                    ItemType = ItemTypes.Ore;
                    Name = "Silver Ore";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Stone":
                    ItemType = ItemTypes.Ore;
                    Name = "Stone";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Uranium":
                    ItemType = ItemTypes.Ore;
                    Name = "Uranium Ore";
                    ItemDefinition = new MyDefinitionId();
                    Volume = 0.37f;

                    break;

                default:
                    ItemType = ItemTypes.Unknown;
                    Name = itemType;
                    Volume = 0;

                    break;
            }

            // "Automatic Rifle", "AutomaticRifleItem", "MyObjectBuilder_PhysicalGunObject", "AutomaticRifle", 14
            // "Precise Rifle", "PreciseAutomaticRifleItem", "MyObjectBuilder_PhysicalGunObject", "PreciseAutomaticRifle", 14
            // "Rapid Fire Rifle", "RapidFireAutomaticRifleItem", "MyObjectBuilder_PhysicalGunObject", "RapidFireAutomaticRifle", 14
            // "Ultimate Rifle", "UltimateAutomaticRifleItem", "MyObjectBuilder_PhysicalGunObject", "UltimateAutomaticRifle", 14
            // "Welder 1", "WelderItem", "MyObjectBuilder_PhysicalGunObject", "Welder", 8
            // "Welder 2", "Welder2Item", "MyObjectBuilder_PhysicalGunObject", "Welder2", 8
            // "Welder 3", "Welder3Item", "MyObjectBuilder_PhysicalGunObject", "Welder3", 8
            // "Welder 4", "Welder4Item", "MyObjectBuilder_PhysicalGunObject", "Welder4", 8
            // "Grinder 1", "AngleGrinderItem", "MyObjectBuilder_PhysicalGunObject", "AngleGrinder", 20
            // "Grinder 2", "AngleGrinder2Item", "MyObjectBuilder_PhysicalGunObject", "AngleGrinder2", 20
            // "Grinder 3", "AngleGrinder3Item", "MyObjectBuilder_PhysicalGunObject", "AngleGrinder3", 20
            // "Grinder 4", "AngleGrinder4Item", "MyObjectBuilder_PhysicalGunObject", "AngleGrinder4", 20
            // "Drill 1", "HandDrillItem", "MyObjectBuilder_PhysicalGunObject", "HandDrill", 25
            // "Drill 2", "HandDrill2Item", "MyObjectBuilder_PhysicalGunObject", "HandDrill2", 25
            // "Drill 3", "HandDrill3Item", "MyObjectBuilder_PhysicalGunObject", "HandDrill3", 25
            // "Drill 4", "HandDrill4Item", "MyObjectBuilder_PhysicalGunObject", "HandDrill4", 25
            // "Oxygen Bottle", "OxygenBottle", "MyObjectBuilder_OxygenContainerObject", "OxygenBottle", 120
            // "Hydrogen Bottle", "HydrogenBottle", "MyObjectBuilder_GasContainerObject", "HydrogenBottle", 120

            // "NATO 5.56x45mm", "NATO_5p56x45mm", "MyObjectBuilder_AmmoMagazine", "NATO_5p56x45mmMagazine", 0.2
            // "NATO 25x184mm", "NATO_25x184mm", "MyObjectBuilder_AmmoMagazine", "NATO_25x184mmMagazine", 16
            // "Missile 200mm", "Missile200mm", "MyObjectBuilder_AmmoMagazine", "Missile200mm", 60
        }
    }
}