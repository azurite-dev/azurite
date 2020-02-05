using System.Linq;

namespace Azurite
{
    public class ShipName {
        public ShipName() { }

        public ShipName(string en, string jp, string cn, string kr) {
            EN = en;
            JP = jp;
            CN = cn;
            KR = kr;
        }

        public string EN {get;set;}
        public string JP {get;set;}
        public string CN {get;set;}
        public string KR {get;set;}

        public static implicit operator string(ShipName s) {
            return s.EN;
        }

        public static implicit operator ShipName(string s) {
            return new ShipName(s, string.Empty, string.Empty, string.Empty);
        }

        public bool AnyNameIs(string s) {
            if (string.IsNullOrWhiteSpace(s)) return false;
            return new[] {EN, JP, CN, KR}.Any(n => n != null && n.Equals(s, System.StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
