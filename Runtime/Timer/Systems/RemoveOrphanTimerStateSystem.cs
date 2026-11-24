namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Removes timer state when its timer component is removed.</summary>
    public class RemoveOrphanTimerStateSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        public void Update()
        {
            World<TWorld>.Query<All<TimerActiveTag>,
                None<TimerComponent>>().BatchDelete<TimerActiveTag>();
            World<TWorld>.Query<All<TimerCompleteTag>,
                None<TimerComponent>>().BatchDelete<TimerCompleteTag>();
        }
    }
}
