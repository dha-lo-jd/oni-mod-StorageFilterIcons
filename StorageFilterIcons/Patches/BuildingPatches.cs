using HarmonyLib;
using StorageFilterIcons.Components;

namespace StorageFilterIcons.Patches;

public static class BuildingPatches
{
    // Attach the overlay to every completed building that has a filter (including buildings from other mods).
    [HarmonyPatch(typeof(BuildingComplete), "OnSpawn")]
    public static class BuildingCompleteOnSpawnPatch
    {
        public static void Postfix(BuildingComplete __instance)
        {
            var go = __instance.gameObject;
            if (go.GetComponent<TreeFilterable>() != null && go.GetComponent<Storage>() != null &&
                go.GetComponent<FilterIconOverlay>() == null)
            {
                go.AddComponent<FilterIconOverlay>();
            }
        }
    }
}
