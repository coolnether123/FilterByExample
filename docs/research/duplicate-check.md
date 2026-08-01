# Duplicate and ecosystem research

Research date: 2026-08-01.

## Closest prior work

[Quick Stockpile Creation](https://steamcommunity.com/sharedfiles/filedetails/?id=1742151109)
by slofa (package ID historically reported as
`longwater.smartstockpilecreation`) is the closest concept found. Its public
description and community references focus on creating item-specific stockpiles
and historically included selected-item/category conveniences. The Workshop
listing is currently removed or incompatible, its last visible update was
2025-06-13, and no public source repository or reusable licensed implementation
was found. Community comments also requested shelf support and source access.

Filter by Example is materially narrower and different: it modifies an existing
clicked storage target, supports linked/shared settings and generic modded
`IStoreSettingsParent` storage, batches actual selected examples, and changes
only exact definitions. Its implementation is original and MIT-licensed.

## Adjacent mods

- Stockpile & Ingredient Filters adds advanced/static filter criteria rather
  than a selected-world-item targeting workflow.
- Filter Manager provides saved/copyable filter presets rather than exact
  by-example mutations.
- Keyz Allow Utilities offers general allow/forbid conveniences, not this
  storage-filter interaction.
- Shelf Limiter changes shelf behavior and is not an example-driven filter
  editor.

## Canonical local corpus

The canonical personal data collection, including the complete local RimWorld
Discord export, was searched for the feature name, allow/disallow-by-example
phrasing, selected-item storage filtering, and the closest known mod names. No
matching implementation, design, or licensed source was found; only unrelated
generic filter discussions appeared.

## Storage ecosystem

[Adaptive Storage Framework](https://github.com/Adaptive-Storage/Adaptive-Storage-Framework)
has public source and is actively designed as a storage framework.
[LWM's Deep Storage](https://steamcommunity.com/sharedfiles/filedetails/?id=1617282896)
and RimFridge-family mods are common compatibility targets. The integration
strategy deliberately avoids their private APIs: any target retaining the
vanilla `IStoreSettingsParent`/`StorageSettings` contract works without a
package-specific dependency.
