using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FilterByExample.Domain;
using static RimWorld.ModTestSupport.Test;

namespace FilterByExample.Tests
{
    internal static class Program
    {
        private static readonly FilterByExampleService<string> Service =
            new FilterByExampleService<string>(StringComparer.Ordinal);

        private static int Main()
        {
            Start("Filter by Example contracts");
            Run("allow exact definitions only", AllowExactDefinitionsOnly);
            Run("disallow exact definitions only", DisallowExactDefinitionsOnly);
            Run("dedupe definitions and shared settings", DedupeDefinitionsAndTargets);
            Run("respect locked parent allow filter", RespectLockedParentFilter);
            Run("preserve unrelated filter state", PreserveUnrelatedFilterState);
            Run("batch independent storage targets", BatchIndependentTargets);
            Run("save reload preserves modded definitions", SaveReloadModdedDefinitions);
            Run("no-op plans do not notify", NoOpDoesNotNotify);
            Run("commands have native configurable keybindings",
                CommandsHaveNativeKeybindings);
            Run("keybindings use deliberate left-hand defaults", KeybindingsUseSafeDefaults);
            Run("small empty drags stay active", SmallEmptyDragsStayActive);
            Run("large empty drags end targeting", LargeEmptyDragsEndTargeting);
            Run("drag designator previews and batches", DragDesignatorContracts);
            return Finish();
        }

        private static void AllowExactDefinitionsOnly()
        {
            var target = new FakeTarget("Shelf", "Steel", "WoodLog");
            FilterMutationResult result = Service.Apply(
                new[] { "MedicineIndustrial", "ComponentIndustrial" },
                new[] { target },
                ExampleFilterOperation.Allow);

            Equal(2, result.ChangedDefinitionCount, "changed definitions");
            SetEqual(
                new[]
                {
                    "Steel",
                    "WoodLog",
                    "MedicineIndustrial",
                    "ComponentIndustrial"
                },
                target.Allowed,
                "allow set");
        }

        private static void DisallowExactDefinitionsOnly()
        {
            var target = new FakeTarget(
                "Stockpile",
                "Steel",
                "WoodLog",
                "ComponentIndustrial");
            FilterMutationResult result = Service.Apply(
                new[] { "Steel", "ComponentIndustrial" },
                new[] { target },
                ExampleFilterOperation.Disallow);

            Equal(2, result.ChangedDefinitionCount, "changed definitions");
            SetEqual(new[] { "WoodLog" }, target.Allowed, "disallow set");
        }

        private static void DedupeDefinitionsAndTargets()
        {
            object sharedSettings = new object();
            var first = new FakeTarget("Linked shelf A", identity: sharedSettings);
            var second = new FakeTarget("Linked shelf B", identity: sharedSettings);
            FilterMutationResult result = Service.Apply(
                new[] { "Steel", "Steel", "Steel" },
                new[] { first, second, first },
                ExampleFilterOperation.Allow);

            Equal(1, result.SelectedDefinitionCount, "definition count");
            Equal(1, result.TargetCount, "target count");
            Equal(1, result.ChangedDefinitionCount, "mutation count");
            Equal(1, first.NotificationCount, "shared settings notification");
            Equal(0, second.NotificationCount, "deduped adapter untouched");
        }

        private static void RespectLockedParentFilter()
        {
            var target = new FakeTarget("Locked storage");
            target.BlockedAllows.Add("Bioferrite");
            FilterMutationResult result = Service.Apply(
                new[] { "Bioferrite", "Steel" },
                new[] { target },
                ExampleFilterOperation.Allow);

            Equal(1, result.BlockedDefinitionCount, "blocked count");
            SetEqual(new[] { "Steel" }, target.Allowed, "allowed set");
            True(!target.Allowed.Contains("Bioferrite"), "blocked def changed");
        }

        private static void PreserveUnrelatedFilterState()
        {
            var target = new FakeTarget("Shelf", "Steel", "MealsSimple");
            string before = target.UnrelatedFingerprint();
            Service.Apply(
                new[] { "Steel", "ComponentIndustrial" },
                new[] { target },
                ExampleFilterOperation.Disallow);

            Equal(before, target.UnrelatedFingerprint(), "unrelated state");
            Equal("0.25:0.95", target.HitPointRange, "hit points");
            Equal("Normal:Legendary", target.QualityRange, "quality");
            SetEqual(
                new[] { "Foods", "Manufactured" },
                target.AllowedCategories,
                "categories");
            SetEqual(
                new[] { "AllowFresh", "AllowNonDeadmansApparel" },
                target.SpecialFilters,
                "special filters");
            Equal("Steel", target.RequiredIngredient, "ingredient");
            Equal("Important", target.Priority, "priority");
        }

        private static void BatchIndependentTargets()
        {
            var zone = new FakeTarget("Stockpile");
            var shelf = new FakeTarget("Shelf");
            FilterMutationResult result = Service.Apply(
                new[] { "MedicineHerbal", "MedicineIndustrial" },
                new[] { zone, shelf },
                ExampleFilterOperation.Allow);

            Equal(2, result.TargetCount, "target count");
            Equal(2, result.ChangedTargetCount, "changed targets");
            Equal(4, result.ChangedDefinitionCount, "changed definitions");
            SetEqual(zone.Allowed, shelf.Allowed, "batch target parity");
        }

        private static void SaveReloadModdedDefinitions()
        {
            const string moddedDef = "ExampleStorageMod_QuantumCratePart";
            var target = new FakeTarget("Modded storage", "Steel");
            Service.Apply(
                new[] { moddedDef },
                new[] { target },
                ExampleFilterOperation.Allow);
            string save = target.Save();
            FakeTarget loaded = FakeTarget.Load(save);

            True(loaded.Allowed.Contains(moddedDef), "modded def did not reload");
            Equal(target.UnrelatedFingerprint(), loaded.UnrelatedFingerprint(),
                "save reload unrelated state");
        }

        private static void NoOpDoesNotNotify()
        {
            var target = new FakeTarget("Shelf", "Steel");
            True(!Service.CanApply(
                new[] { "Steel" },
                new[] { target },
                ExampleFilterOperation.Allow), "no-op advertised");
            FilterMutationResult result = Service.Apply(
                new[] { "Steel" },
                new[] { target },
                ExampleFilterOperation.Allow);

            True(!result.Changed, "no-op changed");
            Equal(0, target.NotificationCount, "no-op notified");
        }

        private static void CommandsHaveNativeKeybindings()
        {
            string root = RepositoryRoot();
            string source = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "Designator_FilterByExample.cs"));
            string factory = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "ExampleSelectionCommands.cs"));
            True(source.Contains(
                    "hotKey = keyBinding",
                    StringComparison.Ordinal) &&
                factory.Contains(
                    "FilterByExampleDefOf.FilterByExample_Allow",
                    StringComparison.Ordinal) &&
                factory.Contains(
                    "FilterByExampleDefOf.FilterByExample_Disallow",
                    StringComparison.Ordinal),
                "both targeting commands must use RimWorld's native hotkey path");
        }

        private static void KeybindingsUseSafeDefaults()
        {
            string root = RepositoryRoot();
            var document = XDocument.Load(Path.Combine(
                root,
                "Defs",
                "KeyBindings.xml"));
            XElement[] definitions = document.Root?
                .Elements("KeyBindingDef")
                .ToArray() ?? Array.Empty<XElement>();
            Equal(2, definitions.Length, "keybinding definition count");
            Equal(
                "Q",
                definitions.Single(definition =>
                    definition.Element("defName")?.Value ==
                    "FilterByExample_Allow")
                    .Element("defaultKeyCodeA")?.Value,
                "allow primary default");
            Equal(
                "E",
                definitions.Single(definition =>
                    definition.Element("defName")?.Value ==
                    "FilterByExample_Disallow")
                    .Element("defaultKeyCodeA")?.Value,
                "disallow primary default");
            foreach (XElement definition in definitions)
            {
                Equal(
                    "None",
                    definition.Element("defaultKeyCodeB")?.Value,
                    "secondary default");
            }
        }

        private static void SmallEmptyDragsStayActive()
        {
            True(EmptyDragRetryPolicy.KeepActive(1, 1), "single miss canceled");
            True(EmptyDragRetryPolicy.KeepActive(5, 5), "five-cell span canceled");
        }

        private static void LargeEmptyDragsEndTargeting()
        {
            True(!EmptyDragRetryPolicy.KeepActive(6, 1), "wide miss stayed active");
            True(!EmptyDragRetryPolicy.KeepActive(1, 6), "tall miss stayed active");
        }

        private static void DragDesignatorContracts()
        {
            string root = RepositoryRoot();
            string commands = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "ExampleSelectionCommands.cs"));
            string designator = File.ReadAllText(Path.Combine(
                root,
                "Source",
                "Presentation",
                "Designator_FilterByExample.cs"));
            True(commands.Contains(
                    "Designator_FilterByExample",
                    StringComparison.Ordinal),
                "commands do not enter the native designator path");
            True(designator.Contains(
                    "FilterByExampleDefOf.FilterByExample_AffectedStorageArea",
                    StringComparison.Ordinal) &&
                designator.Contains(
                    "DesignateMultiCell",
                    StringComparison.Ordinal) &&
                designator.Contains(
                    "DesignatorUtility.DragHighlightCellMat",
                    StringComparison.Ordinal),
                "drag outline, batch release, or target highlighting missing");

            string drawStyles = File.ReadAllText(Path.Combine(
                root,
                "Defs",
                "DrawStyles.xml"));
            True(drawStyles.Contains(
                    "<drawOutline>true</drawOutline>",
                    StringComparison.Ordinal),
                "drag outline was disabled");
            True(drawStyles.Contains(
                    "<drawArea>false</drawArea>",
                    StringComparison.Ordinal),
                "drag area cell count was not disabled");
        }

        private static string RepositoryRoot()
        {
            return Path.GetFullPath(Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                ".."));
        }


        private static void SetEqual(
            IEnumerable<string> expected,
            IEnumerable<string> actual,
            string message)
        {
            string expectedText = string.Join(",", expected.OrderBy(x => x));
            string actualText = string.Join(",", actual.OrderBy(x => x));
            Equal(expectedText, actualText, message);
        }

        private sealed class FakeTarget : IExampleFilterTarget<string>
        {
            internal FakeTarget(
                string label,
                params string[] allowed)
                : this(label, null, allowed)
            {
            }

            internal FakeTarget(
                string label,
                object identity,
                params string[] allowed)
            {
                Label = label;
                Identity = identity ?? new object();
                Allowed = new HashSet<string>(
                    allowed ?? Array.Empty<string>(),
                    StringComparer.Ordinal);
            }

            public object Identity { get; }

            public string Label { get; }

            internal HashSet<string> Allowed { get; }

            internal HashSet<string> BlockedAllows { get; } =
                new HashSet<string>(StringComparer.Ordinal);

            internal HashSet<string> AllowedCategories { get; } =
                new HashSet<string>(
                    new[] { "Foods", "Manufactured" },
                    StringComparer.Ordinal);

            internal HashSet<string> SpecialFilters { get; } =
                new HashSet<string>(
                    new[] { "AllowFresh", "AllowNonDeadmansApparel" },
                    StringComparer.Ordinal);

            internal string HitPointRange { get; } = "0.25:0.95";

            internal string QualityRange { get; } = "Normal:Legendary";

            internal string RequiredStuff { get; } = "Plasteel";

            internal string RequiredIngredient { get; } = "Steel";

            internal string Priority { get; } = "Important";

            internal int NotificationCount { get; private set; }

            public bool Allows(string definition)
            {
                return Allowed.Contains(definition);
            }

            public bool CanAllow(string definition)
            {
                return !BlockedAllows.Contains(definition);
            }

            public void SetAllow(string definition, bool allow)
            {
                if (allow)
                {
                    Allowed.Add(definition);
                }
                else
                {
                    Allowed.Remove(definition);
                }

                NotificationCount++;
            }

            internal string UnrelatedFingerprint()
            {
                return string.Join("|",
                    HitPointRange,
                    QualityRange,
                    string.Join(",", AllowedCategories.OrderBy(x => x)),
                    string.Join(",", SpecialFilters.OrderBy(x => x)),
                    RequiredStuff,
                    RequiredIngredient,
                    Priority);
            }

            internal string Save()
            {
                return string.Join(";",
                    string.Join(",", Allowed.OrderBy(x => x)),
                    HitPointRange,
                    QualityRange,
                    string.Join(",", AllowedCategories.OrderBy(x => x)),
                    string.Join(",", SpecialFilters.OrderBy(x => x)),
                    RequiredStuff,
                    RequiredIngredient,
                    Priority);
            }

            internal static FakeTarget Load(string serialized)
            {
                string[] parts = serialized.Split(';');
                if (parts.Length != 8)
                {
                    throw new InvalidOperationException("Invalid fake save.");
                }

                var target = new FakeTarget(
                    "Reloaded",
                    parts[0].Length == 0
                        ? Array.Empty<string>()
                        : parts[0].Split(','));
                Equal("0.25:0.95", parts[1], "saved hit points");
                Equal("Normal:Legendary", parts[2], "saved quality");
                Equal("Foods,Manufactured", parts[3], "saved categories");
                Equal(
                    "AllowFresh,AllowNonDeadmansApparel",
                    parts[4],
                    "saved specials");
                Equal("Plasteel", parts[5], "saved stuff");
                Equal("Steel", parts[6], "saved ingredient");
                Equal("Important", parts[7], "saved priority");
                return target;
            }
        }
    }
}
