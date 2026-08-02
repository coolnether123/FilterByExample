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

The separately loadable `Developer/FilterByExample.TestFixture` includes a
developer-mode debug action named **Filter by Example / Run exact-filter
contract probe**. Inside RimWorld it
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

## 2026-08-02 drag and keybinding follow-up

- The pure suite now passes 13/13 contracts, including native configurable
  bindings, batch drag release, five-cell retry behavior, affected-cell
  highlighting, outline retention, and suppression of the area-cell number.
- The centralized RimWorld 1.6 build completed with zero warnings and zero
  errors against the Spine 1.0 development line. That drag/keybinding runtime
  assembly is 26,112 bytes with SHA-256
  `9FB79C02578D4FFE0662D98926B04155599EC0AF04ACC4A05A77A5E414886032`.
- `Test-RwtPackage` returned `RWT-BUILD-PACKAGE-VALID`.
- In session `eight-new-e9df076b456e4aa3a99f4b36fb6475eb`, all eight
  gameplay mods, Spine, SOS2, Vehicle Framework, Ideology, and Biotech reached
  a generated colony with development mode enabled.
- The ordinary selected-item gizmo path exposed Allow by example and Disallow
  by example at the far right. Both commands use `Command.hotKey`, so any
  player binding is drawn by RimWorld in the icon's upper-left corner.
- The running assembly's exact-filter debug action reported
  `runtimeContract=pass`; no exception was present in the session log.
- The loaded custom draw style has one filled-rectangle style with
  `drawOutline=true` and `drawArea=false`. RimWorld's own
  `DesignationDragger` gates its area number on that flag, so the boundary and
  affected-storage highlight remain without a cell-count overlay.

## Current release candidate hardening (2026-08-02)

The centralized-service candidate rebuild completed with zero warnings and
zero errors against Spine SHA-256
`3E857A09793BBFF839D0C18D197E480C9365B6384148F49F48669F068BBB9086`.
The current `FilterByExample.dll` is 20,992 bytes, has assembly version
1.0.0.0, and has SHA-256
`37B756F22834920786099B411F7CB26055E04A02727CC197038B984A9EBFB3C4`.
The focused suite passes 13 contracts and 49 assertions, and
`Test-RwtPackage` returns `RWT-BUILD-PACKAGE-VALID`. The shipping package has
one DLL and excludes `Developer/FilterByExample.TestFixture`; the fixture
source and metadata remain available to developers. The runtime record above
remains bound to its exact historical hash, so the parent release pass must
record the final combined launch for this candidate.
