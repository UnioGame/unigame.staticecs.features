namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Applies timer restart requests to surviving entities.</summary>
    public class RestartCooldownSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private EventReceiver<TWorld, RestartCooldownRequest> _receiver;

        public void Init()
        {
            _receiver = World<TWorld>.RegisterEventReceiver<RestartCooldownRequest>();
        }

        public void Update()
        {
            foreach (var item in _receiver)
                CooldownOperations.Restart<TWorld>(item.Value.Target);
        }

        public void Destroy()
        {
            World<TWorld>.DeleteEventReceiver(ref _receiver);
        }
    }
}
