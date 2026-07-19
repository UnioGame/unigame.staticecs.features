 

namespace UniGame.StaticEcs.Features {
    using Unity;

    /// <summary>
    /// Main-default alias for <see cref="DamageFeature{TWorld}"/>. See the world-default
    /// aliases convention for usage rules.
    /// </summary>
    public sealed class DamageFeature : DamageFeature<Main> {
        public DamageFeature(
            bool registerApplySystem = true,
            bool registerDefaultChain = true,
            short applyOrder = DefaultApplyOrder)
            : base(registerApplySystem, registerDefaultChain, applyOrder) {
        }
    }
}
