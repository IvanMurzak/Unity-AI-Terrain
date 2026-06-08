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
        public const string TerrainSetHeightsToolId = "terrain-set-heights";

        [AiTool
        (
            TerrainSetHeightsToolId,
            Title = "Terrain / Set Heights",
            ReadOnlyHint = false,
            DestructiveHint = true,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Set heightmap values over a rectangular region (or the whole terrain) of a `Terrain`. " +
            "Either fill the region with a uniform normalized height, or supply an explicit row-major 2D heights " +
            "array. Heights are normalized [0,1] of the terrain's Y size.")]
        [AiSkillBody("Write heightmap values into a `Terrain`'s `TerrainData`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n" +
            "- `xBase` / `yBase` — top-left heightmap cell of the region (default 0,0).\n" +
            "- `width` / `height` — region size in heightmap cells. When &lt;= 0, the region spans to the edge " +
            "of the heightmap from the base.\n" +
            "- `uniformHeight` — when `heights` is omitted, fill the whole region with this normalized [0,1] value " +
            "(default 0).\n" +
            "- `heights` — optional explicit row-major `[height][width]` array of normalized [0,1] values. When " +
            "provided, its dimensions define the region size (overriding width/height).\n\n" +
            "## Behavior\n\n" +
            "Resolves and clamps the region against `heightmapResolution`, builds a `[h,w]` height array (uniform " +
            "fill or from `heights`), and calls `TerrainData.SetHeights(xBase, yBase, array)`. Destructive: " +
            "overwrites existing heights in the region. Marks dirty + repaints. Runs on the Unity main thread.")]
        [Description("Sets heightmap values over a region (or whole terrain). Uniform fill or explicit 2D array; " +
            "heights are normalized [0,1].")]
        public TerrainSetHeightsResponse SetHeights
        (
            [Description("Reference to the GameObject containing the Terrain component.")]
            GameObjectRef gameObjectRef,
            [Description("Top-left heightmap X cell of the region.")]
            int xBase = 0,
            [Description("Top-left heightmap Y cell of the region.")]
            int yBase = 0,
            [Description("Region width in heightmap cells. <= 0 spans to the heightmap edge.")]
            int width = 0,
            [Description("Region height in heightmap cells. <= 0 spans to the heightmap edge.")]
            int height = 0,
            [Description("Uniform normalized [0,1] height used to fill the region when 'heights' is omitted.")]
            float uniformHeight = 0f,
            [Description("Optional explicit row-major [height][width] array of normalized [0,1] heights.")]
            float[][]? heights = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));
                var resolution = data.heightmapResolution;

                int rw, rh;
                if (heights != null)
                {
                    rh = heights.Length;
                    rw = rh > 0 ? heights[0].Length : 0;
                    if (rh <= 0 || rw <= 0)
                        throw new Exception(Error.InvalidRegion("'heights' array is empty."));
                    var (_, _, cw, ch) = ResolveRegion(xBase, yBase, rw, rh, resolution);
                    if (cw != rw || ch != rh)
                        throw new Exception(Error.InvalidRegion(
                            $"'heights' is {rw}x{rh} but the region from ({xBase},{yBase}) only fits {cw}x{ch} within resolution {resolution}."));
                }
                else
                {
                    var (_, _, cw, ch) = ResolveRegion(xBase, yBase, width, height, resolution);
                    rw = cw;
                    rh = ch;
                }

                var array = new float[rh, rw];
                if (heights != null)
                {
                    for (int y = 0; y < rh; y++)
                    {
                        if (heights[y].Length != rw)
                            throw new Exception(Error.InvalidRegion($"row {y} of 'heights' has length {heights[y].Length}, expected {rw}."));
                        for (int x = 0; x < rw; x++)
                            array[y, x] = Mathf.Clamp01(heights[y][x]);
                    }
                }
                else
                {
                    var v = Mathf.Clamp01(uniformHeight);
                    for (int y = 0; y < rh; y++)
                        for (int x = 0; x < rw; x++)
                            array[y, x] = v;
                }

                data.SetHeights(xBase, yBase, array);

                MarkDirtyAndRepaint(data, terrain.gameObject.scene);

                return new TerrainSetHeightsResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    xBase = xBase,
                    yBase = yBase,
                    width = rw,
                    height = rh,
                    success = true
                };
            });
        }

        public class TerrainSetHeightsResponse
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

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
