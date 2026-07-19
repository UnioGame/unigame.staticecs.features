using System.Threading;
using Cysharp.Threading.Tasks;
using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features
{
    /// <summary>Composes the standard gameplay characteristics and mana regeneration.</summary>
    public class CharacteristicsFeature<TWorld> :
        StaticEcsFeature<TWorld>,
        IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
    {
        private readonly ManaFeature<TWorld> _mana;

        /// <summary>Creates the standard characteristics feature.</summary>
        public CharacteristicsFeature(bool registerManaRegen = true, short manaRegenOrder = ManaFeature<TWorld>.DefaultRegenOrder)
        {
            _mana = new ManaFeature<TWorld>(registerManaRegen, manaRegenOrder);
        }

        /// <inheritdoc />
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            new ModifierBackRefFeature<TWorld>().RegisterTypes(types);
            new HealthFeature<TWorld>().RegisterTypes(types);
            _mana.RegisterTypes(types);
            new SpeedFeature<TWorld>().RegisterTypes(types);
            new ShieldFeature<TWorld>().RegisterTypes(types);
            new CharacteristicFeature<TWorld, BlockChanceCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TWorld, DodgeChanceCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TWorld, ArmorResistCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TWorld, CriticalChanceCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TWorld, CriticalMultiplierCharacteristic>().RegisterTypes(types);
        }

        /// <inheritdoc />
        public UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken)
        {
            return _mana.RegisterSystemsAsync(systems, cancellationToken);
        }
    }
}
