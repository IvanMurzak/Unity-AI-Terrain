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
        public const string TerrainPlaceTreesToolId = "terrain-place-trees";

        [AiTool
        (
            TerrainPlaceTreesToolId,
            Title = "Terrain / Place Trees",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Place trees on a `Terrain` using an existing tree prototype. Either scatter `count` " +
            "trees randomly across a normalized [0,1] sub-rectangle, or place trees at explicit normalized " +
            "positions. Optionally clear existing trees first.")]
        [AiSkillBody("Add `TreeInstance`s to a `Terrain`. Tree positions are normalized [0,1] across the terrain.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n" +
            "- `prototypeIndex` — index of the tree prototype to place (must already be on the terrain, default 0).\n" +
            "- `count` — number of trees to scatter randomly (used when `positions` is omitted; default 100).\n" +
            "- `minX` / `minZ` / `maxX` / `maxZ` — normalized [0,1] sub-rectangle to scatter within " +
            "(default 0..1 on both axes).\n" +
            "- `positions` — optional explicit normalized [0,1] XZ positions (each a `Vector2`, x→X, y→Z). When " +
            "provided, overrides random scatter.\n" +
            "- `heightScale` / `widthScale` — per-instance scale (default 1).\n" +
            "- `clearExisting` — when `true`, removes existing trees before placing (default false).\n\n" +
            "## Behavior\n\n" +
            "Validates the prototype index, builds `TreeInstance`s (sampling the terrain height at each normalized " +
            "position), and assigns them via `TerrainData.SetTreeInstances(..., snapToHeightmap:true)`. Marks dirty " +
            "+ repaints. Runs on the Unity main thread.")]
        [Description("Places trees on a Terrain using an existing prototype: random scatter (count) or explicit " +
            "normalized positions. Optionally clears existing trees.")]
        public TerrainPlaceTreesResponse PlaceTrees
        (
            [Description("Reference to the GameObject containing the Terrain component.")]
            GameObjectRef gameObjectRef,
            [Description("Index of the tree prototype to place (must already be on the terrain).")]
            int prototypeIndex = 0,
            [Description("Number of trees to scatter randomly when 'positions' is omitted.")]
            int count = 100,
            [Description("Normalized [0,1] minimum X of the scatter sub-rectangle.")]
            float minX = 0f,
            [Description("Normalized [0,1] minimum Z of the scatter sub-rectangle.")]
            float minZ = 0f,
            [Description("Normalized [0,1] maximum X of the scatter sub-rectangle.")]
            float maxX = 1f,
            [Description("Normalized [0,1] maximum Z of the scatter sub-rectangle.")]
            float maxZ = 1f,
            [Description("Optional explicit normalized [0,1] XZ positions (Vector2: x->X, y->Z).")]
            Vector2[]? positions = null,
            [Description("Per-instance height scale.")]
            float heightScale = 1f,
            [Description("Per-instance width scale.")]
            float widthScale = 1f,
            [Description("If true, removes existing trees before placing.")]
            bool clearExisting = false
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));

                var prototypeCount = data.treePrototypes != null ? data.treePrototypes.Length : 0;
                if (prototypeCount == 0)
                    throw new Exception("[Error] The terrain has no tree prototypes. Add some via 'terrain-set-tree-prototypes' first.");
                if (prototypeIndex < 0 || prototypeIndex >= prototypeCount)
                    throw new Exception($"[Error] Tree prototype index {prototypeIndex} is out of range. The terrain has {prototypeCount} prototype(s).");

                var instances = new List<TreeInstance>();
                if (!clearExisting)
                    instances.AddRange(data.treeInstances);

                Vector2[] placements;
                if (positions != null && positions.Length > 0)
                {
                    placements = positions;
                }
                else
                {
                    if (count < 0)
                        throw new Exception("[Error] 'count' must be >= 0.");
                    var loX = Mathf.Clamp01(Mathf.Min(minX, maxX));
                    var hiX = Mathf.Clamp01(Mathf.Max(minX, maxX));
                    var loZ = Mathf.Clamp01(Mathf.Min(minZ, maxZ));
                    var hiZ = Mathf.Clamp01(Mathf.Max(minZ, maxZ));
                    placements = new Vector2[count];
                    for (int i = 0; i < count; i++)
                        placements[i] = new Vector2(
                            UnityEngine.Random.Range(loX, hiX),
                            UnityEngine.Random.Range(loZ, hiZ));
                }

                var added = 0;
                foreach (var p in placements)
                {
                    var nx = Mathf.Clamp01(p.x);
                    var nz = Mathf.Clamp01(p.y);
                    var ny = data.GetInterpolatedHeight(nx, nz) / Mathf.Max(0.0001f, data.size.y);
                    instances.Add(new TreeInstance
                    {
                        prototypeIndex = prototypeIndex,
                        position = new Vector3(nx, ny, nz),
                        heightScale = heightScale,
                        widthScale = widthScale,
                        rotation = UnityEngine.Random.Range(0f, Mathf.PI * 2f),
                        color = Color.white,
                        lightmapColor = Color.white
                    });
                    added++;
                }

                data.SetTreeInstances(instances.ToArray(), snapToHeightmap: true);

                MarkDirtyAndRepaint(data, terrain.gameObject.scene);

                return new TerrainPlaceTreesResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    prototypeIndex = prototypeIndex,
                    treesAdded = added,
                    treeInstanceCount = data.treeInstanceCount,
                    success = true
                };
            });
        }

        public class TerrainPlaceTreesResponse
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Index of the placed tree prototype.")]
            public int prototypeIndex;

            [Description("Number of trees added by this call.")]
            public int treesAdded;

            [Description("Total tree instance count after the call.")]
            public int treeInstanceCount;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
