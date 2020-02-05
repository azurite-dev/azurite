namespace Azurite
{
    /// <summary>
    /// DO NOT Enum.Parse this! Thanks to differences in regions, there is some extra logic required here. 
    /// Use <see cref="Azurite.Helpers.ParseRarity(string)"/> instead.
    /// </summary>
    public enum Rarity {
        /// <summary>
        /// At this time, this is only used for the special "Unreleased" rarity.
        /// </summary>
        None,
        Common,
        Rare,
        Elite,
        SuperRare,
        UltraRare,
        Priority,
        Decisive
    }
}
