using System;
using System.Collections.Generic;
using System.IO;
using LiteDB;
using Polly;

namespace Azurite.Index
{
    public class ShipDbClient : IDisposable
    {
        // for the record, this class used to do a lot more things, it's just that in hindsight most of 
        // thos things were terrible ideas so I removed them. At this point, it's just a leaky af abstraction
        // over LiteDB that could probably be replaced with a repository.

        private string _dbFilePath;
        private readonly LiteDatabase _db;

        public ShipDbClient() : this("ships.db")
        {
            
            // _db.Mapper.Entity<ShipName>().Id(sn => sn.EN);
        }

        public ShipDbClient(string dbFileName)
        {
            _dbFilePath = System.IO.Path.Combine(Environment.CurrentDirectory, dbFileName);
            _db = new LiteDatabase(_dbFilePath);
        }
        public void Dispose()
        {
            _db.Dispose();
        }

        public LiteCollection<Ship> GetShipCollection() {
            return _db.GetCollection<Ship>("ships");
        }

        public IEnumerable<Ship> GetShips(Func<LiteCollection<Ship>, IEnumerable<Ship>> filter = null) {
            var db = GetShipCollection();
            return filter == null ? db.FindAll() : filter.Invoke(db);
        }

        public bool RebuildSxS(IEnumerable<Ship> ships, string altFileName = "ships.db.rebuild") {
            var backupPath = Path.Combine(Environment.CurrentDirectory, "ships.db.old");
            var rebuildPath = Path.Combine(Environment.CurrentDirectory, "ships.rebuild");
            using (var newDb = new LiteDatabase(System.IO.Path.Combine(Environment.CurrentDirectory, altFileName)))
            {
                var newCollection = newDb.GetCollection<Ship>("ships");
                newCollection.Upsert(ships);
                newCollection.EnsureIndex(s => s.ShipId);
                newCollection.EnsureIndex(s => s.ShipName);
            }
            System.IO.File.Copy(_dbFilePath, backupPath, true);
            var copy = Policy
                .Handle<Exception>()
                .WaitAndRetry(
                    2, 
                    attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (ex, delay, count, _) => {
                    System.Console.WriteLine($"Found {ex} on attempt {count}. Retrying after {delay.Seconds}s...");
            });
            var op = copy.ExecuteAndCapture(() => {
                System.IO.File.Copy(
                    rebuildPath,
                    _dbFilePath,
                    true);
            });
            if (op.Outcome == OutcomeType.Successful) {
                System.IO.File.Delete(rebuildPath);
                return true;
            } else {
                System.IO.File.Copy(backupPath, _dbFilePath, true);
                return false;
            }
        }
    }
}