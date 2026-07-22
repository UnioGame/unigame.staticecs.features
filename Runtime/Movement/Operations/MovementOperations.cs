namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using Unity;
    using UnityEngine;

    /// <summary>Main-world static helpers. See <see cref="MovementOperations"/> generic overloads.</summary>
    public static partial class MovementOperations
    {
        /// <inheritdoc cref="SetDestination{TWorld}(EntityGID, Vector3)"/>
        public static void SetDestination(EntityGID target, Vector3 destination) =>
            SetDestination<Main>(target, destination);

        /// <inheritdoc cref="StopMovement{TWorld}(EntityGID)"/>
        public static void StopMovement(EntityGID target) => StopMovement<Main>(target);

        /// <inheritdoc cref="IsMoving{TWorld}(EntityGID)"/>
        public static bool IsMoving(EntityGID target) => IsMoving<Main>(target);
    }

    // --- Generic overloads ---

    /// <summary>
    /// Operations for reading and writing <see cref="MovementDestinationComponent"/> on entities.
    /// </summary>
    public static partial class MovementOperations
    {
        /// <summary>
        /// Sets the navigation destination and marks the entity as actively moving.
        /// Creates <see cref="MovementDestinationComponent"/> if the entity does not have one yet.
        /// </summary>
        public static void SetDestination<TWorld>(EntityGID target, Vector3 destination)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
            {
                return;
            }

            if (entity.Has<MovementDestinationComponent>())
            {
                ref var existing = ref entity.Mut<MovementDestinationComponent>();
                existing.Destination = destination;
                existing.IsActive = true;
            }
            else
            {
                entity.Set(
                    new MovementDestinationComponent { Destination = destination, IsActive = true }
                );
            }
        }

        /// <summary>
        /// Clears the active flag so the navigation system stops the agent on the next update.
        /// Does nothing if the entity has no <see cref="MovementDestinationComponent"/>.
        /// </summary>
        public static void StopMovement<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
            {
                return;
            }

            if (entity.Has<MovementDestinationComponent>())
            {
                entity.Mut<MovementDestinationComponent>().IsActive = false;
            }
        }

        /// <summary>
        /// Returns <c>true</c> when the entity has an active <see cref="MovementDestinationComponent"/>.
        /// </summary>
        public static bool IsMoving<TWorld>(EntityGID target)
            where TWorld : struct, IWorldType
        {
            if (!target.TryUnpack<TWorld>(out var entity))
            {
                return false;
            }

            return entity.Has<MovementDestinationComponent>()
                && entity.Read<MovementDestinationComponent>().IsActive;
        }
    }
}
