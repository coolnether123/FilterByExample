using RimWorld;
using Verse;

namespace FilterByExample.Definitions
{
#pragma warning disable CS0649
    [DefOf]
    internal static class FilterByExampleDefOf
    {
        static FilterByExampleDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(
                typeof(FilterByExampleDefOf));
        }

        public static KeyBindingDef FilterByExample_Allow;

        public static KeyBindingDef FilterByExample_Disallow;

        public static DrawStyleCategoryDef
            FilterByExample_AffectedStorageArea;
    }
#pragma warning restore CS0649
}
