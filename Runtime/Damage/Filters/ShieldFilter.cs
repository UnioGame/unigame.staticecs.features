namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Absorbs incoming damage with the target's shield pool before it reaches health. Mutates
    /// the shield characteristic in place and emits <see cref="ShieldDeltaEvent"/> when any
    /// amount is absorbed.
    /// </summary>
    public sealed class ShieldFilter<TWorld> : IDamageFilter<TWorld>
        where TWorld : struct, IWorldType
    {
        public void Apply(ref DamageContext ctx)
        {
            if (ctx.Cancelled || ctx.Type == DamageType.Healing || ctx.Amount <= 0f)
            {
                return;
            }

            if (!ctx.Target.TryUnpack<TWorld>(out var target))
            {
                return;
            }

            if (!target.Has<CharacteristicComponent<ShieldCharacteristic>>())
            {
                return;
            }

            ref var shield = ref target.Ref<CharacteristicComponent<ShieldCharacteristic>>();
            if (shield.Value <= 0f)
            {
                return;
            }

            var absorbed = ctx.Amount < shield.Value ? ctx.Amount : shield.Value;
            CharacteristicOperations.SetValue<TWorld, ShieldCharacteristic>(
                ref shield,
                ctx.Target,
                shield.Value - absorbed
            );
            ctx.Amount -= absorbed;
            ctx.ShieldAbsorbed += absorbed;

            World<TWorld>.SendEvent(
                new ShieldDeltaEvent
                {
                    Target = ctx.Target,
                    Absorbed = absorbed,
                    ShieldRemaining = shield.Value,
                }
            );
        }
    }
}
