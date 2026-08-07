using RimWorld;
using Verse;

namespace FilterByExample.Definitions
{
#pragma warning disable CS0649
    /// <summary>
    /// Provides typed access to XML definitions so presentation code does not
    /// depend on string-based lookups or duplicate RimWorld's Def lifecycle.
    /// </summary>
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

#if !FILTER_BY_EXAMPLE_LEGACY_NO_DRAW_STYLE
        public static DrawStyleCategoryDef
            FilterByExample_AffectedStorageArea;
#endif
    }
#pragma warning restore CS0649
}
