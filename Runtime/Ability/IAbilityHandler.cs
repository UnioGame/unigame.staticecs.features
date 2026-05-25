using System;
using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Strongly-typed callback invoked by <c>AbilityTickSystem</c> when an ability's cast resolves.
    /// Implement this when a registration needs persistent state, allocations-free closures, or
    /// per-ability data; for one-off scripts prefer the delegate overload of
    /// <see cref="AbilityRegistry{TWorld}.Register(AbilityDefinition, AbilityCastDelegate)"/>.
    /// The <paramref name="targets"/> span is owned by the system, do not store it.
    /// </summary>
    public interface IAbilityHandler<TWorld> where TWorld : struct, IWorldType {
        void OnCast(EntityGID caster, ReadOnlySpan<EntityGID> targets);
    }

    public delegate void AbilityCastDelegate(EntityGID caster, ReadOnlySpan<EntityGID> targets);
}
