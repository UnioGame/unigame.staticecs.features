namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Unity;

    /// <summary>
    /// Public entry points for the ability slice. Roster equip/unequip is ability-internal;
    /// readiness checks intentionally cover only ability-internal invariants.
    /// Cooldown / mana / range / line-of-sight live in business-layer services that the caller
    /// composes ahead of <see cref="TryStartCast{TWorld}"/>.
    ///
    /// <para>
    /// <see cref="TryStartCast{TWorld}"/> queues a <see cref="CastAbilityEvent"/> consumed by
    /// <c>AbilityCastSystem</c>; the actual cast-entity is created there. <see cref="Cancel{TWorld}"/>
    /// destroys the foreground cast-entity directly; child cast-entities of a sub-ability tree
    /// are cleaned up by <c>AbilityChildCleanupSystem</c>.
    /// </para>
    /// </summary>
    public static class AbilityOperations
    {
        public static bool Equip<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType
        {
            if (!caster.TryUnpack<TWorld>(out var entity))
                return false;

            if (!entity.Has<World<TWorld>.Multi<AbilitySlotComponent>>())
                entity.Add<World<TWorld>.Multi<AbilitySlotComponent>>();

            ref var roster = ref entity.Ref<World<TWorld>.Multi<AbilitySlotComponent>>();
            for (var i = 0; i < roster.Length; i++)
            {
                if (roster[i].Id == id)
                    return false;
            }

            roster.Add(new AbilitySlotComponent { Id = id });
            return true;
        }

        public static bool Unequip<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType
        {
            if (!caster.TryUnpack<TWorld>(out var entity))
                return false;
            if (!entity.Has<World<TWorld>.Multi<AbilitySlotComponent>>())
                return false;

            ref var roster = ref entity.Ref<World<TWorld>.Multi<AbilitySlotComponent>>();
            for (var i = 0; i < roster.Length; i++)
            {
                if (roster[i].Id == id)
                {
                    roster.RemoveAtSwap(i);
                    return true;
                }
            }
            return false;
        }

        public static bool HasAbility<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType
        {
            if (!caster.TryUnpack<TWorld>(out var entity))
                return false;
            if (!entity.Has<World<TWorld>.Multi<AbilitySlotComponent>>())
                return false;

            ref readonly var roster = ref entity.Read<World<TWorld>.Multi<AbilitySlotComponent>>();
            for (var i = 0; i < roster.Length; i++)
            {
                if (roster[i].Id == id)
                    return true;
            }
            return false;
        }

        public static bool IsCasting<TWorld>(EntityGID caster)
            where TWorld : struct, IWorldType
        {
            if (!caster.TryUnpack<TWorld>(out var entity))
                return false;
            return entity.Has<AbilityActiveCastComponent>();
        }

        /// <summary>
        /// Ability-internal readiness only: caster is alive, ability is registered, caster is
        /// not already in a foreground cast, and the ability sits in the caster's roster.
        /// Cooldown / cost / range checks belong to the business layer (see plan §1b).
        /// </summary>
        public static bool IsReady<TWorld>(EntityGID caster, AbilityId id)
            where TWorld : struct, IWorldType
        {
            if (!caster.TryUnpack<TWorld>(out var entity))
                return false;
            if (!World<TWorld>.HasResource<AbilityRegistry<TWorld>>())
                return false;
            if (!World<TWorld>.GetResource<AbilityRegistry<TWorld>>().Contains(id))
                return false;
            if (entity.Has<AbilityActiveCastComponent>())
                return false;
            if (!HasAbility<TWorld>(caster, id))
                return false;
            return true;
        }

        public static bool TryStartCast<TWorld>(
            EntityGID caster,
            AbilityId id,
            EntityGID target = default
        )
            where TWorld : struct, IWorldType
        {
            if (!IsReady<TWorld>(caster, id))
                return false;

            return World<TWorld>.SendEvent(
                new CastAbilityEvent
                {
                    Caster = caster,
                    AbilityId = id,
                    Target = target,
                }
            );
        }

        /// <summary>
        /// Cancels the foreground cast on <paramref name="caster"/>. The cast-entity is
        /// destroyed (which cleans up its state components automatically); the
        /// <see cref="AbilityCompletedEvent"/> with reason Cancelled is emitted as part of the
        /// destroy path.
        /// </summary>
        public static bool Cancel<TWorld>(EntityGID caster)
            where TWorld : struct, IWorldType
        {
            if (!caster.TryUnpack<TWorld>(out var casterEntity))
                return false;
            if (!casterEntity.Has<AbilityActiveCastComponent>())
                return false;

            var castGid = casterEntity.Read<AbilityActiveCastComponent>().Cast;
            if (!castGid.TryUnpack<TWorld>(out var castEntity))
            {
                casterEntity.Delete<AbilityActiveCastComponent>();
                return false;
            }

            return AbilityCastTermination<TWorld>.TerminateRoot(
                castEntity.GID,
                AbilityCompletedReason.Cancelled);
        }

        // --- Main-default overloads ---

        public static bool Equip(EntityGID caster, AbilityId id) => Equip<Main>(caster, id);

        public static bool Unequip(EntityGID caster, AbilityId id) => Unequip<Main>(caster, id);

        public static bool HasAbility(EntityGID caster, AbilityId id) =>
            HasAbility<Main>(caster, id);

        public static bool IsCasting(EntityGID caster) => IsCasting<Main>(caster);

        public static bool IsReady(EntityGID caster, AbilityId id) => IsReady<Main>(caster, id);

        public static bool TryStartCast(
            EntityGID caster,
            AbilityId id,
            EntityGID target = default
        ) => TryStartCast<Main>(caster, id, target);

        public static bool Cancel(EntityGID caster) => Cancel<Main>(caster);
    }
}
