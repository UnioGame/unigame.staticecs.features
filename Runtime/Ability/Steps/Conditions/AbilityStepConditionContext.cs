using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    public readonly ref struct AbilityStepConditionContext<TWorld>
        where TWorld : struct, IWorldType {
        public readonly EntityGID Caster;
        public readonly EntityGID Owner;
        public readonly EntityGID CastEntity;
        public readonly EntityGID PrimaryTarget;
        public readonly AbilityId AbilityId;

        public AbilityStepConditionContext(
            EntityGID caster,
            EntityGID owner,
            EntityGID castEntity,
            EntityGID primaryTarget,
            AbilityId abilityId) {
            Caster = caster;
            Owner = owner;
            CastEntity = castEntity;
            PrimaryTarget = primaryTarget;
            AbilityId = abilityId;
        }
    }
}
