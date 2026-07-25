namespace UniGame.StaticEcs.Features
{
    using FFS.Libraries.StaticEcs;
    using UniGame.StaticEcs.Unity;

    /// <summary>Main-world damage application system.</summary>
    public struct ApplyDamageSystem : ISystem
    {
        private ApplyDamageSystem<Main> _system;

        /// <inheritdoc />
        public void Init()
        {
            _system.Init();
        }

        /// <inheritdoc />
        public void Update()
        {
            _system.Update();
        }

        /// <inheritdoc />
        public void Destroy()
        {
            _system.Destroy();
        }
    }
}
