# Azurite

## An Unofficial API for Azur Lane

### Introduction

This API aims to provide basic API access for some data surrounding Azur Lane ships.

#### Status and Limitations

This is a new project that's been hacked together over a week or so. As such, it still has some pretty big missing functionality:

- **No skins**: Still unsure whether this will be included in the API.
- **Talents**: Talents aren't available and *may not ever be*. Limitations in how we're retrieving data make it a fragile process to build.
- **Equipment**: Equipment slots are currently included in ship details, but this is currently in preview and **heavily** subject to change
- **Statistics** are implemented, but as they currently stand are also using a (sort of unwieldy) fixed schema. This might change in future versions.

> If there are specific features you would like to see implemented (within reason), open an issue and we can discuss the viability.

### Data Sources

At current, this project retrieves data from the [Azur Lane Wiki](https://azurlane.koumakan.jp/Azur_Lane_Wiki). Since an API that just returned a parsed wiki page per-request would be punishingly awful for the backend wiki, this project includes an "indexing" layer (called `Azurite.Index`) that serves as part-cache, part-database for ship data retrieved from the Wiki. This index does have to be fully populated at least once!

> There's also some light HTTP caching in the Wiki interface code, but `Index` is the superior solution.

#### Alternative Sources

All the ship data retrieved from the Wiki happens in a separate `Azurite.Wiki` project using the `IShipDataProvider` interface so it is reasonably easy to add a new/replacement data source by implementing that interface and returning the correct data.

#### Scraping and Parsing

Since wikis are notoriously awful for programmatic access, we've resorted to just pulling the HTML and scraping it with the XPath-based OpenScraping library. It's far from ideal, but best we have right now.

### Clients and Apps

In addition to the "mainline" API, this project includes a simple command-line to perform local-only basic operations. The CLI must have its index populated before use, but is then able to run completely offline and provides basic ship listing functionality.

Since it uses the excellent `Spectre.Cli`, you can always check the usage by running with `--help`.

### Indexing and Load

> This applies to the CLI as well as hosted APIs

Since we are pulling data from upstream sources (generally the Azur Lane Wiki), we want to be reasonable about the load we put on those sources. Retrieving and parsing the source pages on every request is by far the easiest answer, but would place a ton of extra load on the wiki that we don't want to do to a community resource. This is why the index layer exists, but it still needs data, so on first run (and whenever changes are made) it needs to be rebuilt.

This involves almost 500 separate page loads to the wiki so by default the `IndexBuilder` has a short delay between each page load (5s by default, 2.5s for the CLI). This dramatically increases the time taken to populate the index (can be upwards of an hour by default), but lessens the load on upstream.

> It's possible to disable the delay entirely while building the index, but I recommend against it.

### API Hosting

At this time, we do offer a live hosted instance of the API for public use, but this comes with caveats. The hosted API is only intended for development, testing and *very* light usage scenarios.

That means the hosted API has quite serious restrictions: heavy rate limiting, limited performance and infrequent index rebuilds. If you want to use the API for anything other than very light usage, you should host your own! It's easy and it's possible to run the API itself in almost any app hosting environment.

> Note that the current indexing layer hasn't been tested with large user counts or heavy concurrent load. Report any issues you find!