using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    public static class SpeedOperations {
        public static float Read<TWorld>(EntityGID target, float fallback = 0f)
            where TWorld : struct, IWorldType {
            if (!target.TryUnpack<TWorld>(out var entity)) {
                return fallback;
            }

            if (!entity.Has<CharacteristicComponent<SpeedCharacteristic>>()) {
                return fallback;
            }

            return entity.Read<CharacteristicComponent<SpeedCharacteristic>>().Value;
        }

        public static bool SetBase<TWorld>(EntityGID target, float baseValue, bool resetCurrent = false)
            where TWorld : struct, IWorldType {
            if (!target.TryUnpack<TWorld>(out var entity)) {
                return false;
            }

            if (!entity.Has<CharacteristicComponent<SpeedCharacteristic>>()) {
                return false;
            }

            ref var speed = ref entity.Mut<CharacteristicComponent<SpeedCharacteristic>>();
            speed.SetBaseValue(baseValue, resetCurrent);
            if (resetCurrent) {
                CharacteristicOperations.SetValue<TWorld, SpeedCharacteristic>(ref speed, target, speed.Value);
            }

            return true;
        }

        public static bool SetValue<TWorld>(EntityGID target, float value)
            where TWorld : struct, IWorldType {
            if (!target.TryUnpack<TWorld>(out var entity)) {
                return false;
            }

            if (!entity.Has<CharacteristicComponent<SpeedCharacteristic>>()) {
                return false;
            }

            ref var speed = ref entity.Mut<CharacteristicComponent<SpeedCharacteristic>>();
            return CharacteristicOperations.SetValue<TWorld, SpeedCharacteristic>(ref speed, target, value);
        }

        public static bool ResetToBase<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType {
            if (!target.TryUnpack<TWorld>(out var entity)) {
                return false;
            }

            if (!entity.Has<CharacteristicComponent<SpeedCharacteristic>>()) {
                return false;
            }

            ref var speed = ref entity.Mut<CharacteristicComponent<SpeedCharacteristic>>();
            return CharacteristicOperations.SetValue<TWorld, SpeedCharacteristic>(ref speed, target, speed.BaseValue);
        }
    }
}
