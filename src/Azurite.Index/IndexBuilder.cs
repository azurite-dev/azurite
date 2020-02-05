using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Azurite.Wiki;
using LiteDB;

namespace Azurite.Index
{
    /// <summary>
    /// Client responsible for rebuilding the ships database.
    /// </summary>
    public class IndexBuilder {
        private readonly WikiSearcher _searcher;
        private readonly ShipDbClient _client;

        /// <summary>
        /// Controls whether the index rebuild process logs directly to console
        /// </summary>
        /// <value></value>
        public bool LogToConsole {get;set;}

        /// <summary>
        /// Builds an <see cref="IndexBuilder"/> with the provided searcher and DB client.
        /// </summary>
        /// <param name="searcher">The wiki searcher to use when fetching details.</param>
        /// <param name="client">The DB client to write to.</param>
        public IndexBuilder(WikiSearcher searcher, ShipDbClient client)
        {
            _searcher = searcher;
            _client = client ?? new ShipDbClient();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Honestly this is wrong: we shouldn't be coupling directly to Azurite.Wiki here, 
        /// but resolving an <see cref="Azurite.IShipDataProvider"/> provider from DI.
        /// </remarks>
        /// <param name="searcher">A <see cref="Azurite.Wiki.WikiSearcher"/> instance.</param>
        /// <returns>An <see cref="IndexBuilder"/> builder instance.</returns>
        public IndexBuilder(WikiSearcher searcher) : this(searcher, null)
        {
        }

        private IndexBuilder Log(string message) {
            if (LogToConsole) {
                Console.WriteLine(message);
            }
            return this;
        }

        public async Task BuildShipIndex(int delayMs = 5000, IndexBuildOptions options = IndexBuildOptions.None) {
            var time = new OperationTimer().Start();
            var shipList = await _searcher.GetShipList();
            Log($"Fetched {shipList.Count} ships in {time.Elapsed.ToString()}");
            if (options == IndexBuildOptions.OnlyMissing) {
                using (var db = _client)
                {
                    var current = db.GetShips().ToList();
                    shipList = shipList.Where(s => !current.Any(c => c.ShipId == s.Id) && !s.Rarity.Equals("Unreleased")).ToList();
                }
            }
            var ships = new List<Ship>();
            time.Restart();
            foreach (var ship in shipList)
            {
                try
                {
                    var shipDetails = await _searcher.GetShipDetails(ship);
                    if (LogToConsole) time.WriteToConsole($"Adding {shipDetails.ToString()} to index...");
                    ships.Add(shipDetails);
                    await System.Threading.Tasks.Task.Delay(delayMs);
                }
                catch (Wiki.Diagnostics.ShipNotFoundException ex)
                {
                    Log(ex.Message);
                    // ignored
                }
                catch (Wiki.Diagnostics.FetchPageException ex)
                {
                    Log($"{ex.Message}: {ex.InnerException.Message} ({ex.InnerException.ToString()})");
                    // ignored
                }
            }
            Log($"Retrieved {ships.Count} ship details in {time.Elapsed.ToString()}");
            using (var db = _client ?? new ShipDbClient())
            {
                if (options == IndexBuildOptions.SideBySide) {
                    _client.RebuildSxS(ships.Where(s => s != null));
                } else {
                    var collection = db.GetShipCollection();
                    collection.Upsert(ships.Where(s => s != null));
                    collection.EnsureIndex(s => s.ShipId);
                    collection.EnsureIndex(s => s.ShipName);
                }
            }
            time.Stop();
        }
    }

    public enum IndexBuildOptions {
        None,
        OnlyMissing,
        SideBySide
    }
}
