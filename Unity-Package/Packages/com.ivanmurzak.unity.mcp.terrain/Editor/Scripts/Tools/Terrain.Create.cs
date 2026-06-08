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
#if UNITY_6000_5_OR_NEWER
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using AIGD;
using UnityEditor;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Terrain
    {
        public const string TerrainCreateToolId = "terrain-create";

        [AiTool
        (
            TerrainCreateToolId,
            Title = "Terrain / Create Terrain",
            ReadOnlyHint = false,
            DestructiveHint = false,
            IdempotentHint = false,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Create a new GameObject with a `Terrain` + `TerrainCollider` backed by a freshly " +
            "created `TerrainData` asset. Set heightmap resolution and the terrain world size (width/height/length). " +
            "Returns the new GameObject reference and instanceId.")]
        [AiSkillBody("Create a new GameObject hosting a `Terrain` and a `TerrainCollider` in the active scene, " +
            "backed by a new `TerrainData` asset saved to the project.\n\n" +
            "## Inputs\n\n" +
            "- `name` — optional GameObject name (default `Terrain`).\n" +
            "- `terrainDataAssetPath` — project path for the new `TerrainData` asset (default " +
            "`Assets/TerrainData_<name>.asset`). Must start with `Assets/` and end with `.asset`.\n" +
            "- `heightmapResolution` — heightmap resolution; rounded to a valid `2^n + 1` value (default 513).\n" +
            "- `width` / `length` — horizontal terrain size in world units (X / Z, default 1000 each).\n" +
            "- `height` — vertical terrain size in world units (Y, default 600).\n" +
            "- `position` — optional world position of the terrain GameObject (default zero).\n\n" +
            "## Behavior\n\n" +
            "Creates the `TerrainData`, sets its `heightmapResolution` and `size`, writes it as an asset, then " +
            "creates the GameObject via `Terrain.CreateTerrainGameObject`, positions it, marks the scene dirty, " +
            "repaints, and returns the new GameObject reference and instanceId. Runs on the Unity main thread.")]
        [Description("Creates a new Terrain GameObject backed by a new TerrainData asset. Sets heightmap resolution " +
            "and terrain size.")]
        public TerrainCreateResponse CreateTerrain
        (
            [Description("Name of the new Terrain GameObject.")]
            string? name = null,
            [Description("Project path for the new TerrainData asset (Assets/...asset). Defaults to Assets/TerrainData_<name>.asset.")]
            string? terrainDataAssetPath = null,
            [Description("Heightmap resolution. Rounded up to a valid (2^n + 1) value.")]
            int heightmapResolution = 513,
            [Description("Terrain width in world units (X axis).")]
            float width = 1000f,
            [Description("Terrain length in world units (Z axis).")]
            float length = 1000f,
            [Description("Terrain height in world units (Y axis).")]
            float height = 600f,
            [Description("World-space position of the terrain GameObject.")]
            Vector3? position = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var goName = string.IsNullOrEmpty(name) ? "Terrain" : name!;
                var assetPath = string.IsNullOrEmpty(terrainDataAssetPath)
                    ? $"Assets/TerrainData_{goName}.asset"
                    : terrainDataAssetPath!;

                var data = new TerrainData
                {
                    heightmapResolution = RoundHeightmapResolution(heightmapResolution),
                    size = new Vector3(width, height, length)
                };

                AssetDatabase.CreateAsset(data, assetPath);
                AssetDatabase.SaveAssets();

                var go = UnityEngine.Terrain.CreateTerrainGameObject(data);
                go.name = goName;
                go.transform.position = position ?? Vector3.zero;

                var terrain = go.GetComponent<UnityEngine.Terrain>();

                EditorUtility.SetDirty(go);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
                EditorUtils.RepaintAllEditorWindows();

                return new TerrainCreateResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    terrainRef = terrain != null ? new ComponentRef(terrain) : null,
                    instanceId = go.GetEntityId(),
                    gameObjectName = go.name,
                    terrainDataAssetPath = assetPath,
                    heightmapResolution = data.heightmapResolution,
                    size = data.size
                };
            });
        }

        public class TerrainCreateResponse
        {
            [Description("Reference to the created Terrain GameObject.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the created Terrain component.")]
            public ComponentRef? terrainRef;

            [Description("Instance id of the created GameObject.")]
            public UnityEngine.EntityId instanceId;

            [Description("Name of the created GameObject.")]
            public string gameObjectName = string.Empty;

            [Description("Project path of the created TerrainData asset.")]
            public string terrainDataAssetPath = string.Empty;

            [Description("Resolved heightmap resolution.")]
            public int heightmapResolution;

            [Description("Resolved terrain size (X=width, Y=height, Z=length).")]
            public Vector3 size;
        }
    }
}
#endif
