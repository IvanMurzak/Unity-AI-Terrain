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
        public const string TerrainSetSizeToolId = "terrain-set-size";

        [AiTool
        (
            TerrainSetSizeToolId,
            Title = "Terrain / Set Size",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the world-space size of a `Terrain`'s `TerrainData` — width (X), height (Y), and " +
            "length (Z). Omitted components keep their current value.")]
        [AiSkillBody("Set the world-space size of a `Terrain`'s `TerrainData`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n" +
            "- `width` — optional new width in world units (X). When omitted, the current width is kept.\n" +
            "- `height` — optional new height in world units (Y). When omitted, the current height is kept.\n" +
            "- `length` — optional new length in world units (Z). When omitted, the current length is kept.\n\n" +
            "## Behavior\n\n" +
            "Assigns `TerrainData.size` from the provided components (falling back to the current value for any " +
            "omitted axis). Marks the asset + scene dirty and repaints. Runs on the Unity main thread.")]
        [Description("Sets the world-space size (width/height/length) of a Terrain's TerrainData. Omitted axes are " +
            "left unchanged.")]
        public TerrainSetSizeResponse SetSize
        (
            [Description("Reference to the GameObject containing the Terrain component.")]
            GameObjectRef gameObjectRef,
            [Description("Optional new terrain width in world units (X axis).")]
            float? width = null,
            [Description("Optional new terrain height in world units (Y axis).")]
            float? height = null,
            [Description("Optional new terrain length in world units (Z axis).")]
            float? length = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));

                var current = data.size;
                var newSize = new Vector3(
                    width ?? current.x,
                    height ?? current.y,
                    length ?? current.z);
                data.size = newSize;

                MarkDirtyAndRepaint(data, terrain.gameObject.scene);

                return new TerrainSetSizeResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    size = data.size,
                    success = true
                };
            });
        }

        public class TerrainSetSizeResponse
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Resolved terrain size (X=width, Y=height, Z=length).")]
            public Vector3 size;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
