# Verification

## Automated contracts

`Tests/Mod.Tests.csproj` is a runtime-independent suite for the shared mutation
service. It covers:

1. exact-definition allow and disallow;
2. selected-definition and shared-settings deduplication;
3. parent-filter allow rejection;
4. preservation of categories, special filters, hit-point range, quality range,
   stuff, ingredient, and priority fingerprints;
5. independent multi-target batches;
6. a serialized/reloaded modded-definition fixture; and
7. no-op behavior without notifications.

Run it with:

```powershell
dotnet run --project Tests/Mod.Tests.csproj -c Release
```

`Tests/RimWorldContracts.Tests.csproj` compiles the same service and real
`StorageFilterTarget` against the resolved RimWorld 1.6 assemblies. It is a
compile-time API contract: RimWorld's `ThingDef` initialization requires Unity
and therefore cannot execute correctly as a standalone .NET process.

The production assembly includes a developer-mode debug action named
**Filter by Example / Run exact-filter contract probe**. Inside RimWorld it
executes the assembly-linked behavioral contract against actual `ThingDef`,
`StorageSettings`, and `ThingFilter` objects, then emits
`runtimeContract=pass` or `runtimeContract=fail` to the log.

## Manual interaction matrix

| Scenario | Expected result |
| --- | --- |
| One selected item -> stockpile | Only that exact item definition changes |
| Several selected item types -> shelf | Every distinct selected definition changes once |
| Repeated stacks of one type | One definition mutation |
| Linked shelf member | Shared group filter changes and all members refresh |
| Shelf overlapping stockpile | Short chooser identifies both actionable targets |
| Parent-disallowed item -> allow | Invalid/no change; no illegal child allowance |
| Already-matching target | Cell is not actionable; no callback |
| Save, reload, inspect target | Exact vanilla and modded definitions remain set |
| Modded `IStoreSettingsParent` building/comp | Uses its real visible settings filter |

## Release gates

- Build through the central RimWorld tooling resolver for RimWorld 1.6 with
  Harmony and Spine dependencies.
- Require zero compiler warnings and errors.
- Run the pure suite and assembly-linked compile contract.
- Validate the staged mod package and exact assembly identity.
- Run only through the repository `rwa.cmd` harness with Core, Harmony, Spine,
  and this mod active; execute the in-game contract probe and inspect the Player
  log for load or patch errors.

## 2026-08-01 verification record

- Pure service suite: 8/8 contracts passed.
- RimWorld 1.6 assembly-linked contract: compiled successfully through the
  central resolver.
- Production build: succeeded with 0 warnings and 0 errors.
- Package and explicit-allowlist release staging: valid for RimWorld 1.6 with
  `About` (including the real in-game `Preview.png`),
  `1.6/Assemblies/FilterByExample.dll`, `Languages`, and `LICENSE`.
- The staged package contains exactly five files. The included `LICENSE` has
  SHA-256 `7942115C67A258A6F955EF971F48E7A04C8A1CDCEC8F862CED392F93DA44F05A`.
- `About/Preview.png` has SHA-256
  `CCFC5ECFFA241D0033ACDF1E6B65FA7FF77B569161A2F0D10D3BC2A3A4D47497`.
- Core-only live session: the mod loaded, both contextual commands rendered for
  a selected Steel stack, and the real `ThingFilter` runtime probe passed.
- Live shelf interaction: Disallow by example produced `steelAllowed=false`
  while `woodAllowed=true`, with `priority=Preferred`, `hitPoints=0~1`, and
  `quality=Awful~Legendary` unchanged.
- Normal save and reload: the selected shelf reported the same exact state after
  reload.
- Harmony inspection: one postfix owned by `CoolNether123.FilterByExample`, with
  zero prefixes, transpilers, or finalizers.
- The added commands sort after the vanilla **Allow** command at the far-right
  end of the gizmo row. `Engineering/screenshots/rightmost-gizmos.png` records
  the live ordering; vanilla command positions are unchanged.
- Portable UI evidence is stored under `Engineering/screenshots`: selected
  example commands, targeting mode, the disallow result, and the resulting
  shelf filter. `Engineering` is excluded from the shipping allowlist.

One distribution-metadata warning remains unresolved because Spine currently
has no authoritative public distribution URL:

```text
Mod Filter by Example dependency (CoolNether123.Spine) needs to have <downloadUrl> and/or <steamWorkshopUrl> specified.
```

No URL is invented in `About.xml`. This warning does not represent a load,
patch, mutation, or save failure; it must be resolved when Spine has an
authoritative distribution location.
