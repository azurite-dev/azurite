# Azurite CLI (`Azurite.Console`)

The Azurite CLI is a simple local-only way to to get data on ships.

> WARNING: There will be a significant change to this project in future to allow bypassing the index and hitting the backend providers directly. This may change how the current syntax works.

## Usage

Thanks to [this awesome library the CLI uses](https://github.com/spectresystems/spectre.cli), you can use the `--help` option to get full options and descriptions of the available commands.

```bash
azurite --help
azurite list -h
azurite index --help
azurite list by-name --help
```

The basic commands available are:

- Listing ships
  - By name: find ships by name
  - By class: list only ships from a specific class
  - By faction: list all the ships for a given faction
  - By type: list all the ships for a specific hull type (i.e. CA/CL/BB)
- Get details for a single ship
  - This includes all the details that we pull for a ship: names, details, equipment slots etc

Check the `--help` for each command to see the all your options. Want to find all the Eagle Union CLs that can equip torps? Easy.

```bash
azurite list by-type CL --equip Torpedoes --faction "Eagle Union"
```

### Before you start

You will need to build your local index (the `ships.db` file) *before* you can use the CLI to view ship data. You can build the index using the `index` command. For example:

```bash
azurite index --verbose
```

This will actively pull data from the Wiki so make sure you are online to build your index.