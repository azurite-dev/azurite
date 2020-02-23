using System;
using System.Linq;
using System.Threading.Tasks;
using Azurite.Index;
using Azurite.Wiki;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Cli;
using Spectre.Cli.Extensions.DependencyInjection;

namespace Azurite.Console
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // await GetShip();
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
            var services = new ServiceCollection();
            services.AddSingleton<WikiSearcher>();
            services.AddSingleton<ShipDbClient>();
            services.AddSingleton<IndexBuilder>();
            services.AddSingleton<IShipDataProvider, IndexedDataProvider>();
            var app = new CommandApp(new DependencyInjectionRegistrar(services));
            app.Configure(c => {
                c.AddBranch("list", ls => {
                    ls.AddCommand<ListCommand.ListAllCommand>("all");
                    ls.AddCommand<ListCommand.ListByClassCommand>("by-class");
                    ls.AddCommand<ListCommand.ListByTypeCommand>("by-type");
                    ls.AddCommand<ListCommand.ListByFactionCommand>("by-faction");
                    ls.AddCommand<ListCommand.ListByNameCommand>("by-name");
                });
                c.AddCommand<IndexCommand>("index");
                c.AddCommand<ShowCommand>("get");
            });
            return await app.RunAsync(args);
        }

#if DEBUG
        static async Task GetShip() {
            var searcher = new WikiSearcher();
            var result = (await searcher.GetShipDetails("Mogami")).ToList();
            var results = (await searcher.GetShipDetails("Mogami")).ToList();
        }
#endif
    }
}
