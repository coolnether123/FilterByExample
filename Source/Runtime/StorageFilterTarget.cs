using FilterByExample.Domain;
using RimWorld;
using Verse;

namespace FilterByExample.Runtime
{
    /// <summary>
    /// Adapts one real StorageSettings instance to the domain mutation boundary
    /// while retaining RimWorld's parent-filter restrictions for additions.
    /// </summary>
    internal sealed class StorageFilterTarget :
        IExampleFilterTarget<ThingDef>
    {
        private readonly StorageSettings settings;
        private readonly StorageSettings parentSettings;

        internal StorageFilterTarget(
            StorageSettings settings,
            StorageSettings parentSettings,
            string label)
        {
            this.settings = settings;
            this.parentSettings = parentSettings;
            Label = label ?? string.Empty;
        }

        public object Identity => settings;

        public string Label { get; }

        public bool Allows(ThingDef definition)
        {
            return definition != null && settings.filter.Allows(definition);
        }

        public bool CanAllow(ThingDef definition)
        {
            return definition != null &&
                (parentSettings == null ||
                    parentSettings.AllowedToAccept(definition));
        }

        public void SetAllow(ThingDef definition, bool allow)
        {
            settings.filter.SetAllow(definition, allow);
        }
    }
}
