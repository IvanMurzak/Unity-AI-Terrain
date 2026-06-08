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
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Terrain
    {
        public const string TerrainSetHeightmapResolutionToolId = "terrain-set-heightmap-resolution";

        [AiTool
        (
            TerrainSetHeightmapResolutionToolId,
            Title = "Terrain / Set Heightmap Resolution",
            ReadOnlyHint = false,
            DestructiveHint = true,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the heightmap resolution of a `Terrain`'s `TerrainData`. The value is rounded up " +
            "to a valid `2^n + 1`. Changing resolution resamples the existing heightmap.")]
        [AiSkillBody("Set the heightmap resolution of a `Terrain`'s `TerrainData`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n" +
            "- `heightmapResolution` — requested resolution; rounded up to a valid `2^n + 1` value " +
            "(clamped to Unity's [33, 4097] range).\n\n" +
            "## Behavior\n\n" +
            "Assigns `TerrainData.heightmapResolution`. This is destructive: Unity resamples the existing heightmap " +
            "to the new resolution. Marks the asset + scene dirty and repaints. Runs on the Unity main thread.")]
        [Description("Sets the heightmap resolution of a Terrain's TerrainData (rounded to a valid 2^n + 1). " +
            "Resamples the existing heightmap.")]
        public TerrainSetHeightmapResolutionResponse SetHeightmapResolution
        (
            [Description("Reference to the GameObject containing the Terrain component.")]
            GameObjectRef gameObjectRef,
            [Description("Requested heightmap resolution. Rounded up to a valid (2^n + 1) value.")]
            int heightmapResolution
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));

                var resolved = RoundHeightmapResolution(heightmapResolution);
                data.heightmapResolution = resolved;

                MarkDirtyAndRepaint(data, terrain.gameObject.scene);

                return new TerrainSetHeightmapResolutionResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    heightmapResolution = data.heightmapResolution,
                    success = true
                };
            });
        }

        public class TerrainSetHeightmapResolutionResponse
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Resolved heightmap resolution.")]
            public int heightmapResolution;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
