namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Removes cooldown state when its cooldown component is removed.</summary>
    public class RemoveOrphanCooldownStateSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            World<TWorld>.Query<All<CooldownRemainingComponent>,
                None<CooldownComponent>>().BatchDelete<CooldownRemainingComponent>();
            World<TWorld>.Query<All<CooldownActiveTag>,
                None<CooldownComponent>>().BatchDelete<CooldownActiveTag>();
            World<TWorld>.Query<All<CooldownCompleteTag>,
                None<CooldownComponent>>().BatchDelete<CooldownCompleteTag>();
            World<TWorld>.Query<All<CooldownAutoRestartTag>,
                None<CooldownComponent>>().BatchDelete<CooldownAutoRestartTag>();
            World<TWorld>.Query<All<CooldownDestroyOnCompleteTag>,
                None<CooldownComponent>>().BatchDelete<CooldownDestroyOnCompleteTag>();
        }
    }
}
