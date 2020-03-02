using System;
using System.IO;
using Azurite.Index;
using Azurite.Wiki;
using Microsoft.Extensions.DependencyInjection;

namespace Azurite.Console
{
    public static class ProviderHelpers
    {
        internal static IShipDataProvider GetShipDataProvider(IServiceProvider services) {
            // var providers = services.GetServices<IShipDataProvider>();
            if (File.Exists(System.IO.Path.Combine(Environment.CurrentDirectory, "ships.db"))) {
                return services.GetService<IndexedDataProvider>();
            } else {
                System.Console.BackgroundColor = ConsoleColor.DarkGray;
                System.Console.ForegroundColor = ConsoleColor.Red;
                System.Console.WriteLine("WARNING: No local index found, polling Azur Lane Wiki directly! Run `azurite index` to build a local index.");
                System.Console.ResetColor();
                return services.GetService<WikiSearcher>();
            }
        }
    }
}