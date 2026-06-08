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
using System.Collections;
using com.IvanMurzak.ReflectorNet.Model;
using AIGD;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Terrain.Editor.Tests
{
    /// <summary>
    /// A trivial test-only component with a public C# *field*, used to verify that the generic
    /// 'terrain-modify-component' tool routes field writes through ReflectorNet's `fields` channel
    /// (FieldInfo resolution — no cross-fallback to properties).
    /// </summary>
    public class FieldChannelProbe : MonoBehaviour
    {
        public Vector3 ProbeOffset = Vector3.zero;
    }

    public class TestTerrainGeneric : BaseTest
    {
        [UnityTest]
        public IEnumerator GetComponent_SerializesTerrain()
        {
            var go = CreateTerrainGameObject(GO_TerrainName);
            var terrain = go.GetComponent<UnityEngine.Terrain>();

            var tool = new Tool_Terrain();
            var result = tool.GetComponentData(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                componentRef: new ComponentRef(terrain.GetInstanceID()));

            Assert.IsNotNull(result, "Result should not be null");
            Assert.IsNotNull(result.data, "Serialized data should not be null");
            StringAssert.Contains("Terrain", result.componentType, "Component type should be reported");

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetComponent_FirstTerrainComponent_WhenNoComponentRef()
        {
            var go = CreateTerrainGameObject(GO_TerrainName);

            var tool = new Tool_Terrain();
            var result = tool.GetComponentData(new GameObjectRef(go.GetInstanceID()));

            Assert.IsNotNull(result.data, "Should serialize the first terrain component");
            StringAssert.Contains("Terrain", result.componentType, "Resolved component should be a Terrain type");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ModifyComponent_ProbeOffset_ViaFieldsChannel()
        {
            var go = CreateTerrainGameObject(GO_TerrainName);
            var probe = go.AddComponent<FieldChannelProbe>();
            var reflector = UnityMcpPluginEditor.Instance.Reflector ?? throw new Exception("Reflector not available.");

            var newOffset = new Vector3(0, 5, -12);
            // ProbeOffset is a public *field*, so it must be supplied through the 'fields' channel
            // (AddField). ReflectorNet's TryModify resolves 'props' as PropertyInfo only and 'fields'
            // as FieldInfo only — no cross-fallback.
            var diff = SerializedMember.FromValue(
                    reflector: reflector,
                    name: probe.GetType().Name,
                    type: typeof(FieldChannelProbe),
                    value: null)
                .AddField(SerializedMember.FromValue(
                    reflector: reflector,
                    name: nameof(probe.ProbeOffset),
                    value: newOffset));

            var tool = new Tool_Terrain();
            var result = tool.ModifyComponent(
                gameObjectRef: new GameObjectRef(go.GetInstanceID()),
                data: diff,
                componentRef: new ComponentRef(probe.GetInstanceID()));

            Assert.IsTrue(result.success, "Modification should succeed via the fields channel");
            Assert.AreEqual(newOffset, probe.ProbeOffset, "ProbeOffset field should be modified");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ModifyComponentJson_ProbeOffset_Dispatch()
        {
            var go = CreateTerrainGameObject(GO_TerrainName);
            var probe = go.AddComponent<FieldChannelProbe>();

            var json = $@"{{
                ""gameObjectRef"": {{ ""instanceID"": {go.GetInstanceID()} }},
                ""componentRef"": {{ ""instanceID"": {probe.GetInstanceID()} }},
                ""data"": {{
                    ""typeName"": ""com.IvanMurzak.Unity.MCP.Terrain.Editor.Tests.FieldChannelProbe"",
                    ""fields"": [
                        {{
                            ""name"": ""ProbeOffset"",
                            ""typeName"": ""UnityEngine.Vector3"",
                            ""value"": {{ ""x"": 1.0, ""y"": 2.0, ""z"": -3.0 }}
                        }}
                    ]
                }}
            }}";

            var result = RunToolAllowWarnings(Tool_Terrain.TerrainModifyComponentToolId, json);
            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(new Vector3(1, 2, -3), probe.ProbeOffset, "ProbeOffset should be modified via JSON fields channel");

            yield return null;
        }
    }
}
