namespace UniGame.StaticEcs.Features
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;

    /// <summary>Composes the standard gameplay characteristics and mana regeneration.</summary>
    public class CharacteristicsFeature<TWorld>
        : StaticEcsFeature<TWorld>,
            IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
    {
        /// <summary>Whether mana regeneration is installed in the update group.</summary>
        public bool registerManaRegen = true;

        /// <summary>Execution order of mana regeneration.</summary>
        public short manaRegenOrder = ManaFeature<TWorld>.DefaultRegenOrder;

        /// <summary>Creates the standard characteristics feature.</summary>
        public CharacteristicsFeature(
            bool registerManaRegen = true,
            short manaRegenOrder = ManaFeature<TWorld>.DefaultRegenOrder
        )
        {
            this.registerManaRegen = registerManaRegen;
            this.manaRegenOrder = manaRegenOrder;
        }

        /// <inheritdoc />
        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            new ModifierBackRefFeature<TWorld>().RegisterTypes(types);
            new HealthFeature<TWorld>().RegisterTypes(types);
            new ManaFeature<TWorld>(registerManaRegen, manaRegenOrder).RegisterTypes(types);
            new SpeedFeature<TWorld>().RegisterTypes(types);
            new ShieldFeature<TWorld>().RegisterTypes(types);
            new CharacteristicFeature<TWorld, BlockChanceCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TWorld, DodgeChanceCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TWorld, ArmorResistCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TWorld, CriticalChanceCharacteristic>().RegisterTypes(types);
            new CharacteristicFeature<TWorld, CriticalMultiplierCharacteristic>().RegisterTypes(
                types
            );
        }

        /// <inheritdoc />
        public UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken
        )
        {
            return new ManaFeature<TWorld>(registerManaRegen, manaRegenOrder)
                .RegisterSystemsAsync(systems, cancellationToken);
        }
    }
}
