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
        public const string TerrainSetTreePrototypesToolId = "terrain-set-tree-prototypes";

        [AiTool
        (
            TerrainSetTreePrototypesToolId,
            Title = "Terrain / Set Tree Prototypes",
            ReadOnlyHint = false,
            DestructiveHint = true,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Replace the tree prototypes of a `Terrain` with a new set built from prefab/GameObject " +
            "asset paths. Returns the resulting prototype count.")]
        [AiSkillBody("Replace `TerrainData.treePrototypes` with a new set built from prefab asset paths.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the `Terrain` (required).\n" +
            "- `prefabAssetPaths` — list of prefab/GameObject asset paths; each becomes a `TreePrototype` " +
            "(required, non-empty).\n\n" +
            "## Behavior\n\n" +
            "Loads each prefab, builds a `TreePrototype` with `prefab` set, assigns the array to " +
            "`TerrainData.treePrototypes`, marks dirty + repaints, and returns the count. Destructive (replaces " +
            "existing prototypes; existing placed tree instances may need re-placing). Runs on the Unity main thread.")]
        [Description("Replaces the Terrain's tree prototypes from prefab asset paths. Returns the resulting count.")]
        public TerrainSetTreePrototypesResponse SetTreePrototypes
        (
            [Description("Reference to the GameObject containing the Terrain component.")]
            GameObjectRef gameObjectRef,
            [Description("Prefab/GameObject asset paths; each becomes a TreePrototype.")]
            string[] prefabAssetPaths
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));
            if (prefabAssetPaths == null || prefabAssetPaths.Length == 0)
                throw new ArgumentException("Provide at least one prefab asset path.", nameof(prefabAssetPaths));

            return MainThread.Instance.Run(() =>
            {
                var (terrain, data) = ResolveTerrainData(gameObjectRef, nameof(gameObjectRef));

                var prototypes = new List<TreePrototype>();
                foreach (var path in prefabAssetPaths)
                {
                    if (string.IsNullOrEmpty(path))
                        continue;
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (prefab == null)
                        throw new Exception(Error.PrefabAssetNotFound(path));
                    prototypes.Add(new TreePrototype { prefab = prefab });
                }

                if (prototypes.Count == 0)
                    throw new Exception("[Error] Provide at least one valid prefab asset path for the tree prototypes.");

                data.treePrototypes = prototypes.ToArray();

                MarkDirtyAndRepaint(data, terrain.gameObject.scene);

                return new TerrainSetTreePrototypesResponse
                {
                    gameObjectRef = new GameObjectRef(terrain.gameObject),
                    terrainRef = new ComponentRef(terrain),
                    treePrototypeCount = data.treePrototypes.Length,
                    success = true
                };
            });
        }

        public class TerrainSetTreePrototypesResponse
        {
            [Description("Reference to the Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Number of tree prototypes after the set.")]
            public int treePrototypeCount;

            [Description("Whether the operation succeeded.")]
            public bool success;
        }
    }
}
