using System;
using System.Collections.Generic;
using System.Linq;
using FilterByExample.Domain;
using RimWorld;
using Verse;

namespace FilterByExample.Runtime
{
    /// <summary>
    /// Discovers mutable storage through RimWorld's interface contract so zones,
    /// buildings, linked groups, and compatible modded storage use one path.
    /// </summary>
    internal static class StorageTargetResolver
    {
        internal static List<StorageFilterTarget> Resolve(
            IntVec3 cell,
            Map map)
        {
            var targets = new List<StorageFilterTarget>();
            var seenSettings = new HashSet<StorageSettings>();
            if (map == null || !cell.InBounds(map))
            {
                return targets;
            }

            List<Thing> things = map.thingGrid.ThingsListAtFast(cell);
            for (int index = 0; index < things.Count; index++)
            {
                Thing thing = things[index];
                if (!thing.Spawned ||
                    thing is Blueprint ||
                    thing is Frame ||
                    thing.Faction != null && thing.Faction != Faction.OfPlayer)
                {
                    continue;
                }

                AddParent(
                    thing as IStoreSettingsParent,
                    LabelFor(thing),
                    thing,
                    targets,
                    seenSettings);

                if (thing is ThingWithComps thingWithComps)
                {
                    List<ThingComp> comps = thingWithComps.AllComps;
                    for (int compIndex = 0;
                        compIndex < comps.Count;
                        compIndex++)
                    {
                        AddParent(
                            comps[compIndex] as IStoreSettingsParent,
                            LabelFor(thing),
                            thing,
                            targets,
                            seenSettings);
                    }
                }
            }

            Zone zone = map.zoneManager.ZoneAt(cell);
            AddParent(
                zone as IStoreSettingsParent,
                zone?.label,
                zone,
                targets,
                seenSettings);

            return targets
                .OrderBy(target => target.Label, StringComparer.Ordinal)
                .ToList();
        }

        private static void AddParent(
            IStoreSettingsParent parent,
            string label,
            ISelectable selectableOwner,
            ICollection<StorageFilterTarget> targets,
            ISet<StorageSettings> seenSettings)
        {
            if (parent == null)
            {
                return;
            }

            try
            {
                if (!parent.StorageTabVisible)
                {
                    return;
                }

                StorageSettings settings = parent.GetStoreSettings();
                // Linked group members return the same settings instance; the
                // shared filter must appear as one target even across adapters.
                if (settings?.filter == null || !seenSettings.Add(settings))
                {
                    return;
                }

                targets.Add(new StorageFilterTarget(
                    settings,
                    parent.GetParentStoreSettings(),
                    label,
                    selectableOwner));
            }
            catch (Exception exception)
            {
                // Resolution also runs during hover previews. Log once per
                // incompatible implementation to avoid frame-by-frame spam.
#if FILTER_BY_EXAMPLE_LEGACY_LOG
                Log.Warning(
#else
                Log.WarningOnce(
#endif
                    "[Filter by Example] Ignored incompatible storage target " +
                    (label ?? "<unnamed>") + ": " + exception.Message
#if !FILTER_BY_EXAMPLE_LEGACY_LOG
                    ,
                    (parent.GetType().FullName ?? string.Empty).GetHashCode());
#else
                    );
#endif
            }
        }

        private static string LabelFor(Thing thing)
        {
#if FILTER_BY_EXAMPLE_HAS_STORAGE_GROUP_MEMBER
            if (thing is IStorageGroupMember member && member.Group != null)
            {
#if FILTER_BY_EXAMPLE_HAS_RENAMABLE_LABEL
                return member.Group.RenamableLabel.CapitalizeFirst() +
#else
                return thing.LabelShortCap;
#endif
#if FILTER_BY_EXAMPLE_HAS_RENAMABLE_LABEL
                    " (" + thing.LabelShortCap + ")";
#endif
            }
#endif

            return thing.LabelShortCap;
        }
    }
}
