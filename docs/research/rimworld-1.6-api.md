# RimWorld 1.6 API investigation

Investigation was performed against the local read-only RimWorld 1.6
decompilation before implementation.

## Storage contracts

- `IStoreSettingsParent.GetStoreSettings()` is the authoritative route to the
  effective mutable `StorageSettings` used by the storage UI.
- `IStoreSettingsParent.GetParentStoreSettings()` exposes a fixed or parent
  constraint. Its `AllowedToAccept(ThingDef)` must be respected for allows.
- `StorageSettings.filter` is a `ThingFilter` constructed with the owner's
  change callback. Calling `ThingFilter.SetAllow(ThingDef, bool)` updates an
  exact definition and invokes that callback.
- `StorageSettings.Priority` is independent state and is intentionally never
  read or written by the feature.
- `IStorageGroupMember.Group` identifies linked storage. In RimWorld 1.6,
  grouped storage members return the group's shared settings, and
  `StorageGroup.Notify_SettingsChanged` fans the refresh out to members.

## UI and targeting contracts

- `ITab_Storage` edits the same `GetStoreSettings().filter` object; the mod does
  not maintain a shadow filter.
- `Thing.GetGizmos` is an enumerable extension point suitable for appending two
  contextual commands while preserving all original gizmos.
- `Command_Target`, `TargetingParameters`, and the current map targeter provide
  the normal RimWorld targeting lifecycle and cursor validation.
- `Selector.SelectedObjectsListForReading` provides the actual selected item
  instances. Minified items are unwrapped with `GetInnerIfMinified()` before
  selecting their exact definition.

## Consequences

The implementation mutates only the resolved real filter through
`SetAllow(ThingDef, bool)`. It does not call `CopyAllowancesFrom`, category
overloads, `SetAllowAll`, `SetDisallowAll`, presets, or priority setters. Normal
callback propagation provides immediate UI and hauling refresh, while the base
game's existing `StorageSettings.ExposeData` and `ThingFilter` serialization
provide save/reload persistence, including definitions supplied by other mods.
