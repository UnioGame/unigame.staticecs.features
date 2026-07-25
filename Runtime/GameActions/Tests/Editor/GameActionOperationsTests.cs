namespace UniGame.StaticEcs.Features.Tests
{
    using FFS.Libraries.StaticEcs;
    using NUnit.Framework;
    using UniGame.StaticEcs.Tests;
    using UnityEditor;
    using UnityEngine;
    using Object = UnityEngine.Object;

    // Minimal action types used only in tests.
    internal struct JumpAction : IGameAction
    {
        public float Height;
    }

    internal struct AttackAction : IGameAction
    {
        public int Damage;
    }

    [TestFixture]
    public sealed class GameActionOperationsTests
    {
        private StaticEcsTestWorld<TestGameActionsWorld> _world;

        [SetUp]
        public void SetUp()
        {
            _world = new StaticEcsTestWorld<TestGameActionsWorld>();
            new ModifierBackRefFeature<TestGameActionsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new StunFeature<TestGameActionsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            new GameActionsFeature<TestGameActionsWorld>().InstallResourcesAndRegisterTypesForTest(
                _world
            );
            var types = _world.Types;
            GameActionRegistrar.Register<TestGameActionsWorld, JumpAction>(types, 0);
            GameActionRegistrar.Register<TestGameActionsWorld, AttackAction>(types, 1);
            _world.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _world?.Dispose();
        }

        // --- Stable registry tests ---

        [Test]
        public void Registry_DifferentTypes_HaveUniqueMasks()
        {
            ref var registry =
                ref World<TestGameActionsWorld>
                    .GetResource<GameActionRegistry<TestGameActionsWorld>>();
            Assert.AreNotEqual(
                registry.GetMask<JumpAction>(),
                registry.GetMask<AttackAction>());
        }

        [Test]
        public void Registry_SameType_ReturnsSameMask()
        {
            ref var registry =
                ref World<TestGameActionsWorld>
                    .GetResource<GameActionRegistry<TestGameActionsWorld>>();
            Assert.AreEqual(
                registry.GetMask<JumpAction>(),
                registry.GetMask<JumpAction>());
        }

        [Test]
        public void FeatureAssetConfigurationSurvivesUnitySerialization()
        {
            var source = ScriptableObject.CreateInstance<GameActionsFeatureAsset>();
            var target = ScriptableObject.CreateInstance<GameActionsFeatureAsset>();
            try
            {
                source.feature.registerMaintenance = false;
                source.feature.maintenanceOrder = 73;

                var json = EditorJsonUtility.ToJson(source);
                EditorJsonUtility.FromJsonOverwrite(json, target);

                Assert.IsFalse(target.feature.registerMaintenance);
                Assert.AreEqual(73, target.feature.maintenanceOrder);
            }
            finally
            {
                Object.DestroyImmediate(target);
                Object.DestroyImmediate(source);
            }
        }

        // --- IsAvailable tests ---

        [Test]
        public void IsAvailable_EntityWithoutMask_ReturnsTrue()
        {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();

            Assert.IsTrue(
                GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(entity.GID)
            );
        }

        [Test]
        public void IsAvailable_AllEnabledMask_ReturnsTrue()
        {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllEnabled);

            Assert.IsTrue(
                GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(entity.GID)
            );
        }

        [Test]
        public void IsAvailable_AllDisabledMask_ReturnsFalse()
        {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllDisabled);

            Assert.IsFalse(
                GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(entity.GID)
            );
        }

        [Test]
        public void IsAvailable_DestroyedEntity_ReturnsFalse()
        {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            var gid = entity.GID;
            entity.Destroy();

            Assert.IsFalse(GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(gid));
        }

        // --- EnableAction / DisableAction tests ---

        [Test]
        public void DisableAction_ClearsSpecificBit_OtherBitsUnchanged()
        {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllEnabled);

            GameActionOperations.DisableAction<TestGameActionsWorld, JumpAction>(entity.GID);

            Assert.IsFalse(
                GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(entity.GID)
            );
            Assert.IsTrue(
                GameActionOperations.IsAvailable<TestGameActionsWorld, AttackAction>(entity.GID)
            );
        }

        [Test]
        public void EnableAction_SetsBitAfterDisable()
        {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllDisabled);

            GameActionOperations.EnableAction<TestGameActionsWorld, JumpAction>(entity.GID);

            Assert.IsTrue(
                GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(entity.GID)
            );
            Assert.IsFalse(
                GameActionOperations.IsAvailable<TestGameActionsWorld, AttackAction>(entity.GID)
            );
        }

        // --- Raise tests ---

        [Test]
        public void Raise_MaskedAction_DoesNotSendEvent()
        {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllDisabled);

            var receiver = World<TestGameActionsWorld>.RegisterEventReceiver<
                GameActionEvent<JumpAction>
            >();

            GameActionOperations.Raise<TestGameActionsWorld, JumpAction>(
                entity.GID,
                new JumpAction { Height = 3f }
            );

            var count = 0;
            foreach (var _ in receiver)
            {
                count++;
            }

            World<TestGameActionsWorld>.DeleteEventReceiver(ref receiver);
            Assert.AreEqual(0, count, "Masked action must not emit event.");
        }

        [Test]
        public void Raise_EnabledAction_SendsEventWithCorrectPayload()
        {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllEnabled);

            var receiver = World<TestGameActionsWorld>.RegisterEventReceiver<
                GameActionEvent<JumpAction>
            >();

            GameActionOperations.Raise<TestGameActionsWorld, JumpAction>(
                entity.GID,
                new JumpAction { Height = 2.5f }
            );

            GameActionEvent<JumpAction> received = default;
            var count = 0;
            foreach (var e in receiver)
            {
                received = e.Value;
                count++;
            }

            World<TestGameActionsWorld>.DeleteEventReceiver(ref receiver);
            Assert.AreEqual(1, count);
            Assert.AreEqual(entity.GID, received.Source);
            Assert.AreEqual(2.5f, received.Payload.Height);
        }

        [Test]
        public void Raise_EntityWithoutMask_SendsEvent()
        {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            var receiver = World<TestGameActionsWorld>.RegisterEventReceiver<
                GameActionEvent<JumpAction>
            >();

            GameActionOperations.Raise<TestGameActionsWorld, JumpAction>(
                entity.GID,
                new JumpAction { Height = 1f }
            );

            var count = 0;
            foreach (var _ in receiver)
            {
                count++;
            }

            World<TestGameActionsWorld>.DeleteEventReceiver(ref receiver);
            Assert.AreEqual(1, count);
        }

        // --- ActionMaskMaintenanceSystem tests ---

        [Test]
        public void MaintenanceSystem_StunActive_ClearsAllBits()
        {
            var target = World<TestGameActionsWorld>.NewEntity<Default>();
            var source = World<TestGameActionsWorld>.NewEntity<Default>();
            target.Set(ActionMaskComponent.AllEnabled);

            var system = new ActionMaskMaintenanceSystem<TestGameActionsWorld>();
            system.Init();

            StunOperations.AddSource<TestGameActionsWorld>(target.GID, source.GID);
            system.Update();

            Assert.AreEqual(
                0u,
                target.Read<ActionMaskComponent>().Bits,
                "Stun becoming active must clear all action mask bits."
            );

            system.Destroy();
        }

        [Test]
        public void MaintenanceSystem_StunClears_RestoresAllBits()
        {
            var target = World<TestGameActionsWorld>.NewEntity<Default>();
            var source = World<TestGameActionsWorld>.NewEntity<Default>();
            target.Set(ActionMaskComponent.AllEnabled);

            var system = new ActionMaskMaintenanceSystem<TestGameActionsWorld>();
            system.Init();

            StunOperations.AddSource<TestGameActionsWorld>(target.GID, source.GID);
            system.Update();
            StunOperations.RemoveSource<TestGameActionsWorld>(target.GID, source.GID);
            system.Update();

            Assert.AreEqual(
                uint.MaxValue,
                target.Read<ActionMaskComponent>().Bits,
                "Stun clearing must restore full action mask."
            );

            system.Destroy();
        }

        [Test]
        public void MaintenanceSystem_StunActive_SkipsEntityWithoutMask()
        {
            var target = World<TestGameActionsWorld>.NewEntity<Default>();
            var source = World<TestGameActionsWorld>.NewEntity<Default>();
            // No ActionMaskComponent added.

            var system = new ActionMaskMaintenanceSystem<TestGameActionsWorld>();
            system.Init();

            StunOperations.AddSource<TestGameActionsWorld>(target.GID, source.GID);
            system.Update(); // Must not throw.

            Assert.IsFalse(target.Has<ActionMaskComponent>());

            system.Destroy();
        }
    }
}
