using System.ComponentModel;
using System.Threading.Tasks;
using Azurite.Index;
using Spectre.Cli;

namespace Azurite.Console
{
    public class IndexCommand : CommandBase<IndexCommand.IndexCommandSettings>
    {
        private readonly IndexBuilder _builder;

        public IndexCommand(IShipDataProvider provider, IndexBuilder builder) : base(provider)
        {
            _builder = builder;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, IndexCommandSettings settings)
        {
            var existing = await _provider.GetShipList();
            _builder.LogToConsole = settings.Verbose;
            await _builder.BuildShipIndex(System.Convert.ToInt32(settings.Delay * 1000));
            
            var ships = await _provider.GetShipList();
            System.Console.WriteLine($"Index built with {ships.Count} ships");
            return 202;
        }

        public class IndexCommandSettings : CommandSettings {
            [CommandOption("-d|--delay")]
            [Description("Delay in seconds between fetching ship details. Dramatically increases refresh build time (potentially over an hour) but dramatically eases load on source (i.e. wiki). Defaults to 5s.")]
            public float Delay {get;set;} = 5F;

            [CommandOption("-m|--missing")]
            [Description("Attempts to only retrieve details for ships not already in the index.")]
            public bool OnlyMissing {get;set;}
        }
    }
}