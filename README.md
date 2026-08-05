# Filter by Example

Filter by Example is a RimWorld 1.6 quality-of-life mod for making precise
storage-filter changes from items already on the map.

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
- [Spine](https://github.com/coolnether123/Spine) — the shared runtime used by
  CoolNether123 mods

Spine is used only for its negotiated Harmony-patching capability. This mod has
no settings page because there is no persistent behavior to configure.

## Installation

Install Harmony and Spine, copy `FilterByExample` into RimWorld's `Mods`
folder, then enable Harmony, Spine, and Filter by Example in that order.

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

Released under the [MIT License](LICENSE). Harmony and Spine are used under
their own licenses. Release packages include `LICENSE` alongside the runtime
files so the notice travels with every distributed copy.
