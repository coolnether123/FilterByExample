using System.Collections.Generic;
using FilterByExample.Presentation;
using HarmonyLib;
using Verse;

namespace FilterByExample.Patches
{
    /// <summary>
    /// Adds the feature at RimWorld's existing item-command extension point so
    /// selection and command placement remain owned by the vanilla UI.
    /// </summary>
    [HarmonyPatch(typeof(Thing), nameof(Thing.GetGizmos))]
    internal static class ThingGizmoPatch
    {
        private static void Postfix(
            Thing __instance,
            ref IEnumerable<Gizmo> __result)
        {
            __result = AppendCommands(__result, __instance);
        }

        private static IEnumerable<Gizmo> AppendCommands(
            IEnumerable<Gizmo> original,
            Thing source)
        {
            if (original != null)
            {
                foreach (Gizmo gizmo in original)
                {
                    yield return gizmo;
                }
            }

            foreach (Gizmo gizmo in ExampleSelectionCommands.For(source))
            {
                yield return gizmo;
            }
        }
    }
}
