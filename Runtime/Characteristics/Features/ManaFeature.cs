namespace UniGame.StaticEcs.Features
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;

    public class ManaFeature<TWorld>
        : CharacteristicFeature<TWorld, ManaCharacteristic>,
            IStaticEcsSystemsFeature<TWorld, StaticEcsUpdateSystems>
        where TWorld : struct, IWorldType
    {
        public const short DefaultRegenOrder = 0;

        private readonly short _regenOrder;
        private readonly bool _registerRegen;

        public ManaFeature(bool registerRegen = true, short regenOrder = DefaultRegenOrder)
        {
            _registerRegen = registerRegen;
            _regenOrder = regenOrder;
        }

        public override void RegisterTypes(World<TWorld>.TypeRegistrar types)
        {
            base.RegisterTypes(types);
            types.Component<ManaRegenComponent>();
        }

        public UniTask RegisterSystemsAsync(
            StaticEcsSystemsBuilder<TWorld, StaticEcsUpdateSystems> systems,
            CancellationToken cancellationToken
        )
        {
            if (!_registerRegen)
            {
                return UniTask.CompletedTask;
            }

            systems.Add(new ManaRegenSystem<TWorld>(), _regenOrder);
            return UniTask.CompletedTask;
        }
    }
}
