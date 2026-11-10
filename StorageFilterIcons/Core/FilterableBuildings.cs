using System.Collections.Generic;
using System.Linq;

namespace StorageFilterIcons;

internal static class FilterableBuildings
{
    // Building defs whose completed prefab has a TreeFilterable (storage-like buildings with a filter).
    public static IEnumerable<BuildingDef> GetDefs()
    {
        if (Assets.BuildingDefs == null)
        {
            return Enumerable.Empty<BuildingDef>();
        }

        return Assets.BuildingDefs
            .Where(def => def != null && !def.Deprecated && def.BuildingComplete != null &&
                          def.BuildingComplete.GetComponent<TreeFilterable>() != null)
            .OrderBy(def => def.Name)
            .ToList();
    }
}
