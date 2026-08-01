using System;
using System.Collections.Generic;
using System.Linq;
using FilterByExample.Domain;
using FilterByExample.Runtime;
using RimWorld;
using Verse;

namespace FilterByExample.RimWorldContracts.Tests
{
    internal static class RimWorldContractsProgram
    {
        private static int Main()
        {
            ThingDef selected = NewDef("ExampleMod_Selected");
            ThingDef unrelatedSameCategory = NewDef("ExampleMod_Unrelated");
            ThingDef preexisting = NewDef("Steel");
            var settings = new StorageSettings();
            settings.filter.SetAllow(preexisting, true);
            settings.filter.SetAllow(unrelatedSameCategory, true);
            settings.filter.AllowedHitPointsPercents =
                new FloatRange(0.25f, 0.95f);
            settings.filter.AllowedQualityLevels = new QualityRange(
                QualityCategory.Normal,
                QualityCategory.Legendary);
            FloatRange hitPointsBefore =
                settings.filter.AllowedHitPointsPercents;
            QualityRange qualityBefore =
                settings.filter.AllowedQualityLevels;
            ThingDef[] allowedBefore = settings.filter.AllowedThingDefs.ToArray();

            var target = new StorageFilterTarget(
                settings,
                null,
                "Test shelf");
            var service = new FilterByExampleService<ThingDef>();
            FilterMutationResult result = service.Apply(
                new[] { selected, selected },
                new[] { target, target },
                ExampleFilterOperation.Allow);

            Equal(1, result.SelectedDefinitionCount, "definition dedupe");
            Equal(1, result.TargetCount, "settings dedupe");
            Equal(1, result.ChangedDefinitionCount, "exact mutation count");
            True(settings.filter.Allows(selected), "selected def not allowed");
            True(settings.filter.Allows(unrelatedSameCategory),
                "unrelated same-category def changed");
            True(settings.filter.Allows(preexisting), "preexisting def changed");
            Equal(hitPointsBefore, settings.filter.AllowedHitPointsPercents,
                "hit-point range changed");
            Equal(qualityBefore, settings.filter.AllowedQualityLevels,
                "quality range changed");
            Equal(allowedBefore.Length + 1,
                settings.filter.AllowedThingDefs.Count(),
                "unexpected category-wide mutation");

            service.Apply(
                new[] { selected },
                new[] { target },
                ExampleFilterOperation.Disallow);
            True(!settings.filter.Allows(selected), "selected def not disallowed");
            True(settings.filter.Allows(unrelatedSameCategory),
                "disallow expanded to category");
            Equal(hitPointsBefore, settings.filter.AllowedHitPointsPercents,
                "disallow changed hit points");
            Equal(qualityBefore, settings.filter.AllowedQualityLevels,
                "disallow changed quality");

            Console.WriteLine(
                "PASS: real RimWorld ThingFilter exact-mutation contract");
            return 0;
        }

        private static ThingDef NewDef(string defName)
        {
            return new ThingDef
            {
                defName = defName,
                label = defName,
                category = ThingCategory.Item
            };
        }

        private static void True(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }

        private static void Equal<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
            {
                throw new InvalidOperationException(
                    message + ": expected " + expected + ", actual " + actual);
            }
        }
    }
}
