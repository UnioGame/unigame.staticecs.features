namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;

    /// <summary>Registers a closed action event together with its stable world-local ID.</summary>
    public static class GameActionRegistrar
    {
        /// <summary>Declares one action for a custom world.</summary>
        public static void Register<TWorld, TAction>(
            World<TWorld>.TypeRegistrar types,
            byte id)
            where TWorld : struct, IWorldType
            where TAction : struct, IGameAction
        {
            if (!World<TWorld>.HasResource<GameActionRegistry<TWorld>>())
            {
                throw new System.InvalidOperationException(
                    $"World `{typeof(TWorld).Name}` has no game action registry. " +
                    "Install GameActionsFeature resources before registering actions.");
            }

            ref var registry = ref World<TWorld>.GetResource<GameActionRegistry<TWorld>>();
            registry.Register<TAction>(id);
            types.Event<GameActionEvent<TAction>>();
        }

        // --- Main-default overloads ---

        /// <summary>Declares one action for the Main world.</summary>
        public static void Register<TAction>(
            World<Main>.TypeRegistrar types,
            byte id)
            where TAction : struct, IGameAction
        {
            Register<Main, TAction>(types, id);
        }
    }
}
