namespace Azurite
{
    public class Stars {
        /// <summary>
        /// The default/normal star rating of the ship at construction/acquisition time, before any Limit Breaking.
        /// </summary>
        /// <value>The number of stars.</value>
        public int Default {get;set;}

        /// <summary>
        /// The maximum star rating possible for the ship, after Limit Breaking etc.
        /// </summary>
        /// <value>The maximum number of stars.</value>
        public int Max {get;set;}
    }
}
