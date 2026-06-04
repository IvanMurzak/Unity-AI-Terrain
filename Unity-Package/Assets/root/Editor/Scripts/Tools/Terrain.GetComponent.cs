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
using Microsoft.Extensions.Logging;
using com.IvanMurzak.ReflectorNet.Model;
using com.IvanMurzak.ReflectorNet.Utils;
using AIGD;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using com.IvanMurzak.Unity.MCP.Utils;
using UnityEngine;

namespace com.IvanMurzak.Unity.MCP.Editor.API
{
    public partial class Tool_Terrain
    {
        public const string TerrainGetComponentToolId = "terrain-get-component";

        [AiTool
        (
            TerrainGetComponentToolId,
            Title = "Terrain / Get Component (generic)",
            ReadOnlyHint = true,
            DestructiveHint = false,
            IdempotentHint = true,
            OpenWorldHint = false
        )]
        [AiSkillDescription("Generic read: serialize a terrain-related `Component` (`Terrain` or `TerrainCollider`) " +
            "on a GameObject via ReflectorNet. The escape hatch for fields not covered by the dedicated tools. " +
            "Pair with 'terrain-modify-component'. Read-only.")]
        [AiSkillBody("Serialize a terrain component on a GameObject using ReflectorNet.\n\n" +
            "## Inputs\n\n" +
            "- `gameObjectRef` — the GameObject hosting the component (required).\n" +
            "- `componentRef` — optional. Resolves a specific component when the GameObject has more than one; " +
            "otherwise the first `Terrain` (or, failing that, the first `TerrainCollider`) is used.\n" +
            "- `deepSerialization` — when `true`, recurses through nested objects; otherwise only top-level members.\n\n" +
            "## Behavior\n\n" +
            "Finds the target terrain component, serializes it via ReflectorNet, and returns the serialized member " +
            "plus the resolved component type name. Read-only. Runs on the Unity main thread.")]
        [Description("Generic: serialize a Terrain/TerrainCollider Component on a GameObject via ReflectorNet. " +
            "Read-only. Use terrain-modify-component to write changes back.")]
        public TerrainGetComponentResponse GetComponentData
        (
            [Description("Reference to the GameObject containing the terrain component.")]
            GameObjectRef gameObjectRef,
            [Description("Optional reference to a specific component if the GameObject has multiple.")]
            ComponentRef? componentRef = null,
            [Description("Performs deep serialization including nested objects. Otherwise only top-level members.")]
            bool deepSerialization = false
        )
        {
            if (gameObjectRef == null)
                throw new ArgumentNullException(nameof(gameObjectRef));
            if (!gameObjectRef.IsValid(out var validationError))
                throw new ArgumentException(validationError, nameof(gameObjectRef));

            return MainThread.Instance.Run(() =>
            {
                var go = ResolveGameObject(gameObjectRef, nameof(gameObjectRef));
                var (component, index) = FindTerrainComponent(go, componentRef);
                if (component == null)
                    throw new Exception(Error.NoTerrainComponent());

                var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception(Error.ReflectorNotAvailable());
                var logger = UnityLoggerFactory.LoggerFactory.CreateLogger<Tool_Terrain>();

                return new TerrainGetComponentResponse
                {
                    gameObjectRef = new GameObjectRef(go),
                    componentRef = new ComponentRef(component),
                    componentIndex = index,
                    componentType = component.GetType().FullName ?? component.GetType().Name,
                    data = reflector.Serialize(
                        obj: component,
                        name: component.GetType().Name,
                        recursive: deepSerialization,
                        logger: logger)
                };
            });
        }

        /// <summary>
        /// Locate a terrain-related component on the GameObject. When componentRef resolves, returns the matching
        /// component; otherwise returns the first Terrain, falling back to the first TerrainCollider.
        /// </summary>
        static (UnityEngine.Component? component, int index) FindTerrainComponent(GameObject go, ComponentRef? componentRef)
        {
            var all = go.GetComponents<UnityEngine.Component>();

            if (componentRef != null && componentRef.IsValid(out _))
            {
                for (int i = 0; i < all.Length; i++)
                {
                    var comp = all[i];
                    if (comp != null && componentRef.Matches(comp, i))
                        return (comp, i);
                }
                return (null, -1);
            }

            for (int i = 0; i < all.Length; i++)
                if (all[i] is UnityEngine.Terrain)
                    return (all[i], i);

            for (int i = 0; i < all.Length; i++)
                if (all[i] is UnityEngine.TerrainCollider)
                    return (all[i], i);

            return (null, -1);
        }

        public class TerrainGetComponentResponse
        {
            [Description("Reference to the GameObject containing the component.")]
            public GameObjectRef? gameObjectRef;

            [Description("Reference to the serialized component.")]
            public ComponentRef? componentRef;

            [Description("Index of the component in the GameObject's component list.")]
            public int componentIndex = -1;

            [Description("Full type name of the serialized component.")]
            public string componentType = string.Empty;

            [Description("Serialized component data.")]
            public SerializedMember? data;
        }
    }
}
