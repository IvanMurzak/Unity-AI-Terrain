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
        public const string TerrainRemoveLayerToolId = "terrain-remove-layer";

        [AiTool
        (
            TerrainRemoveLayerToolId,
            Title = "Terrain / Remove Layer",
            ReadOnlyHint = false,
            DestructiveHint = true,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Remove a `TerrainLayer` from a `Terrain` by its index. Returns the remaining layer " +
            "count.")]
        [AiSkillBody("Remove a `TerrainLayer` from a `Terrain`'s `TerrainData.terrainLayers` by index.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n" +
            "- `layerIndex` — index of the layer to remove (required).\n\n" +
            "## Behavior\n\n" +
            "Validates the index against the current layer count, removes the layer, reassigns " +
            "`TerrainData.terrainLayers`, marks the asset + scene dirty, repaints, and returns the remaining layer " +
            "count. Destructive. Runs on the Unity main thread.")]
        [Description("Removes a TerrainLayer from a Terrain by index. Returns the remaining layer count.")]
        public TerrainRemoveLayerResponse RemoveLayer
        (
            [Description("Reference to the GameObject containing the Terrain component.")]
            GameObjectRef gameObjectRef,
            [Description("Index of the TerrainLayer to remove.")]
            int layerIndex
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));

                var layers = new List<UnityEngine.TerrainLayer>(data.terrainLayers ?? Array.Empty<UnityEngine.TerrainLayer>());
                if (layerIndex < 0 || layerIndex >= layers.Count)
                    throw new Exception(Error.LayerIndexOutOfRange(layerIndex, layers.Count));

                layers.RemoveAt(layerIndex);
                data.terrainLayers = layers.ToArray();

                MarkDirtyAndRepaint(data, terrain.gameObject.scene);

                return new TerrainRemoveLayerResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    layerCount = layers.Count,
                    success = true
                };
            });
        }

        public class TerrainRemoveLayerResponse
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Total number of TerrainLayers after the removal.")]
            public int layerCount;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
