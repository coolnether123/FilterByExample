# Filter by Example Agent Guide

Own only this repository. Mutate only a resolved target's real
`StorageSettings.filter` through `SetAllow(ThingDef, bool)`. Dedupe definitions
and settings identities, honor parent/fixed filters for allow operations, and
never copy or replace the whole filter. Do not change categories, special
filters, ranges, ingredients, priority, or settings ownership.

Use `IStoreSettingsParent` so vanilla and modded storage remain compatible.
Resolve overlapping shelf/zone targets explicitly. This settings-free mod uses
Spine only for runtime and patch installation.

Keep both native key bindings rebindable and wired through `Command.hotKey`;
do not add global input polling. Use RimWorld's rectangle designator, highlight
only actionable storage, dedupe shared settings, and apply once on release.
Keep every mod gizmo at the far right.

Package metadata, `Tools\CascadeManifest.json`, project files, and the complete
`Tests` directory define support and verification. Follow the shared
build/runtime workflow in `A:\Dev\RimWorld\AGENTS.md`.
