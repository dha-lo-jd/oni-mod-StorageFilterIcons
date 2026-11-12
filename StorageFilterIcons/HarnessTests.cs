#if DEBUG && HARNESS
// In-game tests run by Gravitas.TestHarness:
//   python Gravitas.TestProbe/probe.py run --project <this repo> --tests '{"tags":["storagefiltericons"]}'
// Compiled only in Debug builds that set HarnessDllPath in Local.Build.props (Check / Arrange come from the harness assembly).
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gravitas.TestHarness;
using HarmonyLib;
using PeterHan.PLib.Options;
using StorageFilterIcons.Components;
using StorageFilterIcons.Options;
using StorageFilterIcons.Patches;
using UnityEngine;

namespace StorageFilterIcons;

internal static class HarnessTests
{
    private const string Locker = "StorageLocker";

    // Solids a Storage Bin accepts. The tests mark them as discovered so that they do not depend on the save.
    private static readonly SimHashes[] Solids =
    {
        SimHashes.SandStone, SimHashes.IgneousRock, SimHashes.Granite, SimHashes.Obsidian, SimHashes.SedimentaryRock,
        SimHashes.Cuprite, SimHashes.IronOre, SimHashes.GoldAmalgam, SimHashes.Dirt, SimHashes.Clay, SimHashes.Carbon,
        SimHashes.Algae,
    };

    // UserMod2.OnLoad applies the patches; a second PatchAll would run every patch twice.
    [HarnessTest(Name = "storagefiltericons.patches_applied_once", Tags = "storagefiltericons", RequiresGame = false)]
    public static void PatchesAppliedOnce()
    {
        Check.True(StorageFilterIconsMod.modPath != null, "StorageFilterIconsMod.OnLoad has not run");

        var spawn = Harmony.GetPatchInfo(AccessTools.Method(typeof(BuildingComplete), "OnSpawn"));
        Check.NotNull(spawn, "BuildingComplete.OnSpawn is not patched");
        Check.Equal(1, spawn.Postfixes.Count(p => p.PatchMethod.DeclaringType == typeof(BuildingPatches.BuildingCompleteOnSpawnPatch)),
            "postfixes on BuildingComplete.OnSpawn");

        var begin = Harmony.GetPatchInfo(AccessTools.Method(typeof(HoverTextDrawer), nameof(HoverTextDrawer.BeginShadowBar)));
        Check.NotNull(begin, "HoverTextDrawer.BeginShadowBar is not patched");
        Check.Equal(1, begin.Prefixes.Count(p => p.PatchMethod.DeclaringType == typeof(HoverCardPatches.BeginShadowBarPatch)),
            "prefixes on HoverTextDrawer.BeginShadowBar");

        var end = Harmony.GetPatchInfo(AccessTools.Method(typeof(HoverTextDrawer), nameof(HoverTextDrawer.EndShadowBar)));
        Check.NotNull(end, "HoverTextDrawer.EndShadowBar is not patched");
        Check.Equal(1, end.Prefixes.Count(p => p.PatchMethod.DeclaringType == typeof(HoverCardPatches.EndShadowBarPatch)),
            "prefixes on HoverTextDrawer.EndShadowBar");
    }

    // The settings live outside the mod folder, which a Workshop update replaces.
    [HarnessTest(Name = "storagefiltericons.config_in_shared_folder", Tags = "storagefiltericons", RequiresGame = false)]
    public static void ConfigInSharedFolder()
    {
        var path = POptions.GetConfigFilePath(typeof(ModOptions)).Replace('\\', '/');
        Check.True(path.EndsWith("/mods/config/StorageFilterIcons/config.json"), "config file is at " + path);
    }

    // The settings screen lists the filterable buildings, and only those.
    [HarnessTest(Name = "storagefiltericons.filterable_buildings_listed", Tags = "storagefiltericons")]
    public static void FilterableBuildingsListed()
    {
        var ids = FilterableBuildings.GetDefs().Select(d => d.PrefabID).ToList();
        Check.True(ids.Contains(Locker), "Storage Bin is not listed");
        Check.True(ids.Contains("StorageLockerSmart"), "Smart Storage Bin is not listed");
        Check.True(ids.Contains("Refrigerator"), "Refrigerator is not listed");
        Check.False(ids.Contains("LiquidReservoir"), "Liquid Reservoir has no storage filter but is listed");
        Check.False(ids.Contains("Ladder"), "Ladder is listed");
    }

    // A building with a storage filter gets the overlay; the individually selected resources become icons.
    [HarnessTest(Name = "storagefiltericons.selected_resources_shown", Tags = "storagefiltericons")]
    public static IEnumerator SelectedResourcesShown()
    {
        var locker = BuildLocker();
        try
        {
            yield return null;
            yield return null;
            var overlay = locker.GetComponent<FilterIconOverlay>();
            Check.NotNull(overlay, "the Storage Bin has no FilterIconOverlay");
            var filterable = locker.GetComponent<TreeFilterable>();
            var tags = DiscoverSolids(locker);

            filterable.UpdateFilters(new HashSet<Tag>(tags.Take(3)));
            Check.Equal(3, overlay.Icons.Count, "icons for 3 selected resources");
            Check.Equal(0, overlay.HiddenCount, "hidden count for 3 selected resources");
            Check.True(overlay.IsActive, "the overlay is not visible");
            Check.Equal(3, ActiveIcons(locker), "icon sprites drawn");
            var expected = new HashSet<Sprite>(tags.Take(3).Select(t => Def.GetUISprite(t).first));
            Check.True(overlay.Icons.All(i => expected.Contains(i.Sprite)), "an icon does not belong to a selected resource");
        }
        finally
        {
            Arrange.Destroy(locker);
        }
    }

    // Nothing is drawn when no resource is selected, and nothing when every resource is selected.
    [HarnessTest(Name = "storagefiltericons.hidden_when_none_or_all", Tags = "storagefiltericons")]
    public static IEnumerator HiddenWhenNoneOrAll()
    {
        var locker = BuildLocker();
        try
        {
            yield return null;
            yield return null;
            var overlay = locker.GetComponent<FilterIconOverlay>();
            Check.NotNull(overlay, "the Storage Bin has no FilterIconOverlay");
            var filterable = locker.GetComponent<TreeFilterable>();
            var storage = locker.GetComponent<Storage>();
            var tags = DiscoverSolids(locker);

            filterable.UpdateFilters(new HashSet<Tag>(tags.Take(2)));
            Check.Equal(2, overlay.Icons.Count, "icons before clearing the filter");

            filterable.UpdateFilters(new HashSet<Tag>());
            Check.Equal(0, overlay.Icons.Count, "icons with nothing selected");
            Check.False(overlay.IsActive, "the overlay is visible with nothing selected");

            var everything = new HashSet<Tag>();
            foreach (var category in storage.storageFilters)
                foreach (var item in DiscoveredResources.Instance.GetDiscoveredResourcesFromTag(category))
                    everything.Add(item);
            filterable.UpdateFilters(everything);
            Check.Equal(0, overlay.Icons.Count, "icons with everything selected (" + everything.Count + " resources)");
            Check.False(overlay.IsActive, "the overlay is visible with everything selected");
        }
        finally
        {
            Arrange.Destroy(locker);
        }
    }

    // At most 8 icons are drawn; the rest is shown as "+N".
    [HarnessTest(Name = "storagefiltericons.more_than_eight", Tags = "storagefiltericons")]
    public static IEnumerator MoreThanEight()
    {
        var locker = BuildLocker();
        try
        {
            yield return null;
            yield return null;
            var overlay = locker.GetComponent<FilterIconOverlay>();
            Check.NotNull(overlay, "the Storage Bin has no FilterIconOverlay");
            var tags = DiscoverSolids(locker);

            locker.GetComponent<TreeFilterable>().UpdateFilters(new HashSet<Tag>(tags.Take(10)));
            Check.Equal(10, overlay.Icons.Count, "icons for 10 selected resources");
            Check.Equal(2, overlay.HiddenCount, "hidden count for 10 selected resources");
            Check.Equal(FilterIconOverlay.MaxIcons, ActiveIcons(locker), "icon sprites drawn");
            var more = locker.GetComponentsInChildren<TextMesh>(false).FirstOrDefault();
            Check.NotNull(more, "the +N label is not shown");
            Check.Equal("+2", more.text, "text of the +N label");
        }
        finally
        {
            Arrange.Destroy(locker);
        }
    }

    // Icons follow the filter screen: resources of one category are in alphabetical order of their names.
    [HarnessTest(Name = "storagefiltericons.sorted_by_name", Tags = "storagefiltericons")]
    public static IEnumerator SortedByName()
    {
        var locker = BuildLocker();
        try
        {
            yield return null;
            yield return null;
            var overlay = locker.GetComponent<FilterIconOverlay>();
            Check.NotNull(overlay, "the Storage Bin has no FilterIconOverlay");
            DiscoverSolids(locker);

            // All four are raw minerals, so they share a category.
            var rocks = new[] { SimHashes.SandStone, SimHashes.Granite, SimHashes.Obsidian, SimHashes.IgneousRock }
                .Select(hash => ElementLoader.FindElementByHash(hash)).ToList();
            Check.Equal(1, rocks.Select(e => e.GetMaterialCategoryTag()).Distinct().Count(), "categories of the four rocks");

            locker.GetComponent<TreeFilterable>().UpdateFilters(new HashSet<Tag>(rocks.Select(e => e.tag)));
            var expected = rocks.OrderBy(e => e.tag.ProperNameStripLink(), System.StringComparer.CurrentCulture)
                .Select(e => Def.GetUISprite(e.tag).first).ToList();
            Check.Equal(expected.Count, overlay.Icons.Count, "icons for the four rocks");
            for (var i = 0; i < expected.Count; i++)
                Check.True(overlay.Icons[i].Sprite == expected[i], "icon " + i + " is out of order");
        }
        finally
        {
            Arrange.Destroy(locker);
        }
    }

    // A building switched off in the settings shows no icons; switching it back on restores them.
    [HarnessTest(Name = "storagefiltericons.building_switched_off", Tags = "storagefiltericons")]
    public static IEnumerator BuildingSwitchedOff()
    {
        var options = ModOptions.Instance;
        var saved = options.DisabledBuildings;
        var locker = BuildLocker();
        try
        {
            yield return null;
            yield return null;
            var overlay = locker.GetComponent<FilterIconOverlay>();
            Check.NotNull(overlay, "the Storage Bin has no FilterIconOverlay");
            var tags = DiscoverSolids(locker);
            options.DisabledBuildings = new List<string>();
            locker.GetComponent<TreeFilterable>().UpdateFilters(new HashSet<Tag>(tags.Take(3)));
            Check.Equal(3, overlay.Icons.Count, "icons while the building is switched on");

            options.DisabledBuildings = new List<string> { Locker };
            options.OnOptionsChanged();
            Check.Equal(0, overlay.Icons.Count, "icons while the building is switched off");
            Check.False(overlay.IsActive, "the overlay is visible while the building is switched off");

            options.DisabledBuildings = new List<string>();
            options.OnOptionsChanged();
            Check.Equal(3, overlay.Icons.Count, "icons after switching the building back on");
        }
        finally
        {
            // Only the in-memory options were changed; config.json is untouched.
            options.DisabledBuildings = saved;
            Arrange.Destroy(locker);
        }
    }

    // Places a Storage Bin in the nearest free 1x2 spot. The test save is an empty sandbox with no floor near the
    // anchor, so the build-location rule (on floor) is skipped: the overlay does not depend on it.
    private static GameObject BuildLocker()
    {
        var def = Assets.GetBuildingDef(Locker);
        Grid.CellToXY(Arrange.HomeCell(), out var cx, out var cy);
        for (var r = 0; r <= 40; r++)
            for (var dx = -r; dx <= r; dx++)
                for (var dy = -r; dy <= r; dy++)
                {
                    if (System.Math.Max(System.Math.Abs(dx), System.Math.Abs(dy)) != r) continue;
                    var cell = Grid.XYToCell(cx + dx, cy + dy);
                    if (!IsFree(cell) || !IsFree(Grid.CellAbove(cell))) continue;
                    var locker = def.Build(cell, Orientation.Neutral, null, def.DefaultElements(), 293.15f, false,
                        GameClock.Instance.GetTime());
                    Check.NotNull(locker, "the game did not build a Storage Bin");
                    return locker;
                }

        Check.Fail("no free 1x2 spot for a Storage Bin near the anchor cell");
        return null;
    }

    private static bool IsFree(int cell) =>
        Grid.IsValidCell(cell) && Grid.IsVisible(cell) && !Grid.Solid[cell] && Grid.Objects[cell, (int)ObjectLayer.Building] == null;

    // Marks the test solids as discovered and returns the ones the building's filter offers (at least 10).
    private static List<Tag> DiscoverSolids(GameObject building)
    {
        var filters = building.GetComponent<Storage>().storageFilters;
        var tags = new List<Tag>();
        foreach (var hash in Solids)
        {
            var element = ElementLoader.FindElementByHash(hash);
            var category = element.GetMaterialCategoryTag();
            if (!filters.Contains(category)) continue;
            DiscoveredResources.Instance.Discover(element.tag, category);
            tags.Add(element.tag);
        }

        Check.True(tags.Count >= 10, "only " + tags.Count + " of the test solids can be stored in the building");
        return tags;
    }

    private static int ActiveIcons(GameObject building) => building.GetComponentsInChildren<SpriteRenderer>(false).Length;
}
#endif
