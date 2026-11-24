namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>
    /// Consumes <see cref="IncomingDamageEvent"/>, runs the registered
    /// <c>DamageFilterChain&lt;TWorld&gt;</c>, and applies the surviving amount to the target's
    /// <see cref="HealthCharacteristic"/>. Emits the matching terminating event for each path:
    /// <see cref="DamageDodgedEvent"/>, <see cref="DamageBlockedEvent"/>, or
    /// <see cref="DamageAppliedEvent"/> with <see cref="DeathPendingTag"/> on lethal hits.
    /// </summary>
    public struct ApplyDamageSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private EventReceiver<TWorld, IncomingDamageEvent> _receiver;

        public void Init()
        {
            _receiver = World<TWorld>.RegisterEventReceiver<IncomingDamageEvent>();
        }

        public void Update()
        {
            foreach (var e in _receiver)
                DamageOperations.Resolve<TWorld>(in e.Value);
        }

        public void Destroy()
        {
            World<TWorld>.DeleteEventReceiver(ref _receiver);
        }
    }
}
