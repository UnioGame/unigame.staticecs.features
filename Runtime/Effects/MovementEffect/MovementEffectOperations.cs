namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;

    /// <summary>Places, reads and drops the <see cref="MovementEffectComponent"/> of an entity.</summary>
    public static class MovementEffectOperations
    {
        public const float MinDirectionSqrMagnitude = 1e-6f;

        public static bool Has<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            return target.TryUnpack<TWorld>(out var entity) && entity.Has<MovementEffectComponent>();
        }

        /// <summary>Normalizes a planar direction; false when it is not finite or too short to point anywhere.</summary>
        public static bool TryNormalize(float directionX, float directionZ, out float x, out float z)
        {
            x = 0f;
            z = 0f;
            var sqrLength = directionX * directionX + directionZ * directionZ;
            if (!float.IsFinite(sqrLength) || sqrLength <= MinDirectionSqrMagnitude)
                return false;

            var inverseLength = 1f / (float)System.Math.Sqrt(sqrLength);
            x = directionX * inverseLength;
            z = directionZ * inverseLength;
            return true;
        }

        /// <summary>Stores the normalized direction on the target; false for a missing target or no direction.</summary>
        public static bool Set<TWorld>(EntityGID target, float directionX, float directionZ)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity) ||
                !TryNormalize(directionX, directionZ, out var x, out var z))
                return false;

            entity.Set(new MovementEffectComponent { DirectionX = x, DirectionZ = z });
            return true;
        }

        public static void Clear<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            if (target.TryUnpack<TWorld>(out var entity) && entity.Has<MovementEffectComponent>())
                entity.Delete<MovementEffectComponent>();
        }

        // --- Main-default overloads ---

        public static bool Has(EntityGID target) => Has<Main>(target);

        public static bool Set(EntityGID target, float directionX, float directionZ) =>
            Set<Main>(target, directionX, directionZ);

        public static void Clear(EntityGID target) => Clear<Main>(target);
    }
}
