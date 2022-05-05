using VRage.Game;

namespace IngameScript
{
    public class Component
    {
        private MyDefinitionId _itemDefinition;

        public Component(string subtypeId, int amount)
        {
            Amount = amount;
            Build(subtypeId);
        }

        public int Amount { get; }

        public string Name { get; private set; }

        public int Volume { get; private set; }

        private void Build(string subtypeId)
        {
            _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/" + subtypeId);

            switch (subtypeId)
            {
                case "BulletproofGlass":
                    Name = "Bulletproof Glass";
                    Volume = 8;

                    break;
                case "Canvas":
                    Name = "Canvas";
                    Volume = 8;

                    break;
                case "Computer":
                    Name = "Computer";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/ComputerComponent");
                    Volume = 1;

                    break;
                case "Construction":
                    Name = "Construction Comp";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/ConstructionComponent");
                    Volume = 2;

                    break;
                case "Detector":
                    Name = "Detector Comp";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/DetectorComponent");
                    Volume = 6;

                    break;
                case "Display":
                    Name = "Display";
                    Volume = 6;

                    break;
                case "Girder":
                    Name = "Girder";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/GirderComponent");
                    Volume = 2;

                    break;
                case "GravityGenerator":
                    Name = "Gravity Gen. Comp";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/GravityGeneratorComponent");
                    Volume = 200;

                    break;
                case "InteriorPlate":
                    Name = "Interior Plate";
                    Volume = 5;

                    break;
                case "LargeTube":
                    Name = "Large Steel Tube";
                    Volume = 38;

                    break;
                case "Medical":
                    Name = "Medical Comp";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/MedicalComponent");
                    Volume = 160;

                    break;
                case "MetalGrid":
                    Name = "Metal Grid";
                    Volume = 15;

                    break;
                case "Motor":
                    Name = "Motor";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/MotorComponent");
                    Volume = 8;

                    break;
                case "PowerCell":
                    Name = "Power Cell";
                    Volume = 45;

                    break;
                case "RadioCommunication":
                    Name = "Radio Comm. Comp";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/RadioCommunicationComponent");
                    Volume = 70;

                    break;
                case "Reactor":
                    Name = "Reactor Comp";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/ReactorComponent");
                    Volume = 8;

                    break;
                case "SmallTube":
                    Name = "Small Steel Tube";
                    Volume = 2;

                    break;
                case "SolarCell":
                    Name = "Solar Cell";
                    Volume = 20;

                    break;
                case "SteelPlate":
                    Name = "Steel Plate";
                    Volume = 3;

                    break;
                case "Superconductor":
                    Name = "Superconductor";
                    Volume = 8;

                    break;
                case "Thrust":
                    Name = "Thruster Comp";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/ThrustComponent");
                    Volume = 10;

                    break;
                case "Explosives":
                    Name = "Explosives";
                    _itemDefinition = MyDefinitionId.Parse("BlueprintDefinition/ExplosivesComponent");
                    Volume = 2;

                    break;
            }

            // "Automatic Rifle", "AutomaticRifleItem", "MyObjectBuilder_PhysicalGunObject", "AutomaticRifle", 14
            // "Precise Rifle", "PreciseAutomaticRifleItem", "MyObjectBuilder_PhysicalGunObject", "PreciseAutomaticRifle", 14
            // "Rapid Fire Rifle", "RapidFireAutomaticRifleItem", "MyObjectBuilder_PhysicalGunObject", "RapidFireAutomaticRifle", 14
            // "Ultimate Rifle", "UltimateAutomaticRifleItem", "MyObjectBuilder_PhysicalGunObject", "UltimateAutomaticRifle", 14
            //
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
            //
            // "Hydrogen Bottle", "HydrogenBottle", "MyObjectBuilder_GasContainerObject", "HydrogenBottle", 120
            // "NATO 5.56x45mm", "NATO_5p56x45mm", "MyObjectBuilder_AmmoMagazine", "NATO_5p56x45mmMagazine", 0.2
            // "NATO 25x184mm", "NATO_25x184mm", "MyObjectBuilder_AmmoMagazine", "NATO_25x184mmMagazine", 16
            // "Missile 200mm", "Missile200mm", "MyObjectBuilder_AmmoMagazine", "Missile200mm", 60
            //
            // "Cobalt Ore", "Cobalt", "MyObjectBuilder_Ore", "None", 0.37
            // "Gold Ore", "Gold", "MyObjectBuilder_Ore", "None", 0.37
            // "Ice", "Ice", "MyObjectBuilder_Ore", "None", 0.37
            // "Iron Ore", "Iron", "MyObjectBuilder_Ore", "None", 0.37
            // "Magnesium Ore", "Magnesium", "MyObjectBuilder_Ore", "None", 0.37
            // "Nickel Ore", "Nickel", "MyObjectBuilder_Ore", "None", 0.37
            // "Platinum Ore", "Platinum", "MyObjectBuilder_Ore", "None", 0.37
            // "Scrap Ore", "Scrap", "MyObjectBuilder_Ore", "None", 0.37
            // "Silicon Ore", "Silicon", "MyObjectBuilder_Ore", "None", 0.37
            // "Silver Ore", "Silver", "MyObjectBuilder_Ore", "None", 0.37
            // "Stone", "Stone", "MyObjectBuilder_Ore", "None", 0.37
            // "Uranium Ore", "Uranium", "MyObjectBuilder_Ore", "None", 0.37
            // "Cobalt Ingot", "Cobalt", "MyObjectBuilder_Ingot", "None", 0.112
            // "Gold Ingot", "Gold", "MyObjectBuilder_Ingot", "None", 0.052
            // "Gravel", "Stone", "MyObjectBuilder_Ingot", "None", 0.37
            // "Iron Ingot", "Iron", "MyObjectBuilder_Ingot", "None", 0.127
            // "Magnesium Powder", "Magnesium", "MyObjectBuilder_Ingot", "None", 0.575
            // "Nickel Ingot", "Nickel", "MyObjectBuilder_Ingot", "None", 0.112
            // "Platinum Ingot", "Platinum", "MyObjectBuilder_Ingot", "None", 0.047
            // "Silicon Wafer", "Silicon", "MyObjectBuilder_Ingot", "None", 0.429
            // "Silver Ingot", "Silver", "MyObjectBuilder_Ingot", "None", 0.095
            // "Uranium Ingot", "Uranium", "MyObjectBuilder_Ingot", "None", 0.052
        }
    }
}
