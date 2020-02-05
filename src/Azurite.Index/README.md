# Azurite.Index

This project is, at its simplest, a caching layer. It builds an index of ship data based on the output of another `IShipDataProvider` then exposes that back to the consuming app through `IShipDataProvider`. This means we don't have to hit web servers/scrape/parse/whatever on every request.

> Currently, this project is coupled directly to the `WikiSearcher` in `Azurite.Wiki`. This should probably be fixed.

The backing store is a small LiteDB database called `ships.db` which indexes every ship.