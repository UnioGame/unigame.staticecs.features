namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;

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
            var updateEnabled =
                World<TWorld>.HasResource<Unity.StaticEcsSystemsConfig>() &&
                World<TWorld>.GetResource<Unity.StaticEcsSystemsConfig>().update;
            if (!updateEnabled || !config.RegisterRegen)
            {
                return;
            }

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
