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
        public const string TerrainAddLayerToolId = "terrain-add-layer";

        [AiTool
        (
            TerrainAddLayerToolId,
            Title = "Terrain / Add Layer",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Add a `TerrainLayer` to a `Terrain`. Provide either the asset path of an existing " +
            "`.terrainlayer` asset, or a texture asset path to build a new TerrainLayer from. Returns the index of " +
            "the added layer.")]
        [AiSkillBody("Append a `TerrainLayer` to a `Terrain`'s `TerrainData.terrainLayers`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n" +
            "- `terrainLayerAssetPath` — optional project path to an existing `.terrainlayer` asset to add.\n" +
            "- `diffuseTextureAssetPath` — optional project path to a `Texture2D`. When `terrainLayerAssetPath` is " +
            "omitted, a new `TerrainLayer` is created in-memory with this diffuse texture and added.\n" +
            "- `tileSizeX` / `tileSizeY` — tile size (world units) for a newly-created layer (default 15,15).\n\n" +
            "## Behavior\n\n" +
            "Loads or creates the `TerrainLayer`, appends it to `TerrainData.terrainLayers`, marks the asset + scene " +
            "dirty, repaints, and returns the new layer index. Runs on the Unity main thread.")]
        [Description("Adds a TerrainLayer to a Terrain (from an existing .terrainlayer asset or a new layer built " +
            "from a texture). Returns the added layer index.")]
        public TerrainAddLayerResponse AddLayer
        (
            [Description("Reference to the GameObject containing the Terrain component.")]
            GameObjectRef gameObjectRef,
            [Description("Optional project path to an existing .terrainlayer asset to add.")]
            string? terrainLayerAssetPath = null,
            [Description("Optional project path to a Texture2D used to build a new TerrainLayer (when no asset path is given).")]
            string? diffuseTextureAssetPath = null,
            [Description("Tile size X (world units) for a newly-created layer.")]
            float tileSizeX = 15f,
            [Description("Tile size Y (world units) for a newly-created layer.")]
            float tileSizeY = 15f
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));

                UnityEngine.TerrainLayer layer;
                if (!string.IsNullOrEmpty(terrainLayerAssetPath))
                {
                    layer = AssetDatabase.LoadAssetAtPath<UnityEngine.TerrainLayer>(terrainLayerAssetPath);
                    if (layer == null)
                        throw new Exception(Error.TerrainLayerAssetNotFound(terrainLayerAssetPath!));
                }
                else
                {
                    var texture = string.IsNullOrEmpty(diffuseTextureAssetPath)
                        ? null
                        : AssetDatabase.LoadAssetAtPath<Texture2D>(diffuseTextureAssetPath);
                    if (!string.IsNullOrEmpty(diffuseTextureAssetPath) && texture == null)
                        throw new Exception(Error.TextureAssetNotFound(diffuseTextureAssetPath!));

                    layer = new UnityEngine.TerrainLayer
                    {
                        diffuseTexture = texture,
                        tileSize = new Vector2(tileSizeX, tileSizeY)
                    };
                }

                var layers = new List<UnityEngine.TerrainLayer>(data.terrainLayers ?? Array.Empty<UnityEngine.TerrainLayer>());
                layers.Add(layer);
                data.terrainLayers = layers.ToArray();

                MarkDirtyAndRepaint(data, terrain.gameObject.scene);

                return new TerrainAddLayerResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    layerIndex = layers.Count - 1,
                    layerCount = layers.Count,
                    success = true
                };
            });
        }

        public class TerrainAddLayerResponse
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Index of the added TerrainLayer.")]
            public int layerIndex;

            [Description("Total number of TerrainLayers after the add.")]
            public int layerCount;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
