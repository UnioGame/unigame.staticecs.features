namespace UniGame.StaticEcs.Features
{
    using System;
    using Unity;

    /// <summary>
    /// Main-default alias for <see cref="DamageFeature{TWorld}"/>. See the world-default
    /// aliases convention for usage rules.
    /// </summary>
    [Serializable]
    public sealed class DamageFeature : DamageFeature<Main>
    {
        /// <summary>Creates the Main-world damage feature with default configuration.</summary>
        public DamageFeature() { }

        /// <summary>Creates the Main-world damage feature.</summary>
        public DamageFeature(
            bool registerApplySystem = true,
            bool registerDefaultChain = true,
            short applyOrder = DefaultApplyOrder
        )
            : base(registerApplySystem, registerDefaultChain, applyOrder) { }
    }
}
