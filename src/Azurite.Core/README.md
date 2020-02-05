# Azurite.Core

This library serves as the primary source of shared object models and logic and is going to be consumed by essentially every Azurite project.

As such, it has *no* dependencies and no client-/application-specific logic. Note that this extends to data sources: while Azur Lane Wiki is currently the priamry source of info, `Azurite.Core` doesn't know about that and just consumes an `IShipDataProvider`.