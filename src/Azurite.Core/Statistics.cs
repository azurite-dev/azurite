namespace Azurite.Core
{
    public class ShipStatistics
    {
        public int HP { get; set; }
        public int Firepower { get; set; }
        public int AntiAir { get; set; }
        public int AntiSub { get; set; }
        public int Torpedo { get; set; }
        public int Aviation { get; set; }
        public int Luck { get; set; }
        public int Reload { get; set; }
        public int Evasion { get; set; }
        public int OilCost { get; set; }
        public int Speed { get; set; }
        public int Accuracy { get; set; }
        public int Oxygen { get; set; }
        public int Ammunition { get; set; }
        public ArmorType Armor { get; set; }

        public enum ArmorType
        {
            Light,
            Medium,
            Heavy
        }
    }
}