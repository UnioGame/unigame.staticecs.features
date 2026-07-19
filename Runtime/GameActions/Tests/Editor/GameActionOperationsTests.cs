using FFS.Libraries.StaticEcs;
using NUnit.Framework;

namespace UniGame.StaticEcs.Features.Tests {
    // Minimal action types used only in tests.
    internal struct JumpAction : IGameAction { public float Height; }
    internal struct AttackAction : IGameAction { public int Damage; }

    [TestFixture]
    public sealed class GameActionOperationsTests {
        [SetUp]
        public void SetUp() {
            World<TestGameActionsWorld>.Create(WorldConfig.Default());
            new ModifierBackRefFeature<TestGameActionsWorld>().RegisterTypes(World<TestGameActionsWorld>.Types());
            new StunFeature<TestGameActionsWorld>().RegisterTypes(World<TestGameActionsWorld>.Types());
            new GameActionsFeature<TestGameActionsWorld>().RegisterTypes(World<TestGameActionsWorld>.Types());
            World<TestGameActionsWorld>.Types()
                .Event<GameActionEvent<JumpAction>>()
                .Event<GameActionEvent<AttackAction>>();
            World<TestGameActionsWorld>.Initialize();
        }

        [TearDown]
        public void TearDown() {
            if (World<TestGameActionsWorld>.Status != WorldStatus.NotCreated) {
                World<TestGameActionsWorld>.Destroy();
            }
        }

        // --- ActionBit tests ---

        [Test]
        public void ActionBit_DifferentTypes_HaveUniqueIndices() {
            Assert.AreNotEqual(ActionBit<JumpAction>.Index, ActionBit<AttackAction>.Index);
        }

        [Test]
        public void ActionBit_SameType_ReturnsSameIndex() {
            Assert.AreEqual(ActionBit<JumpAction>.Index, ActionBit<JumpAction>.Index);
        }

        // --- IsAvailable tests ---

        [Test]
        public void IsAvailable_EntityWithoutMask_ReturnsTrue() {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();

            Assert.IsTrue(GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(entity.GID));
        }

        [Test]
        public void IsAvailable_AllEnabledMask_ReturnsTrue() {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllEnabled);

            Assert.IsTrue(GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(entity.GID));
        }

        [Test]
        public void IsAvailable_AllDisabledMask_ReturnsFalse() {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllDisabled);

            Assert.IsFalse(GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(entity.GID));
        }

        [Test]
        public void IsAvailable_DestroyedEntity_ReturnsFalse() {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            var gid = entity.GID;
            entity.Destroy();

            Assert.IsFalse(GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(gid));
        }

        // --- EnableAction / DisableAction tests ---

        [Test]
        public void DisableAction_ClearsSpecificBit_OtherBitsUnchanged() {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllEnabled);

            GameActionOperations.DisableAction<TestGameActionsWorld, JumpAction>(entity.GID);

            Assert.IsFalse(GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(entity.GID));
            Assert.IsTrue(GameActionOperations.IsAvailable<TestGameActionsWorld, AttackAction>(entity.GID));
        }

        [Test]
        public void EnableAction_SetsBitAfterDisable() {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllDisabled);

            GameActionOperations.EnableAction<TestGameActionsWorld, JumpAction>(entity.GID);

            Assert.IsTrue(GameActionOperations.IsAvailable<TestGameActionsWorld, JumpAction>(entity.GID));
            Assert.IsFalse(GameActionOperations.IsAvailable<TestGameActionsWorld, AttackAction>(entity.GID));
        }

        // --- Raise tests ---

        [Test]
        public void Raise_MaskedAction_DoesNotSendEvent() {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllDisabled);

            var receiver = World<TestGameActionsWorld>.RegisterEventReceiver<GameActionEvent<JumpAction>>();

            GameActionOperations.Raise<TestGameActionsWorld, JumpAction>(entity.GID, new JumpAction { Height = 3f });

            var count = 0;
            foreach (var _ in receiver) {
                count++;
            }

            World<TestGameActionsWorld>.DeleteEventReceiver(ref receiver);
            Assert.AreEqual(0, count, "Masked action must not emit event.");
        }

        [Test]
        public void Raise_EnabledAction_SendsEventWithCorrectPayload() {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            entity.Set(ActionMaskComponent.AllEnabled);

            var receiver = World<TestGameActionsWorld>.RegisterEventReceiver<GameActionEvent<JumpAction>>();

            GameActionOperations.Raise<TestGameActionsWorld, JumpAction>(entity.GID, new JumpAction { Height = 2.5f });

            GameActionEvent<JumpAction> received = default;
            var count = 0;
            foreach (var e in receiver) {
                received = e.Value;
                count++;
            }

            World<TestGameActionsWorld>.DeleteEventReceiver(ref receiver);
            Assert.AreEqual(1, count);
            Assert.AreEqual(entity.GID, received.Source);
            Assert.AreEqual(2.5f, received.Payload.Height);
        }

        [Test]
        public void Raise_EntityWithoutMask_SendsEvent() {
            var entity = World<TestGameActionsWorld>.NewEntity<Default>();
            var receiver = World<TestGameActionsWorld>.RegisterEventReceiver<GameActionEvent<JumpAction>>();

            GameActionOperations.Raise<TestGameActionsWorld, JumpAction>(entity.GID, new JumpAction { Height = 1f });

            var count = 0;
            foreach (var _ in receiver) {
                count++;
            }

            World<TestGameActionsWorld>.DeleteEventReceiver(ref receiver);
            Assert.AreEqual(1, count);
        }

        // --- ActionMaskMaintenanceSystem tests ---

        [Test]
        public void MaintenanceSystem_StunActive_ClearsAllBits() {
            var target = World<TestGameActionsWorld>.NewEntity<Default>();
            var source = World<TestGameActionsWorld>.NewEntity<Default>();
            target.Set(ActionMaskComponent.AllEnabled);

            var system = new ActionMaskMaintenanceSystem<TestGameActionsWorld>();
            system.Init();

            StunOperations.AddSource<TestGameActionsWorld>(target.GID, source.GID);
            system.Update();

            Assert.AreEqual(0u, target.Read<ActionMaskComponent>().Bits,
                "Stun becoming active must clear all action mask bits.");

            system.Destroy();
        }

        [Test]
        public void MaintenanceSystem_StunClears_RestoresAllBits() {
            var target = World<TestGameActionsWorld>.NewEntity<Default>();
            var source = World<TestGameActionsWorld>.NewEntity<Default>();
            target.Set(ActionMaskComponent.AllEnabled);

            var system = new ActionMaskMaintenanceSystem<TestGameActionsWorld>();
            system.Init();

            StunOperations.AddSource<TestGameActionsWorld>(target.GID, source.GID);
            system.Update();
            StunOperations.RemoveSource<TestGameActionsWorld>(target.GID, source.GID);
            system.Update();

            Assert.AreEqual(uint.MaxValue, target.Read<ActionMaskComponent>().Bits,
                "Stun clearing must restore full action mask.");

            system.Destroy();
        }

        [Test]
        public void MaintenanceSystem_StunActive_SkipsEntityWithoutMask() {
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
