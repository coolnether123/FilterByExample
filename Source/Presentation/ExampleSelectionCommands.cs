using System.Collections.Generic;
using FilterByExample.Definitions;
using FilterByExample.Domain;
using RimWorld;
using Verse;

namespace FilterByExample.Presentation
{
    internal static class ExampleSelectionCommands
    {
        internal static IEnumerable<Gizmo> For(Thing source)
        {
            List<ThingDef> definitions = SelectedDefinitions(source);
            if (definitions.Count == 0)
            {
                yield break;
            }

            yield return CreateDesignator(
                definitions,
                ExampleFilterOperation.Allow);
            yield return CreateDesignator(
                definitions,
                ExampleFilterOperation.Disallow);
        }

        private static Designator_FilterByExample CreateDesignator(
            IReadOnlyList<ThingDef> definitions,
            ExampleFilterOperation operation)
        {
            bool allow = operation == ExampleFilterOperation.Allow;
            return new Designator_FilterByExample(
                definitions,
                operation,
                (allow ? "FBE_AllowCommand" : "FBE_DisallowCommand")
                    .Translate(),
                (allow ? "FBE_AllowDescription" : "FBE_DisallowDescription")
                    .Translate(definitions.Count),
                allow ? TexCommand.ForbidOff : TexCommand.ForbidOn,
                allow
                    ? FilterByExampleDefOf.FilterByExample_Allow
                    : FilterByExampleDefOf.FilterByExample_Disallow);
        }

        private static List<ThingDef> SelectedDefinitions(Thing source)
        {
            if (source == null || Find.Selector == null)
            {
                return new List<ThingDef>();
            }

            var definitions = new List<ThingDef>();
            var seen = new HashSet<ThingDef>();
            bool sourceIsFirst = false;
            foreach (object selected in
                Find.Selector.SelectedObjectsListForReading)
            {
                if (!(selected is Thing thing) ||
                    thing.def.category != ThingCategory.Item)
                {
                    continue;
                }

                Thing actualThing = thing.GetInnerIfMinified();
                ThingDef definition = actualThing?.def;
                if (definition == null ||
                    !definition.EverStorable(willMinifyIfPossible: true))
                {
                    continue;
                }

                if (!sourceIsFirst)
                {
                    if (!ReferenceEquals(thing, source))
                    {
                        return new List<ThingDef>();
                    }

                    sourceIsFirst = true;
                }

                if (seen.Add(definition))
                {
                    definitions.Add(definition);
                }
            }

            return sourceIsFirst ? definitions : new List<ThingDef>();
        }
    }
}
