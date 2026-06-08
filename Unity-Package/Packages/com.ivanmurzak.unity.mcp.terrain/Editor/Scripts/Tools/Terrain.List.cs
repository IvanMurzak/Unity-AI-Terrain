/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                        │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-AI-Terrain)        │
│  Copyright (c) 2025 Ivan Murzak                                             │
│  Licensed under the MIT License.                                            │
│  See the LICENSE file in the project root for more information.             │
└─────────────────────────────────────────────────────────────────────────────┘
*/

#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Terrain
    {
        public const string TerrainListToolId = "terrain-list";

        [AiTool
        (
            TerrainListToolId,
            Title = "Terrain / List Terrains",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("List every `Terrain` in the active scene with its name, world size, heightmap " +
            "resolution, and TerrainLayer count. Read-only.")]
        [AiSkillBody("List every `Terrain` in the active scene.\n\n" +
            "## Inputs\n\n" +
            "- `includeInactive` (bool, default true) — include Terrains on inactive/disabled GameObjects.\n\n" +
            "## Behavior\n\n" +
            "Finds all `Terrain` instances, and for each returns the GameObject reference, the name, the " +
            "`TerrainData.size`, the heightmap resolution, and the number of TerrainLayers. Read-only. The whole " +
            "call runs on the Unity main thread.")]
        [Description("Lists all Terrains in the active scene with name, size, heightmap resolution and layer count. " +
            "Read-only.")]
        public TerrainListResponse ListTerrains
        (
            [Description("If true (default), include Terrains on inactive/disabled GameObjects; if false, only active ones.")]
            bool includeInactive = true
        )
        {
            return MainThread.Instance.Run(() =>
            {
#if UNITY_2023_1_OR_NEWER
                var terrains = UnityEngine.Object.FindObjectsByType<UnityEngine.Terrain>(
                    includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude, FindObjectsSortMode.None);
#else
                var terrains = UnityEngine.Object.FindObjectsOfType<UnityEngine.Terrain>(includeInactive);
#endif

                var items = new List<TerrainListItem>(terrains.Length);
                foreach (var terrain in terrains)
                {
                    if (terrain == null)
                        continue;
                    var data = terrain.terrainData;
                    items.Add(new TerrainListItem
                    {
                        gameObjectRef = new GameObjectRef(terrain.gameObject),
                        terrainRef = new ComponentRef(terrain),
                        name = terrain.name,
                        size = data != null ? data.size : Vector3.zero,
                        heightmapResolution = data != null ? data.heightmapResolution : 0,
                        layerCount = data != null && data.terrainLayers != null ? data.terrainLayers.Length : 0
                    });
                }

                return new TerrainListResponse
                {
                    count = items.Count,
                    terrains = items.ToArray()
                };
            });
        }

        public class TerrainListResponse
        {
            [Description("Number of Terrains found.")]
            public int count;

            [Description("The Terrains in the active scene.")]
            public TerrainListItem[] terrains = Array.Empty<TerrainListItem>();
        }

        public class TerrainListItem
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Name of the terrain GameObject.")]
            public string name = string.Empty;

            [Description("Terrain size (X=width, Y=height, Z=length).")]
            public Vector3 size;

            [Description("Heightmap resolution.")]
            public int heightmapResolution;

            [Description("Number of TerrainLayers assigned.")]
            public int layerCount;
        }
    }
}
