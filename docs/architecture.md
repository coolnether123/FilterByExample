# Architecture

## Data flow

`Thing.GetGizmos` contributes one allow designator and one disallow designator
for the first eligible selected item. The presentation layer collects and
deduplicates the selected items' `ThingDef` values. RimWorld's own filled-
rectangle designator owns click capture, drag boundaries, and release. The mod
resolves storage only inside that boundary, highlights cells containing filters
that will change, deduplicates shared settings, and applies the release as one
batch. A one-cell overlap still opens a short `FloatMenu` rather than guessing.

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
adapters. Linked storage-group members therefore resolve to their shared group
settings instead of receiving per-building copies.

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
and `EmptyDragRetryPolicy.KeepActive`.
They isolate, respectively, idempotent bootstrap, lazy enumerable composition,
selection ownership, and the tested retry boundary.
`EmptyDragRetryPolicy` currently has one production caller; keeping it internal
is recommended because it creates a game-independent test seam for the exact
five-cell behavior. Promoting any of these helpers to a public service would
expose implementation detail. If a second presentation surface is added,
selection resolution should then move behind a presentation-facing interface
instead of being copied.
