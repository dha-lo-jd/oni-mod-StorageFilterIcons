using System.Collections.Generic;
using System.Linq;
using PeterHan.PLib.Options;
using PeterHan.PLib.UI;
using UnityEngine;

namespace StorageFilterIcons.Options;

// Options entry showing one checkbox per filterable storage building (enumerated from the loaded building defs,
// so buildings added by other mods are included). The stored value is the list of DISABLED prefab IDs.
public sealed class BuildingToggleListEntry : OptionsEntry
{
    private readonly HashSet<string> disabled = new HashSet<string>();

    public BuildingToggleListEntry(string field, IOptionSpec spec) : base(field, spec)
    {
    }

    public override object Value
    {
        get => disabled.OrderBy(s => s).ToList();
        set
        {
            disabled.Clear();
            if (value is IEnumerable<string> list)
            {
                foreach (var id in list)
                {
                    disabled.Add(id);
                }
            }
        }
    }

    public override bool WriteTo(object settings)
    {
        if (settings is ModOptions options)
        {
            var before = new HashSet<string>(options.DisabledBuildings ?? new List<string>());
            var changed = !before.SetEquals(disabled);
            options.DisabledBuildings = (List<string>)Value;
            return changed;
        }

        return false;
    }

    public override void CreateUIEntry(PGridPanel parent, ref int row)
    {
        parent.AddChild(new PLabel("BuildingsLabel")
        {
            Text = LookInStrings(Title),
            ToolTip = LookInStrings(Tooltip),
            TextStyle = PUITuning.Fonts.TextLightStyle,
        }, new GridComponentSpec(row, 0) { ColumnSpan = 2, Margin = LABEL_MARGIN, Alignment = TextAnchor.MiddleLeft });
        row++;
        parent.AddRow(new GridRowSpec());
        parent.AddChild(BuildList(), new GridComponentSpec(row, 0) { ColumnSpan = 2, Margin = CONTROL_MARGIN, Alignment = TextAnchor.MiddleLeft });
    }

    public override GameObject GetUIComponent() => new GameObject("Empty");

    private PPanel BuildList()
    {
        var panel = new PPanel("BuildingList") { Direction = PanelDirection.Vertical, Alignment = TextAnchor.UpperLeft, Spacing = 2 };
        foreach (var def in FilterableBuildings.GetDefs())
        {
            var id = def.PrefabID;
            panel.AddChild(new PCheckBox("Toggle_" + id)
            {
                Text = def.Name == null ? id : global::STRINGS.UI.StripLinkFormatting(def.Name),
                TextStyle = PUITuning.Fonts.TextLightStyle,
                InitialState = disabled.Contains(id) ? PCheckBox.STATE_UNCHECKED : PCheckBox.STATE_CHECKED,
                OnChecked = (source, state) =>
                {
                    // The callback receives the state being left; the new state is its opposite.
                    if (state == PCheckBox.STATE_UNCHECKED)
                    {
                        disabled.Remove(id);
                        PCheckBox.SetCheckState(source, PCheckBox.STATE_CHECKED);
                    }
                    else
                    {
                        disabled.Add(id);
                        PCheckBox.SetCheckState(source, PCheckBox.STATE_UNCHECKED);
                    }
                },
            });
        }

        return panel;
    }
}
