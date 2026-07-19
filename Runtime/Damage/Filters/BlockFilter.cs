using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    /// <summary>
    /// Cancels the damage event when the target carries <see cref="BlockableTag"/> and a roll
    /// against <see cref="BlockChanceCharacteristic"/> succeeds. Healing events bypass the filter.
    /// </summary>
    public sealed class BlockFilter<TWorld> : IDamageFilter<TWorld>
        where TWorld : struct, IWorldType {
        public void Apply(ref DamageContext ctx) {
            if (ctx.Cancelled || ctx.Type == DamageType.Healing) {
                return;
            }

            if (!ctx.Target.TryUnpack<TWorld>(out var target)) {
                return;
            }

            if (!target.Has<BlockableTag>()) {
                return;
            }

            if (!target.Has<CharacteristicComponent<BlockChanceCharacteristic>>()) {
                return;
            }

            var chance = target.Read<CharacteristicComponent<BlockChanceCharacteristic>>().Value;
            if (chance <= 0f) {
                return;
            }

            ref var rng = ref World<TWorld>.GetResource<IDamageRng>();
            if (!rng.RollChance(chance)) {
                return;
            }

            ctx.Cancelled = true;
            ctx.CancelReason = DamageCancelReason.Blocked;
        }
    }
}
