using System;
using System.Collections.Generic;

namespace FilterByExample.Domain
{
    public sealed class FilterByExampleService<TDefinition>
    {
        private readonly IEqualityComparer<TDefinition> definitionComparer;

        public FilterByExampleService(
            IEqualityComparer<TDefinition> definitionComparer = null)
        {
            this.definitionComparer = definitionComparer ??
                EqualityComparer<TDefinition>.Default;
        }

        public bool CanApply(
            IEnumerable<TDefinition> examples,
            IEnumerable<IExampleFilterTarget<TDefinition>> targets,
            ExampleFilterOperation operation)
        {
            List<TDefinition> definitions = UniqueDefinitions(examples);
            foreach (IExampleFilterTarget<TDefinition> target in
                UniqueTargets(targets))
            {
                for (int index = 0; index < definitions.Count; index++)
                {
                    if (CanApply(definitions[index], target, operation))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CanApply(
            TDefinition definition,
            IExampleFilterTarget<TDefinition> target,
            ExampleFilterOperation operation)
        {
            if (ReferenceEquals(definition, null) || target == null)
            {
                return false;
            }

            if (operation == ExampleFilterOperation.Allow &&
                !target.CanAllow(definition))
            {
                return false;
            }

            bool allow = operation == ExampleFilterOperation.Allow;
            return target.Allows(definition) != allow;
        }

        public FilterMutationResult Apply(
            IEnumerable<TDefinition> examples,
            IEnumerable<IExampleFilterTarget<TDefinition>> targets,
            ExampleFilterOperation operation)
        {
            List<TDefinition> definitions = UniqueDefinitions(examples);
            List<IExampleFilterTarget<TDefinition>> uniqueTargets =
                UniqueTargets(targets);
            int changedTargets = 0;
            int changedDefinitions = 0;
            int blockedDefinitions = 0;

            for (int targetIndex = 0;
                targetIndex < uniqueTargets.Count;
                targetIndex++)
            {
                IExampleFilterTarget<TDefinition> target =
                    uniqueTargets[targetIndex];
                bool targetChanged = false;
                for (int definitionIndex = 0;
                    definitionIndex < definitions.Count;
                    definitionIndex++)
                {
                    TDefinition definition = definitions[definitionIndex];
                    if (operation == ExampleFilterOperation.Allow &&
                        !target.CanAllow(definition))
                    {
                        blockedDefinitions++;
                        continue;
                    }

                    bool allow = operation == ExampleFilterOperation.Allow;
                    if (target.Allows(definition) == allow)
                    {
                        continue;
                    }

                    target.SetAllow(definition, allow);
                    changedDefinitions++;
                    targetChanged = true;
                }

                if (targetChanged)
                {
                    changedTargets++;
                }
            }

            return new FilterMutationResult(
                definitions.Count,
                uniqueTargets.Count,
                changedTargets,
                changedDefinitions,
                blockedDefinitions);
        }

        private List<TDefinition> UniqueDefinitions(
            IEnumerable<TDefinition> examples)
        {
            var result = new List<TDefinition>();
            var seen = new HashSet<TDefinition>(definitionComparer);
            if (examples == null)
            {
                return result;
            }

            foreach (TDefinition definition in examples)
            {
                if (!ReferenceEquals(definition, null) && seen.Add(definition))
                {
                    result.Add(definition);
                }
            }

            return result;
        }

        private static List<IExampleFilterTarget<TDefinition>> UniqueTargets(
            IEnumerable<IExampleFilterTarget<TDefinition>> targets)
        {
            var result = new List<IExampleFilterTarget<TDefinition>>();
            var seen = new HashSet<object>(ReferenceIdentityComparer.Instance);
            if (targets == null)
            {
                return result;
            }

            foreach (IExampleFilterTarget<TDefinition> target in targets)
            {
                object identity = target?.Identity;
                if (identity != null && seen.Add(identity))
                {
                    result.Add(target);
                }
            }

            return result;
        }

    }
}
