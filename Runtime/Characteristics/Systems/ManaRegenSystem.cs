namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Time;

    public sealed class ManaRegenSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            var dt = World<TWorld>.GetResource<EcsTime>().DeltaTime;
            if (dt <= 0f)
            {
                return;
            }

            foreach (
                var entity in World<TWorld>
                    .Query<All<CharacteristicComponent<ManaCharacteristic>, ManaRegenComponent>>()
                    .Entities()
            )
            {
                ref readonly var regen = ref entity.Read<ManaRegenComponent>();
                if (regen.Rate == 0f)
                {
                    continue;
                }

                ref var mana = ref entity.Mut<CharacteristicComponent<ManaCharacteristic>>();
                CharacteristicOperations.AddValue<TWorld, ManaCharacteristic>(
                    ref mana,
                    entity.GID,
                    regen.Rate * dt
                );
            }
        }
    }
}
