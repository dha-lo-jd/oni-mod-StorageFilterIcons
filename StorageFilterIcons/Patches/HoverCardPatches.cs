using HarmonyLib;
using StorageFilterIcons.Components;
using StorageFilterIcons.Options;
using UnityEngine;

namespace StorageFilterIcons.Patches;

// Adds a row of filter icons to a building's hover card.
// The card is built sequentially per selectable: BeginShadowBar -> title (GameUtil.GetUnitFormattedName(go)) -> ... -> EndShadowBar.
// We remember the object named in the title and draw the icon row just before the shadow bar is closed.
public static class HoverCardPatches
{
    private static GameObject lastObject;
    private static HoverTextConfiguration configuration;

    [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginShadowBar))]
    public static class BeginShadowBarPatch
    {
        public static void Prefix()
        {
            lastObject = null;
        }
    }

    [HarmonyPatch(typeof(GameUtil), nameof(GameUtil.GetUnitFormattedName), typeof(GameObject), typeof(bool))]
    public static class GetUnitFormattedNamePatch
    {
        public static void Prefix(GameObject go)
        {
            if (lastObject == null)
            {
                lastObject = go;
            }
        }
    }

    [HarmonyPatch(typeof(HoverTextDrawer), nameof(HoverTextDrawer.EndShadowBar))]
    public static class EndShadowBarPatch
    {
        public static void Prefix(HoverTextDrawer __instance)
        {
            var go = lastObject;
            lastObject = null;
            if (go == null || !ModOptions.Instance.ShowInHoverCard)
            {
                return;
            }

            var overlay = go.GetComponent<FilterIconOverlay>();
            if (overlay == null || !overlay.IsEnabledByOptions || overlay.Icons.Count == 0)
            {
                return;
            }

            __instance.NewLine();
            var shown = System.Math.Min(overlay.Icons.Count, FilterIconOverlay.MaxIcons);
            for (var i = 0; i < shown; i++)
            {
                var icon = overlay.Icons[i];
                __instance.DrawIcon(icon.Sprite, icon.Color.WithAlpha(1f), 18, 2);
            }

            if (overlay.HiddenCount > 0)
            {
                if (configuration == null)
                {
                    configuration = Object.FindFirstObjectByType<HoverTextConfiguration>();
                }

                if (configuration != null)
                {
                    __instance.DrawText("+" + overlay.HiddenCount, configuration.Styles_BodyText.Standard);
                }
            }
        }
    }

    private static Color WithAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
