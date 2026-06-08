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
        public const string TerrainSampleHeightsToolId = "terrain-sample-heights";

        [AiTool
        (
            TerrainSampleHeightsToolId,
            Title = "Terrain / Sample Heights",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Read heightmap values over a rectangular region (or the whole terrain) of a `Terrain`. " +
            "Returns the normalized [0,1] heights plus min/max/average statistics. Read-only.")]
        [AiSkillBody("Sample heightmap values from a `Terrain`'s `TerrainData`.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n" +
            "- `xBase` / `yBase` — top-left heightmap cell of the region (default 0,0).\n" +
            "- `width` / `height` — region size in heightmap cells. When &lt;= 0, the region spans to the edge " +
            "of the heightmap from the base.\n" +
            "- `includeArray` — when `true`, include the full row-major `[height][width]` heights array in the " +
            "response (can be large); otherwise only statistics are returned (default false).\n\n" +
            "## Behavior\n\n" +
            "Resolves and clamps the region against `heightmapResolution`, calls `TerrainData.GetHeights`, computes " +
            "min/max/average over the sampled cells, and optionally returns the full array. Read-only. Runs on the " +
            "Unity main thread.")]
        [Description("Reads heightmap values over a region (or whole terrain). Returns normalized [0,1] heights " +
            "with min/max/average statistics. Read-only.")]
        public TerrainSampleHeightsResponse SampleHeights
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
            [Description("If true, include the full row-major [height][width] heights array in the response.")]
            bool includeArray = false
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));
                var resolution = data.heightmapResolution;

                var (rx, ry, rw, rh) = ResolveRegion(xBase, yBase, width, height, resolution);
                var sampled = data.GetHeights(rx, ry, rw, rh);

                float min = float.MaxValue;
                float max = float.MinValue;
                double sum = 0;
                for (int y = 0; y < rh; y++)
                {
                    for (int x = 0; x < rw; x++)
                    {
                        var v = sampled[y, x];
                        if (v < min) min = v;
                        if (v > max) max = v;
                        sum += v;
                    }
                }
                var count = rw * rh;

                float[][]? jagged = null;
                if (includeArray)
                {
                    jagged = new float[rh][];
                    for (int y = 0; y < rh; y++)
                    {
                        jagged[y] = new float[rw];
                        for (int x = 0; x < rw; x++)
                            jagged[y][x] = sampled[y, x];
                    }
                }

                return new TerrainSampleHeightsResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    xBase = rx,
                    yBase = ry,
                    width = rw,
                    height = rh,
                    min = count > 0 ? min : 0f,
                    max = count > 0 ? max : 0f,
                    average = count > 0 ? (float)(sum / count) : 0f,
                    heights = jagged
                };
            });
        }

        public class TerrainSampleHeightsResponse
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

            [Description("Minimum normalized [0,1] height in the region.")]
            public float min;

            [Description("Maximum normalized [0,1] height in the region.")]
            public float max;

            [Description("Average normalized [0,1] height in the region.")]
            public float average;

            [Description("Row-major [height][width] heights array, or null when includeArray is false.")]
            public float[][]? heights;
        }
    }
}
