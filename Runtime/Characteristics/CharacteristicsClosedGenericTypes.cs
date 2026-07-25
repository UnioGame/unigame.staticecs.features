[assembly: UniGame.StaticEcs.Unity.StaticEcsTypeRegistrar(
    typeof(UniGame.StaticEcs.Features.CharacteristicsClosedGenericTypes))]

namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UnityEngine.Scripting;

    [Preserve]
    internal sealed class CharacteristicsClosedGenericTypes :
        IStaticEcsTypeRegistrar<Main>
    {
        public void Register(World<Main>.TypeRegistrar types)
        {
            CharacteristicTypeRegistration.Register<HealthCharacteristic>(types);
            CharacteristicTypeRegistration.Register<ManaCharacteristic>(types);
            CharacteristicTypeRegistration.Register<SpeedCharacteristic>(types);
            CharacteristicTypeRegistration.Register<ShieldCharacteristic>(types);
            CharacteristicTypeRegistration.Register<BlockChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<DodgeChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<ArmorResistCharacteristic>(types);
            CharacteristicTypeRegistration.Register<CriticalChanceCharacteristic>(types);
            CharacteristicTypeRegistration.Register<CriticalMultiplierCharacteristic>(types);
        }
    }

    /// <summary>Registers the closed ECS types owned by one characteristic marker.</summary>
    public static class CharacteristicTypeRegistration
    {
        /// <summary>Registers one characteristic for a custom world.</summary>
        public static void Register<TWorld, TCharacteristic>(
            World<TWorld>.TypeRegistrar types)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType
        {
            types.Component<CharacteristicComponent<TCharacteristic>>();
            types.Event<CharacteristicChangedEvent<TCharacteristic>>();
            types.Multi<CharacteristicModifierComponent<TCharacteristic>>();
        }

        // --- Main-default overloads ---

        /// <summary>Registers one characteristic for the Main world.</summary>
        public static void Register<TCharacteristic>(
            World<Main>.TypeRegistrar types)
            where TCharacteristic : struct, ICharacteristicType
        {
            Register<Main, TCharacteristic>(types);
        }
    }
}
