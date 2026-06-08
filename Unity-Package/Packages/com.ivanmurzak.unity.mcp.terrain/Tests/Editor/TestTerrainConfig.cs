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
using AIGD;
using com.IvanMurzak.Unity.MCP.Editor.API;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace com.IvanMurzak.Unity.MCP.Terrain.Editor.Tests
{
    public class TestTerrainConfig : BaseTest
    {
        Texture2D CreateTextureAsset(string label)
        {
            var assetPath = $"Assets/TestTerrainTex_{label}_{Guid.NewGuid():N}.asset";
            var tex = new Texture2D(4, 4);
            AssetDatabase.CreateAsset(tex, assetPath);
            AssetDatabase.SaveAssets();
            TrackAsset(assetPath);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        [UnityTest]
        public IEnumerator SetHeightmapResolution_RoundsToValidValue()
        {
            var go = CreateTerrainGameObject(GO_TerrainName, heightmapResolution: 33, size: 100f);

            var tool = new Tool_Terrain();
            // 100 is not a valid (2^n + 1) value; expect rounding up to 129.
            var result = tool.SetHeightmapResolution(new GameObjectRef(go.GetInstanceID()), heightmapResolution: 100);

            Assert.IsTrue(result.success, "SetHeightmapResolution should succeed");
            Assert.AreEqual(129, result.heightmapResolution, "Resolution should round up to a valid 2^n + 1 value");

            yield return null;
        }

        [UnityTest]
        public IEnumerator AddLayer_FromTexture_ThenRemove()
        {
            var go = CreateTerrainGameObject(GO_TerrainName);
            var goRef = new GameObjectRef(go.GetInstanceID());
            var tex = CreateTextureAsset("layer");
            var texPath = AssetDatabase.GetAssetPath(tex);

            var tool = new Tool_Terrain();
            var addResult = tool.AddLayer(goRef, diffuseTextureAssetPath: texPath);

            Assert.IsTrue(addResult.success, "AddLayer should succeed");
            Assert.AreEqual(0, addResult.layerIndex, "First added layer should be index 0");
            Assert.AreEqual(1, addResult.layerCount, "Layer count should be 1");

            var removeResult = tool.RemoveLayer(goRef, layerIndex: 0);
            Assert.IsTrue(removeResult.success, "RemoveLayer should succeed");
            Assert.AreEqual(0, removeResult.layerCount, "Layer count should be 0 after removal");

            yield return null;
        }

        [UnityTest]
        public IEnumerator PaintLayer_WritesAlphamap()
        {
            var go = CreateTerrainGameObject(GO_TerrainName);
            var goRef = new GameObjectRef(go.GetInstanceID());
            var data = go.GetComponent<UnityEngine.Terrain>().terrainData;

            var tool = new Tool_Terrain();
            tool.AddLayer(goRef, diffuseTextureAssetPath: AssetDatabase.GetAssetPath(CreateTextureAsset("a")));
            tool.AddLayer(goRef, diffuseTextureAssetPath: AssetDatabase.GetAssetPath(CreateTextureAsset("b")));

            var paint = tool.PaintLayer(goRef, layerIndex: 1, xBase: 0, yBase: 0, width: 2, height: 2, strength: 1f);
            Assert.IsTrue(paint.success, "PaintLayer should succeed");

            var maps = data.GetAlphamaps(0, 0, 2, 2);
            Assert.AreEqual(1f, maps[0, 0, 1], 0.001f, "Painted layer should dominate the region");
            Assert.AreEqual(0f, maps[0, 0, 0], 0.001f, "Other layer should be cleared in the region");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetDetailPrototypes_FromTexture()
        {
            var go = CreateTerrainGameObject(GO_TerrainName);
            var goRef = new GameObjectRef(go.GetInstanceID());
            var texPath = AssetDatabase.GetAssetPath(CreateTextureAsset("grass"));

            var tool = new Tool_Terrain();
            var result = tool.SetDetailPrototypes(goRef, textureAssetPaths: new[] { texPath });

            Assert.IsTrue(result.success, "SetDetailPrototypes should succeed");
            Assert.AreEqual(1, result.detailPrototypeCount, "Should have one detail prototype");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetTreePrototypes_PlaceTrees()
        {
            var go = CreateTerrainGameObject(GO_TerrainName);
            var goRef = new GameObjectRef(go.GetInstanceID());

            // Build a simple prefab asset to use as a tree.
            var prefabSource = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var prefabPath = $"Assets/TestTree_{Guid.NewGuid():N}.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(prefabSource, prefabPath);
            UnityEngine.Object.DestroyImmediate(prefabSource);
            TrackAsset(prefabPath);

            var tool = new Tool_Terrain();
            var protoResult = tool.SetTreePrototypes(goRef, prefabAssetPaths: new[] { prefabPath });
            Assert.IsTrue(protoResult.success, "SetTreePrototypes should succeed");
            Assert.AreEqual(1, protoResult.treePrototypeCount, "Should have one tree prototype");

            var placeResult = tool.PlaceTrees(goRef, prototypeIndex: 0, count: 10);
            Assert.IsTrue(placeResult.success, "PlaceTrees should succeed");
            Assert.AreEqual(10, placeResult.treesAdded, "Should add 10 trees");
            Assert.GreaterOrEqual(placeResult.treeInstanceCount, 10, "Tree instance count should be at least 10");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetNeighbors_WiresCenterTerrain()
        {
            var center = CreateTerrainGameObject("CenterTerrain");
            var right = CreateTerrainGameObject("RightTerrain");

            var tool = new Tool_Terrain();
            var result = tool.SetNeighbors(
                new GameObjectRef(center.GetInstanceID()),
                rightRef: new GameObjectRef(right.GetInstanceID()));

            Assert.IsTrue(result.success, "SetNeighbors should succeed");
            Assert.AreEqual("RightTerrain", result.right, "Right neighbor should be reported");

            yield return null;
        }
    }
}
