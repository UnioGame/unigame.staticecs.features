[assembly: UniGame.StaticEcs.Unity.StaticEcsTypeRegistrar(
    typeof(UniGame.StaticEcs.Features.EffectsClosedGenericTypes))]

namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;
    using UnityEngine.Scripting;

    [Preserve]
    internal sealed class EffectsClosedGenericTypes : IStaticEcsTypeRegistrar<Main>
    {
        public void Register(World<Main>.TypeRegistrar types)
        {
            EffectTypeRegistration.Register<HealOverTimeEffect>(types);
            EffectTypeRegistration.Register<StunEffect>(types);
            EffectTypeRegistration.Register<ModificationEffect<SpeedCharacteristic>>(types);
            types.Component<ModificationEffectComponent<SpeedCharacteristic>>();
        }
    }

    /// <summary>Registers the closed ECS types owned by one effect marker.</summary>
    public static class EffectTypeRegistration
    {
        /// <summary>Registers one effect for a custom world.</summary>
        public static void Register<TWorld, TEffect>(
            World<TWorld>.TypeRegistrar types)
            where TWorld : struct, IWorldType
            where TEffect : struct, IEffectType
        {
            types.Component<EffectComponent<TEffect>>();
            types.Component<PendingEffectComponent<TEffect>>();
            types.Event<EffectAppliedEvent<TEffect>>();
            types.Event<EffectRefreshedEvent<TEffect>>();
            types.Event<EffectRemovedEvent<TEffect>>();
        }

        // --- Main-default overloads ---

        /// <summary>Registers one effect for the Main world.</summary>
        public static void Register<TEffect>(
            World<Main>.TypeRegistrar types)
            where TEffect : struct, IEffectType
        {
            Register<Main, TEffect>(types);
        }
    }
}
