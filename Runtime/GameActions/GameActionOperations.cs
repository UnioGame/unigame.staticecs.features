using FFS.Libraries.StaticEcs;


namespace UniGame.StaticEcs.Features
{
    using Unity;

    /// <summary>
    /// Entry points for routing gameplay input through the <see cref="GameActionEvent{TAction}"/>
    /// pipeline with <see cref="ActionMaskComponent"/> gating.
    /// </summary>
    public static class GameActionOperations
    {
        // --- Generic TWorld overloads ---

        /// <summary>
        /// Sends a <see cref="GameActionEvent{TAction}"/> if the world is initialized and the
        /// source entity's <see cref="ActionMaskComponent"/> has the corresponding bit set.
        /// Entities without the component are treated as fully enabled.
        /// </summary>
        /// <returns><c>true</c> if the event was sent.</returns>
        public static bool Raise<TWorld, TAction>(EntityGID source, in TAction payload)
            where TWorld : struct, IWorldType
            where TAction : struct, IGameAction
        {
            if (World<TWorld>.Status != WorldStatus.Initialized)
            {
                return false;
            }

            if (!IsAvailable<TWorld, TAction>(source))
            {
                return false;
            }

            return World<TWorld>.SendEvent(new GameActionEvent<TAction>
            {
                Source = source,
                Payload = payload,
            });
        }

        /// <summary>
        /// Returns <c>true</c> when the source entity either has no
        /// <see cref="ActionMaskComponent"/> or has the bit for
        /// <typeparamref name="TAction"/> set.
        /// </summary>
        public static bool IsAvailable<TWorld, TAction>(EntityGID source)
            where TWorld : struct, IWorldType
            where TAction : struct, IGameAction
        {
            if (!source.TryUnpack<TWorld>(out var entity))
            {
                return false;
            }

            if (!entity.Has<ActionMaskComponent>())
            {
                return true;
            }

            return (entity.Read<ActionMaskComponent>().Bits & ActionBit<TAction>.Mask) != 0;
        }

        /// <summary>Sets the action bit for <typeparamref name="TAction"/> on the source entity.</summary>
        public static void EnableAction<TWorld, TAction>(EntityGID source)
            where TWorld : struct, IWorldType
            where TAction : struct, IGameAction
        {
            if (!source.TryUnpack<TWorld>(out var entity))
            {
                return;
            }

            if (!entity.Has<ActionMaskComponent>())
            {
                return;
            }

            entity.Mut<ActionMaskComponent>().Bits |= ActionBit<TAction>.Mask;
        }

        /// <summary>Clears the action bit for <typeparamref name="TAction"/> on the source entity.</summary>
        public static void DisableAction<TWorld, TAction>(EntityGID source)
            where TWorld : struct, IWorldType
            where TAction : struct, IGameAction
        {
            if (!source.TryUnpack<TWorld>(out var entity))
            {
                return;
            }

            if (!entity.Has<ActionMaskComponent>())
            {
                return;
            }

            entity.Mut<ActionMaskComponent>().Bits &= ~ActionBit<TAction>.Mask;
        }

        // --- Main-default overloads ---

        /// <inheritdoc cref="Raise{TWorld,TAction}"/>
        public static bool Raise<TAction>(EntityGID source, in TAction payload)
            where TAction : struct, IGameAction
            => Raise<Main, TAction>(source, payload);

        /// <inheritdoc cref="IsAvailable{TWorld,TAction}"/>
        public static bool IsAvailable<TAction>(EntityGID source)
            where TAction : struct, IGameAction
            => IsAvailable<Main, TAction>(source);

        /// <inheritdoc cref="EnableAction{TWorld,TAction}"/>
        public static void EnableAction<TAction>(EntityGID source)
            where TAction : struct, IGameAction
            => EnableAction<Main, TAction>(source);

        /// <inheritdoc cref="DisableAction{TWorld,TAction}"/>
        public static void DisableAction<TAction>(EntityGID source)
            where TAction : struct, IGameAction
            => DisableAction<Main, TAction>(source);
    }
}