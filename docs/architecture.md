# Architecture

## Data flow

`Thing.GetGizmos` contributes one allow command and one disallow command for the
first eligible selected item. The presentation layer collects and deduplicates
the selected items' `ThingDef` values. RimWorld targeting then resolves storage
at the clicked cell. When more than one independently actionable filter is
present, a short `FloatMenu` makes the choice explicit.

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
- A no-op is not presented as a valid targeting cell and never triggers a
  settings callback.

## Intentional one-caller helpers

The production one-caller helpers are `ModBootstrap.InstallPatches`,
`ThingGizmoPatch.AppendCommands`, `ExampleSelectionCommands.SelectedDefinitions`,
`ExampleSelectionCommands.IsActionable`,
`ExampleSelectionCommands.ChooseAndApply`, and
`FilterByExampleService.NeedsChange`. They isolate, respectively, idempotent
bootstrap, lazy enumerable composition, selection ownership, target validation,
ambiguity resolution, and the operation predicate. Promoting them to public
services or merging them into callers would either expose implementation detail
or obscure the layer boundary, so no further abstraction is recommended. If a
second presentation surface is added, selection/target-validation helpers
should then move behind a presentation-facing interface instead of being
copied.
