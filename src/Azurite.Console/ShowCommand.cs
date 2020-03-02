using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Spectre.Cli;
using static Azurite.Console.PrintHelpers;

namespace Azurite.Console
{
    public class ShowCommand : CommandBase<ShowCommand.ShowCommandSettings>
    {
        public class ShowCommandSettings : CommandSettings {
            [CommandArgument(0, "<ID>")]
            [Description("The ID of the ship to get details for. Check `list by-name` to get an ID for the ship.")]
            public string ShipId {get;set;}

            [CommandOption("-s|--stat-level")]
            [Description("Ship level to get stats for. Supports '0' (base), '100' or '120'.")]
            public int StatLevel {get;set;} = -1;
        }
        public ShowCommand(IShipDataProvider provider) : base(provider)
        {
        }

        public override async Task<int> ExecuteAsync(CommandContext context, ShowCommandSettings settings)
        {
            var ship = (await _provider.GetShipList()).Where(s => s.Id.Equals(settings.ShipId, System.StringComparison.InvariantCultureIgnoreCase));
            if (ship.Count() != 1) {
                System.Console.WriteLine($"Requested ship was not found by ID '{settings.ShipId}'.");
                return 404;
            }
            var details = await _provider.GetShipDetails(ship.First());
            if (Helpers.IsRetrofit(details.ShipId) && settings.StatLevel == 0) {
                settings.StatLevel = -1;
            }
            return PrintSingleShip(details, statLevel: settings.StatLevel);
        }
    }
}