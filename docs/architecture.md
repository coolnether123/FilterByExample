# Architecture

## Data flow

`Thing.GetGizmos` contributes one allow designator and one disallow designator
for the first eligible selected item. The presentation layer collects and
deduplicates the selected items' `ThingDef` values. RimWorld's own filled-
rectangle designator owns click capture, drag boundaries, and release. The mod
resolves storage only inside that boundary, highlights cells containing filters
that will change, deduplicates shared settings, and applies the release as one
batch. A successful single-cell release selects the resolved storage owner,
opens the Inspect tab, and retains a presentation-only highlight state keyed to
the exact target `ThingFilter`; a Harmony prefix on
`Listing_TreeThingFilter.DoThingDef` paints every selected example definition's
row before vanilla labels and checkboxes draw. A one-cell overlap still opens a
short `FloatMenu` rather than guessing, and the chosen target follows the same
selection/highlight path.

An empty selection no larger than five by five cells keeps the selected
designator active for an immediate retry. A successful operation and a larger
empty selection end the tool normally.

Each designator receives its own `KeyBindingDef` through `Command.hotKey`.
RimWorld therefore owns rebinding, key labels, event handling, and conflict
presentation. Both defaults are `None`; the mod does not poll input or reserve
a key that already belongs to vanilla.

`StorageTargetResolver` locates zones, things, and thing comps implementing
`IStoreSettingsParent`. It resolves their actual `StorageSettings`, drops
duplicates by settings-object identity, and constructs `StorageFilterTarget`
adapters while retaining the selectable Thing/Zone owner. Linked storage-group
members therefore resolve to their shared group settings instead of receiving
per-building copies.

`HighlightState` and `HighlightDrawer` are presentation-only. The state stores
the exact target filter, operation, and deduplicated selected definitions; the
row patch only paints matching rows and never changes filter data. The row fill
uses Better Work Tab Dev's restrained translucent teal selection color so a
single item and a multi-item example selection read as one consistent action.

`FilterByExampleService<TDefinition>` is the single mutation service. It is
independent of RimWorld and receives only definition values plus
`IExampleFilterTarget<TDefinition>` adapters. It deduplicates definitions using
definition equality and targets using reference identity, validates parent
constraints for allow operations, skips no-ops, and invokes only `SetAllow` for
entries that differ.

## Mutation invariant

The production mutation boundary is exactly:

```csharp
settings.filter.SetAllow(definition, allow);
```

There is no category mutation, filter copy, bulk reset, preset application,
quality or hit-point mutation, stuff/ingredient mutation, or priority mutation.
RimWorld's own `ThingFilter.SetAllow(ThingDef, bool)` callback performs the
normal immediate storage refresh and save persistence.

## Failure containment

- Invalid, hidden, unspawned, blueprint, frame, and hostile targets are ignored.
- A parent filter that rejects a definition prevents an allow operation but does
  not prevent a disallow operation.
- A modded `IStoreSettingsParent` that throws is skipped with one warning per
  parent type, avoiding repeated hover-time log spam.
- A no-op is not highlighted and never triggers a settings callback.

## Intentional one-caller helpers

The production one-caller helpers are `ModBootstrap.InstallPatches`,
`ThingGizmoPatch.AppendCommands`, `ExampleSelectionCommands.SelectedDefinitions`,
`EmptyDragRetryPolicy.KeepActive`, `HighlightState.Set`,
`HighlightState.Matches`, and `HighlightDrawer.DrawHighlight`.
They isolate, respectively, idempotent bootstrap, lazy enumerable composition,
selection ownership, the tested retry boundary, highlight-state ownership,
target matching, and row drawing.
These helpers currently have one production caller where noted; keeping them
internal is recommended because each creates a focused seam at a game/UI
boundary. Promoting any of these helpers to a public service would expose
implementation detail. If a second presentation surface is added, selection or
highlight resolution should then move behind a presentation-facing interface
instead of being copied.
