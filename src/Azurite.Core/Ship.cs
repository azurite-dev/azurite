using System;
using System.Collections.Generic;
using static Azurite.Helpers;

namespace Azurite
{
    public class Ship
    {
        public static Ship FromSummary(ShipSummary summary) {
            return new Ship {
                ShipId = summary.Id,
                ShipName = summary.Name,
                Type = summary.Type,
                // Subtype = summary.Subtype,
                Faction = new Faction { Name = summary.FactionName, Prefix = string.Empty},
                Rarity = ParseRarity(summary.Rarity)
            };
        }

        /// <summary>
        /// This is the game-internal ship ID, which is also listed out on the Wiki.
        /// </summary>
        /// <remarks>
        /// Importantly, this value is different for retrofit ships than their "normal" counterparts
        /// </remarks>
        /// <value>The game ID of the ship.</value>
        public string ShipId {get; set;}

        /// <summary>
        /// This is the normal ship name, without prefixes or any extras.
        /// </summary>
        /// <remarks>
        /// Importantly, this value is the same for retrofit ships as their "normal" counterparts
        /// </remarks>
        /// <value>The name of the ship.</value>
        public ShipName ShipName {get;set;}

        /// <summary>
        /// This is the hull type of the ship.
        /// </summary>
        /// <remarks>
        /// There's a surprising amount of these! If you want to categorise them as they are in-app, you will need to do this yourself. 
        /// For example, here a BB, BC and BM are all different types. Likewise, CV and CVL are different types here, but grouped in-game.
        /// </remarks>
        /// <value>The full hull type.</value>
        public string Type {get;set;}

        /// <summary>
        /// The subtype of the ship (if applicable). Only a handful of ships actually use this value.
        /// </summary>
        /// <remarks>
        /// At the time of writing, the only Subtypes in use are the Sakura "BBV" ships and Mogami (they change archetype on retrofit).
        /// </remarks>
        /// <value>The subtype of the ship, or null if not applicable.</value>
        // [Obsolete]
        // public string Subtype {get;set;}

        /// <summary>
        /// The class of the ship (if applicable). This will be the same as the <see cref="Ship.ShipName"/> name when its the lead ship of the class.
        /// </summary>
        /// <remarks>
        /// This has little-to-no impact in-game, beyond mentions in biographies. Some perks, however, are class-specific.
        /// </remarks>
        /// <value>The class of the ship.</value>
        public string Class {get;set;}

        /// <summary>
        /// The star rating of the ship.
        /// </summary>
        /// <value>The star rating of the ship.</value>
        public Stars Stars {get;set;}

        /// <summary>
        /// The rarity rating of the ship.
        /// </summary>
        /// <value>The rarity of the ship.</value>
        public Rarity Rarity {get;set;}

        /// <summary>
        /// The URL used to source ship information. This will not always be set.
        /// </summary>
        /// <value>The URL ship details were retrieved from.</value>
        public string Url {get;set;}

        /// <summary>
        /// The time required to build this ship from its pool.
        /// </summary>
        /// <value>The build time of this ship. Will be null if not available from any pools.</value>
        public TimeSpan? BuildTime {get;set;}

        /// <summary>
        /// The "special" build type for ships not available from construction pools.
        /// </summary>
        /// <remarks>Of particular note, this includes Research ships</remarks>
        /// <value>The source for this ship when not available in a pool.</value>
        public string BuildType {get;set;}

        /// <summary>
        /// Not currently implemented!
        /// </summary>
        /// <value>None.</value>
        public IEnumerable<Skin> Skins {get;set;}

        /// <summary>
        /// The faction/nation this ship belongs to.
        /// </summary>
        /// <value>The nation this ship hails from.</value>
        public Faction Faction {get;set;}

        public IEnumerable<EquipmentSlot> Equipment {get;set;}
        
        public override string ToString() {
            return this.ToString(true);
        }

        public string ToString(bool includeType) {
            var prefix = Faction.Prefix ?? string.Empty;
            return $"{(string.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix + " ")}{ShipName.EN}{(includeType ? $" ({Type.ToPrefix()})" : string.Empty)}";
        }
    }
}
