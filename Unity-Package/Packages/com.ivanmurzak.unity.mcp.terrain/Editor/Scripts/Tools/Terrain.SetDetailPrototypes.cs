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
        public const string TerrainSetDetailPrototypesToolId = "terrain-set-detail-prototypes";

        [AiTool
        (
            TerrainSetDetailPrototypesToolId,
            Title = "Terrain / Set Detail Prototypes",
            ReadOnlyHint = false,
            DestructiveHint = true,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Replace the detail prototypes (grass / detail meshes) of a `Terrain` with a new set " +
            "built from texture or prefab asset paths. Returns the resulting prototype count.")]
        [AiSkillBody("Replace `TerrainData.detailPrototypes` with a new set.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n" +
            "- `textureAssetPaths` — optional list of `Texture2D` asset paths; each becomes a Grass-billboard " +
            "detail prototype.\n" +
            "- `prefabAssetPaths` — optional list of prefab/GameObject asset paths; each becomes a VertexLit " +
            "detail-mesh prototype.\n\n" +
            "At least one of the two lists must be non-empty.\n\n" +
            "## Behavior\n\n" +
            "Loads each asset, builds a `DetailPrototype` (texture → `usePrototypeMesh=false`+`prototypeTexture`, " +
            "prefab → `usePrototypeMesh=true`+`prototype`), assigns the array to `TerrainData.detailPrototypes`, " +
            "marks dirty + repaints, and returns the count. Destructive (replaces existing prototypes). Runs on the " +
            "Unity main thread.")]
        [Description("Replaces the Terrain's detail prototypes (grass/detail meshes) from texture and/or prefab " +
            "asset paths. Returns the resulting count.")]
        public TerrainSetDetailPrototypesResponse SetDetailPrototypes
        (
            [Description("Reference to the GameObject containing the Terrain component.")]
            GameObjectRef gameObjectRef,
            [Description("Optional Texture2D asset paths; each becomes a grass-billboard detail prototype.")]
            string[]? textureAssetPaths = null,
            [Description("Optional prefab/GameObject asset paths; each becomes a detail-mesh prototype.")]
            string[]? prefabAssetPaths = null
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));

                var prototypes = new List<DetailPrototype>();

                if (textureAssetPaths != null)
                {
                    foreach (var path in textureAssetPaths)
                    {
                        if (string.IsNullOrEmpty(path))
                            continue;
                        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                        if (tex == null)
                            throw new Exception(Error.TextureAssetNotFound(path));
                        prototypes.Add(new DetailPrototype
                        {
                            usePrototypeMesh = false,
                            prototypeTexture = tex,
                            renderMode = DetailRenderMode.GrassBillboard,
                            healthyColor = Color.white,
                            dryColor = new Color(0.8f, 0.7f, 0.4f, 1f)
                        });
                    }
                }

                if (prefabAssetPaths != null)
                {
                    foreach (var path in prefabAssetPaths)
                    {
                        if (string.IsNullOrEmpty(path))
                            continue;
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (prefab == null)
                            throw new Exception(Error.PrefabAssetNotFound(path));
                        prototypes.Add(new DetailPrototype
                        {
                            usePrototypeMesh = true,
                            prototype = prefab,
                            renderMode = DetailRenderMode.VertexLit,
                            healthyColor = Color.white,
                            dryColor = new Color(0.8f, 0.7f, 0.4f, 1f)
                        });
                    }
                }

                if (prototypes.Count == 0)
                    throw new Exception("[Error] Provide at least one texture or prefab asset path for the detail prototypes.");

                data.detailPrototypes = prototypes.ToArray();

                MarkDirtyAndRepaint(data, terrain.gameObject.scene);

                return new TerrainSetDetailPrototypesResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    detailPrototypeCount = data.detailPrototypes.Length,
                    success = true
                };
            });
        }

        public class TerrainSetDetailPrototypesResponse
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Number of detail prototypes after the set.")]
            public int detailPrototypeCount;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
