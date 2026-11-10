using System.Collections.Generic;
using Newtonsoft.Json;
using PeterHan.PLib;
using PeterHan.PLib.Options;

namespace StorageFilterIcons.Options;

public enum HoverScaleMode
{
    [Option("StorageFilterIcons.STRINGS.OPTIONS.HOVER_SCALE.ENLARGE", "StorageFilterIcons.STRINGS.OPTIONS.HOVER_SCALE.ENLARGE_TOOLTIP")]
    Enlarge,

    [Option("StorageFilterIcons.STRINGS.OPTIONS.HOVER_SCALE.NORMAL", "StorageFilterIcons.STRINGS.OPTIONS.HOVER_SCALE.NORMAL_TOOLTIP")]
    Normal,

    [Option("StorageFilterIcons.STRINGS.OPTIONS.HOVER_SCALE.HIDE", "StorageFilterIcons.STRINGS.OPTIONS.HOVER_SCALE.HIDE_TOOLTIP")]
    Hide,
}

public enum HoverOpacityMode
{
    [Option("StorageFilterIcons.STRINGS.OPTIONS.HOVER_OPACITY.TRANSLUCENT", "StorageFilterIcons.STRINGS.OPTIONS.HOVER_OPACITY.TRANSLUCENT_TOOLTIP")]
    Translucent,

    [Option("StorageFilterIcons.STRINGS.OPTIONS.HOVER_OPACITY.OPAQUE", "StorageFilterIcons.STRINGS.OPTIONS.HOVER_OPACITY.OPAQUE_TOOLTIP")]
    Opaque,
}

[JsonObject(MemberSerialization.OptIn)]
public sealed class ModOptions : SingletonOptions<ModOptions>, IOptions
{
    [Option("StorageFilterIcons.STRINGS.OPTIONS.OPACITY.NAME", "StorageFilterIcons.STRINGS.OPTIONS.OPACITY.TOOLTIP")]
    [Limit(0.1, 1.0)]
    [JsonProperty]
    public float Opacity { get; set; } = 0.5f;

    [Option("StorageFilterIcons.STRINGS.OPTIONS.HOVER_SCALE.NAME", "StorageFilterIcons.STRINGS.OPTIONS.HOVER_SCALE.TOOLTIP")]
    [JsonProperty]
    public HoverScaleMode HoverScale { get; set; } = HoverScaleMode.Enlarge;

    [Option("StorageFilterIcons.STRINGS.OPTIONS.HOVER_OPACITY.NAME", "StorageFilterIcons.STRINGS.OPTIONS.HOVER_OPACITY.TOOLTIP")]
    [JsonProperty]
    public HoverOpacityMode HoverOpacity { get; set; } = HoverOpacityMode.Opaque;

    [Option("StorageFilterIcons.STRINGS.OPTIONS.SHOW_IN_HOVER_CARD.NAME", "StorageFilterIcons.STRINGS.OPTIONS.SHOW_IN_HOVER_CARD.TOOLTIP")]
    [JsonProperty]
    public bool ShowInHoverCard { get; set; } = true;

    // Prefab IDs of buildings whose overlay is switched off. Everything else is enabled, including buildings added later.
    [Option("StorageFilterIcons.STRINGS.OPTIONS.DISABLED_BUILDINGS.NAME", "StorageFilterIcons.STRINGS.OPTIONS.DISABLED_BUILDINGS.TOOLTIP")]
    [DynamicOption(typeof(BuildingToggleListEntry))]
    [JsonProperty]
    public List<string> DisabledBuildings { get; set; } = new List<string>();

    public bool IsBuildingEnabled(string prefabId) => DisabledBuildings == null || !DisabledBuildings.Contains(prefabId);

    public IEnumerable<IOptionsEntry> CreateOptions() => null;

    public void OnOptionsChanged()
    {
        Components.FilterIconOverlay.RefreshAll();
    }
}
