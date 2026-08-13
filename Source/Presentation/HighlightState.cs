using System.Collections.Generic;
using FilterByExample.Domain;
using Verse;

namespace FilterByExample.Presentation
{
    /// <summary>
    /// Retains the last successful single-target operation for the storage
    /// filter UI, using the real ThingFilter instance as the target identity.
    /// </summary>
    internal static class HighlightState
    {
        private static readonly List<ThingDef> selectedThingDefs =
            new List<ThingDef>();

        private static ThingFilter targetFilter;

        public static ThingFilter TargetFilter => targetFilter;

        public static ExampleFilterOperation Operation { get; private set; }

        public static IReadOnlyList<ThingDef> SelectedThingDefs =>
            selectedThingDefs;

        internal static void Set(
            ThingFilter filter,
            ExampleFilterOperation operation,
            IEnumerable<ThingDef> definitions)
        {
            targetFilter = filter;
            Operation = operation;
            selectedThingDefs.Clear();

            if (definitions == null)
            {
                return;
            }

            var seen = new HashSet<ThingDef>();
            foreach (ThingDef definition in definitions)
            {
                if (definition != null && seen.Add(definition))
                {
                    selectedThingDefs.Add(definition);
                }
            }
        }

        internal static bool Matches(
            ThingFilter filter,
            ThingDef definition)
        {
            return definition != null &&
                ReferenceEquals(targetFilter, filter) &&
                selectedThingDefs.Contains(definition);
        }
    }
}
