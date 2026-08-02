using System.Reflection;
using Spine.Api;
using Spine.Harmony;
using Verse;

namespace FilterByExample.Bootstrap
{
    public sealed class FilterByExampleMod : Mod
    {
        private static readonly IHarmonyPatchInstaller PatchInstaller =
            SpineApi.Patching.CreateInstaller(
                "CoolNether123.FilterByExample",
                "[Filter by Example]");

        public FilterByExampleMod(ModContentPack content)
            : base(content)
        {
            SpineApi.Runtime.Require(new SpineRequirement(
                "CoolNether123.FilterByExample",
                new SemanticVersion(1, 0, 0),
                SpineCapability.HarmonyPatching));
            InstallPatches();
        }

        private static void InstallPatches()
        {
            PatchInstaller.PatchAllOnce(Assembly.GetExecutingAssembly());
        }
    }
}
