using System;
using System.Collections.Generic;
using System.Linq;
using FilterByExample.Domain;
using FilterByExample.Runtime;
using RimWorld;
using Verse;

namespace FilterByExample.Presentation
{
    internal static class ExampleSelectionCommands
    {
        private static readonly FilterByExampleService<ThingDef> Service =
            new FilterByExampleService<ThingDef>();

        internal static IEnumerable<Gizmo> For(Thing source)
        {
            List<ThingDef> definitions = SelectedDefinitions(source);
            if (definitions.Count == 0)
            {
                yield break;
            }

            yield return CreateCommand(
                definitions,
                ExampleFilterOperation.Allow);
            yield return CreateCommand(
                definitions,
                ExampleFilterOperation.Disallow);
        }

        private static Command_Target CreateCommand(
            IReadOnlyList<ThingDef> definitions,
            ExampleFilterOperation operation)
        {
            Map map = Find.CurrentMap;
            var parameters = new TargetingParameters
            {
                canTargetLocations = true,
                canTargetBuildings = true,
                canTargetPawns = false,
                canTargetAnimals = false,
                canTargetHumans = false,
                canTargetMechs = false,
                canTargetItems = false,
                canTargetPlants = false,
                canTargetFires = false,
                mapObjectTargetsMustBeAutoAttackable = false,
                validator = target => IsActionable(
                    target.Cell,
                    map,
                    definitions,
                    operation)
            };

            bool allow = operation == ExampleFilterOperation.Allow;
            return new Command_Target
            {
                defaultLabel = (allow
                    ? "FBE_AllowCommand"
                    : "FBE_DisallowCommand").Translate(),
                defaultDesc = (allow
                    ? "FBE_AllowDescription"
                    : "FBE_DisallowDescription").Translate(
                        definitions.Count),
                icon = allow ? TexCommand.ForbidOff : TexCommand.ForbidOn,
                Order = float.MaxValue,
                targetingParams = parameters,
                action = target => ChooseAndApply(
                    target.Cell,
                    map,
                    definitions,
                    operation)
            };
        }

        private static bool IsActionable(
            IntVec3 cell,
            Map map,
            IReadOnlyList<ThingDef> definitions,
            ExampleFilterOperation operation)
        {
            List<StorageFilterTarget> targets =
                StorageTargetResolver.Resolve(cell, map);
            return Service.CanApply(definitions, targets, operation);
        }

        private static void ChooseAndApply(
            IntVec3 cell,
            Map map,
            IReadOnlyList<ThingDef> definitions,
            ExampleFilterOperation operation)
        {
            List<StorageFilterTarget> actionableTargets =
                StorageTargetResolver.Resolve(cell, map)
                    .Where(target => Service.CanApply(
                        definitions,
                        new[] { target },
                        operation))
                    .ToList();
            if (actionableTargets.Count == 0)
            {
                Messages.Message(
                    "FBE_NoValidTarget".Translate(),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            if (actionableTargets.Count == 1)
            {
                Apply(definitions, actionableTargets[0], operation);
                return;
            }

            var options = new List<FloatMenuOption>(actionableTargets.Count);
            for (int index = 0; index < actionableTargets.Count; index++)
            {
                StorageFilterTarget target = actionableTargets[index];
                options.Add(new FloatMenuOption(
                    target.Label,
                    () => Apply(definitions, target, operation)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private static void Apply(
            IReadOnlyList<ThingDef> definitions,
            StorageFilterTarget target,
            ExampleFilterOperation operation)
        {
            FilterMutationResult result = Service.Apply(
                definitions,
                new[] { target },
                operation);
            if (!result.Changed)
            {
                Messages.Message(
                    "FBE_NoChange".Translate(target.Label),
                    MessageTypeDefOf.RejectInput,
                    historical: false);
                return;
            }

            string key = operation == ExampleFilterOperation.Allow
                ? "FBE_AllowedResult"
                : "FBE_DisallowedResult";
            Messages.Message(
                key.Translate(result.ChangedDefinitionCount, target.Label),
                MessageTypeDefOf.TaskCompletion,
                historical: false);
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
