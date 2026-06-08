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
using UnityEditor;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Terrain
    {
        public const string TerrainGetToolId = "terrain-get";

        [AiTool
        (
            TerrainGetToolId,
            Title = "Terrain / Get Terrain",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Inspect a `Terrain` — its `TerrainData` size, heightmap / alphamap / detail resolutions, " +
            "the list of TerrainLayers, tree/detail prototype counts, tree-instance count, and neighbor terrains. " +
            "Read-only.")]
        [AiSkillBody("Inspect a `Terrain` component in the active scene and report its `TerrainData` configuration.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n\n" +
            "## Behavior\n\n" +
            "Reads `TerrainData.size`, `heightmapResolution`, `alphamapResolution`, `detailResolution`, the " +
            "`terrainLayers` (with their diffuse-texture names), the tree- and detail-prototype counts, the number " +
            "of placed tree instances, and the left/top/right/bottom neighbor terrains. Read-only — nothing is " +
            "mutated. The whole call runs on the Unity main thread.")]
        [Description("Get the configuration of a Terrain: size, resolutions, layers, prototypes, tree count, and " +
            "neighbors. Read-only.")]
        public TerrainGetResponse GetTerrain
        (
            [Description("Reference to the GameObject containing the Terrain component.")]
            GameObjectRef gameObjectRef
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));
            if (!gameObjectRef.IsValid(out var validationError))
                throw new ArgumentException(validationError, nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));

                var layers = new List<TerrainLayerInfo>();
                var terrainLayers = data.terrainLayers;
                if (terrainLayers != null)
                {
                    for (int i = 0; i < terrainLayers.Length; i++)
                    {
                        var layer = terrainLayers[i];
                        layers.Add(new TerrainLayerInfo
                        {
                            index = i,
                            name = layer != null ? layer.name : "(null)",
                            assetPath = layer != null ? AssetDatabase.GetAssetPath(layer) : null,
                            diffuseTexture = layer != null && layer.diffuseTexture != null ? layer.diffuseTexture.name : null
                        });
                    }
                }

                return new TerrainGetResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    size = data.size,
                    heightmapResolution = data.heightmapResolution,
                    alphamapResolution = data.alphamapResolution,
                    detailResolution = data.detailResolution,
                    layers = layers.ToArray(),
                    treePrototypeCount = data.treePrototypes != null ? data.treePrototypes.Length : 0,
                    detailPrototypeCount = data.detailPrototypes != null ? data.detailPrototypes.Length : 0,
                    treeInstanceCount = data.treeInstanceCount,
                    leftNeighbor = terrain.leftNeighbor != null ? terrain.leftNeighbor.name : null,
                    topNeighbor = terrain.topNeighbor != null ? terrain.topNeighbor.name : null,
                    rightNeighbor = terrain.rightNeighbor != null ? terrain.rightNeighbor.name : null,
                    bottomNeighbor = terrain.bottomNeighbor != null ? terrain.bottomNeighbor.name : null
                };
            });
        }

        public class TerrainGetResponse
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Terrain size (X=width, Y=height, Z=length) in world units.")]
            public Vector3 size;

            [Description("Heightmap resolution.")]
            public int heightmapResolution;

            [Description("Alphamap (splatmap) resolution.")]
            public int alphamapResolution;

            [Description("Detail resolution.")]
            public int detailResolution;

            [Description("The TerrainLayers assigned to the terrain.")]
            public TerrainLayerInfo[] layers = Array.Empty<TerrainLayerInfo>();

            [Description("Number of tree prototypes.")]
            public int treePrototypeCount;

            [Description("Number of detail prototypes.")]
            public int detailPrototypeCount;

            [Description("Number of placed tree instances.")]
            public int treeInstanceCount;

            [Description("Name of the left neighbor terrain, or null.")]
            public string? leftNeighbor;

            [Description("Name of the top neighbor terrain, or null.")]
            public string? topNeighbor;

            [Description("Name of the right neighbor terrain, or null.")]
            public string? rightNeighbor;

            [Description("Name of the bottom neighbor terrain, or null.")]
            public string? bottomNeighbor;
        }

        public class TerrainLayerInfo
        {
            [Description("Index of the layer in the terrain's layer list.")]
            public int index;

            [Description("Name of the TerrainLayer asset.")]
            public string name = string.Empty;

            [Description("Project asset path of the TerrainLayer, or null.")]
            public string? assetPath;

            [Description("Name of the layer's diffuse texture, or null.")]
            public string? diffuseTexture;
        }
    }
}
