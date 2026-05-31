using FFS.Libraries.StaticEcs;

namespace unigame.staticecs.features {
    /// <summary>
    /// Typed event dispatched by <see cref="GameActionOperations.Raise{TWorld,TAction}"/> when
    /// the source entity's <see cref="ActionMaskComponent"/> permits the action, or when no mask
    /// component is present (fully-enabled fallback).
    /// </summary>
    public struct GameActionEvent<TAction> : IEvent
        where TAction : struct, IGameAction {
        /// <summary>Entity that triggered the action.</summary>
        public EntityGID Source;

        /// <summary>Action-specific payload.</summary>
        public TAction Payload;
    }
}
