# Filter by Example

Filter by Example lets you update RimWorld 1.6 storage filters from items
already on the map.

## Use

1. Select one item, or multi-select several item stacks.
2. Choose **Allow by example** or **Disallow by example** on the selected
   item's command bar.
3. Click one storage target, or drag across several storage targets.

While dragging, RimWorld draws the selection boundary and the mod highlights
only storage filters that will actually change. Release to update every
highlighted target once. It deliberately omits RimWorld's area-cell number; the
affected highlights are the confirmation. A small empty click or drag up to
five cells wide and tall keeps the tool active so a near miss can be retried
without choosing the command again.

After a successful single-target click, the storage remains selected and its
Inspect tab opens. Every selected example item is highlighted in the storage
filter using the same translucent teal row treatment as Better Work Tab's
selection highlights. Dragging across multiple storages keeps the batch action
and does not replace it with an arbitrary single storage selection.

Both actions have their own entries in RimWorld's normal Controls menu.
**Allow by example** defaults to **Q** and **Disallow by example** defaults to
**E**, keeping both actions under the left hand. Their letters appear in the
top-left of the command gizmos, and either binding can be changed through
RimWorld's ordinary controls.

If a shelf and a stockpile overlap, the mod presents a short chooser naming the
valid targets. It never guesses which filter you meant.

Only the selected items' exact `ThingDef` entries are changed. The operation
does not alter categories, hit-point or quality ranges, special filters, stuff,
ingredients, storage priority, or other unrelated allowances. Duplicate
selected item types and linked shelves sharing one settings object are applied
once.

Commands only appear for actual, storable item examples. A clicked target must
expose a visible, mutable RimWorld storage filter and must belong to the player
or be factionless (as stockpile zones are).

## Requirements

- RimWorld 1.6
- [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
- [SpineLib](https://steamcommunity.com/sharedfiles/filedetails/?id=3778463813),
  the shared runtime for CoolNether123 mods

The mod uses SpineLib only to install its Harmony patches. The mod has no
settings page because there is no persistent behavior to configure.

## Installation

Install Harmony and SpineLib, copy `FilterByExample` into RimWorld's `Mods`
folder, then enable Harmony, SpineLib, and Filter by Example in that order.

The mod stores no save data and is safe to add to or remove from an existing
save.

## Compatibility

The target resolver uses RimWorld's `IStoreSettingsParent` contract rather than
checking specific building definitions. This covers vanilla stockpile zones,
shelves, linked storage groups, storage buildings, and modded storage that
preserves the vanilla contract. See [compatibility notes](docs/compatibility.md)
for tested and investigated boundaries.

## Documentation

The implementation is separated into a pure mutation service, RimWorld storage
adapters, target resolution, and presentation.

- [Architecture](docs/architecture.md)
- [RimWorld 1.6 API investigation](docs/research/rimworld-1.6-api.md)
- [Duplicate research](docs/research/duplicate-check.md)
- [Verification record](docs/verification.md)

## Developer fixture

Live debug actions are isolated in `Developer/FilterByExample.TestFixture`, a
separately loadable developer mod. Build and load that folder only for harness
verification; it is never part of the Filter by Example shipping package.

## License

Released under the [MIT License](LICENSE). Harmony and SpineLib are used under
their own licenses. Release packages include `LICENSE` alongside the runtime
files so the notice travels with every distributed copy.
