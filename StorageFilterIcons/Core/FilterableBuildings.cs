using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace StorageFilterIcons;

internal static class FilterableBuildings
{
    private static readonly Dictionary<Type, bool> UsesFilteredStorageCache = new Dictionary<Type, bool>();

    // Building defs whose completed building has a storage filter (the ones FilterIconOverlay is attached to).
    public static IEnumerable<BuildingDef> GetDefs()
    {
        if (Assets.BuildingDefs == null)
        {
            return Enumerable.Empty<BuildingDef>();
        }

        return Assets.BuildingDefs
            .Where(def => def != null && !def.Deprecated && def.BuildingComplete != null && HasStorageFilter(def.BuildingComplete))
            .OrderBy(def => def.Name)
            .ToList();
    }

    // A spawned building has a TreeFilterable either because its config adds one (rocket cargo bays) or because one of
    // its components creates a FilteredStorage, which adds the TreeFilterable only when the building is instantiated
    // (Storage Bin, Refrigerator, ...). The prefab is all we have here, so the second case is recognised by that field.
    private static bool HasStorageFilter(GameObject prefab)
    {
        if (prefab.GetComponent<Storage>() == null)
        {
            return false;
        }

        if (prefab.GetComponent<TreeFilterable>() != null)
        {
            return true;
        }

        foreach (var component in prefab.GetComponents<KMonoBehaviour>())
        {
            if (component != null && UsesFilteredStorage(component.GetType()))
            {
                return true;
            }
        }

        return false;
    }

    private static bool UsesFilteredStorage(Type type)
    {
        if (UsesFilteredStorageCache.TryGetValue(type, out var known))
        {
            return known;
        }

        var result = false;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        for (var t = type; t != null && t != typeof(KMonoBehaviour) && !result; t = t.BaseType)
        {
            result = t.GetFields(flags).Any(f => f.FieldType == typeof(FilteredStorage));
        }

        UsesFilteredStorageCache[type] = result;
        return result;
    }
}
