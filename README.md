# Storage Filter Icons

An Oxygen Not Included mod that overlays the icons of the **filtered resources** on the sprite of storage buildings,
so you can tell at a glance what each Storage Locker, Smart Storage, Refrigerator or reservoir is set to hold.

## Features

- **Filter icons on the building**: the resources selected in a building's storage filter are drawn as semi-transparent
  icons over the building's main sprite. Icons are arranged in a grid that fits the building's footprint and are
  centered on it.
- **Same order as the filter screen**: categories alphabetically, resources alphabetically within a category.
- **Up to 8 icons**: when more resources are selected, the remaining ones are shown as `+N`.
- **Individual resources only**: nothing is drawn when no resource is selected or when everything is selected.
  Resources without an icon are skipped.
- **Hover behavior**: while the cursor is over a building, its icons can be enlarged, left unchanged or hidden, and
  drawn translucent or fully opaque.
- **Hover card**: the icons are also listed in the card shown when you hover a building (can be turned off).
- **Works with any filtered storage**: every building that has a storage filter is supported automatically,
  including buildings added by other mods. Individual buildings can be switched off in the settings.

## Settings

Open the mod's settings from the Mods screen (powered by PLib).

| Option | Description |
|---|---|
| Icon opacity | Opacity of the overlay icons (0.1 - 1.0, default 0.5) |
| Icons on hover: size | Enlarge (default) / Unchanged / Hide |
| Icons on hover: opacity | Translucent / Opaque (default). Ignored when the size option is Hide |
| Show icons in hover card | Also list the icons in the building's hover card (default on) |
| Buildings | One checkbox per filterable storage building; unchecked buildings show no icons |

Settings are stored in the mod's `config.json`.

## Translations

Option labels are translated through `translations/<language code>.po` (English and Japanese are included).

## Compatibility

- ONI build: **744825 or later**
- Required DLC: none
- [PLib](https://github.com/peterhaneve/ONIMods) is embedded into the mod DLL (ILRepack); no separate install needed.
- Patches `BuildingComplete.OnSpawn` (adds the overlay component) and, for the hover card,
  `HoverTextDrawer.BeginShadowBar` / `EndShadowBar` and `GameUtil.GetUnitFormattedName` (Harmony).

## Known limitations

- Overlay icons may overlap the meters of reservoir-type buildings.

## Local build setup

Build settings follow the `PinnedMaterials` / `CritterCensus` layout.

1. Copy `StorageFilterIcons/Local.Build.props.example` to `StorageFilterIcons/Local.Build.props` (git-ignored).
2. Set `GameManagedPath` (`OxygenNotIncluded_Data\Managed`) and `ModInstallPath` (e.g. `...\mods\local\StorageFilterIcons`).
   Alternatively set the environment variables `ONI_GAME_MANAGED_PATH` / `ONI_MOD_INSTALL_PATH`.
3. `dotnet build` — PLib is merged into the DLL, then the DLL, pdb, `mod.yaml`, `mod_info.yaml` and
   `translations/*.po` are copied to `ModInstallPath`.
4. `dotnet build -c Release` produces a distributable folder in `release/StorageFilterIcons` (git-ignored).

## License

MIT
