namespace Azurite
{
    public class Faction {
        /// <summary>
        /// The full name of the faction/nation.
        /// </summary>
        /// <value>The faction name.</value>
        public string Name {get;set;}

        /// <summary>
        /// The prefix used for ships from this nation.
        /// </summary>
        /// <remarks>
        /// Contrary to what you'd think, this won't always be set. Not every faction actually has a prefix. 
        /// Notably, a handful of the "collaboration" factions don't have any ship prefix.
        /// </remarks>
        /// <value>The hull prefix.</value>
        public string Prefix {get;set;}
    }
}
