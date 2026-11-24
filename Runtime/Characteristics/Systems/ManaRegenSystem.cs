namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Time;

    /// <summary>
    /// Restores mana by the mana regeneration characteristic, in mana per second of <see cref="EcsTime"/>.
    /// Entities with <see cref="ManaRegenBlockedTag"/> do not regenerate.
    /// </summary>
    public class ManaRegenSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            var step = World<TWorld>.GetResource<EcsTime>().DeltaTime;
            if (step <= 0f || !float.IsFinite(step))
                return;

            // FFSECS0050 collapses distinct closed CharacteristicComponent<T> filters to one generic definition.
#pragma warning disable FFSECS0050
            var regenerating = default(And<All<CharacteristicComponent<ManaCharacteristic>, CharacteristicComponent<ManaRegenCharacteristic>>, None<ManaRegenBlockedTag>>);
            foreach (var entity in World<TWorld>.Query(regenerating).Entities())
#pragma warning restore FFSECS0050
            {
                var regen = entity.Read<CharacteristicComponent<ManaRegenCharacteristic>>().Value;
                if (!float.IsFinite(regen) || regen <= 0f)
                    continue;

                // The component is stored only after a real change, so a full mana pool is not marked changed.
                var mana = entity.Read<CharacteristicComponent<ManaCharacteristic>>();
                if (CharacteristicOperations.AddValue<TWorld, ManaCharacteristic>(
                        ref mana,
                        entity.GID,
                        regen * step))
                    entity.Set(mana);
            }
        }
    }
}
