using FFS.Libraries.StaticEcs;
using NUnit.Framework;
using System;
using UnityEditor;
using UnityEngine;

namespace unigame.staticecs.features.Tests {
    [TestFixture]
    public sealed class AbilityAuthoringTests {
        private AbilityDatabase _database;
        private AbilityAsset _firstAsset;
        private AbilityAsset _secondAsset;

        [TearDown]
        public void TearDown() {
            try {
                if (World<TestAbilityWorld>.Status != WorldStatus.NotCreated) {
                    World<TestAbilityWorld>.Destroy();
                }
            } finally {
                DestroyObject(_database);
                DestroyObject(_firstAsset);
                DestroyObject(_secondAsset);

                _database = null;
                _firstAsset = null;
                _secondAsset = null;
            }
        }

        [Test]
        public void DatabaseFeature_RegistersAbilityAssets() {
            _firstAsset = CreateAbilityAsset(501, "Wait", new WaitStepConfig(0.25f));
            _database = CreateDatabase(_firstAsset);

            CreateWorld(_database);

            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            Assert.AreEqual(1, registry.Count);
            Assert.IsTrue(registry.TryGet(501, out var definition, out var root));
            Assert.AreEqual("Wait", definition.DisplayName);
            Assert.IsInstanceOf<WaitStepConfig>(root);
            Assert.AreEqual(0.25f, ((WaitStepConfig) root).Duration);
        }

        [Test]
        public void DatabaseFeature_ThrowsOnDuplicateIds() {
            _firstAsset = CreateAbilityAsset(502, "First", new WaitStepConfig(0.1f));
            _secondAsset = CreateAbilityAsset(502, "Second", new WaitStepConfig(0.2f));
            _database = CreateDatabase(_firstAsset, _secondAsset);

            World<TestAbilityWorld>.Create(WorldConfig.Default());
            new AbilityFeature<TestAbilityWorld>(registerSystems: false).RegisterTypes(World<TestAbilityWorld>.Types());

            var exception = Assert.Throws<InvalidOperationException>(() =>
                new AbilityDatabaseFeature<TestAbilityWorld>(_database).RegisterTypes(World<TestAbilityWorld>.Types()));
            StringAssert.Contains("duplicate ability id", exception.Message);

            World<TestAbilityWorld>.Initialize();
        }

        [Test]
        public void DatabaseFeature_InstantiatesAssetsByDefault() {
            var sourceRoot = new WaitStepConfig(0.5f);
            _firstAsset = CreateAbilityAsset(503, "Clone", sourceRoot);
            _database = CreateDatabase(_firstAsset);

            CreateWorld(_database);

            var registry = World<TestAbilityWorld>.GetResource<AbilityRegistry<TestAbilityWorld>>();
            var runtimeRoot = registry.GetRoot(503);

            Assert.IsNotNull(runtimeRoot);
            Assert.AreNotSame(sourceRoot, runtimeRoot);
            Assert.IsInstanceOf<WaitStepConfig>(runtimeRoot);
            Assert.AreEqual(0.5f, ((WaitStepConfig) runtimeRoot).Duration);
        }

        private static void CreateWorld(AbilityDatabase database) {
            World<TestAbilityWorld>.Create(WorldConfig.Default());
            new AbilityFeature<TestAbilityWorld>(registerSystems: false).RegisterTypes(World<TestAbilityWorld>.Types());
            new AbilityDatabaseFeature<TestAbilityWorld>(database).RegisterTypes(World<TestAbilityWorld>.Types());
            World<TestAbilityWorld>.Initialize();
        }

        private static AbilityAsset CreateAbilityAsset(int id, string displayName, IAbilityStepConfig root) {
            var asset = ScriptableObject.CreateInstance<AbilityAsset>();
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("_id").intValue = id;
            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_root").managedReferenceValue = root;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return asset;
        }

        private static AbilityDatabase CreateDatabase(params AbilityAsset[] assets) {
            var database = ScriptableObject.CreateInstance<AbilityDatabase>();
            var serialized = new SerializedObject(database);
            var abilities = serialized.FindProperty("_abilities");
            abilities.arraySize = assets.Length;
            for (var i = 0; i < assets.Length; i++) {
                abilities.GetArrayElementAtIndex(i).objectReferenceValue = assets[i];
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return database;
        }

        private static void DestroyObject(UnityEngine.Object asset) {
            if (asset != null) {
                UnityEngine.Object.DestroyImmediate(asset);
            }
        }
    }
}
