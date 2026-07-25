namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using Modifiers;
    using UniGame.Core.Runtime;

    /// <summary>Composes the standard gameplay characteristics and mana regeneration.</summary>
    public class CharacteristicsFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        /// <summary>Whether the standard mana regeneration system is installed.</summary>
        public bool registerManaRegen = true;

        /// <summary>Execution order of mana regeneration.</summary>
        public short manaRegenOrder = ManaFeature<TWorld>.DefaultRegenOrder;

        /// <inheritdoc />
        public override async UniTask InitializeAsync(ILifeTime lifeTime)
        {
            if (!World<TWorld>.HasResource<ModifierRegistry>())
            {
                var registry = new ModifierRegistry();
                World<TWorld>.SetResource(registry);
            }

            if (!World<TWorld>.HasResource<ManaRegenConfig>())
            {
                var config = new ManaRegenConfig
                {
                    RegisterRegen = registerManaRegen,
                    RegenOrder = manaRegenOrder,
                };

                World<TWorld>.SetResource(config);
            }

            await new ModifierBackRefFeature<TWorld>().InitializeAsync(lifeTime);
            await new HealthFeature<TWorld>().InitializeAsync(lifeTime);
            await new ManaFeature<TWorld>().InitializeAsync(lifeTime);
            await new SpeedFeature<TWorld>().InitializeAsync(lifeTime);
            await new ShieldFeature<TWorld>().InitializeAsync(lifeTime);
            await new CharacteristicFeature<TWorld, BlockChanceCharacteristic>()
                .InitializeAsync(lifeTime);
            await new CharacteristicFeature<TWorld, DodgeChanceCharacteristic>()
                .InitializeAsync(lifeTime);
            await new CharacteristicFeature<TWorld, ArmorResistCharacteristic>()
                .InitializeAsync(lifeTime);
            await new CharacteristicFeature<TWorld, CriticalChanceCharacteristic>()
                .InitializeAsync(lifeTime);
            await new CharacteristicFeature<TWorld, CriticalMultiplierCharacteristic>()
                .InitializeAsync(lifeTime);

        }
    }
}
