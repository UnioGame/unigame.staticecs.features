namespace UniGame.StaticEcs.Features
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;

    /// <summary>Composes the standard heal-over-time, stun, and speed-modification effects.</summary>
    public class EffectsFeature<TWorld>
        : StaticEcsFeature<TWorld>,
            IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
    {
        /// <inheritdoc />
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            new HealOverTimeFeature<TWorld>().RegisterTypes(types);
            new StunEffectFeature<TWorld>().RegisterTypes(types);
            new ModificationEffectFeature<TWorld, SpeedCharacteristic>().RegisterTypes(types);
        }

        /// <inheritdoc />
        public async UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken
        )
        {
            await new HealOverTimeFeature<TWorld>().RegisterSystemsAsync(systems, cancellationToken);
            await new StunEffectFeature<TWorld>().RegisterSystemsAsync(systems, cancellationToken);
            await new ModificationEffectFeature<TWorld, SpeedCharacteristic>()
                .RegisterSystemsAsync(systems, cancellationToken);
        }
    }
}
