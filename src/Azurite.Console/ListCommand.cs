using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ConsoleTables;
using Spectre.Cli;
using static Azurite.Helpers;
using static Azurite.Console.PrintHelpers;

namespace Azurite.Console
{
    public class CommandSettings : Spectre.Cli.CommandSettings {

        [CommandOption("-v|--verbose")]
        public bool Verbose {get;set;}

    }

    public abstract class CommandBase<T> : AsyncCommand<T> where T : CommandSettings {
        protected readonly IShipDataProvider _provider;

        public CommandBase(IShipDataProvider provider)
        {
            _provider = provider;
        }
    }

    public class ListCommandSettings : CommandSettings
    {
        [Description("Changes how ship details are output. Can be one of 'json', 'table' or 'list'.")]
        [CommandOption("--output")]
        public ListOutput Output {get;set;} = ListOutput.Table;
    }

    public class ListCommand {
        public class ListAllCommand : CommandBase<ListCommandSettings>
        {
            public ListAllCommand(IShipDataProvider provider) : base(provider)
            {
            }

            public override async Task<int> ExecuteAsync(CommandContext context, ListCommandSettings settings)
            {
                var ships = await _provider.GetShipList();
                var details = ships.Select(s => _provider.GetShipDetails(s).Result).ToList();
                return PrintShipList(settings, details);
            }
        }

        public class ListByClassCommand : CommandBase<ListByClassCommand.ListByClassCommandSettings> {
            public ListByClassCommand(IShipDataProvider provider) : base(provider)
            {
            }

            public override async Task<int> ExecuteAsync(CommandContext context, ListByClassCommandSettings settings)
            {
                var ships = await _provider.GetShipList();
                var details = ships
                    .Select(s => _provider.GetShipDetails(s).Result)
                    .Where(c => {
                        return c.Class
                            .Trim()
                            .Equals(settings.Class.Trim(), System.StringComparison.InvariantCultureIgnoreCase);
                    }).ToList();
                return PrintShipList(settings, details);
            }

            public class ListByClassCommandSettings : ListCommandSettings {
                [CommandArgument(0, "<CLASS>")]
                public string Class {get;set;}
            }

        }

        public class ListByTypeCommand : CommandBase<ListByTypeCommand.ListByTypeCommandSettings> {
            public ListByTypeCommand(IShipDataProvider provider) : base(provider)
            {
            }

            public override async Task<int> ExecuteAsync(CommandContext context, ListByTypeCommandSettings settings)
            {
                var ships = await _provider.GetShipList();
                if (settings.Type.Length < 4) {
                    settings.Type = settings.Type.ExpandPrefix();
                }
                var matched = ships
                    .Where(s => s.Type.Equals(settings.Type, StringComparison.InvariantCultureIgnoreCase))
                    .Select(s => _provider.GetShipDetails(s).Result)
                    .Where(s => string.IsNullOrWhiteSpace(settings.Equipment) 
                        ? true 
                        : s.Equipment.Any(e => e.Type.Contains(settings.Equipment)))
                    .Where(s => string.IsNullOrWhiteSpace(settings.FactionName) 
                        ? true 
                        : s.Faction.Name.Equals(settings.FactionName.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                return PrintShipList(settings, matched);
            }

            public class ListByTypeCommandSettings : ListCommandSettings {
                [CommandArgument(0, "<TYPE>")]
                [Description("The type to search by. Will automatically expand abbreviations if provided.")]
                [Required]
                public string Type {get;set;}

                [CommandOption("-e|--equip")]
                [Description("PREVIEW: Limits to ships that have at least one slot that can equip the specified item.")]
                public string Equipment {get;set;}

                [CommandOption("-f|--faction")]
                [Description("Limits results to ships from the specified faction")]
                public string FactionName {get;set;}
            }

        }

        public class ListByFactionCommand : CommandBase<ListByFactionCommand.ListByFactionCommandSettings> {
            public ListByFactionCommand(IShipDataProvider provider) : base(provider)
            {
            }

            public override async Task<int> ExecuteAsync(CommandContext context, ListByFactionCommandSettings settings)
            {
                var ships = await _provider.GetShipList();
                var matched = ships
                    .Where(s => s.FactionName.Equals(settings.Faction.Trim(), StringComparison.InvariantCultureIgnoreCase))
                    .Select(s => _provider.GetShipDetails(s).Result)
                    .OrderBy(s => s.Faction.Name)
                    .ToList();
                return PrintShipList(settings, matched);
            }

            public class ListByFactionCommandSettings : ListCommandSettings {
                [CommandArgument(0, "<FACTION>")]
                [Description("Faction to search for.")]
                [Required]
                public string Faction {get;set;}

            }
        }

        public class ListByNameCommand : CommandBase<ListByNameCommand.ListByNameCommandSettings> {
            public ListByNameCommand(IShipDataProvider provider) : base(provider)
            {
            }

            public override async Task<int> ExecuteAsync(CommandContext context, ListByNameCommandSettings settings)
            {
                var ships = (await _provider.GetShipDetails(settings.ShipName)).ToList();
                ships = ships.Where(sd => settings.GetRetrofit 
                    ? IsRetrofit(sd.ShipId) 
                    : settings.NoRetrofit 
                        ? !IsRetrofit(sd.ShipId) 
                        : true)
                    .ToList();
                if (ships.Count() == 1) {
                    //single ship display
                    return PrintSingleShip(ships.First(), settings.IncludeAllNames);
                } else {
                    return PrintShipList(settings, ships);
                }
            }

            public class ListByNameCommandSettings : ListCommandSettings {
                [CommandArgument(0, "<NAME>")]
                [Description("Name of the ship")]
                [Required]
                public string ShipName {get;set;}

                [CommandOption("-r|--retrofit")]
                [Description("Gets the retrofit where available.")]
                public bool GetRetrofit {get;set;}

                [CommandOption("--no-retrofit")]
                [Description("Specifically excludes the retrofit where available.")]
                public bool NoRetrofit {get;set;}

                [CommandOption("-n|--names")]
                [Description("Lists all names, not just EN ones. Might upset your console.")]
                public bool IncludeAllNames {get;set;}
            }
        }
    }

    

    public enum ListOutput {
        Table,
        List,
        Json
    }
}