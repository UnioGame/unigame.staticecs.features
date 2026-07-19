using FFS.Libraries.StaticEcs;
 

namespace UniGame.StaticEcs.Features {
    using Unity;

    public static class CharacteristicOperations {
        public static bool SetValue<TWorld, TCharacteristic>(
            ref CharacteristicComponent<TCharacteristic> characteristic,
            EntityGID target,
            float value)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            var previous = characteristic.Value;
            characteristic.SetValue(value);

            if (previous == characteristic.Value) {
                return false;
            }

            SendChanged<TWorld, TCharacteristic>(target, previous, in characteristic);
            return true;
        }

        public static bool AddValue<TWorld, TCharacteristic>(
            ref CharacteristicComponent<TCharacteristic> characteristic,
            EntityGID target,
            float delta)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            return SetValue<TWorld, TCharacteristic>(ref characteristic, target, characteristic.Value + delta);
        }

        public static bool SetBaseValue<TWorld, TCharacteristic>(
            ref CharacteristicComponent<TCharacteristic> characteristic,
            EntityGID target,
            float value,
            bool resetValue = false)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            var previousValue = characteristic.Value;
            var previousBase = characteristic.BaseValue;

            characteristic.SetBaseValue(value, resetValue);

            if (previousBase == characteristic.BaseValue && previousValue == characteristic.Value) {
                return false;
            }

            if (previousValue == characteristic.Value) {
                return true;
            }

            SendChanged<TWorld, TCharacteristic>(target, previousValue, in characteristic);
            return true;
        }

        public static bool SetLimits<TWorld, TCharacteristic>(
            ref CharacteristicComponent<TCharacteristic> characteristic,
            EntityGID target,
            float min,
            float max,
            bool clampCurrent = true)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            var previousValue = characteristic.Value;

            characteristic.SetLimits(min, max, clampCurrent);

            if (previousValue == characteristic.Value) {
                return false;
            }

            SendChanged<TWorld, TCharacteristic>(target, previousValue, in characteristic);
            return true;
        }

        internal static void SendChanged<TWorld, TCharacteristic>(
            EntityGID target,
            float previous,
            in CharacteristicComponent<TCharacteristic> characteristic)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            World<TWorld>.SendEvent(new CharacteristicChangedEvent<TCharacteristic> {
                Target = target,
                PreviousValue = previous,
                Value = characteristic.Value,
                BaseValue = characteristic.BaseValue,
                MinValue = characteristic.MinValue,
                MaxValue = characteristic.MaxValue
            });
        }

        // --- Main-default overloads ---

        public static bool SetValue<TCharacteristic>(
            ref CharacteristicComponent<TCharacteristic> characteristic,
            EntityGID target,
            float value)
            where TCharacteristic : struct, ICharacteristicType
            => SetValue<Main, TCharacteristic>(ref characteristic, target, value);

        public static bool AddValue<TCharacteristic>(
            ref CharacteristicComponent<TCharacteristic> characteristic,
            EntityGID target,
            float delta)
            where TCharacteristic : struct, ICharacteristicType
            => AddValue<Main, TCharacteristic>(ref characteristic, target, delta);

        public static bool SetBaseValue<TCharacteristic>(
            ref CharacteristicComponent<TCharacteristic> characteristic,
            EntityGID target,
            float value,
            bool resetValue = false)
            where TCharacteristic : struct, ICharacteristicType
            => SetBaseValue<Main, TCharacteristic>(ref characteristic, target, value, resetValue);

        public static bool SetLimits<TCharacteristic>(
            ref CharacteristicComponent<TCharacteristic> characteristic,
            EntityGID target,
            float min,
            float max,
            bool clampCurrent = true)
            where TCharacteristic : struct, ICharacteristicType
            => SetLimits<Main, TCharacteristic>(ref characteristic, target, min, max, clampCurrent);
    }
}
