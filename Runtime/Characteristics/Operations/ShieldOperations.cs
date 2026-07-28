namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    public static class ShieldOperations
    {
        public static bool Add<TWorld>(EntityGID target, float amount)
            where TWorld : struct, IWorldType
        {
            if (amount <= 0f)
                return false;

            if (!target.TryUnpack<TWorld>(out var entity))
                return false;

            if (!entity.Has<CharacteristicComponent<ShieldCharacteristic>>())
                return false;

            ref var shield = ref entity.Mut<CharacteristicComponent<ShieldCharacteristic>>();
            return CharacteristicOperations.AddValue<TWorld, ShieldCharacteristic>(
                ref shield,
                target,
                amount
            );
        }

        public static float Consume<TWorld>(EntityGID target, float amount)
            where TWorld : struct, IWorldType
        {
            if (amount <= 0f)
                return 0f;

            if (!target.TryUnpack<TWorld>(out var entity))
                return 0f;

            if (!entity.Has<CharacteristicComponent<ShieldCharacteristic>>())
                return 0f;

            ref var shield = ref entity.Mut<CharacteristicComponent<ShieldCharacteristic>>();
            var available = shield.Value;
            if (available <= 0f)
                return 0f;

            var consumed = amount > available ? available : amount;
            CharacteristicOperations.AddValue<TWorld, ShieldCharacteristic>(
                ref shield,
                target,
                -consumed
            );
            return consumed;
        }

        public static bool Break<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
                return false;

            if (!entity.Has<CharacteristicComponent<ShieldCharacteristic>>())
                return false;

            ref var shield = ref entity.Mut<CharacteristicComponent<ShieldCharacteristic>>();
            return CharacteristicOperations.SetValue<TWorld, ShieldCharacteristic>(
                ref shield,
                target,
                0f
            );
        }
    }
}
