using System.Threading;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features
{
    /// <summary>Composes the standard heal-over-time, stun, and speed-modification effects.</summary>
    public class EffectsFeature<TWorld> :
        StaticEcsFeature<TWorld>,
        IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
    {
        private readonly HealOverTimeFeature<TWorld> _heal = new();
        private readonly StunEffectFeature<TWorld> _stun = new();
        private readonly ModificationEffectFeature<TWorld, SpeedCharacteristic> _speed = new();

        /// <inheritdoc />
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            _heal.RegisterTypes(types);
            _stun.RegisterTypes(types);
            _speed.RegisterTypes(types);
        }

        /// <inheritdoc />
        public async UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken)
        {
            await _heal.RegisterSystemsAsync(systems, cancellationToken);
            await _stun.RegisterSystemsAsync(systems, cancellationToken);
            await _speed.RegisterSystemsAsync(systems, cancellationToken);
        }
    }
}
