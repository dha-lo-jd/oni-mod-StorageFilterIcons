# CLAUDE.md

ONI mod. Build: `dotnet build` (Debug copies to `ModInstallPath` from `Local.Build.props`; Release creates `release/StorageFilterIcons`). 共通事項は HQ（親フォルダ）の `CLAUDE.md`。機能と設定の説明は `README.md`。

- PLib は ILRepack で Mod の DLL に統合している（csproj の `MergeAssemblies`）。
- 設定は `mods/config/StorageFilterIcons/config.json`（`ModOptions` の `[ConfigFile(SharedConfigLocation: true)]`）。Workshop の更新で Mod フォルダが入れ替わっても残る。
- Workshop 用の説明文と画像は `Workshop/`。機能や設定を変えたら、`README.md` と英日の説明文も合わせる。

## 実機検証
ONI 実機で検証するには Gravitas.TestHarness を使う。テストは `[HarnessTest]` を付けた static メソッドとして書く（`StorageFilterIcons/HarnessTests.cs`、`#if DEBUG && HARNESS`。`HARNESS` は `Local.Build.props` に `HarnessDllPath` があるときだけ定義される）。
実行: `python C:/Users/dha_1/ONI-Modding/GravitasTestHarness/Gravitas.TestProbe/probe.py run --project . --tests '{"tags":["storagefiltericons"]}'`
ゲームを起動し本番の mods.json を書き換えるので、実行前にユーザーに確認する。詳細は harness repo の `README.md` と `protocol.md`。

テストが見ていないもの: ホバー時の拡大・不透明化、ホバーカードへの表示、設定画面の操作、見た目（位置や大きさ）。
