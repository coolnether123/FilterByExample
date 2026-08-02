using System;
using System.Collections.Generic;
using System.Linq;
using FilterByExample.Domain;
using FilterByExample.Runtime;
using LudeonTK;
using RimWorld;
using Verse;

namespace FilterByExample.Diagnostics
{
    internal static class FilterByExampleDebugActions
    {
        [DebugAction(
            "Filter by Example",
            "Log selected storage contract state",
            actionType = DebugActionType.Action)]
        private static void LogSelectedStorageContractState()
        {
            Thing selected = Find.Selector?.SingleSelectedThing;
            IStoreSettingsParent parent = selected as IStoreSettingsParent;
            StorageSettings settings = parent?.GetStoreSettings();
            ThingDef steel =
                DefDatabase<ThingDef>.GetNamedSilentFail("Steel");
            ThingDef wood =
                DefDatabase<ThingDef>.GetNamedSilentFail("WoodLog");
            if (selected == null || settings?.filter == null || steel == null)
            {
                Log.Warning(
                    "[Filter by Example] selectedStorageState=unavailable");
                return;
            }

            Log.Message(
                "[Filter by Example] selectedStorageState=complete " +
                "thingId=" + selected.thingIDNumber +
                " def=" + selected.def.defName +
                " steelAllowed=" +
                settings.filter.Allows(steel).ToString().ToLowerInvariant() +
                " woodAllowed=" +
                (wood != null && settings.filter.Allows(wood))
                    .ToString().ToLowerInvariant() +
                " priority=" + settings.Priority +
                " hitPoints=" +
                settings.filter.AllowedHitPointsPercents +
                " quality=" + settings.filter.AllowedQualityLevels);
        }

        [DebugAction(
            "Filter by Example",
            "Run exact-filter contract probe",
            actionType = DebugActionType.Action)]
        private static void RunExactFilterContractProbe()
        {
            try
            {
                ThingDef selected = RequiredDef("Steel");
                ThingDef unrelated = RequiredDef("WoodLog");
                ThingDef secondUnrelated = RequiredDef("ComponentIndustrial");
                var settings = new StorageSettings();
                settings.filter.SetAllow(unrelated, true);
                settings.filter.SetAllow(secondUnrelated, true);
                settings.filter.AllowedHitPointsPercents =
                    new FloatRange(0.25f, 0.95f);
                settings.filter.AllowedQualityLevels = new QualityRange(
                    QualityCategory.Normal,
                    QualityCategory.Legendary);

                ThingDef[] baseline = settings.filter.AllowedThingDefs
                    .OrderBy(definition => definition.defName)
                    .ToArray();
                FloatRange hitPoints =
                    settings.filter.AllowedHitPointsPercents;
                QualityRange quality =
                    settings.filter.AllowedQualityLevels;
                var target = new StorageFilterTarget(
                    settings,
                    null,
                    "runtime contract storage");
                var service = new FilterByExampleService<ThingDef>();

                FilterMutationResult allowResult = service.Apply(
                    new[] { selected, selected },
                    new[] { target, target },
                    ExampleFilterOperation.Allow);
                Require(
                    allowResult.SelectedDefinitionCount == 1 &&
                    allowResult.TargetCount == 1 &&
                    allowResult.ChangedDefinitionCount == 1,
                    "allow result counts were not deduplicated");
                Require(settings.filter.Allows(selected),
                    "selected definition was not allowed");
                Require(settings.filter.Allows(unrelated),
                    "unrelated definition changed during allow");
                Require(settings.filter.Allows(secondUnrelated),
                    "second unrelated definition changed during allow");
                Require(
                    settings.filter.AllowedHitPointsPercents == hitPoints,
                    "hit-point range changed during allow");
                Require(
                    settings.filter.AllowedQualityLevels == quality,
                    "quality range changed during allow");

                FilterMutationResult disallowResult = service.Apply(
                    new[] { selected },
                    new[] { target },
                    ExampleFilterOperation.Disallow);
                Require(disallowResult.ChangedDefinitionCount == 1,
                    "disallow result count was incorrect");
                Require(!settings.filter.Allows(selected),
                    "selected definition was not disallowed");
                Require(
                    baseline.SequenceEqual(
                        settings.filter.AllowedThingDefs
                            .OrderBy(definition => definition.defName)),
                    "unrelated allowed definitions changed");
                Require(
                    settings.filter.AllowedHitPointsPercents == hitPoints,
                    "hit-point range changed during disallow");
                Require(
                    settings.filter.AllowedQualityLevels == quality,
                    "quality range changed during disallow");

                Log.Message(
                    "[Filter by Example] runtimeContract=pass " +
                    "selected=" + selected.defName +
                    " unrelated=" + unrelated.defName + "," +
                    secondUnrelated.defName +
                    " allowedCount=" + baseline.Length);
            }
            catch (Exception exception)
            {
                Log.Error(
                    "[Filter by Example] runtimeContract=fail " +
                    exception);
            }
        }

        private static ThingDef RequiredDef(string defName)
        {
            ThingDef definition =
                DefDatabase<ThingDef>.GetNamedSilentFail(defName);
            if (definition == null)
            {
                throw new InvalidOperationException(
                    "Required definition is unavailable: " + defName);
            }

            return definition;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
