using System.Reflection;
using HarmonyLib;
using Spine.Api;
using Spine.Harmony;
using Verse;

namespace FilterByExample.Bootstrap
{
    public sealed class FilterByExampleMod : Mod
    {
        private static bool patchesInstalled;

        public FilterByExampleMod(ModContentPack content)
            : base(content)
        {
            SpineApi.Runtime.Require(new SpineRequirement(
                "CoolNether123.FilterByExample",
                new SemanticVersion(1, 2, 0),
                SpineCapability.HarmonyPatching));
            InstallPatches();
        }

        private static void InstallPatches()
        {
            if (patchesInstalled)
            {
                return;
            }

            var harmony = new HarmonyLib.Harmony(
                "CoolNether123.FilterByExample");
            HarmonyUtil.PatchAll(
                harmony,
                Assembly.GetExecutingAssembly(),
                new HarmonyUtil.PatchOptions
                {
                    OnResult = (target, result) =>
                    {
                        if (result.StartsWith("error:") ||
                            result.StartsWith("skipped:"))
                        {
                            Log.Warning(
                                "[Filter by Example] " + target + ": " +
                                result);
                        }
                        else if (Prefs.DevMode)
                        {
                            Log.Message(
                                "[Filter by Example] " + target + ": " +
                                result);
                        }
                    }
                });
            patchesInstalled = true;
        }
    }
}
