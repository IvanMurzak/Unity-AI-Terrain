/*
┌─────────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak/Unity-AI-Terrain)       │
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
        public const string TerrainSetNeighborsToolId = "terrain-set-neighbors";

        [AiTool
        (
            TerrainSetNeighborsToolId,
            Title = "Terrain / Set Neighbors",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set the neighboring `Terrain`s of a center `Terrain` (left / top / right / bottom) so " +
            "Unity blends seams and LOD across tiles. Omitted neighbors are treated as null.")]
        [AiSkillBody("Wire up the neighbor terrains of a center `Terrain` via `Terrain.SetNeighbors`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the center Terrain GameObject (required).\n" +
            "- `leftRef` / `topRef` / `rightRef` / `bottomRef` — optional GameObjects hosting the neighboring " +
            "`Terrain`s. Any omitted side is set to null.\n\n" +
            "## Behavior\n\n" +
            "Resolves each provided neighbor GameObject to its `Terrain` component, calls " +
            "`center.SetNeighbors(left, top, right, bottom)`, and triggers `Terrain.Flush()`. Marks the scene dirty " +
            "and repaints. Runs on the Unity main thread.")]
        [Description("Sets the left/top/right/bottom neighbor Terrains of a center Terrain so Unity blends seams " +
            "across tiles.")]
        public TerrainSetNeighborsResponse SetNeighbors
        (
            [Description("Reference to the center Terrain GameObject.")]
            GameObjectRef gameObjectRef,
            [Description("Optional GameObject hosting the left neighbor Terrain.")]
            GameObjectRef? leftRef = null,
            [Description("Optional GameObject hosting the top neighbor Terrain.")]
            GameObjectRef? topRef = null,
            [Description("Optional GameObject hosting the right neighbor Terrain.")]
            GameObjectRef? rightRef = null,
            [Description("Optional GameObject hosting the bottom neighbor Terrain.")]
            GameObjectRef? bottomRef = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var center = ResolveTerrain(gameObjectRef, nameof(gameObjectRef));

                var left = ResolveOptionalTerrain(leftRef, nameof(leftRef));
                var top = ResolveOptionalTerrain(topRef, nameof(topRef));
                var right = ResolveOptionalTerrain(rightRef, nameof(rightRef));
                var bottom = ResolveOptionalTerrain(bottomRef, nameof(bottomRef));

                center.SetNeighbors(left, top, right, bottom);
                UnityEngine.Terrain.SetConnectivityDirty();
                center.Flush();

                MarkDirtyAndRepaint(center, center.gameObject.scene);

                return new TerrainSetNeighborsResponse
                {
                    gameObjectRef = new GameObjectRef(center.gameObject),
                    terrainRef = new ComponentRef(center),
                    left = left != null ? left.name : null,
                    top = top != null ? top.name : null,
                    right = right != null ? right.name : null,
                    bottom = bottom != null ? bottom.name : null,
                    success = true
                };
            });
        }

        static UnityEngine.Terrain? ResolveOptionalTerrain(GameObjectRef? gameObjectRef, string paramName)
        {
            var transform = ResolveOptionalTransform(gameObjectRef, paramName);
            if (transform == null)
                return null;
            var terrain = transform.GetComponent<UnityEngine.Terrain>();
            if (terrain == null)
                throw new ArgumentException(Error.TerrainNotFound(), paramName);
            return terrain;
        }

        public class TerrainSetNeighborsResponse
        {
            [Description("Reference to the center Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the center Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Name of the left neighbor, or null.")]
            public string? left;

            [Description("Name of the top neighbor, or null.")]
            public string? top;

            [Description("Name of the right neighbor, or null.")]
            public string? right;

            [Description("Name of the bottom neighbor, or null.")]
            public string? bottom;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
