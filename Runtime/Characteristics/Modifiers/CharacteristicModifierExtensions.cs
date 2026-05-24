using FFS.Libraries.StaticEcs;
using unigame.staticecs.unity;

namespace unigame.staticecs.features {
    public static class CharacteristicModifierExtensions {
        public static bool ApplyModifier<TWorld, TCharacteristic>(
            EntityGID target,
            EntityGID source,
            CharacteristicModifierOp op,
            float value)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            if (!target.TryUnpack<TWorld>(out var targetEntity)) {
                return false;
            }

            ref var entries = ref EnsureModifierStorage<TWorld, TCharacteristic>(targetEntity);
            entries.Add(new CharacteristicModifierEntry<TCharacteristic> {
                Source = (EntityGIDCompact)source,
                Op = op,
                Value = value
            });

            ModifierBackRefRegistrar.Register<TWorld>(source, target, CharacteristicFlagOf<TCharacteristic>.Value);

            RecomputeValueInternal<TWorld, TCharacteristic>(target, targetEntity);
            return true;
        }

        public static int RemoveModifiersFromSource<TWorld, TCharacteristic>(
            EntityGID target,
            EntityGID source)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            if (!target.TryUnpack<TWorld>(out var targetEntity)) {
                return 0;
            }

            if (!targetEntity.Has<World<TWorld>.Multi<CharacteristicModifierEntry<TCharacteristic>>>()) {
                return 0;
            }

            ref var entries = ref targetEntity.Ref<World<TWorld>.Multi<CharacteristicModifierEntry<TCharacteristic>>>();
            if (entries.IsEmpty) {
                return 0;
            }

            var compactSource = (EntityGIDCompact)source;
            var removed = 0;

            for (var i = entries.Length - 1; i >= 0; i--) {
                if (entries[i].Source.Equals(compactSource)) {
                    entries.RemoveAtSwap(i);
                    removed++;
                }
            }

            if (removed > 0) {
                RecomputeValueInternal<TWorld, TCharacteristic>(target, targetEntity);
            }

            return removed;
        }

        public static bool RemoveModifierAt<TWorld, TCharacteristic>(EntityGID target, int index)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            if (!target.TryUnpack<TWorld>(out var targetEntity)) {
                return false;
            }

            if (!targetEntity.Has<World<TWorld>.Multi<CharacteristicModifierEntry<TCharacteristic>>>()) {
                return false;
            }

            ref var entries = ref targetEntity.Ref<World<TWorld>.Multi<CharacteristicModifierEntry<TCharacteristic>>>();
            if (index < 0 || index >= entries.Length) {
                return false;
            }

            entries.RemoveAtSwap(index);
            RecomputeValueInternal<TWorld, TCharacteristic>(target, targetEntity);
            return true;
        }

        public static bool RecomputeValue<TWorld, TCharacteristic>(EntityGID target)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            if (!target.TryUnpack<TWorld>(out var targetEntity)) {
                return false;
            }

            return RecomputeValueInternal<TWorld, TCharacteristic>(target, targetEntity);
        }

        private static bool RecomputeValueInternal<TWorld, TCharacteristic>(
            EntityGID target,
            World<TWorld>.Entity targetEntity)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            if (!targetEntity.Has<CharacteristicComponent<TCharacteristic>>()) {
                return false;
            }

            ref var characteristic = ref targetEntity.Ref<CharacteristicComponent<TCharacteristic>>();
            var previous = characteristic.Value;
            var newValue = ComputeFromModifiers<TWorld, TCharacteristic>(targetEntity, characteristic.BaseValue);

            characteristic.SetValue(newValue);

            if (previous == characteristic.Value) {
                return false;
            }

            CharacteristicOperations.SendChanged<TWorld, TCharacteristic>(target, previous, in characteristic);
            return true;
        }

        private static float ComputeFromModifiers<TWorld, TCharacteristic>(
            World<TWorld>.Entity targetEntity,
            float baseValue)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            if (!targetEntity.Has<World<TWorld>.Multi<CharacteristicModifierEntry<TCharacteristic>>>()) {
                return baseValue;
            }

            ref var entries = ref targetEntity.Ref<World<TWorld>.Multi<CharacteristicModifierEntry<TCharacteristic>>>();
            if (entries.IsEmpty) {
                return baseValue;
            }

            var addSum = 0f;
            var mulProduct = 1f;
            var hasOverride = false;
            var overrideValue = 0f;

            for (var i = 0; i < entries.Length; i++) {
                ref var entry = ref entries[i];
                switch (entry.Op) {
                    case CharacteristicModifierOp.Add:
                        addSum += entry.Value;
                        break;
                    case CharacteristicModifierOp.Mul:
                        mulProduct *= entry.Value;
                        break;
                    case CharacteristicModifierOp.Override:
                        hasOverride = true;
                        overrideValue = entry.Value;
                        break;
                }
            }

            if (hasOverride) {
                return overrideValue;
            }

            return (baseValue + addSum) * mulProduct;
        }

        private static ref World<TWorld>.Multi<CharacteristicModifierEntry<TCharacteristic>>
            EnsureModifierStorage<TWorld, TCharacteristic>(World<TWorld>.Entity targetEntity)
            where TWorld : struct, IWorldType
            where TCharacteristic : struct, ICharacteristicType {
            if (!targetEntity.Has<World<TWorld>.Multi<CharacteristicModifierEntry<TCharacteristic>>>()) {
                targetEntity.Add<World<TWorld>.Multi<CharacteristicModifierEntry<TCharacteristic>>>();
            }

            return ref targetEntity.Ref<World<TWorld>.Multi<CharacteristicModifierEntry<TCharacteristic>>>();
        }

        // --- Main-default overloads ---

        public static bool ApplyModifier<TCharacteristic>(
            EntityGID target,
            EntityGID source,
            CharacteristicModifierOp op,
            float value)
            where TCharacteristic : struct, ICharacteristicType
            => ApplyModifier<Main, TCharacteristic>(target, source, op, value);

        public static int RemoveModifiersFromSource<TCharacteristic>(EntityGID target, EntityGID source)
            where TCharacteristic : struct, ICharacteristicType
            => RemoveModifiersFromSource<Main, TCharacteristic>(target, source);

        public static bool RemoveModifierAt<TCharacteristic>(EntityGID target, int index)
            where TCharacteristic : struct, ICharacteristicType
            => RemoveModifierAt<Main, TCharacteristic>(target, index);

        public static bool RecomputeValue<TCharacteristic>(EntityGID target)
            where TCharacteristic : struct, ICharacteristicType
            => RecomputeValue<Main, TCharacteristic>(target);
    }
}
