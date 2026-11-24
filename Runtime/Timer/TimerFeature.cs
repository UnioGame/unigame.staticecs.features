namespace UniGame.StaticEcs.Features
{
    using Cysharp.Threading.Tasks;
    using FFS.Libraries.StaticEcs;
    using UniGame.Core.Runtime;
    using UniGame.StaticEcs.Time;
    using UniGame.StaticEcs.Unity;

    /// <summary>Runs count-up timers and cooldowns in any update world.</summary>
    public class TimerFeature<TWorld> : StaticEcsFeature<TWorld>
        where TWorld : struct, IWorldType
    {
        public const short CooldownCleanupOrder = -92;
        public const short TimerCleanupOrder = -91;
        public const short CooldownUpdateOrder = -90;
        public const short TimerUpdateOrder = -88;
        public const short CooldownAutoRestartOrder = -87;
        public const short CooldownRestartOrder = -86;
        public const short TimerControlOrder = -85;
        public const short CooldownDestroyOrder = -80;

        public override async UniTask InitializeAsync(ILifeTime lifeTime)
        {
            World<TWorld>.Resource<EcsTime> time = default;
            await time.GetAsync(lifeTime);

            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new RemoveOrphanCooldownStateSystem<TWorld>(), CooldownCleanupOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new RemoveOrphanTimerStateSystem<TWorld>(), TimerCleanupOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new UpdateCooldownSystem<TWorld>(), CooldownUpdateOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new UpdateTimerSystem<TWorld>(), TimerUpdateOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new AutoRestartCooldownSystem<TWorld>(), CooldownAutoRestartOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new RestartCooldownSystem<TWorld>(), CooldownRestartOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new ApplyTimerControlSystem<TWorld>(), TimerControlOrder);
            World<TWorld>.Systems<StaticEcsUpdateSystems>.Add(
                new DestroyCompletedCooldownSystem<TWorld>(), CooldownDestroyOrder);
        }
    }
}
