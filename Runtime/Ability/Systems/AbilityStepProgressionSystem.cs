using System.Collections.Generic;
using FFS.Libraries.StaticEcs;
 

namespace UniGame.StaticEcs.Features {
    using Time;

    public sealed class AbilityStepProgressionSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType {
        private readonly List<EntityGID> _readyBuffer = new(8);
        private EventReceiver<TWorld, AbilityBranchCompletedEvent> _branchReceiver;
        private bool _branchReceiverInitialized;

        public void Init() {
            _branchReceiver = World<TWorld>.RegisterEventReceiver<AbilityBranchCompletedEvent>();
            _branchReceiverInitialized = true;
        }

        public void Update() {
            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>()) {
                return;
            }
            if (!World<TWorld>.HasResource<AbilityStepActivatorRegistry<TWorld>>()) {
                return;
            }

            ProcessBranchCompletedEvents();
            CollectReadyCasts();
            if (_readyBuffer.Count == 0) {
                return;
            }

            var registry = World<TWorld>.GetResource<AbilityRegistry<TWorld>>();
            var activators = World<TWorld>.GetResource<AbilityStepActivatorRegistry<TWorld>>();

            for (var i = 0; i < _readyBuffer.Count; i++) {
                AdvanceCast(_readyBuffer[i], registry, activators);
            }
            _readyBuffer.Clear();
        }

        public void Destroy() {
            if (_branchReceiverInitialized) {
                World<TWorld>.DeleteEventReceiver(ref _branchReceiver);
                _branchReceiverInitialized = false;
            }
        }

        private void ProcessBranchCompletedEvents() {
            if (!_branchReceiverInitialized) {
                return;
            }

            foreach (var e in _branchReceiver) {
                var ev = e.Value;
                if (!ev.ParentCast.TryUnpack<TWorld>(out var parent)) {
                    continue;
                }
                if (!parent.Has<AbilityParallelWaitingTag>()) {
                    continue;
                }
                if (!parent.Has<World<TWorld>.Multi<AbilityParallelBranchEntry>>()) {
                    continue;
                }
                if (!parent.Has<World<TWorld>.Multi<AbilityStackFrame>>()) {
                    continue;
                }

                ref var branches = ref parent.Ref<World<TWorld>.Multi<AbilityParallelBranchEntry>>();
                var branchFound = false;
                for (var i = 0; i < branches.Length; i++) {
                    if (!branches[i].BranchCast.Equals(ev.BranchCast) || branches[i].Completed) {
                        continue;
                    }

                    branches[i].Completed = true;
                    branches[i].Status = ev.Status;
                    branchFound = true;
                    break;
                }

                if (!branchFound) {
                    continue;
                }

                ref var frames = ref parent.Ref<World<TWorld>.Multi<AbilityStackFrame>>();
                if (frames.Length == 0) {
                    continue;
                }

                var topIndex = frames.Length - 1;
                ref var top = ref frames[topIndex];
                if (top.Kind != AbilityStepKind.Parallel) {
                    continue;
                }

                if (ev.Status == StepStatus.Success) {
                    top.SuccessCount++;
                } else {
                    top.FailedCount++;
                }

                var parallel = (ParallelStepConfig)top.Composite;
                if (!TryResolveParallelJoin(in top, parallel, out var status)) {
                    continue;
                }

                if (parallel.CancelRemainingOnJoin) {
                    CancelRemainingBranches(in branches);
                }

                parent.Delete<AbilityParallelWaitingTag>();
                parent.Set(new AbilityStepLastStatus { Status = status });
                parent.Set<AbilityStepReadyTag>();
            }
        }

        private static bool TryResolveParallelJoin(
            in AbilityStackFrame frame,
            ParallelStepConfig parallel,
            out StepStatus status) {
            if (parallel.JoinPolicy == ParallelJoinPolicy.AnySuccess) {
                if (frame.SuccessCount > 0) {
                    status = StepStatus.Success;
                    return true;
                }
                if (frame.FailedCount >= frame.ChildrenTotal) {
                    status = StepStatus.Failed;
                    return true;
                }

                status = StepStatus.Running;
                return false;
            }

            if (frame.FailedCount > 0) {
                status = StepStatus.Failed;
                return true;
            }
            if (frame.SuccessCount >= frame.ChildrenTotal) {
                status = StepStatus.Success;
                return true;
            }

            status = StepStatus.Running;
            return false;
        }

        private static void CancelRemainingBranches(in World<TWorld>.Multi<AbilityParallelBranchEntry> branches) {
            for (var i = 0; i < branches.Length; i++) {
                if (branches[i].Completed) {
                    continue;
                }
                if (branches[i].BranchCast.TryUnpack<TWorld>(out var branch)) {
                    branch.Destroy();
                }
            }
        }

        private void CollectReadyCasts() {
            _readyBuffer.Clear();
            foreach (var entity in World<TWorld>
                         .Query<All<AbilityCastRuntimeComponent, AbilityStepReadyTag>>()
                         .Entities()) {
                _readyBuffer.Add(entity.GID);
            }
        }

        private static void AdvanceCast(
            EntityGID castGid,
            AbilityRegistry<TWorld> registry,
            AbilityStepActivatorRegistry<TWorld> activators) {
            if (!castGid.TryUnpack<TWorld>(out var castEntity)) {
                return;
            }
            if (!castEntity.Has<AbilityCastRuntimeComponent>()) {
                return;
            }
            if (castEntity.Has<AbilityParallelWaitingTag>()) {
                return;
            }

            var status = castEntity.Has<AbilityStepLastStatus>()
                ? castEntity.Read<AbilityStepLastStatus>().Status
                : StepStatus.Success;

            if (castEntity.Has<AbilityStepLastStatus>()) {
                castEntity.Delete<AbilityStepLastStatus>();
            }
            if (castEntity.Has<AbilityStepReadyTag>()) {
                castEntity.Delete<AbilityStepReadyTag>();
            }

            CloseCurrentLeaf(castEntity, castGid, status);

            while (true) {
                if (status == StepStatus.Failed) {
                    TerminateCast(castEntity, castGid, AbilityCompletedReason.Cancelled);
                    return;
                }

                var result = ResolveNext(castEntity, castGid, registry);
                if (result.Kind == TraversalResultKind.Complete) {
                    TerminateCast(castEntity, castGid, AbilityCompletedReason.Success);
                    return;
                }
                if (result.Kind == TraversalResultKind.RunningComposite) {
                    return;
                }
                if (result.Kind == TraversalResultKind.NoOpSuccess) {
                    status = StepStatus.Success;
                    continue;
                }

                var nextLeaf = result.Leaf;
                if (!activators.TryResolve(nextLeaf.GetType(), out var activator)) {
                    status = StepStatus.Failed;
                    continue;
                }

                var runtime = castEntity.Read<AbilityCastRuntimeComponent>();
                EmitStepStarted(castEntity, castGid, nextLeaf, runtime.AbilityId);

                var ctx = new AbilityStepActivationContext<TWorld>(
                    runtime.Caster,
                    ResolveOwner(castEntity, runtime.Caster),
                    castGid,
                    runtime.PrimaryTarget,
                    runtime.AbilityId);

                var activationStatus = activator.Activate(nextLeaf, in ctx);

                if (activationStatus == StepStatus.Running) {
                    castEntity.Set(new AbilityCurrentLeaf { Config = nextLeaf });
                    return;
                }

                EmitStepCompleted(castEntity, castGid, nextLeaf, runtime.AbilityId, activationStatus);
                status = activationStatus;
            }
        }

        private static void CloseCurrentLeaf(World<TWorld>.Entity castEntity, EntityGID castGid, StepStatus status) {
            if (!castEntity.Has<AbilityCurrentLeaf>()) {
                return;
            }

            var leaf = castEntity.Read<AbilityCurrentLeaf>().Config;
            castEntity.Delete<AbilityCurrentLeaf>();

            var abilityId = castEntity.Read<AbilityCastRuntimeComponent>().AbilityId;
            EmitStepCompleted(castEntity, castGid, leaf, abilityId, status);
        }

        private static TraversalResult ResolveNext(
            World<TWorld>.Entity castEntity,
            EntityGID castGid,
            AbilityRegistry<TWorld> registry) {
            ref var runtime = ref castEntity.Mut<AbilityCastRuntimeComponent>();

            if (!runtime.RootEntered) {
                runtime.RootEntered = true;
                var root = ResolveRoot(castEntity, registry, runtime.AbilityId);
                return Descend(root, castEntity, castGid);
            }

            if (!castEntity.Has<World<TWorld>.Multi<AbilityStackFrame>>()) {
                return TraversalResult.Complete();
            }

            while (castEntity.Has<World<TWorld>.Multi<AbilityStackFrame>>()) {
                ref var frames = ref castEntity.Ref<World<TWorld>.Multi<AbilityStackFrame>>();
                if (frames.Length == 0) {
                    break;
                }

                var topIndex = frames.Length - 1;
                ref var top = ref frames[topIndex];

                if (top.Kind == AbilityStepKind.Sequence) {
                    var nextChildIndex = top.Cursor + 1;
                    if (nextChildIndex < top.ChildrenTotal) {
                        top.Cursor = nextChildIndex;
                        var seq = (SequenceStepConfig)top.Composite;
                        return Descend(seq.GetChild(nextChildIndex), castEntity, castGid);
                    }
                    frames.RemoveAtSwap(topIndex);
                    continue;
                }

                if (top.Kind == AbilityStepKind.Repeat) {
                    var repeat = (RepeatStepConfig)top.Composite;
                    var nextIteration = top.Cursor + 1;
                    if (nextIteration < top.ChildrenTotal && ShouldRunRepeat(repeat, castEntity, castGid)) {
                        top.Cursor = nextIteration;
                        return Descend(repeat.Body, castEntity, castGid);
                    }
                    frames.RemoveAtSwap(topIndex);
                    continue;
                }

                if (top.Kind == AbilityStepKind.Parallel) {
                    frames.RemoveAtSwap(topIndex);
                    if (castEntity.Has<World<TWorld>.Multi<AbilityParallelBranchEntry>>()) {
                        castEntity.Delete<World<TWorld>.Multi<AbilityParallelBranchEntry>>();
                    }
                    continue;
                }

                frames.RemoveAtSwap(topIndex);
            }

            return TraversalResult.Complete();
        }

        private static IAbilityStepConfig ResolveRoot(
            World<TWorld>.Entity castEntity,
            AbilityRegistry<TWorld> registry,
            AbilityId abilityId) {
            if (castEntity.Has<AbilityInlineRootConfig>()) {
                return castEntity.Read<AbilityInlineRootConfig>().Root;
            }

            return registry.TryGet(abilityId, out _, out var root) ? root : null;
        }

        private static TraversalResult Descend(IAbilityStepConfig config, World<TWorld>.Entity castEntity, EntityGID castGid) {
            while (config != null && IsComposite(config.Kind)) {
                if (config is SequenceStepConfig sequence) {
                    if (sequence.ChildCount == 0) {
                        return TraversalResult.NoOpSuccess();
                    }
                    PushFrame(castEntity, new AbilityStackFrame {
                        Composite = sequence,
                        Kind = AbilityStepKind.Sequence,
                        Cursor = 0,
                        ChildrenTotal = sequence.ChildCount,
                    });
                    config = sequence.GetChild(0);
                    continue;
                }

                if (config is ConditionalStepConfig conditional) {
                    var conditionResult = EvaluateCondition(conditional.Condition, castEntity, castGid);
                    config = conditionResult ? conditional.IfTrue : conditional.IfFalse;
                    if (config == null) {
                        return TraversalResult.NoOpSuccess();
                    }
                    continue;
                }

                if (config is RepeatStepConfig repeat) {
                    if (repeat.MaxIterations <= 0 || repeat.Body == null || !ShouldRunRepeat(repeat, castEntity, castGid)) {
                        return TraversalResult.NoOpSuccess();
                    }
                    PushFrame(castEntity, new AbilityStackFrame {
                        Composite = repeat,
                        Kind = AbilityStepKind.Repeat,
                        Cursor = 0,
                        ChildrenTotal = repeat.MaxIterations,
                    });
                    config = repeat.Body;
                    continue;
                }

                if (config is ParallelStepConfig parallel) {
                    return StartParallel(parallel, castEntity, castGid);
                }

                return TraversalResult.NoOpSuccess();
            }

            return config == null ? TraversalResult.NoOpSuccess() : TraversalResult.ForLeaf(config);
        }

        private static TraversalResult StartParallel(ParallelStepConfig parallel, World<TWorld>.Entity castEntity, EntityGID castGid) {
            if (parallel.ChildCount == 0) {
                return TraversalResult.NoOpSuccess();
            }

            if (!castEntity.Has<World<TWorld>.Multi<AbilityParallelBranchEntry>>()) {
                castEntity.Add<World<TWorld>.Multi<AbilityParallelBranchEntry>>();
            }

            ref var branches = ref castEntity.Ref<World<TWorld>.Multi<AbilityParallelBranchEntry>>();
            branches.Clear();

            PushFrame(castEntity, new AbilityStackFrame {
                Composite = parallel,
                Kind = AbilityStepKind.Parallel,
                Cursor = 0,
                ChildrenTotal = parallel.ChildCount,
            });

            var runtime = castEntity.Read<AbilityCastRuntimeComponent>();
            var owner = ResolveOwner(castEntity, runtime.Caster);
            for (var i = 0; i < parallel.ChildCount; i++) {
                var branch = AbilityCastFactory.SpawnBranch<TWorld>(
                    castGid,
                    parallel.GetChild(i),
                    runtime.AbilityId,
                    runtime.Caster,
                    owner,
                    runtime.PrimaryTarget);
                branches.Add(new AbilityParallelBranchEntry {
                    BranchCast = branch,
                    Status = StepStatus.Running,
                    Completed = false,
                });
            }

            castEntity.Set<AbilityParallelWaitingTag>();
            return TraversalResult.RunningComposite();
        }

        private static bool ShouldRunRepeat(RepeatStepConfig repeat, World<TWorld>.Entity castEntity, EntityGID castGid) {
            return repeat.WhileCondition == null || EvaluateCondition(repeat.WhileCondition, castEntity, castGid);
        }

        private static bool EvaluateCondition(IAbilityStepCondition condition, World<TWorld>.Entity castEntity, EntityGID castGid) {
            if (condition == null) {
                return false;
            }

            var runtime = castEntity.Read<AbilityCastRuntimeComponent>();
            var ctx = new AbilityStepConditionContext<TWorld>(
                runtime.Caster,
                ResolveOwner(castEntity, runtime.Caster),
                castGid,
                runtime.PrimaryTarget,
                runtime.AbilityId);
            return condition.Evaluate(in ctx);
        }

        private static EntityGID ResolveOwner(World<TWorld>.Entity castEntity, EntityGID fallback) {
            return castEntity.Has<AbilityCastOwnerRef>() ? castEntity.Read<AbilityCastOwnerRef>().Owner : fallback;
        }

        private static bool IsComposite(AbilityStepKind kind) {
            return kind == AbilityStepKind.Sequence
                || kind == AbilityStepKind.Parallel
                || kind == AbilityStepKind.Conditional
                || kind == AbilityStepKind.Repeat;
        }

        private static void PushFrame(World<TWorld>.Entity castEntity, AbilityStackFrame frame) {
            if (!castEntity.Has<World<TWorld>.Multi<AbilityStackFrame>>()) {
                castEntity.Add<World<TWorld>.Multi<AbilityStackFrame>>();
            }
            ref var frames = ref castEntity.Ref<World<TWorld>.Multi<AbilityStackFrame>>();
            frames.Add(frame);
        }

        private static void EmitStepStarted(
            World<TWorld>.Entity castEntity,
            EntityGID castGid,
            IAbilityStepConfig leaf,
            AbilityId abilityId) {
            var startedAt = World<TWorld>.HasResource<EcsTime>()
                ? World<TWorld>.GetResource<EcsTime>().Now
                : 0f;

            if (!castEntity.Has<World<TWorld>.Multi<AbilityActiveStepEntry>>()) {
                castEntity.Add<World<TWorld>.Multi<AbilityActiveStepEntry>>();
            }
            ref var activeSteps = ref castEntity.Ref<World<TWorld>.Multi<AbilityActiveStepEntry>>();
            activeSteps.Add(new AbilityActiveStepEntry {
                NodeGuid = leaf.NodeGuid,
                Kind = leaf.Kind,
                StartedAt = startedAt,
            });

            World<TWorld>.SendEvent(new AbilityStepStartedEvent {
                CastEntity = castGid,
                AbilityId = abilityId,
                NodeGuid = leaf.NodeGuid,
                Kind = leaf.Kind,
            });
        }

        private static void EmitStepCompleted(
            World<TWorld>.Entity castEntity,
            EntityGID castGid,
            IAbilityStepConfig leaf,
            AbilityId abilityId,
            StepStatus finalStatus) {
            if (castEntity.Has<World<TWorld>.Multi<AbilityActiveStepEntry>>()) {
                ref var activeSteps = ref castEntity.Ref<World<TWorld>.Multi<AbilityActiveStepEntry>>();
                for (var i = activeSteps.Length - 1; i >= 0; i--) {
                    ref readonly var entry = ref activeSteps.Get(i);
                    if (entry.Kind == leaf.Kind && string.Equals(entry.NodeGuid, leaf.NodeGuid)) {
                        activeSteps.RemoveAtSwap(i);
                        break;
                    }
                }
            }

            World<TWorld>.SendEvent(new AbilityStepCompletedEvent {
                CastEntity = castGid,
                AbilityId = abilityId,
                NodeGuid = leaf.NodeGuid,
                Kind = leaf.Kind,
                FinalStatus = finalStatus,
            });
        }

        private static void TerminateCast(World<TWorld>.Entity castEntity, EntityGID castGid, AbilityCompletedReason reason) {
            var runtime = castEntity.Read<AbilityCastRuntimeComponent>();
            var caster = runtime.Caster;

            if (castEntity.Has<AbilityBranchSubcastTag>() && castEntity.Has<AbilityCastParentRef>()) {
                World<TWorld>.SendEvent(new AbilityBranchCompletedEvent {
                    ParentCast = castEntity.Read<AbilityCastParentRef>().Parent,
                    BranchCast = castGid,
                    AbilityId = runtime.AbilityId,
                    Status = reason == AbilityCompletedReason.Success ? StepStatus.Success : StepStatus.Failed,
                });
            }

            if (caster.TryUnpack<TWorld>(out var casterEntity) && casterEntity.Has<AbilityActiveCastRef>()) {
                var current = casterEntity.Read<AbilityActiveCastRef>().Cast;
                if (current.Equals(castGid)) {
                    casterEntity.Delete<AbilityActiveCastRef>();
                }
            }

            castEntity.Destroy();

            World<TWorld>.SendEvent(new AbilityCompletedEvent {
                Caster = caster,
                AbilityId = runtime.AbilityId,
                CastEntity = castGid,
                Reason = reason,
            });
        }

        private readonly struct TraversalResult {
            public readonly TraversalResultKind Kind;
            public readonly IAbilityStepConfig Leaf;

            private TraversalResult(TraversalResultKind kind, IAbilityStepConfig leaf) {
                Kind = kind;
                Leaf = leaf;
            }

            public static TraversalResult ForLeaf(IAbilityStepConfig leaf) {
                return new TraversalResult(TraversalResultKind.Leaf, leaf);
            }

            public static TraversalResult RunningComposite() {
                return new TraversalResult(TraversalResultKind.RunningComposite, null);
            }

            public static TraversalResult NoOpSuccess() {
                return new TraversalResult(TraversalResultKind.NoOpSuccess, null);
            }

            public static TraversalResult Complete() {
                return new TraversalResult(TraversalResultKind.Complete, null);
            }
        }

        private enum TraversalResultKind : byte {
            Leaf = 0,
            RunningComposite = 1,
            NoOpSuccess = 2,
            Complete = 3,
        }
    }
}
