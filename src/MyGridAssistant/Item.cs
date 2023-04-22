using VRage.Game;

namespace MyGridAssistant
{
    public class Item
    {
        public enum ItemSubTypes
        {
            BulletproofGlass,
            Canvas,
            Computer,
            Construction,
            Detector,
            Display,
            Explosives,
            Girder,
            GravityGenerator,
            InteriorPlate,
            LargeTube,
            Medical,
            MetalGrid,
            Motor,
            PowerCell,
            RadioCommunication,
            Reactor,
            SmallTube,
            SolarCell,
            SteelPlate,
            Superconductor,
            Thrust,
            Cobalt,
            Gold,
            Stone,
            Iron,
            Magnesium,
            Nickel,
            Platinum,
            Silicon,
            Silver,
            Uranium,
            Ice,
            Scrap,
            Unknown,
            AutomaticRifle,
            PreciseAutomaticRifle,
            RapidFireAutomaticRifle,
            UltimateAutomaticRifle,
            Nato5P56X45MmMagazine,
            Nato25X184MmMagazine,
            Missile200Mm,
            RapidFireAutomaticRifleGunMag50Rd,
            AutomaticRifleGunMag20Rd,
            SpaceCredit,
            AFullAutoPistolMagazine,
            HydrogenBottle,
            OxygenBottle,
            Welder,
            AngleGrinder,
            HandDrill,
            Welder2,
            AngleGrinder2,
            HandDrill2,
            Powerkit,
            EngineerPlushie,
            HandDrill3,
            HandDrill4,
            AngleGrinder3,
            AngleGrinder4,
            Welder3,
            Welder4,
            AmmoCannonMagazine,
            AssaultCannonShell,
            LargeRailgunAmmo
        }

        public enum ItemTypes
        {
            Unknown,
            Component,
            Ore,
            Ingot,
            Tools,
            Ammunition,
            Consumable
        }

        public Item(MyDefinitionId itemDefinitionId, int amount)
        {
            Amount = amount;
            Build(itemDefinitionId);
        }

        public MyDefinitionId ItemDefinitionId { get; private set; }

        public int Amount { get; set; }

        public string Name { get; private set; }

        public float Volume { get; private set; }

        public ItemTypes ItemType { get; private set; }

        public ItemSubTypes ItemSubType { get; private set; }

        public bool IsSameAs(Item itemToCompareWith)
        {
            if (itemToCompareWith == null) return false;
            if (ItemType != itemToCompareWith.ItemType) return false;
            if (ItemSubType != itemToCompareWith.ItemSubType) return false;

            return true;
        }

        private void Build(MyDefinitionId definitionId)
        {
            switch (definitionId.ToString())
            {
                case "MyObjectBuilder_BlueprintDefinition/BulletproofGlass":
                case "MyObjectBuilder_Component/BulletproofGlass":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.BulletproofGlass;
                    Name = "Bulletproof Glass";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/BulletproofGlass");
                    Volume = 8;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/Canvas":
                case "MyObjectBuilder_Component/Canvas":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Canvas;
                    Name = "Canvas";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Canvas");
                    Volume = 8;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/ComputerComponent":
                case "MyObjectBuilder_Component/Computer":
                    Name = "Computer";
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Computer;
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ComputerComponent");
                    Volume = 1;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/ConstructionComponent":
                case "MyObjectBuilder_Component/Construction":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Construction;
                    Name = "Construction Comp";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ConstructionComponent");
                    Volume = 2;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/DetectorComponent":
                case "MyObjectBuilder_Component/Detector":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Detector;
                    Name = "Detector Comp";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/DetectorComponent");
                    Volume = 6;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/Display":
                case "MyObjectBuilder_Component/Display":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Display;
                    Name = "Display";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Display");
                    Volume = 6;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/ExplosivesComponent":
                case "MyObjectBuilder_Component/Explosives":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Explosives;
                    Name = "Explosives";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ExplosivesComponent");
                    Volume = 2;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/GirderComponent":
                case "MyObjectBuilder_Component/Girder":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Girder;
                    Name = "Girder";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/GirderComponent");
                    Volume = 2;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent":
                case "MyObjectBuilder_Component/GravityGenerator":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.GravityGenerator;
                    Name = "Gravity Gen. Comp";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/GravityGeneratorComponent");
                    Volume = 200;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/InteriorPlate":
                case "MyObjectBuilder_Component/InteriorPlate":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.InteriorPlate;
                    Name = "Interior Plate";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/InteriorPlate");
                    Volume = 5;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/LargeTube":
                case "MyObjectBuilder_Component/LargeTube":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.LargeTube;
                    Name = "Large Steel Tube";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/LargeTube");
                    Volume = 38;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/MedicalComponent":
                case "MyObjectBuilder_Component/Medical":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Medical;
                    Name = "Medical Comp";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MedicalComponent");
                    Volume = 160;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/MetalGrid":
                case "MyObjectBuilder_Component/MetalGrid":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.MetalGrid;
                    Name = "Metal Grid";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MetalGrid");
                    Volume = 15;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/MotorComponent":
                case "MyObjectBuilder_Component/Motor":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Motor;
                    Name = "Motor";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MotorComponent");
                    Volume = 8;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/PowerCell":
                case "MyObjectBuilder_Component/PowerCell":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.PowerCell;
                    Name = "Power Cell";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/PowerCell");
                    Volume = 45;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent":
                case "MyObjectBuilder_Component/RadioCommunication":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.RadioCommunication;
                    Name = "Radio Comm. Comp";
                    ItemDefinitionId =
                        MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/RadioCommunicationComponent");

                    Volume = 70;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/ReactorComponent":
                case "MyObjectBuilder_Component/Reactor":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Reactor;
                    Name = "Reactor Comp";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ReactorComponent");
                    Volume = 8;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/SmallTube":
                case "MyObjectBuilder_Component/SmallTube":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.SmallTube;
                    Name = "Small Steel Tube";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SmallTube");
                    Volume = 2;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/SolarCell":
                case "MyObjectBuilder_Component/SolarCell":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.SolarCell;
                    Name = "Solar Cell";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SolarCell");
                    Volume = 20;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/SteelPlate":
                case "MyObjectBuilder_Component/SteelPlate":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.SteelPlate;
                    Name = "Steel Plate";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SteelPlate");
                    Volume = 3;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/Superconductor":
                case "MyObjectBuilder_Component/Superconductor":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Superconductor;
                    Name = "Superconductor";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Superconductor");
                    Volume = 8;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/ThrustComponent":
                case "MyObjectBuilder_Component/Thrust":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.Thrust;
                    Name = "Thruster Comp";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/ThrustComponent");
                    Volume = 10;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/EngineerPlushie":
                case "MyObjectBuilder_Component/EngineerPlushie":
                    ItemType = ItemTypes.Component;
                    ItemSubType = ItemSubTypes.EngineerPlushie;
                    Name = "Engineer Plushie";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/EngineerPlushie");
                    Volume = 3;

                    break;

                case "MyObjectBuilder_BlueprintDefinition/CobaltOreToIngot":
                case "MyObjectBuilder_Ingot/Cobalt":
                    ItemType = ItemTypes.Ingot;
                    ItemSubType = ItemSubTypes.Cobalt;
                    Name = "Cobalt Ingot";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/CobaltOreToIngot");
                    Volume = 1.12f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/GoldOreToIngot":
                case "MyObjectBuilder_Ingot/Gold":
                    ItemType = ItemTypes.Ingot;
                    ItemSubType = ItemSubTypes.Gold;
                    Name = "Gold Ingot";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/GoldOreToIngot");
                    Volume = 5.2f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/StoneOreToIngot":
                case "MyObjectBuilder_Ingot/Stone":
                    ItemType = ItemTypes.Ingot;
                    ItemSubType = ItemSubTypes.Stone;
                    Name = "Gravel";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/StoneOreToIngot");
                    Volume = 3.7f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/IronOreToIngot":
                case "MyObjectBuilder_Ingot/Iron":
                    ItemType = ItemTypes.Ingot;
                    ItemSubType = ItemSubTypes.Iron;
                    Name = "Iron Ingot";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/IronOreToIngot");
                    Volume = 1.27f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/MagnesiumOreToIngot":
                case "MyObjectBuilder_Ingot/Magnesium":
                    ItemType = ItemTypes.Ingot;
                    ItemSubType = ItemSubTypes.Magnesium;
                    Name = "Magnesium Powder";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MagnesiumOreToIngot");
                    Volume = 5.75f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/NickelOreToIngot":
                case "MyObjectBuilder_Ingot/Nickel":
                    ItemType = ItemTypes.Ingot;
                    ItemSubType = ItemSubTypes.Nickel;
                    Name = "Nickel Ingot";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/NickelOreToIngot");
                    Volume = 1.12f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/PlatinumOreToIngot":
                case "MyObjectBuilder_Ingot/Platinum":
                    ItemType = ItemTypes.Ingot;
                    ItemSubType = ItemSubTypes.Platinum;
                    Name = "Platinum Ingot";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/PlatinumOreToIngot");
                    Volume = 4.7f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/SiliconOreToIngot":
                case "MyObjectBuilder_Ingot/Silicon":
                    ItemType = ItemTypes.Ingot;
                    ItemSubType = ItemSubTypes.Silicon;
                    Name = "Silicon Wafer";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SiliconOreToIngot");
                    Volume = 4.29f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/SilverOreToIngot":
                case "MyObjectBuilder_Ingot/Silver":
                    ItemType = ItemTypes.Ingot;
                    ItemSubType = ItemSubTypes.Silver;
                    Name = "Silver Ingot";
                    ItemDefinitionId = new MyDefinitionId();
                    Volume = 9.5f;

                    break;
                case "MyObjectBuilder_BlueprintDefinition/UraniumOreToIngot":
                case "MyObjectBuilder_Ingot/Uranium":
                    ItemType = ItemTypes.Ingot;
                    ItemSubType = ItemSubTypes.Uranium;
                    Name = "Uranium Ingot";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/UraniumOreToIngot");
                    Volume = 5.2f;

                    break;

                case "MyObjectBuilder_Ore/Cobalt":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Cobalt;
                    Name = "Cobalt Ore";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Cobalt");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Gold":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Gold;
                    Name = "Gold Ore";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Gold");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Ice":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Ice;
                    Name = "Ice";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Ice");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Iron":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Iron;
                    Name = "Iron Ore";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Iron");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Magnesium":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Magnesium;
                    Name = "Magnesium Ore";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Magnesium");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Nickel":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Nickel;
                    Name = "Nickel Ore";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Nickel");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Platinum":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Platinum;
                    Name = "Platinum Ore";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Platinum");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Scrap":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Scrap;
                    Name = "Scrap Ore";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Scrap");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Silicon":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Silicon;
                    Name = "Silicon Ore";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Silicon");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Silver":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Silver;
                    Name = "Silver Ore";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Silver");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Stone":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Stone;
                    Name = "Stone";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Stone");
                    Volume = 0.37f;

                    break;
                case "MyObjectBuilder_Ore/Uranium":
                    ItemType = ItemTypes.Ore;
                    ItemSubType = ItemSubTypes.Uranium;
                    Name = "Uranium Ore";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_Ore/Uranium");
                    Volume = 0.37f;

                    break;

                case "MyObjectBuilder_PhysicalGunObject/AutomaticRifleItem":
                case "MyObjectBuilder_BlueprintDefinition/AutomaticRifle":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.AutomaticRifle;
                    Name = "Automatic Rifle";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/AutomaticRifle");
                    Volume = 14f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/PreciseAutomaticRifleItem":
                case "MyObjectBuilder_BlueprintDefinition/PreciseAutomaticRifle":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.PreciseAutomaticRifle;
                    Name = "Precise Rifle";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/PreciseAutomaticRifle");
                    Volume = 14f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/RapidFireAutomaticRifleItem":
                case "MyObjectBuilder_BlueprintDefinition/RapidFireAutomaticRifle":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.RapidFireAutomaticRifle;
                    Name = "Rapid Fire Rifle";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/RapidFireAutomaticRifle");

                    Volume = 14f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/UltimateAutomaticRifleItem":
                case "MyObjectBuilder_BlueprintDefinition/UltimateAutomaticRifle":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.UltimateAutomaticRifle;
                    Name = "Ultimate Rifle";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/UltimateAutomaticRifle");
                    Volume = 14f;

                    break;

                case "MyObjectBuilder_PhysicalGunObject/WelderItem":
                case "MyObjectBuilder_BlueprintDefinition/Welder":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.Welder;
                    Name = "Welder 1";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Welder");
                    Volume = 8f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/Welder2Item":
                case "MyObjectBuilder_BlueprintDefinition/Welder2":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.Welder2;
                    Name = "Welder 2";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Welder2");
                    Volume = 8f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/Welder3Item":
                case "MyObjectBuilder_BlueprintDefinition/Welder3":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.Welder3;
                    Name = "Welder 3";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Welder3");
                    Volume = 8f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/Welder4Item":
                case "MyObjectBuilder_BlueprintDefinition/Welder4":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.Welder4;
                    Name = "Welder 4";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Welder4");
                    Volume = 8f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/AngleGrinderItem":
                case "MyObjectBuilder_BlueprintDefinition/AngleGrinder":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.AngleGrinder;
                    Name = "Grinder 1";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/AngleGrinder");
                    Volume = 20f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/AngleGrinder2Item":
                case "MyObjectBuilder_BlueprintDefinition/Angle2Grinder":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.AngleGrinder2;
                    Name = "Grinder 2";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/AngleGrinder2");
                    Volume = 20f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/AngleGrinder3Item":
                case "MyObjectBuilder_BlueprintDefinition/Angle3Grinder":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.AngleGrinder3;
                    Name = "Grinder 3";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/AngleGrinder3");
                    Volume = 20f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/AngleGrinder4Item":
                case "MyObjectBuilder_BlueprintDefinition/Angle4Grinder":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.AngleGrinder4;
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/AngleGrinder4");
                    Name = "Grinder 4";
                    Volume = 20f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/HandDrillItem":
                case "MyObjectBuilder_BlueprintDefinition/HandDrill":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.HandDrill;
                    Name = "Drill 1";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/HandDrill");
                    Volume = 25f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/HandDrill2Item":
                case "MyObjectBuilder_BlueprintDefinition/HandDrill2":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.HandDrill2;
                    Name = "Drill 2";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/HandDrill2");
                    Volume = 25f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/HandDrill3Item":
                case "MyObjectBuilder_BlueprintDefinition/HandDrill3":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.HandDrill3;
                    Name = "Drill 3";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/HandDrill3");
                    Volume = 25f;

                    break;
                case "MyObjectBuilder_PhysicalGunObject/HandDrill4Item":
                case "MyObjectBuilder_BlueprintDefinition/HandDrill4":
                    ItemType = ItemTypes.Tools;
                    ItemSubType = ItemSubTypes.HandDrill4;
                    Name = "Drill 4";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/HandDrill4");
                    Volume = 25f;

                    break;
                case "MyObjectBuilder_AmmoMagazine/NATO_5p56x45mm":
                case "MyObjectBuilder_BlueprintDefinition/NATO_5p56x45mmMagazine":
                    ItemType = ItemTypes.Ammunition;
                    ItemSubType = ItemSubTypes.Nato5P56X45MmMagazine;
                    Name = "NATO 5.56x45mm";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/NATO_5p56x45mmMagazine");
                    Volume = 0.2f;

                    break;
                case "MyObjectBuilder_AmmoMagazine/NATO_25x184mm":
                case "MyObjectBuilder_BlueprintDefinition/NATO_25x184mmMagazine":
                    ItemType = ItemTypes.Ammunition;
                    ItemSubType = ItemSubTypes.Nato25X184MmMagazine;
                    Name = "Gatling ammo box";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/NATO_25x184mmMagazine");
                    Volume = 16f;

                    break;
                case "MyObjectBuilder_AmmoMagazine/Missile200mm":
                case "MyObjectBuilder_BlueprintDefinition/Missile200mm":
                    ItemType = ItemTypes.Ammunition;
                    ItemSubType = ItemSubTypes.Missile200Mm;
                    Name = "Missile 200mm";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Missile200mm");
                    Volume = 60f;

                    break;
                case "MyObjectBuilder_AmmoMagazine/RapidFireAutomaticRifleGun_Mag_50rd":
                case "MyObjectBuilder_BlueprintDefinition/RapidFireAutomaticRifleGun_Mag_50rd":
                    ItemType = ItemTypes.Ammunition;
                    ItemSubType = ItemSubTypes.RapidFireAutomaticRifleGunMag50Rd;
                    Name = "MR-50A Magazine";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/RapidFireAutomaticRifleGun_Mag_50rd");

                    Volume = 2.7f;

                    break;
                case "MyObjectBuilder_AmmoMagazine/AutomaticRifleGun_Mag_20rd":
                case "MyObjectBuilder_BlueprintDefinition/AutomaticRifleGun_Mag_20rd":
                    ItemType = ItemTypes.Ammunition;
                    ItemSubType = ItemSubTypes.AutomaticRifleGunMag20Rd;
                    Name = "MR-50A Magazine";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/AutomaticRifleGun_Mag_20rd");

                    Volume = 1.8f;

                    break;
                case "MyObjectBuilder_AmmoMagazine/FullAutoPistolMagazine":
                case "MyObjectBuilder_BlueprintDefinition/FullAutoPistolMagazine":
                    ItemType = ItemTypes.Ammunition;
                    ItemSubType = ItemSubTypes.AFullAutoPistolMagazine;
                    Name = "S-20A Magazine";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/FullAutoPistolMagazine");
                    Volume = 3.45f;

                    break;

                case "MyObjectBuilder_AmmoMagazine/AutocannonClip":
                case "MyObjectBuilder_BlueprintDefinition/AutocannonClip":
                    ItemType = ItemTypes.Ammunition;
                    ItemSubType = ItemSubTypes.AmmoCannonMagazine;
                    Name = "Auto Cannon Magazine";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/AutocannonClip");
                    Volume = 2.64f;

                    break;

                case "MyObjectBuilder_AmmoMagazine/MediumCalibreAmmo":
                case "MyObjectBuilder_BlueprintDefinition/MediumCalibreAmmo":
                    ItemType = ItemTypes.Ammunition;
                    ItemSubType = ItemSubTypes.AssaultCannonShell;
                    Name = "Assault Cannon Shell";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/MediumCalibreAmmo");
                    Volume = 3.0f;

                    break;
                
                case "MyObjectBuilder_AmmoMagazine/LargeRailgunAmmo":
                case "MyObjectBuilder_BlueprintDefinition/LargeRailgunAmmo":
                    ItemType = ItemTypes.Ammunition;
                    ItemSubType = ItemSubTypes.LargeRailgunAmmo;
                    Name = "Large Railgun Sabot";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/LargeRailgunAmmo");
                    Volume = 4.0f;

                    break;

                case "MyObjectBuilder_PhysicalObject/SpaceCredit":
                case "MyObjectBuilder_BlueprintDefinition/SpaceCredit":
                    ItemType = ItemTypes.Consumable;
                    ItemSubType = ItemSubTypes.SpaceCredit;
                    Name = "Space Credit";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/SpaceCredit");
                    Volume = 8.26f;

                    break;
                case "MyObjectBuilder_GasContainerObject/HydrogenBottle":
                case "MyObjectBuilder_BlueprintDefinition/HydrogenBottle":
                    ItemType = ItemTypes.Consumable;
                    ItemSubType = ItemSubTypes.HydrogenBottle;
                    Name = "Hydrogen Bottle";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/HydrogenBottle");
                    Volume = 120f;

                    break;
                case "MyObjectBuilder_OxygenContainerObject/OxygenBottle":
                case "MyObjectBuilder_BlueprintDefinition/OxygenBottle":
                    ItemType = ItemTypes.Consumable;
                    ItemSubType = ItemSubTypes.OxygenBottle;
                    Name = "Oxygen Bottle";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/OxygenBottle");
                    Volume = 120f;

                    break;
                case "MyObjectBuilder_ConsumableItem/Powerkit":
                case "MyObjectBuilder_BlueprintDefinition/Powerkit":
                    ItemType = ItemTypes.Consumable;
                    ItemSubType = ItemSubTypes.Powerkit;
                    Name = "Powerkit";
                    ItemDefinitionId = MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/Powerkit");
                    Volume = 18f;

                    break;

                //
                default:
                    ItemType = ItemTypes.Unknown;
                    ItemSubType = ItemSubTypes.Unknown;
                    Name = definitionId.ToString();
                    Volume = 0;

                    break;
            }
        }
    }
}
