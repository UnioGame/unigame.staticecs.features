namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;

    /// <summary>Applies queued count-up timer commands in event order.</summary>
    public class ApplyTimerControlSystem<TWorld> : ISystem
        where TWorld : struct, IWorldType
    {
        private EventReceiver<TWorld, TimerControlRequest> _receiver;

        public void Init()
        {
            _receiver = World<TWorld>.RegisterEventReceiver<TimerControlRequest>();
        }

        public void Update()
        {
            foreach (var item in _receiver)
            {
                var request = item.Value;
                switch (request.Action)
                {
                    case TimerAction.Start:
                        TimerOperations.Start<TWorld>(request.Target);
                        break;
                    case TimerAction.Pause:
                        TimerOperations.Pause<TWorld>(request.Target);
                        break;
                    case TimerAction.Reset:
                        TimerOperations.Reset<TWorld>(request.Target);
                        break;
                    case TimerAction.Restart:
                        TimerOperations.Restart<TWorld>(request.Target);
                        break;
                }
            }
        }

        public void Destroy()
        {
            World<TWorld>.DeleteEventReceiver(ref _receiver);
        }
    }
}
