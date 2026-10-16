# AGENTS.md — Unity-AI-Terrain

Unity package `com.ivanmurzak.unity.mcp.terrain` — wraps Unity's built-in **Terrain** modules
(`com.unity.modules.terrain` + `com.unity.modules.terrainphysics`) available to Unity-MCP's RQL tools (`rql-query` /
`rql-mutate`; no extension MCP tools): the `terrain` source (Create, CreateLayer), Terrain methods for heights, splat
painting, layers, prototypes, trees and neighbors, validated TerrainData members, and `terrain.*` height sampling
functions. An extension of
[AI Game Developer](https://github.com/IvanMurzak/Unity-MCP) (`com.ivanmurzak.unity.mcp`), generated from
`Unity-AI-Tools-Template`; the user-facing tool list is in `README.md`.

## Layout

- `Unity-Package/` — Unity project hosting the package; only `Packages/com.ivanmurzak.unity.mcp.terrain/`
  ships.
  - `package.json` — the version source of truth and the dependency pins.
  - `Editor/Scripts/Rql/` — branch `experiment/rql`: the `TerrainModule` (IRqlModule: `terrain` source, `terrain.*`
    functions, one adapter per file, the agent guide); replaces the former `terrain-*` tools.
  - `Runtime/` — runtime assembly (currently empty).
  - `Tests/Editor/Rql/` (EditMode, `TerrainTestBase` + one file per adapter / source) and `Tests/Runtime/` (PlayMode).
- `Unity-Tests/<unity-version>/` — one test project per supported Unity version, consuming the package via
  `file:`. A new version must also be added to `test_pull_request.yml` and `release.yml` (and
  `open-all-projects-*`).
- `Installer/` — Unity project that exports the `.unitypackage` installer
  (`Assets/com.IvanMurzak/<name> Installer/`).
- `commands/` — PowerShell helpers (below) and `check-versions.py` (synced in by the release cascade — do not
  hand-edit; CI runs it as the `version-consistency` gate).
- `.github/workflows/` — `test_pull_request.yml` (PR test matrix), `test_unity_plugin.yml` (reusable test
  job), `release.yml`, `bump_version.yml`.
- `docs/` — user docs: `Deploy-{OpenUPM,GitHub,npmjs}.md`, `Manual-Package-Rename.md`, `openupm-signing.md`,
  `img/`, `promo/`.

## Commands (PowerShell, from the repo root)

```powershell
./commands/bump-version.ps1 -NewVersion "x.y.z"   # package.json + Installer.cs in lock-step; -WhatIf to preview
./commands/get-version.ps1
./commands/update-ai-game-developer.ps1           # bump the com.ivanmurzak.unity.mcp pin; -WhatIf to preview
./commands/open-all-projects-windows.ps1          # open all Unity projects (or -unix.sh)
```

Tests run inside the Unity Editor (NUnit + `[UnityTest]`); CI runs them with `game-ci/unity-test-runner` in
EditMode / PlayMode / Standalone for every Unity version. `release.yml` releases on push to `main` only when
the `package.json` version has no tag yet (installer, signed UPM tarball for OpenUPM, test matrix, tag, GitHub
Release); dispatch `bump_version.yml` to bump in CI. Under `pull_request_target`, tests need the
maintainer-applied `ci-ok` label and abort if the PR touches `.github/workflows/`.

## Critical invariants

- **RQL surface.** Everything is registered by `TerrainModule` (`IRqlModule`, Order 100, found through TypeCache) and
  documented in its `AddGuide("Terrain", …)` text, which is rendered into the `unity-rql` skill.
- **Undo.** The engine records the call target (the Terrain); heights, splat maps, layers and trees live in the
  TerrainData asset, so every Terrain method writes through `TerrainOps.Write` (records the TerrainData, and the splat
  textures when painting, then marks both dirty).
- **Copies.** TerrainData returns copies of its layer / prototype / tree arrays: the TerrainData adapter assigns element
  writes back (`CopyOnRead`), and prototype members are read-only (an element is replaced whole from an object literal).
- **Terrain is a built-in module**: it ships with and version-locks to the Editor; no extra registry package
  or asmdef reference beyond the base UnityEngine assemblies.
- **Alias `UnityEngine.Terrain`** inside the `…Rql.Terrains` namespace block: short names may resolve to namespaces.
- Every C# file starts with the copyright box header and `#nullable enable`.
