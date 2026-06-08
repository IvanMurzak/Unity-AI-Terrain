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
    public class TestTerrainLifecycle : BaseTest
    {
        [UnityTest]
        public IEnumerator CreateTerrain_AddsTerrainAndData()
        {
            var assetPath = $"Assets/TestTerrainData_Create_{Guid.NewGuid():N}.asset";
            TrackAsset(assetPath);

            var tool = new Tool_Terrain();
            var result = tool.CreateTerrain(
                name: GO_TerrainName,
                terrainDataAssetPath: assetPath,
                heightmapResolution: 65,
                width: 200f,
                length: 200f,
                height: 50f,
                position: new Vector3(5, 0, 5));

            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(GO_TerrainName, result.gameObjectName, "Name should match");
            Assert.AreEqual(65, result.heightmapResolution, "Heightmap resolution should match");
            Assert.AreEqual(new Vector3(200f, 50f, 200f), result.size, "Size should match");

            var go = GameObject.Find(GO_TerrainName);
            Assert.IsNotNull(go, "Terrain GameObject should exist in scene");
            var terrain = go!.GetComponent<UnityEngine.Terrain>();
            Assert.IsNotNull(terrain, "Terrain component should be attached");
            Assert.IsNotNull(terrain!.terrainData, "TerrainData should be assigned");
            Assert.AreEqual(new Vector3(5, 0, 5), go.transform.position, "Position should be applied");

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetTerrain_ReportsConfig()
        {
            var go = CreateTerrainGameObject(GO_TerrainName, heightmapResolution: 65, size: 150f);

            var tool = new Tool_Terrain();
            var result = tool.GetTerrain(new GameObjectRef(go.GetInstanceID()));

            Assert.IsNotNull(result, "Result should not be null");
            Assert.AreEqual(65, result.heightmapResolution, "Heightmap resolution should be reported");
            Assert.AreEqual(150f, result.size.x, 0.01f, "Width should be reported");
            Assert.IsNotNull(result.layers, "Layers array should not be null");

            yield return null;
        }

        [UnityTest]
        public IEnumerator ListTerrains_FindsCreatedTerrains()
        {
            CreateTerrainGameObject("TerrainA");
            CreateTerrainGameObject("TerrainB");

            var tool = new Tool_Terrain();
            var result = tool.ListTerrains();

            Assert.IsNotNull(result, "Result should not be null");
            Assert.GreaterOrEqual(result.count, 2, "Should find at least the two created terrains");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetSize_UpdatesTerrainData()
        {
            var go = CreateTerrainGameObject(GO_TerrainName, heightmapResolution: 33, size: 100f);

            var tool = new Tool_Terrain();
            var result = tool.SetSize(new GameObjectRef(go.GetInstanceID()), width: 300f, height: 75f, length: 250f);

            Assert.IsTrue(result.success, "SetSize should succeed");
            Assert.AreEqual(new Vector3(300f, 75f, 250f), go.GetComponent<UnityEngine.Terrain>().terrainData.size,
                "TerrainData size should be updated");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetHeights_Uniform_ThenSample()
        {
            var go = CreateTerrainGameObject(GO_TerrainName, heightmapResolution: 33, size: 100f);
            var goRef = new GameObjectRef(go.GetInstanceID());

            var tool = new Tool_Terrain();
            var setResult = tool.SetHeights(goRef, uniformHeight: 0.5f);
            Assert.IsTrue(setResult.success, "SetHeights should succeed");

            var sample = tool.SampleHeights(goRef);
            Assert.AreEqual(0.5f, sample.min, 0.001f, "Min sampled height should be the uniform value");
            Assert.AreEqual(0.5f, sample.max, 0.001f, "Max sampled height should be the uniform value");
            Assert.AreEqual(0.5f, sample.average, 0.001f, "Average sampled height should be the uniform value");

            yield return null;
        }

        [UnityTest]
        public IEnumerator SetHeights_Region_AffectsOnlyRegion()
        {
            var go = CreateTerrainGameObject(GO_TerrainName, heightmapResolution: 33, size: 100f);
            var goRef = new GameObjectRef(go.GetInstanceID());

            var tool = new Tool_Terrain();
            // Raise a 4x4 region in the corner.
            var setResult = tool.SetHeights(goRef, xBase: 0, yBase: 0, width: 4, height: 4, uniformHeight: 1f);
            Assert.IsTrue(setResult.success, "Region SetHeights should succeed");
            Assert.AreEqual(4, setResult.width, "Region width should be 4");
            Assert.AreEqual(4, setResult.height, "Region height should be 4");

            var regionSample = tool.SampleHeights(goRef, xBase: 0, yBase: 0, width: 4, height: 4);
            Assert.AreEqual(1f, regionSample.average, 0.001f, "Region should be fully raised");

            yield return null;
        }
    }
}
