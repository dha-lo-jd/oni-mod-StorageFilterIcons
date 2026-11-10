using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StorageFilterIcons;

internal readonly struct FilterIcon
{
    public readonly Sprite Sprite;
    public readonly Color Color;

    public FilterIcon(Sprite sprite, Color color)
    {
        Sprite = sprite;
        Color = color;
    }
}

internal static class FilterIconSource
{
    /// <summary>
    /// Icons of the individually selected resources, in the same order as the filter side screen
    /// (categories alphabetically, items alphabetically within a category).
    /// Returns an empty list when nothing is selected or when everything is selected, and skips tags that have no icon.
    /// </summary>
    public static List<FilterIcon> GetIcons(TreeFilterable filterable, Storage storage)
    {
        var result = new List<FilterIcon>();
        if (filterable == null || storage == null || storage.storageFilters == null ||
            DiscoveredResources.Instance == null)
        {
            return result;
        }

        var accepted = filterable.AcceptedTags;
        if (accepted == null || accepted.Count == 0)
        {
            return result;
        }

        var seen = new HashSet<Tag>();
        var selected = new List<Tag>();
        var allSelected = true;
        var anyItem = false;

        foreach (var category in SortedByName(storage.storageFilters))
        {
            if (!DiscoveredResources.Instance.IsDiscovered(category))
            {
                continue;
            }

            var categorySelected = accepted.Contains(category);
            var items = DiscoveredResources.Instance.GetDiscoveredResourcesFromTag(category)
                .Where(t => !filterable.ForbiddenTags.Contains(t));
            foreach (var item in SortedByName(items))
            {
                anyItem = true;
                if (categorySelected || accepted.Contains(item))
                {
                    if (seen.Add(item))
                    {
                        selected.Add(item);
                    }
                }
                else
                {
                    allSelected = false;
                }
            }
        }

        if (!anyItem || allSelected)
        {
            return result;
        }

        foreach (var tag in selected)
        {
            if (TryGetIcon(tag, out var icon))
            {
                result.Add(icon);
            }
        }

        return result;
    }

    private static List<Tag> SortedByName(IEnumerable<Tag> tags)
    {
        return tags.Select(t => (tag: t, name: t.ProperNameStripLink()))
            .OrderBy(x => x.name, System.StringComparer.CurrentCulture)
            .Select(x => x.tag)
            .ToList();
    }

    private static bool TryGetIcon(Tag tag, out FilterIcon icon)
    {
        icon = default;
        var pair = Def.GetUISprite(tag);
        var sprite = pair?.first;
        if (sprite == null || sprite.name == "unknown" || sprite.name == "unknown_far")
        {
            return false;
        }

        icon = new FilterIcon(sprite, pair.second);
        return true;
    }
}
