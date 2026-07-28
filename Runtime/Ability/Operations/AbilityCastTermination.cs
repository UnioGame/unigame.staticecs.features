namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    internal static class AbilityCastTermination<TWorld>
        where TWorld : struct, IWorldType
    {
        internal static bool TerminateRoot(
            EntityGID castGid,
            AbilityCompletedReason reason)
        {
            return Terminate(
                castGid,
                reason,
                notifyParent: false,
                emitRootCompletion: true);
        }

        internal static bool TerminateBranch(
            EntityGID castGid,
            AbilityCompletedReason reason)
        {
            return Terminate(
                castGid,
                reason,
                notifyParent: true,
                emitRootCompletion: false);
        }

        internal static bool TerminateSilently(
            EntityGID castGid,
            AbilityCompletedReason reason)
        {
            return Terminate(
                castGid,
                reason,
                notifyParent: false,
                emitRootCompletion: false);
        }

        private static bool Terminate(
            EntityGID castGid,
            AbilityCompletedReason reason,
            bool notifyParent,
            bool emitRootCompletion)
        {
            if (!castGid.TryUnpack<TWorld>(out var castEntity) ||
                !castEntity.Has<AbilityCastComponent>())
                return false;

            var runtime = castEntity.Read<AbilityCastComponent>();
            CancelBranches(castEntity, reason);
            CancelCurrentLeaf(castEntity, castGid, runtime);

            var isBranch =
                castEntity.Has<AbilityBranchSubcastTag>() &&
                castEntity.Has<AbilityParentCastComponent>();
            if (notifyParent && isBranch)
                World<TWorld>.SendEvent(
                    new AbilityBranchCompletedEvent
                    {
                        ParentCast =
                            castEntity.Read<AbilityParentCastComponent>().Parent,
                        BranchCast = castGid,
                        AbilityId = runtime.AbilityId,
                        Status =
                            reason == AbilityCompletedReason.Success
                                ? StepStatus.Success
                                : StepStatus.Failed,
                    });

            ClearForegroundCast(runtime.Caster, castGid);
            castEntity.Destroy();

            if (emitRootCompletion && !isBranch)
                World<TWorld>.SendEvent(
                    new AbilityCompletedEvent
                    {
                        Caster = runtime.Caster,
                        AbilityId = runtime.AbilityId,
                        CastEntity = castGid,
                        Reason = reason,
                    });

            return true;
        }

        private static void CancelBranches(
            World<TWorld>.Entity castEntity,
            AbilityCompletedReason reason)
        {
            if (!castEntity.Has<World<TWorld>.Multi<AbilityBranchComponent>>())
                return;

            ref var branches =
                ref castEntity.Ref<World<TWorld>.Multi<AbilityBranchComponent>>();
            for (var i = 0; i < branches.Length; i++)
            {
                ref var branch = ref branches[i];
                if (branch.Completed)
                    continue;

                TerminateSilently(branch.BranchCast, reason);
            }
        }

        private static void CancelCurrentLeaf(
            World<TWorld>.Entity castEntity,
            EntityGID castGid,
            in AbilityCastComponent runtime)
        {
            if (!castEntity.Has<AbilityCurrentStepComponent>() ||
                !World<TWorld>.HasResource<AbilityStepActivatorRegistry<TWorld>>())
                return;

            var current = castEntity.Read<AbilityCurrentStepComponent>().Config;
            if (current == null)
                return;

            ref var activators =
                ref World<TWorld>.GetResource<AbilityStepActivatorRegistry<TWorld>>();
            if (!activators.TryResolve(current.GetType(), out var activator))
                return;

            var owner = castEntity.Has<AbilityCastOwnerComponent>()
                ? castEntity.Read<AbilityCastOwnerComponent>().Owner
                : runtime.Caster;
            var context = new AbilityStepCancelContext<TWorld>(
                runtime.Caster,
                owner,
                castGid,
                runtime.AbilityId);
            activator.Cancel(current, in context);
        }

        private static void ClearForegroundCast(EntityGID caster, EntityGID castGid)
        {
            if (!caster.TryUnpack<TWorld>(out var casterEntity) ||
                !casterEntity.Has<AbilityActiveCastComponent>())
                return;

            if (casterEntity.Read<AbilityActiveCastComponent>().Cast.Equals(castGid))
                casterEntity.Delete<AbilityActiveCastComponent>();
        }
    }
}
