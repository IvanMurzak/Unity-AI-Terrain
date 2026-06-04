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
        public const string TerrainPaintLayerToolId = "terrain-paint-layer";

        [AiTool
        (
            TerrainPaintLayerToolId,
            Title = "Terrain / Paint Layer",
            ReadOnlyHint = false,
            DestructiveHint = true,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Paint a `TerrainLayer` over a rectangular region (or the whole terrain) by writing the " +
            "alphamap (splatmap) so the chosen layer has the given weight there and the other layers are reduced " +
            "proportionally. Alphamap weights are normalized [0,1].")]
        [AiSkillBody("Write the alphamap (splatmap) of a `Terrain` so one `TerrainLayer` dominates a region.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n" +
            "- `layerIndex` — index of the `TerrainLayer` to paint (required; must already be on the terrain).\n" +
            "- `xBase` / `yBase` — top-left alphamap cell of the region (default 0,0).\n" +
            "- `width` / `height` — region size in alphamap cells. When &lt;= 0, spans to the alphamap edge.\n" +
            "- `strength` — target normalized [0,1] weight for the chosen layer (default 1 = fully painted).\n\n" +
            "## Behavior\n\n" +
            "Reads the region alphamaps via `GetAlphamaps`, sets the chosen layer's weight to `strength` and scales " +
            "the remaining layers so each cell's weights sum to 1, then writes back with `SetAlphamaps`. Destructive. " +
            "Marks dirty + repaints. Runs on the Unity main thread.")]
        [Description("Paints a TerrainLayer over a region by writing the alphamap so the chosen layer has the given " +
            "weight and others are reduced proportionally.")]
        public TerrainPaintLayerResponse PaintLayer
        (
            [Description("Reference to the GameObject containing the Terrain component.")]
            GameObjectRef gameObjectRef,
            [Description("Index of the TerrainLayer to paint (must already be on the terrain).")]
            int layerIndex,
            [Description("Top-left alphamap X cell of the region.")]
            int xBase = 0,
            [Description("Top-left alphamap Y cell of the region.")]
            int yBase = 0,
            [Description("Region width in alphamap cells. <= 0 spans to the alphamap edge.")]
            int width = 0,
            [Description("Region height in alphamap cells. <= 0 spans to the alphamap edge.")]
            int height = 0,
            [Description("Target normalized [0,1] weight for the chosen layer (1 = fully painted).")]
            float strength = 1f
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));

                var layerCount = data.terrainLayers != null ? data.terrainLayers.Length : 0;
                if (layerCount == 0)
                    throw new Exception("[Error] The terrain has no TerrainLayers to paint. Add a layer first via 'terrain-add-layer'.");
                if (layerIndex < 0 || layerIndex >= layerCount)
                    throw new Exception(Error.LayerIndexOutOfRange(layerIndex, layerCount));

                var resolution = data.alphamapResolution;
                var (rx, ry, rw, rh) = ResolveRegion(xBase, yBase, width, height, resolution);

                var target = Mathf.Clamp01(strength);
                var maps = data.GetAlphamaps(rx, ry, rw, rh); // [rh, rw, layerCount]

                for (int y = 0; y < rh; y++)
                {
                    for (int x = 0; x < rw; x++)
                    {
                        maps[y, x, layerIndex] = target;

                        // Distribute remaining weight (1 - target) across the other layers,
                        // preserving their relative proportions; fall back to uniform if all were zero.
                        var remaining = 1f - target;
                        var otherSum = 0f;
                        for (int l = 0; l < layerCount; l++)
                            if (l != layerIndex)
                                otherSum += maps[y, x, l];

                        if (layerCount == 1)
                        {
                            maps[y, x, layerIndex] = 1f;
                        }
                        else if (otherSum > 0f)
                        {
                            var scale = remaining / otherSum;
                            for (int l = 0; l < layerCount; l++)
                                if (l != layerIndex)
                                    maps[y, x, l] *= scale;
                        }
                        else
                        {
                            var even = remaining / (layerCount - 1);
                            for (int l = 0; l < layerCount; l++)
                                if (l != layerIndex)
                                    maps[y, x, l] = even;
                        }
                    }
                }

                data.SetAlphamaps(rx, ry, maps);

                MarkDirtyAndRepaint(data, terrain.gameObject.scene);

                return new TerrainPaintLayerResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    layerIndex = layerIndex,
                    xBase = rx,
                    yBase = ry,
                    width = rw,
                    height = rh,
                    success = true
                };
            });
        }

        public class TerrainPaintLayerResponse
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Index of the painted TerrainLayer.")]
            public int layerIndex;

            [Description("Resolved region top-left X cell.")]
            public int xBase;

            [Description("Resolved region top-left Y cell.")]
            public int yBase;

            [Description("Resolved region width in cells.")]
            public int width;

            [Description("Resolved region height in cells.")]
            public int height;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
