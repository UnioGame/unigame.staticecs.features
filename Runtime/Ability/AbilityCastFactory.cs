using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    internal static class AbilityCastFactory {
        public static EntityGID SpawnRoot<TWorld>(EntityGID casterGid, AbilityId abilityId, EntityGID target)
            where TWorld : struct, IWorldType {
            var castEntity = SpawnBase<TWorld>(casterGid, casterGid, abilityId, target);

            if (casterGid.TryUnpack<TWorld>(out var casterEntity)) {
                casterEntity.Set(new AbilityActiveCastRef {
                    Cast = castEntity.GID,
                });
            }

            World<TWorld>.SendEvent(new AbilityStartedEvent {
                Caster = casterGid,
                AbilityId = abilityId,
                CastEntity = castEntity.GID,
            });

            return castEntity.GID;
        }

        public static EntityGID SpawnBranch<TWorld>(
            EntityGID parentCast,
            IAbilityStepConfig root,
            AbilityId abilityId,
            EntityGID caster,
            EntityGID owner,
            EntityGID primaryTarget)
            where TWorld : struct, IWorldType {
            var castEntity = SpawnBase<TWorld>(caster, owner, abilityId, primaryTarget);
            castEntity.Set(new AbilityCastParentRef { Parent = parentCast });
            castEntity.Set(new AbilityInlineRootConfig { Root = root });
            castEntity.Set<AbilityBranchSubcastTag>();
            return castEntity.GID;
        }

        private static World<TWorld>.Entity SpawnBase<TWorld>(
            EntityGID caster,
            EntityGID owner,
            AbilityId abilityId,
            EntityGID primaryTarget)
            where TWorld : struct, IWorldType {
            var castEntity = World<TWorld>.NewEntity<Default>();

            castEntity.Set(new AbilityCastRuntimeComponent {
                AbilityId = abilityId,
                Caster = caster,
                PrimaryTarget = primaryTarget,
                RootEntered = false,
            });
            castEntity.Set(new AbilityCastOwnerRef {
                Owner = owner,
            });
            castEntity.Set(new AbilityStepLastStatus {
                Status = StepStatus.Success,
            });
            castEntity.Set<AbilityStepReadyTag>();
            return castEntity;
        }
    }
}
