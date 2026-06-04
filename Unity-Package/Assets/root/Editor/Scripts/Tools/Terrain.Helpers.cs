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
using AIGD;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using UnityEditor;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Terrain
    {
        /// <summary>Resolve a required GameObjectRef to its GameObject (throws on failure).</summary>
        static GameObject ResolveGameObject(GameObjectRef? gameObjectRef, string paramName)
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(paramName);
            if (!gameObjectRef.IsValid(out var validationError))
                throw new ArgumentException(validationError, paramName);

            var go = gameObjectRef.FindGameObject(out var error);
            if (error != null)
                throw new Exception(error);
            if (go == null)
                throw new Exception(Error.GameObjectNotFound());

            return go;
        }

        /// <summary>Resolve a required GameObjectRef to a Terrain component (throws on failure).</summary>
        static UnityEngine.Terrain ResolveTerrain(GameObjectRef? gameObjectRef, string paramName)
        {
            var go = ResolveGameObject(gameObjectRef, paramName);
            var terrain = go.GetComponent<UnityEngine.Terrain>();
            if (terrain == null)
                throw new Exception(Error.TerrainNotFound());
            return terrain;
        }

        /// <summary>Resolve a required GameObjectRef to a Terrain + its non-null TerrainData (throws on failure).</summary>
        static (UnityEngine.Terrain terrain, UnityEngine.TerrainData data) ResolveTerrainData(GameObjectRef? gameObjectRef, string paramName)
        {
            var terrain = ResolveTerrain(gameObjectRef, paramName);
            var data = terrain.terrainData;
            if (data == null)
                throw new Exception(Error.TerrainDataNotFound());
            return (terrain, data);
        }

        /// <summary>Resolve an optional GameObjectRef to a Transform (null when the ref is null/empty).</summary>
        static Transform? ResolveOptionalTransform(GameObjectRef? gameObjectRef, string paramName)
        {
            if (gameObjectRef == null || !gameObjectRef.IsValid(out _))
                return null;

            var go = gameObjectRef.FindGameObject(out var error);
            if (error != null)
                throw new ArgumentException(error, paramName);
            if (go == null)
                throw new ArgumentException(Error.GameObjectNotFound(), paramName);

            return go.transform;
        }

        /// <summary>Mark a scene object dirty and repaint the editor after a mutation.</summary>
        static void MarkDirtyAndRepaint(UnityEngine.Object target, UnityEngine.SceneManagement.Scene scene)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            com.IvanMurzak.Unity.MCP.Editor.Utils.EditorUtils.RepaintAllEditorWindows();
        }

        /// <summary>
        /// Clamp and validate a rectangular region against a resolution. Returns the validated region.
        /// When width/height are &lt;= 0 the region spans the full resolution from (xBase,yBase).
        /// </summary>
        static (int x, int y, int w, int h) ResolveRegion(int xBase, int yBase, int width, int height, int resolution)
        {
            if (resolution <= 0)
                throw new Exception(Error.InvalidRegion($"resolution must be positive, got {resolution}."));
            if (xBase < 0 || yBase < 0)
                throw new Exception(Error.InvalidRegion($"xBase/yBase must be >= 0, got ({xBase},{yBase})."));
            if (xBase >= resolution || yBase >= resolution)
                throw new Exception(Error.InvalidRegion($"xBase/yBase ({xBase},{yBase}) must be < resolution {resolution}."));

            var w = width <= 0 ? resolution - xBase : width;
            var h = height <= 0 ? resolution - yBase : height;

            if (xBase + w > resolution)
                w = resolution - xBase;
            if (yBase + h > resolution)
                h = resolution - yBase;

            if (w <= 0 || h <= 0)
                throw new Exception(Error.InvalidRegion($"resolved region size ({w}x{h}) is empty."));

            return (xBase, yBase, w, h);
        }

        /// <summary>
        /// Round a requested heightmap resolution up to the nearest valid Unity terrain heightmap
        /// resolution of the form (2^n + 1), clamped to Unity's [33, 4097] range.
        /// </summary>
        static int RoundHeightmapResolution(int requested)
        {
            const int min = 33;
            const int max = 4097;
            if (requested <= min)
                return min;
            if (requested >= max)
                return max;

            // Find the smallest (2^n + 1) >= requested.
            var n = 1;
            while ((n + 1) < requested)
                n <<= 1;
            var result = n + 1;
            return Mathf.Clamp(result, min, max);
        }
    }
}
