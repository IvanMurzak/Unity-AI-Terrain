<h1 align="center"><a href="https://github.com/IvanMurzak/Unity-AI-Terrain?tab=readme-ov-file#unity-ai-terrain">Unity AI Terrain</a></h1>

<div align="center" width="100%">

[![MCP](https://badge.mcpx.dev 'MCP Server')](https://modelcontextprotocol.io/introduction)
[![OpenUPM](https://img.shields.io/npm/v/com.ivanmurzak.unity.mcp.terrain?label=OpenUPM&registry_uri=https://package.openupm.com&labelColor=333A41 'OpenUPM package')](https://openupm.com/packages/com.ivanmurzak.unity.mcp.terrain/)
[![Unity Editor](https://img.shields.io/badge/Editor-X?style=flat&logo=unity&labelColor=333A41&color=2A2A2A 'Unity Editor supported')](https://unity.com/releases/editor/archive)
[![r](https://github.com/IvanMurzak/Unity-AI-Terrain/workflows/release/badge.svg 'Tests Passed')](https://github.com/IvanMurzak/Unity-AI-Terrain/actions/workflows/release.yml)</br>
[![Discord](https://img.shields.io/badge/Discord-Join-7289da?logo=discord&logoColor=white&labelColor=333A41 'Join')](https://discord.gg/cfbdMZX99G)
[![Stars](https://img.shields.io/github/stars/IvanMurzak/Unity-AI-Terrain 'Stars')](https://github.com/IvanMurzak/Unity-AI-Terrain/stargazers)
[![License](https://img.shields.io/github/license/IvanMurzak/Unity-AI-Terrain?label=License&labelColor=333A41)](https://github.com/IvanMurzak/Unity-AI-Terrain/blob/main/LICENSE)
[![Stand With Ukraine](https://raw.githubusercontent.com/vshymanskyy/StandWithUkraine/main/badges/StandWithUkraine.svg)](https://stand-with-ukraine.pp.ua)

</div>

AI-powered tools for the Unity [Terrain](https://docs.unity3d.com/Manual/script-Terrain.html) workflow. Create Terrain GameObjects backed by new TerrainData, set heightmap resolution and world size, sculpt and sample heights over a region or the whole terrain, manage TerrainLayers, paint texture layers onto the alphamap, configure detail and tree prototypes, scatter trees, stitch neighbor terrains, and modify any terrain component field directly through natural language commands — no manual inspector navigation. Wraps Unity's built-in **Terrain** modules (`com.unity.modules.terrain` + `com.unity.modules.terrainphysics`). Ideal for rapid greyboxing, procedural landscape generation, and terrain authoring. Built on top of the [AI Game Developer](https://github.com/IvanMurzak/Unity-MCP) platform.

### How to use

- [Instructions](https://github.com/IvanMurzak/Unity-MCP?tab=readme-ov-file#step-2-install-mcp-client)
- [Video Tutorial for Visual Studio Code](https://www.youtube.com/watch?v=ZhP7Ju91mOE)
- [Video Tutorial for Visual Studio](https://www.youtube.com/watch?v=RGdak4T69mc)

[![DOWNLOAD INSTALLER](https://github.com/IvanMurzak/Unity-MCP/blob/main/docs/img/button/button_download.svg?raw=true)](https://github.com/IvanMurzak/Unity-AI-Terrain/releases/latest/download/AI-Terrain-Installer.unitypackage)

### Stability status

| Unity Version | Editmode                                                                                                                                                                                                  | Playmode                                                                                                                                                                                                  | Standalone                                                                                                                                                                                                  |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 2022.3.62f3   | [![r](https://github.com/IvanMurzak/Unity-AI-Terrain/workflows/release/badge.svg?job=test-unity-2022-3-62f3-editmode)](https://github.com/IvanMurzak/Unity-AI-Terrain/actions/workflows/release.yml)       | [![r](https://github.com/IvanMurzak/Unity-AI-Terrain/workflows/release/badge.svg?job=test-unity-2022-3-62f3-playmode)](https://github.com/IvanMurzak/Unity-AI-Terrain/actions/workflows/release.yml)       | [![r](https://github.com/IvanMurzak/Unity-AI-Terrain/workflows/release/badge.svg?job=test-unity-2022-3-62f3-standalone)](https://github.com/IvanMurzak/Unity-AI-Terrain/actions/workflows/release.yml)       |
| 2023.2.22f1   | [![r](https://github.com/IvanMurzak/Unity-AI-Terrain/workflows/release/badge.svg?job=test-unity-2023-2-22f1-editmode)](https://github.com/IvanMurzak/Unity-AI-Terrain/actions/workflows/release.yml)       | [![r](https://github.com/IvanMurzak/Unity-AI-Terrain/workflows/release/badge.svg?job=test-unity-2023-2-22f1-playmode)](https://github.com/IvanMurzak/Unity-AI-Terrain/actions/workflows/release.yml)       | [![r](https://github.com/IvanMurzak/Unity-AI-Terrain/workflows/release/badge.svg?job=test-unity-2023-2-22f1-standalone)](https://github.com/IvanMurzak/Unity-AI-Terrain/actions/workflows/release.yml)       |
| 6000.3.1f1    | [![r](https://github.com/IvanMurzak/Unity-AI-Terrain/workflows/release/badge.svg?job=test-unity-6000-3-1f1-editmode)](https://github.com/IvanMurzak/Unity-AI-Terrain/actions/workflows/release.yml)        | [![r](https://github.com/IvanMurzak/Unity-AI-Terrain/workflows/release/badge.svg?job=test-unity-6000-3-1f1-playmode)](https://github.com/IvanMurzak/Unity-AI-Terrain/actions/workflows/release.yml)        | [![r](https://github.com/IvanMurzak/Unity-AI-Terrain/workflows/release/badge.svg?job=test-unity-6000-3-1f1-standalone)](https://github.com/IvanMurzak/Unity-AI-Terrain/actions/workflows/release.yml)        |

## AI Terrain Tools

16 tools, grouped by purpose:

### Terrain lifecycle

- `terrain-create` - Create a `Terrain` GameObject backed by a new `TerrainData` asset (heightmap resolution + size)
- `terrain-list` - List all Terrains in the active scene (name, size, heightmap resolution, layer count)
- `terrain-get` - Get a Terrain's data (size, resolutions, layers, prototypes, tree count, neighbors)

### Shape & heightmap

- `terrain-set-heightmap-resolution` - Set the heightmap resolution (rounded to a valid 2^n + 1)
- `terrain-set-size` - Set the terrain world size (width / height / length)
- `terrain-set-heights` - Set heightmap values over a region (or whole terrain): uniform fill or explicit 2D array
- `terrain-sample-heights` - Read heightmap values over a region (or whole terrain) with min/max/average stats

### Layers & painting

- `terrain-add-layer` - Add a `TerrainLayer` (from an existing asset or a new layer built from a texture)
- `terrain-remove-layer` - Remove a `TerrainLayer` by index
- `terrain-paint-layer` - Paint a layer over a region by writing the alphamap (splatmap)

### Detail, trees & tiling

- `terrain-set-detail-prototypes` - Replace the detail prototypes (grass / detail meshes) from textures / prefabs
- `terrain-set-tree-prototypes` - Replace the tree prototypes from prefabs
- `terrain-place-trees` - Place trees: random scatter (count) or explicit normalized positions
- `terrain-set-neighbors` - Set the left / top / right / bottom neighbor Terrains so Unity blends seams

### Generic

- `terrain-get-component` - Generic read: serialize a `Terrain` / `TerrainCollider` component via ReflectorNet
- `terrain-modify-component` - Generic write: apply a `SerializedMember` diff to a terrain component via ReflectorNet (escape hatch for fields not covered by the dedicated tools)

## Installation

### Option 1 - Installer

- **[Download Installer](https://github.com/IvanMurzak/Unity-AI-Terrain/releases/latest/download/AI-Terrain-Installer.unitypackage)**
- **Import installer into Unity project**
  > - You can double-click on the file - Unity will open it automatically
  > - OR: Open Unity Editor first, then click on `Assets/Import Package/Custom Package`, and choose the file

### Option 2 - OpenUPM-CLI

- [Install OpenUPM-CLI](https://github.com/openupm/openupm-cli#installation)
- Open the command line in your Unity project folder

```bash
openupm add com.ivanmurzak.unity.mcp.terrain
```
