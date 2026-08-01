# Compatibility

## Compatibility model

Filter by Example depends on behavior, not package IDs. A target is supported
when its zone, thing, or thing comp implements RimWorld's
`IStoreSettingsParent`, returns a non-null `StorageSettings.filter`, and exposes
the storage tab. No storage mod is patched directly and no reflection is used.

| Target type | Expected behavior | Basis |
| --- | --- | --- |
| Vanilla stockpile zone | Supported | Zone implements `IStoreSettingsParent` |
| Vanilla shelf/storage building | Supported | Building implements `IStoreSettingsParent` |
| Linked shelves/storage groups | Supported | Members return shared group settings; identity deduplication prevents repeat writes |
| Multi-cell storage building | Supported | Every occupied cell resolves the same settings identity |
| Storage implemented by a `ThingComp` | Supported | Resolver inspects all comps implementing `IStoreSettingsParent` |
| Locked/fixed parent filter | Safe partial support | Parent-ineligible definitions are skipped on allow; disallow remains valid |
| Overlapping shelf and stockpile | Supported | Explicit target chooser; no implicit precedence |
| LWM's Deep Storage family | Contract-compatible when the build preserves `IStoreSettingsParent` | Uses vanilla storage settings rather than a package-specific API |
| Adaptive Storage Framework | Contract-compatible for storage exposing the vanilla parent/settings contract | No dependency or reflective coupling |
| RimFridge storage | Contract-compatible when exposed through vanilla storage settings | No definition whitelist |
| Multiplayer | Not claimed | Command synchronization needs validation against a specific Multiplayer release |

The three named storage frameworks are investigated compatibility targets, not
hard dependencies. Future releases that stop exposing vanilla storage settings
would require a small adapter at `StorageTargetResolver`, leaving the mutation
service unchanged.

## Dependencies

The mod requires Harmony and Spine 1.2+. Spine negotiates only
`HarmonyPatching`; no optional Spine settings or save-data facilities are used.
The Harmony patch touches only `Thing.GetGizmos` and appends commands without
replacing vanilla gizmos.
