namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    public static class ManaOperations
    {
        public static bool TrySpend<TWorld>(EntityGID target, float amount)
            where TWorld : struct, IWorldType
        {
            if (amount <= 0f)
                return false;

            if (!target.TryUnpack<TWorld>(out var entity))
                return false;

            if (!entity.Has<CharacteristicComponent<ManaCharacteristic>>())
                return false;

            ref var mana = ref entity.Mut<CharacteristicComponent<ManaCharacteristic>>();
            if (mana.Value < amount)
                return false;

            CharacteristicOperations.AddValue<TWorld, ManaCharacteristic>(
                ref mana,
                target,
                -amount
            );
            return true;
        }

        public static bool Restore<TWorld>(EntityGID target, float amount)
            where TWorld : struct, IWorldType
        {
            if (amount <= 0f)
                return false;

            if (!target.TryUnpack<TWorld>(out var entity))
                return false;

            if (!entity.Has<CharacteristicComponent<ManaCharacteristic>>())
                return false;

            ref var mana = ref entity.Mut<CharacteristicComponent<ManaCharacteristic>>();
            return CharacteristicOperations.AddValue<TWorld, ManaCharacteristic>(
                ref mana,
                target,
                amount
            );
        }

        public static void SetRegenRate<TWorld>(EntityGID target, float rate)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return;

            if (entity.Has<ManaRegenComponent>())
            {
                ref var regen = ref entity.Mut<ManaRegenComponent>();
                regen.Rate = rate;
                return;
            }

            entity.Set(new ManaRegenComponent { Rate = rate });
        }
    }
}
