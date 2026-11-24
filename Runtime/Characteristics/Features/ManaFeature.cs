namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using Time;
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs.Unity;

    /// <summary>Mana characteristic with the optional mana regeneration system.</summary>
    /// <remarks>Regeneration requires <c>EcsTimeFeature</c> in the same world.</remarks>
    public class ManaFeature<TWorld>
        : CharacteristicFeature<TWorld, ManaCharacteristic>
        where TWorld : struct, IWorldType
    {
        public const short DefaultRegenOrder = 0;

        /// <inheritdoc />
        public override async UniTask InitializeAsync(ILifeTime lifeTime)
        {
            await base.InitializeAsync(lifeTime);
            if (!World<TWorld>.HasResource<ManaRegenConfig>())
            {
                var defaultConfig = new ManaRegenConfig();
                World<TWorld>.SetResource(defaultConfig);
            }

            var config = World<TWorld>.GetResource<ManaRegenConfig>();
            if (!config.RegisterRegen)
                return;

            World<TWorld>.Resource<EcsTime> time = default;
            await time.GetAsync(lifeTime);

            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new ManaRegenSystem<TWorld>(),
                config.RegenOrder);
        }
    }

    /// <summary>Controls mana regeneration system composition.</summary>
    public sealed class ManaRegenConfig : IResource
    {
        /// <summary>Whether mana regeneration is installed.</summary>
        public bool RegisterRegen = true;

        /// <summary>Execution order of mana regeneration.</summary>
        public short RegenOrder;
    }
}
