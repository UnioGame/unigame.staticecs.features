using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Context passed to a leaf step activator. <see cref="CastEntity"/> hosts every per-cast
    /// state component (wait timers, AoE buffer, stack frames). <see cref="Owner"/> equals
    /// <see cref="Caster"/> for root casts and is inherited from the parent for sub-ability casts.
    /// </summary>
    public readonly ref struct AbilityStepActivationContext<TWorld>
        where TWorld : struct, IWorldType {
        public readonly EntityGID Caster;
        public readonly EntityGID Owner;
        public readonly EntityGID CastEntity;
        public readonly EntityGID PrimaryTarget;
        public readonly AbilityId AbilityId;

        public AbilityStepActivationContext(
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

    public readonly ref struct AbilityStepCancelContext<TWorld>
        where TWorld : struct, IWorldType {
        public readonly EntityGID Caster;
        public readonly EntityGID Owner;
        public readonly EntityGID CastEntity;
        public readonly AbilityId AbilityId;

        public AbilityStepCancelContext(
            EntityGID caster,
            EntityGID owner,
            EntityGID castEntity,
            AbilityId abilityId) {
            Caster = caster;
            Owner = owner;
            CastEntity = castEntity;
            AbilityId = abilityId;
        }
    }
}
