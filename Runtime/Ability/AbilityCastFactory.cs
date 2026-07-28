namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    internal static class AbilityCastFactory
    {
        public static EntityGID SpawnRoot<TWorld>(
            EntityGID casterGid,
            AbilityId abilityId,
            EntityGID target
        )
            where TWorld : struct, IWorldType
        {
            var castEntity = SpawnBase<TWorld>(casterGid, casterGid, abilityId, target);

            if (casterGid.TryUnpack<TWorld>(out var casterEntity))
                casterEntity.Set(new AbilityActiveCastComponent { Cast = castEntity.GID });

            World<TWorld>.SendEvent(
                new AbilityStartedEvent
                {
                    Caster = casterGid,
                    AbilityId = abilityId,
                    CastEntity = castEntity.GID,
                }
            );

            return castEntity.GID;
        }

        public static EntityGID SpawnBranch<TWorld>(
            EntityGID parentCast,
            IAbilityStepConfig root,
            AbilityId abilityId,
            EntityGID caster,
            EntityGID owner,
            EntityGID primaryTarget
        )
            where TWorld : struct, IWorldType
        {
            var castEntity = SpawnBase<TWorld>(caster, owner, abilityId, primaryTarget);
            castEntity.Set(new AbilityParentCastComponent { Parent = parentCast });
            castEntity.Set(new AbilityRootComponent { Root = root });
            castEntity.Set<AbilityBranchSubcastTag>();
            return castEntity.GID;
        }

        private static World<TWorld>.Entity SpawnBase<TWorld>(
            EntityGID caster,
            EntityGID owner,
            AbilityId abilityId,
            EntityGID primaryTarget
        )
            where TWorld : struct, IWorldType
        {
            var castEntity = World<TWorld>.NewEntity<Default>();

            castEntity.Set(
                new AbilityCastComponent
                {
                    AbilityId = abilityId,
                    Caster = caster,
                    PrimaryTarget = primaryTarget,
                    RootEntered = false,
                }
            );
            castEntity.Set(new AbilityCastOwnerComponent { Owner = owner });
            castEntity.Set(new AbilityStepStatusComponent { Status = StepStatus.Success });
            castEntity.Set<AbilityStepReadyTag>();
            return castEntity;
        }
    }
}
