# Copilot instructions for StorageFilterIcons

ONI (Oxygen Not Included) mod. Structure and build setup intentionally mirror the sibling `PinnedMaterials` / `CritterCensus` solutions.

- `StorageFilterIcons/StorageFilterIcons.csproj`: `netstandard2.1`, LangVersion 10.0, references game DLLs directly from `$(GameManagedPath)`.
- `StorageFilterIcons/Local.Build.props` (git-ignored; template: `Local.Build.props.example`) defines `GameManagedPath` and `ModInstallPath`. Env fallbacks: `ONI_GAME_MANAGED_PATH`, `ONI_MOD_INSTALL_PATH`.
- Debug builds copy the DLL, pdb, `mod.yaml`, `mod_info.yaml`, `translations/*.po` and `ModAssets/*.png` to `$(ModInstallPath)`.
- Release builds produce a clean distribution folder in `release/StorageFilterIcons` (git-ignored).

## Build

```bash
dotnet build
```

Requires a local ONI install (real game assemblies; there is no stub mode). No tests or linters are configured.

## Code layout

- `StorageFilterIconsMod.cs`: `UserMod2` entry point; applies all Harmony patches and loads translations.
- File-scoped namespaces, no implicit usings, nullable disabled.
