using FFS.Libraries.StaticEcs;

namespace UniGame.StaticEcs.Features {
    internal static class DamageCharacteristicHelper {
        public static bool TryReadValue<TWorld, TStat>(EntityGID entity, out float value)
            where TWorld : struct, IWorldType
            where TStat : struct, ICharacteristicType {
            if (!entity.TryUnpack<TWorld>(out var resolved)) {
                value = 0f;
                return false;
            }

            if (!resolved.Has<CharacteristicComponent<TStat>>()) {
                value = 0f;
                return false;
            }

            value = resolved.Read<CharacteristicComponent<TStat>>().Value;
            return true;
        }
    }
}
