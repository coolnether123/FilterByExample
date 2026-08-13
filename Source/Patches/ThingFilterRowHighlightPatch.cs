using System.Reflection;
using FilterByExample.Presentation;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FilterByExample.Patches
{
    /// <summary>
    /// Paints matching selected definitions before vanilla storage-filter row
    /// labels and checkboxes are drawn.
    /// </summary>
    [HarmonyPatch]
    internal static class ThingFilterRowHighlightPatch
    {
        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(Listing_TreeThingFilter),
                "DoThingDef",
                new[]
                {
                    typeof(ThingDef),
                    typeof(int),
                    typeof(Map)
                });
        }

        private static void Prefix(
            Listing_TreeThingFilter __instance,
            ThingDef tDef,
            ThingFilter ___filter)
        {
            if (!HighlightState.Matches(___filter, tDef))
            {
                return;
            }

            HighlightDrawer.DrawHighlight(
                new Rect(
                    0f,
                    __instance.CurHeight,
                    __instance.ColumnWidth,
                    __instance.lineHeight));
        }
    }
}
