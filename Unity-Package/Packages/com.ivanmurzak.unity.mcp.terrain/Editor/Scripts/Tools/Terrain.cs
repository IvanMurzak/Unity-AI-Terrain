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
using com.IvanMurzak.McpPlugin;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    [AiToolType]
    public partial class Tool_Terrain
    {
        public static class Error
        {
            public static string GameObjectNotFound()
                => "[Error] GameObject not found. Provide a valid reference to an existing GameObject.";

            public static string TerrainNotFound()
                => "[Error] Terrain component not found on the target GameObject. " +
                   "Make sure the GameObject has a Terrain component attached.";

            public static string TerrainDataNotFound()
                => "[Error] The Terrain has no TerrainData assigned. Assign a TerrainData asset first " +
                   "(e.g. via 'terrain-create').";

            public static string ComponentNotFound(string componentTypeName)
                => $"[Error] Component '{componentTypeName}' not found on the target GameObject.";

            public static string NoTerrainComponent()
                => "[Error] No Terrain-related component found on the specified GameObject.";

            public static string TypeNotFound(string typeName)
                => $"[Error] Type '{typeName}' could not be resolved. Provide a full type name " +
                   "(e.g. 'UnityEngine.Terrain').";

            public static string TerrainLayerAssetNotFound(string path)
                => $"[Error] TerrainLayer asset could not be loaded from '{path}'. Provide a valid asset path " +
                   "to a TerrainLayer (.terrainlayer) asset.";

            public static string TextureAssetNotFound(string path)
                => $"[Error] Texture2D asset could not be loaded from '{path}'. Provide a valid asset path " +
                   "to a Texture2D.";

            public static string PrefabAssetNotFound(string path)
                => $"[Error] Prefab/GameObject asset could not be loaded from '{path}'. Provide a valid asset path.";

            public static string LayerIndexOutOfRange(int index, int count)
                => $"[Error] TerrainLayer index {index} is out of range. The terrain has {count} layer(s).";

            public static string InvalidRegion(string reason)
                => $"[Error] Invalid region: {reason}";

            public static string ReflectorNotAvailable()
                => "[Error] ReflectorNet reflector is not available.";
        }
    }
}
